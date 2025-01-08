using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Structure, IDamageable {

    public static Fire Instance;

    [SerializeField] private CircleCollider2D fireRadiusCollider;

    [SerializeField] private bool isMainFire;
    [SerializeField] private bool isEndLevelFire;
    [SerializeField] private bool isHubFire;

    [SerializeField] private float calmFireRadius;
    [SerializeField] private float mildFireRadius;
    [SerializeField] private float wildFireRadius;
    [SerializeField] private float insaneFireRadius;

    [SerializeField] private float orbFuelValue;
    [SerializeField] private float fuelDepletionRate;

    [SerializeField] private int criticalFuelTreshold;
    [SerializeField] private int calmFuelTreshold;
    [SerializeField] private int mildFuelTreshold;
    [SerializeField] private int wildFuelTreshold;
    [SerializeField] private int insaneFuelTreshold;
    [SerializeField] private int maxFuelTreshold;
    [SerializeField] private float extractingEmberFuelRateDepletion = 5f;
    [SerializeField] private float respawningPlayerFuelRateDepletion = 2f;
    [SerializeField] private float debugFuelLevel;

    private FireOrbCollider fireOrbCollider;
    private float fuelLevel;
    private float damageToFuelConversionRate = 5f;

    public enum State {
        extinguished,
        calm,
        mild,
        wild,
        insane,
    }
    private State state;

    public event EventHandler<OnFireChangedStateEventArgs> OnFireChangedState;
    public class OnFireChangedStateEventArgs {
        public State previousState;
        public State newState;
    }

    public event EventHandler OnInitialFireActivated;
    public event EventHandler OnFireFuelled;
    public static event EventHandler OnAnyFireFuelled;
    public event EventHandler OnFireDamageTaken;
    public event EventHandler OnFireEmberExtractionStarted;
    public static event EventHandler OnAnyFireEmberExtractionStarted;
    public event EventHandler OnFireEmberExtractionStopped;
    public static event EventHandler OnAnyFireEmberExtractionStopped;

    private bool isTutorial;
    private bool lerping;
    private bool extractingEmber;
    private float extractingEmberTimer;
    private float extractingEmberTime = 5f;
    private bool respawningPlayer;
    private float respawningPlayerTimer;
    private float respawningPlayerTime = 5f;

    private float lerpTimer;
    private float lerpDuration = 2f;
    private float initialFireAOEValue;
    private float finalFireAOEValue;

    protected override void Awake() {
        if(isMainFire) {
            Instance = this;
            gameObject.SetActive(false);
        }

        if(isEndLevelFire) {
            EndLevelArea.Instance.SetEndLevelFireLit();
        }

        base.Awake();

        fireOrbCollider = GetComponentInChildren<FireOrbCollider>();
    }

    protected override void Start() {
        if(!isHubFire) {
            base.Start();
        } else {
            GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
            GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
            GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;
        }

        isTutorial = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial;

        fireOrbCollider.OnOrbFellInFire += FireOrbCollider_OnOrbFellInFire;

        if (isMainFire) {

            fuelLevel = mildFuelTreshold - 1;
            Player.Instance.OnPlayerBackToTentToRespawn += Player_OnPlayerBackToTentToRespawn;
            ChangeState(State.calm);

        }

        if(isEndLevelFire) {
            fuelLevel = insaneFuelTreshold - 1;
            lerpDuration = 5f;
            ChangeState(State.wild);
        }

        if(isHubFire) {
            state = State.calm;
            ChangeState(State.calm);
        }


        if(isHubFire) {
            SetStructurePrimaryFunctionUnlocked(false);

            if (MetaProgressionManager.Instance.GetHubFireEmberExtractable()) {
                SetStructureSecondaryFunctionUnlocked(true);
                ActivateStructureSecondaryFunctionInteraction(true);
                SetCurrentStructureInteractionType(StructureInteractionType.secondaryFunction);
            };

        } else {
            SetFireCurrentMaxFuelTreshold();
        }
    }

    private void Player_OnPlayerBackToTentToRespawn(object sender, EventArgs e) {
        respawningPlayer = true;
        respawningPlayerTimer = 5f;
    }

    private void Update() {
        if (extractingEmber) {
            extractingEmberTimer -= Time.deltaTime;
            fuelLevel -= Time.deltaTime * extractingEmberFuelRateDepletion;
            if (extractingEmberTimer < 0) {
                extractingEmber = false;
                StartCoroutine(ExtractEmber());
            }

        } else if (respawningPlayer) {
            respawningPlayerTimer -= Time.deltaTime;
            fuelLevel -= Time.deltaTime * respawningPlayerFuelRateDepletion;
            if (respawningPlayerTimer < 0) {
                respawningPlayer = false;
            }
        }
        else {
            if (fuelLevel > 0) {
                fuelLevel -= Time.deltaTime * fuelDepletionRate;
            }
        }

        if (isHubFire) return;

        if (lerping) {

            lerpTimer += Time.deltaTime;
            float normalizedTime = lerpTimer / lerpDuration;

            if (normalizedTime >= 1) {
                normalizedTime = 1;
                lerpTimer = 0;
                lerping = false;
            }

            float currentFireAOEValue = Mathf.Lerp(initialFireAOEValue, finalFireAOEValue, normalizedTime);

            ChangeFireRadius(currentFireAOEValue);
        }

        CheckFireStateDowngrade();
        CheckFireSecondaryFunctionInteractable();
        debugFuelLevel = fuelLevel;
    }
    
    private void ChangeFireRadius(float fireRadius) {
        fireRadiusCollider.radius = fireRadius;
    }

    private void FireOrbCollider_OnOrbFellInFire(object sender, EventArgs e) {
        fuelLevel += orbFuelValue;

        Debug.Log("fuelLevel " + fuelLevel);
        Debug.Log("maxFuelTreshold " + maxFuelTreshold);
        if(fuelLevel >= maxFuelTreshold) {
            fuelLevel = maxFuelTreshold;
        }

        CheckFireStateUpgrade();

        OnFireFuelled?.Invoke(this, EventArgs.Empty);
        OnAnyFireFuelled?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator ExtractEmber() {
        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.ember), transform.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomForce(-7,7,3, 5);
        collectible.SetCollectibleUnInteractable(1f);
        collectible.SetCanNeverBePickedUpByWorker();
        ActivateStructureSecondaryFunctionInteraction(false);

        yield return new WaitForSeconds(2f);
        OnFireEmberExtractionStopped?.Invoke(this, EventArgs.Empty);
        OnAnyFireEmberExtractionStopped?.Invoke(this, EventArgs.Empty);
    }

    private void CheckFireFeedable() {
        if (isTutorial) return;

        if(fuelLevel + orbFuelValue <= maxFuelTreshold) {
            SetStructurePrimaryFunctionUnlocked(true);
        } else {
            SetStructurePrimaryFunctionUnlocked(false);
        }

        if (state == State.extinguished) {
            SetStructurePrimaryFunctionUnlocked(false);
        }
    } 

    private void CheckFireStateDowngrade() {

        if (fuelLevel < 0 && state == State.calm) {
            ChangeState(State.extinguished);
        }

        if(fuelLevel <= mildFuelTreshold && state == State.mild) {
            ChangeState(State.calm);
        }

        if (fuelLevel <= wildFuelTreshold && state == State.wild) {
            ChangeState(State.mild);
        }

        if (fuelLevel <= insaneFuelTreshold && state == State.insane) {
            ChangeState(State.wild);
        }
        CheckFireFeedable();
    }

    private void CheckFireStateUpgrade() {
       
        if (state == State.calm && fuelLevel >= mildFuelTreshold) {
            ChangeState(State.mild);
        }

        if (state == State.mild && fuelLevel >= wildFuelTreshold) {
            ChangeState(State.wild);
        }

        if(state == State.wild && fuelLevel >= insaneFuelTreshold) {
            ChangeState(State.insane);
        }
    }

    private void CheckFireSecondaryFunctionInteractable() {
        if (PlayerCurrencies.Instance.GetCarryingEmber()) {
            SetStructureSecondaryFunctionUnlocked(false);
        };

        if (extractingEmber) return;
        if (isTutorial) return;
        if (isEndLevelFire) return;

        if(fuelLevel > (maxFuelTreshold - orbFuelValue)) {
            SetStructureSecondaryFunctionUnlocked(true);
        } else {
            SetStructureSecondaryFunctionUnlocked(false);
        }
    }

    private void ChangeState(State newState) {
        SetFireAOEValues(newState);

        State previousState = state;
        state = newState;

        OnFireChangedState?.Invoke(this, new OnFireChangedStateEventArgs {
            previousState = previousState,
            newState = newState
        });

        if(newState == State.extinguished && isMainFire) {
            LevelManager.Instance.LooseLevel();
        }
    }

    private void SetFireCurrentMaxFuelTreshold() {
        State maxState = State.mild;

        if (Tent.Instance.GetStructureLevel() == 2) {
            maxState = State.wild;
        }
        if (Tent.Instance.GetStructureLevel() == 3) {
            maxState = State.insane;
        }
        if (Tent.Instance.GetStructureLevel() == 4) {
            maxState = State.insane;
        }

        if (maxState == State.calm) {
            maxFuelTreshold = mildFuelTreshold;
        }
        if (maxState == State.mild) {
            maxFuelTreshold = wildFuelTreshold;
        }
        if (maxState == State.wild) {
            maxFuelTreshold = insaneFuelTreshold;
        }
    }

    public void ManualSetFireCurrentMaxFuelTreshold(State state) {
        if (state == State.calm) {
            maxFuelTreshold = mildFuelTreshold;
        }
        if (state == State.mild) {
            maxFuelTreshold = wildFuelTreshold;
        }
        if (state == State.wild) {
            maxFuelTreshold = insaneFuelTreshold;
        }
    }

    private void SetFireAOEValues(State newState) {

        if (state == State.extinguished) {
            initialFireAOEValue = 0;
        }

        if (state == State.mild) {
            initialFireAOEValue = mildFireRadius;
        }
        if (state == State.calm) {
            initialFireAOEValue = calmFireRadius;
        }
        if (state == State.wild) {
            initialFireAOEValue = wildFireRadius;
        }
        if (state == State.insane) {
            initialFireAOEValue = insaneFireRadius;
        }

        if (newState == State.extinguished) {
            finalFireAOEValue = 0;
        }
        if (newState == State.mild) {
            finalFireAOEValue = mildFireRadius;
        }
        if (newState == State.calm) {
            finalFireAOEValue = calmFireRadius;
        }
        if (newState == State.wild) {
            finalFireAOEValue = wildFireRadius;
        }
        if (newState == State.insane) {
            finalFireAOEValue = insaneFireRadius;
        }

        lerping = true;
    }

    protected override void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;

        playerInteracting = true;

        if (currentStructureInteractionType == StructureInteractionType.secondaryFunction) {
            // Player is trying to extract ember
            OnFireEmberExtractionStarted?.Invoke(this, EventArgs.Empty);
            OnAnyFireEmberExtractionStarted?.Invoke(this, EventArgs.Empty);
            extractingEmber = true;
            extractingEmberTimer = extractingEmberTime;
        } else {
            payCurrencyUI.SetPlayerInteracting(true);
        }

    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerCanInteract) return;
        if (!playerInteracting) return;

        playerInteracting = false;
        payCurrencyUI.SetPlayerInteracting(false);

        if(extractingEmber) {
            extractingEmber = false;
            OnFireEmberExtractionStopped?.Invoke(this, EventArgs.Empty);
            OnAnyFireEmberExtractionStopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ActivateInitialFire() {
        OnInitialFireActivated?.Invoke(this, EventArgs.Empty);
        PlayerCurrencies.Instance.SetCarryingEmber(false);
    }

    public State GetState() {
        return state;
    }

    public float GetCalmFireRadius() {
        return calmFireRadius;
    }
    public float GetMildFireRadius() {
        return mildFireRadius;
    }
    public float GetWildFireRadius() {
        return wildFireRadius;
    }

    public float GetCurrentFireRadius() {
        if(state == State.calm) {
            return calmFireRadius;
        }

        if(state == State.mild) {
            return mildFireRadius;
        }

        if(state == State.wild) {
            return wildFireRadius;
        }

        if(state == State.insane) {
            return insaneFireRadius;
        }

        return 0f;
    }
    public float GetInsaneFireRadius() {
        return insaneFireRadius;
    }

    public float GetCriticalFuelTreshold() {
        return criticalFuelTreshold;
    }
    public float GetCalmFireTreshold() {
        return calmFuelTreshold;
    }

    public float GetMildFireTreshold() {
        return mildFuelTreshold;
    }

    public float GetWildFireTreshold() {
        return wildFuelTreshold;
    }

    public float GetInsaneFireTreshold() {
        return insaneFuelTreshold;
    }

    public float GetMaxFireTreshold() {
        return maxFuelTreshold;
    }

    public float GetLerpDuration() {
        return lerpDuration;
    }
    public float GetOrbFuelValue() {
        return orbFuelValue;
    }

    public float GetCurrentFuelLevel() {
        return fuelLevel;
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit = false) {
        fuelLevel -= (damage * damageToFuelConversionRate);
        CheckFireStateDowngrade();
        OnFireDamageTaken?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {

    }

    public void SetHubFireEmberExtractable() {
        MetaProgressionManager.Instance.SetHubFireEmberExtractable();
        SetStructureSecondaryFunctionUnlocked(true);
        ActivateStructureSecondaryFunctionInteraction(true);
        SetCurrentStructureInteractionType(StructureInteractionType.secondaryFunction);
    }
    public Transform GetProjectileTarget() {
        return transform;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }
}
