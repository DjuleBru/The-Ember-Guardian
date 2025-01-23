using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DogAnimatorManager : MonoBehaviour {

    [SerializeField] private Animator dogBodyAnimator;

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

    private void Awake() {
        animator = GetComponent<Animator>();
        dog = GetComponentInParent<Dog>();
        dogAI = GetComponentInParent<DogAI>();
        dogMovement = GetComponentInParent<MobMovement>();

        Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;

        sniffTimer = sniffTrialRate;
        sitTimer = sitTrialRate;
        sleepTimer = sleepTrialRate;
    }

    private void Start() {
        dogAI.OnStateChanged += DogAI_OnStateChanged;
        dogAI.OnDogBite += DogAI_OnDogBite;
        dogAI.SetReadyToMove(false);
        DogDigAbility.Instance.OnSniffStart += DogDigAbility_OnSniffStart;

        digAbilityUnlocked = DogStats.Instance.GetdigResourceAbilityUnlocked();
    }

    private void DogAI_OnDogBite(object sender, EventArgs e) {
        animator.SetTrigger("Bite");
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        dogBodyAnimator.SetTrigger("Teleport_Out");
    }

    private void Portal_OnAnyPlayerTeleported(object sender, EventArgs e) {
        dogBodyAnimator.SetTrigger("Teleport");
    }

    private void DogAI_OnStateChanged(object sender, System.EventArgs e) {
        DogAI.State newState = dogAI.GetState();
        ResetAllTriggers();
        //ResetAllBools();

        if (newState == DogAI.State.stay) {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
            animator.ResetTrigger("Stand");
            animator.SetTrigger("Sit");
        }

        if (newState == DogAI.State.idle || newState == DogAI.State.stayAtCamp) {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
        }

        if (newState == DogAI.State.runWithPlayer || newState == DogAI.State.runToCamp) {
            animator.SetTrigger("Wake");
            animator.SetTrigger("Stand");
            animator.SetBool("Running", true);
        }

        if (newState == DogAI.State.walkWithPlayer) {
            animator.SetTrigger("Wake");
            animator.SetTrigger("Stand");
            animator.SetBool("Running", false);
        }

        if(newState == DogAI.State.growling) {
            animator.SetBool("Growling", true);
        } else {
            animator.SetBool("Growling", false);
        }


        if (newState == DogAI.State.attacking) {
            animator.SetBool("Running", true);
            animator.SetBool("Barking", true);
            animator.SetBool("Growling", false);
            return;
        }

        if (newState == DogAI.State.barking) {
            animator.SetBool("Barking", true);
        }
        else {
            animator.SetBool("Barking", false);
        }
    }

    private void Update() {

        HandleXScale();
        HandleAnimatorMovementBool();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string stateName = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => Animator.StringToHash(clip.name) == stateInfo.shortNameHash)?.name;

        if(!digAbilityUnlocked) {
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

        if (moving) {
            HandleScaleChange(moveDir);
        } else {
            HandleScaleChange(watchDir);
        }

    }

    private void HandleScaleChange(float watchDir) {
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
        dogAI.SetReadyToMove(true);
    }

    public void SetUnReadyToMove() {
        dogAI.SetReadyToMove(false);
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
    }
}
