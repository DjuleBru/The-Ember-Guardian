using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DogAnimatorManager : MonoBehaviour {

    [SerializeField] private Animator dogBodyAnimator;
    [SerializeField] private RuntimeAnimatorController germanShepherdAnimator;
    [SerializeField] private RuntimeAnimatorController goldenAnimator;
    [SerializeField] private RuntimeAnimatorController darkCompanionAnimator;
    [SerializeField] private PetDog petDog;
    [SerializeField] private DogAI_DarkCompanion darkCompanionAI;
    [SerializeField] private DogAI_Retreiver retreiverAI;

    private Animator animator;
    private Dog dog;
    private DogAI dogAI;
    private MobMovement dogMovement;

    private float moveDir;
    private float watchDir;
    private float previousWatchDir = 1f;
    private bool moving;
    private bool digAbilityUnlocked;

    private float sniffTimer;
    private float sniffDuration;
    private float sniffMinDuration = 2f;
    private float sniffMaxDuration = 6f;
    private float sniffTrialRate = 4f;
    private float sniffProbability = .33f;

    private float sitTimer;
    private float sitDuration;
    private float sitMinDuration = 4f;
    private float sitMaxDuration = 8f;
    private float sitTrialRate = 4f;
    private float sitProbability = .5f;

    private float sleepTimer;
    private float sleepDuration;
    private float sleepMinDuration = 15f;
    private float sleepMaxDuration = 20f;
    private float sleepTrialRate = 4f;
    private float sleepProbability = .2f;

    private float barkTimer;
    private float barkDuration = 2f;

    public event EventHandler OnFootstepTriggered;
    public event EventHandler OnDogSniffed;
    public event EventHandler OnDogSniffedEnd;
    public event EventHandler OnDogPant;
    public event EventHandler OnDogBreathe;
    public event EventHandler OnDogSit;
    public event EventHandler OnDogGroan;
    public event EventHandler OnDogGrowl;
    public event EventHandler OnDogBark;
    public event EventHandler OnDogBite;

    private bool running;
    private bool shootingContinuousLaser;
    private bool stomping;

    private void Awake() {
        animator = GetComponent<Animator>();
        dog = GetComponentInParent<Dog>();
        dogMovement = GetComponentInParent<MobMovement>();

        dog.OnDogTypeChanged += Dog_OnDogTypeChanged;
        petDog.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
        petDog.OnPlayerRefreshedPettingDog += PetDog_OnPlayerRefreshedPettingDog;
        petDog.OnPlayerStoppedPettingDog += PetDog_OnPlayerStoppedPettingDog;
        Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyDogWarped += FastTravelTP_OnAnyDogWarped;

        sniffTimer = sniffTrialRate;
        sitTimer = sitTrialRate;
        sleepTimer = sleepTrialRate;
    }


    private void Start() {
        foreach(DogAI dogAI in Dog.Instance.GetDogAIList()) {
            dogAI.OnStateChanged += DogAI_OnStateChanged;
            dogAI.OnDogBite += DogAI_OnDogBite;
        }
        dogAI = Dog.Instance.GetCurrentDogAI();

        darkCompanionAI.OnLaserAbilityStarted += DarkCompanionAI_OnLaserAbilityStarted;
        darkCompanionAI.OnStompAbilityStarted += DarkCompanionAI_OnStompAbilityStarted;
        darkCompanionAI.OnLaserAbilityEnded += DarkCompanionAI_OnLaserAbilityEnded;
        darkCompanionAI.OnStompAbilityEnded += DarkCompanionAI_OnStompAbilityEnded;
        retreiverAI.OnDogStartedDroppingCurrency += RetreiverAI_OnDogStartedDroppingCurrency;

        dogMovement.SetReadyToMoveAnimator(false);
        DogDigAbility.Instance.OnSniffStart += DogDigAbility_OnSniffStart;

        digAbilityUnlocked = DogStats.Instance.GetGermanShepherdDigResourceAbilityUnlocked();
        DogStats.Instance.OnNewAbilityUnlocked += DogStats_OnNewAbilityUnlocked;

        SetDogTypeAnimator();
    }

    private void DogStats_OnNewAbilityUnlocked(object sender, EventArgs e) {
        digAbilityUnlocked = DogStats.Instance.GetGermanShepherdDigResourceAbilityUnlocked();
    }

    private void Update() {
        if (!stomping && !shootingContinuousLaser) {
            HandleXScale();
        }
        HandleAnimatorMovementBool();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string stateName = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => Animator.StringToHash(clip.name) == stateInfo.shortNameHash)?.name;

        if (!digAbilityUnlocked) {
            if (stateName == "Doggo_Walk") {
                HandleSniffStart();
            }
        }

        if (stateName == "Doggo_WalkSniff") {
            HandleSniffEnd();
        }

        if (stateName == "Doggo_Idle") {
            HandleSitStart();
            HandleSleepStart();
        }

        if (stateName == "Doggo_SitIdle") {
            HandleSitEnd();
        }

        if (stateName == "Doggo_Sleep") {
            HandleSleepEnd();
        }

        if (stateName == "Doggo_Bark") {
            HandleBarkEnd();
        }
    }
    private void DarkCompanionAI_OnStompAbilityEnded(object sender, EventArgs e) {
        stomping = false;
    }

    private void DarkCompanionAI_OnLaserAbilityEnded(object sender, EventArgs e) {
        shootingContinuousLaser = false;
    }

    private void DarkCompanionAI_OnStompAbilityStarted(object sender, EventArgs e) {
        animator.SetTrigger("Stomp");
        stomping = true;
    }

    private void DarkCompanionAI_OnLaserAbilityStarted(object sender, EventArgs e) {
        animator.SetTrigger("Laser");
        shootingContinuousLaser = true;
        HandleScaleChange(Dog.Instance.GetComponent<DogAI_DarkCompanion>().GetWatchDir());
    }

    private void Dog_OnDogTypeChanged(object sender, EventArgs e) {
        SetDogTypeAnimator();
        dogAI = Dog.Instance.GetCurrentDogAI();
    }
    private void SetDogTypeAnimator() {
        Dog.DogType dogType = Dog.Instance.GetDogType();

        if(dogType == Dog.DogType.GermanShepherd) {
            animator.runtimeAnimatorController = germanShepherdAnimator;
        }
        if (dogType == Dog.DogType.GoldenRetreiver) {
            animator.runtimeAnimatorController = goldenAnimator;
        }
        if (dogType == Dog.DogType.DarkCompanion) {
            animator.runtimeAnimatorController = darkCompanionAnimator;
        }
    }

    private void DogAI_OnDogBite(object sender, EventArgs e) {
        animator.SetTrigger("Bite");
    }

    private void PetDog_OnPlayerStartedPettingDog(object sender, EventArgs e) {
        animator.ResetTrigger("Pet_Loop");
        animator.ResetTrigger("Pet_End");
        animator.SetTrigger("Pet");
    }

    private void PetDog_OnPlayerRefreshedPettingDog(object sender, EventArgs e) {
        animator.ResetTrigger("Pet");
        animator.ResetTrigger("Pet_End");
        animator.SetTrigger("Pet_Loop");
    }
    private void PetDog_OnPlayerStoppedPettingDog(object sender, EventArgs e) {
        animator.ResetTrigger("Pet");
        animator.ResetTrigger("Pet_Loop");
        animator.SetTrigger("Pet_End");
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        dogBodyAnimator.SetTrigger("Teleport_Out");
    }

    private void Portal_OnAnyPlayerTeleported(object sender, EventArgs e) {
        dogBodyAnimator.SetTrigger("Teleport");
    }

    private void FastTravelTP_OnAnyDogWarped(object sender, EventArgs e) {
        dogBodyAnimator.SetTrigger("Teleport_Out");
    }
    private void DogAI_OnStateChanged(object sender, System.EventArgs e) {
        DogAI.State newState = dog.GetCurrentDogAI().GetState();
        ResetAllTriggers();
        CheckStopSniffing();
        sitTimer = sitTrialRate;
        sleepTimer = sleepTrialRate;

        if (newState == DogAI.State.stay) {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
            animator.ResetTrigger("Stand");
            animator.SetTrigger("Sit");
        }

        if (newState == DogAI.State.idle || newState == DogAI.State.stayAtCamp || newState == DogAI.State.nightInCampIdle) {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
        }

        if (newState == DogAI.State.runWithPlayer || newState == DogAI.State.runToCamp || newState == DogAI.State.nightInCampRunToClosestCreature || newState == DogAI.State.pickingUpOrbs || newState == DogAI.State.runToPlayer) {
            animator.SetTrigger("Wake");
            animator.SetTrigger("Stand");
            animator.SetBool("Running", true);
        }

        if (newState == DogAI.State.walkWithPlayer) {
            animator.SetTrigger("Wake");
            animator.SetTrigger("Stand");
            animator.SetBool("Walking", true);
            animator.SetBool("Running", false);
        }

        if(newState == DogAI.State.growling || newState == DogAI.State.nightInCampGrowlAtIncomingCreature) {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
            animator.SetBool("Growling", true);
        } else {
            animator.SetBool("Growling", false);
        }


        if (newState == DogAI.State.attacking) {
            animator.SetBool("Running", true);
            animator.SetBool("Barking", true);
            animator.SetBool("Growling", false);
            barkTimer = barkDuration;
            return;
        }

        if (newState == DogAI.State.barking) {
            animator.SetBool("Barking", true);
            animator.SetBool("Growling", false);
            barkTimer = barkDuration;
        }
        else {
            animator.SetBool("Barking", false);
        }
    }

    private void RetreiverAI_OnDogStartedDroppingCurrency(object sender, EventArgs e) {
        animator.SetBool("Running", false);
        animator.SetBool("Walking", false);
    }


    private void CheckStopSniffing() {
        if (!digAbilityUnlocked) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Doggo_WalkSniff")) {
            animator.SetBool("Sniffing", false);
            OnDogSniffedEnd?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DogDigAbility_OnSniffStart(object sender, EventArgs e) {
        sniffDuration = UnityEngine.Random.Range(sniffMinDuration, sniffMaxDuration);
        sniffTimer = sniffDuration;

        animator.SetBool("Sniffing", true);
    }

    private void HandleSniffStart() {
        sniffTimer -= Time.deltaTime;

        if(sniffTimer < 0) {
            sniffTimer = sniffTrialRate;

            float randomFloat = UnityEngine.Random.Range(0f,1f);

            if(randomFloat < sniffProbability) {

                sniffDuration = UnityEngine.Random.Range(sniffMinDuration, sniffMaxDuration);
                sniffTimer = sniffDuration;

                animator.SetBool("Sniffing", true);
            }
        }
    }

    private void HandleSniffEnd() {
        sniffTimer -= Time.deltaTime;

        if (sniffTimer < 0) {
            sniffTimer = sniffTrialRate;

            animator.SetBool("Sniffing", false);
            OnDogSniffedEnd?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleSitStart() {
        sitTimer -= Time.deltaTime;

        if (sitTimer < 0) {
            sitTimer = sitTrialRate;

            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            if (randomFloat < sitProbability) {
                sitDuration = UnityEngine.Random.Range(sitMinDuration, sitMaxDuration);
                sitTimer = sitDuration;

                animator.ResetTrigger("Stand");
                animator.SetTrigger("Sit");

            }
        }
    }

    private void HandleSitEnd() {
        sitTimer -= Time.deltaTime;

        if (sitTimer < 0) {
            sitTimer = sitTrialRate;

            animator.SetTrigger("Stand");
        }
    }

    private void HandleSleepStart() {
        sleepTimer -= Time.deltaTime;

        if (sleepTimer < 0) {
            sleepTimer = sleepTrialRate;

            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            if (randomFloat < sleepProbability) {
                sleepDuration = UnityEngine.Random.Range(sleepMinDuration, sleepMaxDuration);
                sleepTimer = sleepDuration;

                animator.ResetTrigger("Wake");
                animator.SetTrigger("Sleep");

            }
        }
    }

    private void HandleSleepEnd() {
        sleepTimer -= Time.deltaTime;

        if (sleepTimer < 0) {
            sleepTimer = sitTrialRate;

            animator.SetTrigger("Wake");
        }
    }

    private void HandleBarkEnd() {
        barkTimer -= Time.deltaTime;

        if (barkTimer < 0) {
            barkTimer = barkDuration;

            animator.SetBool("Growling", true);
            animator.SetBool("Barking", false);
        }
    }

    private void HandleAnimatorMovementBool() {

        if (moveDir != 0) {

            if (!moving) {
                animator.SetBool("Walking", true);
            }

            moving = true;

        }
        else {
            if (moving) {
                animator.SetBool("Walking", false);
            }

            moving = false;

        }
    }

    private void HandleXScale() {
        moveDir = dogMovement.GetMoveDirFloat();

        if (dogAI.GetClosestCreature() != null) {
            float dirToCreature = dogAI.GetClosestCreature().transform.position.x - transform.position.x;
            if (dirToCreature < 0) {
                watchDir = -1f;
            }
            else {
                watchDir = 1f;
            }
        }

        if (dogAI.GetClosestIncomingCreature() != null) {
            float dirToCreature = dogAI.GetClosestIncomingCreature().transform.position.x - transform.position.x;
            if (dirToCreature < 0) {
                watchDir = -1f;
            }
            else {
                watchDir = 1f;
            }
        }

        if (moving) {
            HandleScaleChange(moveDir);
        } else {
            HandleScaleChange(watchDir);
        }

    }

    public void HandleScaleChange(float watchDir) {
        if (watchDir < 0 && previousWatchDir > 0) {
            previousWatchDir = watchDir;
            Vector3 newScale = new Vector3(-1, 1, 1);
            transform.localScale = newScale;
        }

        if (watchDir > 0 && previousWatchDir < 0) {
            previousWatchDir = watchDir;
            Vector3 newScale = new Vector3(1, 1, 1);
            transform.localScale = newScale;
        }
    }

    public void SetReadyToMove() {
        //dogAI.SetReadyToMove(true);
        dogMovement.SetReadyToMoveAnimator(true);
    }

    public void SetUnReadyToMove() {
        //dogAI.SetReadyToMove(false);
        dogMovement.SetReadyToMoveAnimator(false);
    }

    private void ResetAllTriggers() {
        animator.ResetTrigger("Sit");
        animator.ResetTrigger("Stand");
        animator.ResetTrigger("Sleep");
        animator.ResetTrigger("Wake");
    }

    private void ResetAllBools() {
        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
        animator.SetBool("Sniffing", false);
        animator.SetBool("Barking", false);
    }

    public void TriggerFootstep() {
        OnFootstepTriggered?.Invoke(this, EventArgs.Empty);
    }
    public void TriggerSniff() {
        OnDogSniffed?.Invoke(this, EventArgs.Empty);
    }
    public void TriggerPant() {
        OnDogPant?.Invoke(this, EventArgs.Empty);
    }
    public void TriggerBreathe() {
        OnDogBreathe?.Invoke(this, EventArgs.Empty);
    }
    public void TriggerGroan() {
        OnDogGroan?.Invoke(this, EventArgs.Empty);
    }
    public void TriggerGrowl() {
        OnDogGrowl?.Invoke(this, EventArgs.Empty);
    }
    public void TriggerBark() {
        OnDogBark?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerBite() {
        OnDogBite?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy() {
        Portal.OnAnyPlayerTeleported -= Portal_OnAnyPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        DogDigAbility.Instance.OnSniffStart -= DogDigAbility_OnSniffStart;
        FastTravelTP.OnAnyDogWarped -= FastTravelTP_OnAnyDogWarped;
    }
}
