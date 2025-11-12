using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Structure, IDamageable {

    public enum PrimordialFireColor {
        Orange,
        Red,
        Blue,
        Green,
        Purple,
    }

    public static Fire Instance;

    [SerializeField] private CircleCollider2D fireRadiusCollider;

    [SerializeField] private bool isMainFire;
    [SerializeField] private bool isEndLevelFire;
    [SerializeField] private bool isSecondaryFire;
    [SerializeField] private bool isHubFire;
    [SerializeField] private bool isPrimordialFire;
    [ShowIf("isPrimordialFire")]
    [SerializeField] private PrimordialFireColor primordialFireColor;

    [SerializeField] private float calmFireRadius;
    [SerializeField] private float mildFireRadius;
    [SerializeField] private float wildFireRadius;
    [SerializeField] private float insaneFireRadius;

    private float orbFuelValue = 10;
    [SerializeField] private float fuelDepletionRate = 0.05f;
    private int maxFuelTreshold = 50;
    private int currentMaxFuelTreshold = 50;
    private float fuelTickValue = 10f/3f;

    [SerializeField] private int criticalFuelTreshold = 14;
    [SerializeField] private int calmFuelTreshold = 0;
    [SerializeField] private int mildFuelTreshold = 10;
    [SerializeField] private int wildFuelTreshold = 20;
    [SerializeField] private int insaneFuelTreshold = 55;

    private float extractingEmberFuelRateDepletion = 8f;
    [SerializeField] private float respawningPlayerFuelRateDepletion = 2f;
    [SerializeField] private float debugFuelLevel;
    [SerializeField] private Transform emberSpawnPosition;

    private FireOrbCollider fireOrbCollider;
    private bool initialFireLit;
    private bool mainFireEmberExtractable;
    private bool emberExtracted;
    private bool emberExtractionDisabled;
    private float fuelLevel;
    private float damageToFuelConversionRate = 5f;

    private StructureLocation_SecondaryFire secondaryFireStructureLocation;

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
    public event EventHandler OnFuelLevelLoaded;
    public event EventHandler OnFireFuelled;
    public static event EventHandler OnAnyFireFuelled;
    public event EventHandler OnFireDamageTaken;
    public event EventHandler OnFireEmberExtracted;
    public event EventHandler OnFireEmberExtractionStarted;
    public static event EventHandler OnAnyFireEmberExtractionStarted;
    public event EventHandler OnFireEmberExtractionStopped;
    public event EventHandler OnPrimordialFireLit;
    public static event EventHandler OnAnyFireEmberExtractionStopped;
    public static event EventHandler OnAnySecondaryFireReset;
    public static event EventHandler OnFireExtinguishedByPlayerRespawning;

    private bool lockFireInteractionFunctionsUpdate;
    private bool justFuelledFire;
    private bool lerping;
    private bool extractingEmber;
    private float extractingEmberTimer;
    private float extractingEmberTime = 3f;
    private float extractingEmberFuelUsed;
    private bool respawningPlayer;
    private float respawningPlayerTimer;
    private float respawningPlayerTime = 5f;

    private float lerpTimer;
    private float lerpDuration = 2f;
    private float initialFireAOEValue;
    private float finalFireAOEValue;

    private float fuelFireNightCooldown = 5f;
    private float fuelFireNightTimer;
    private bool fuelFireOnCooldown;

    protected override void Awake() {
        if(isMainFire) {
            Instance = this;
            gameObject.SetActive(false);
        }

        if(isEndLevelFire) {
            if(EndLevelArea.Instance_Left != null) {
                if(transform.position.x < 0) {
                    EndLevelArea.Instance_Left.SetEndLevelFireLit();
                } else {
                    EndLevelArea.Instance.SetEndLevelFireLit();
                }
            } else {
                EndLevelArea.Instance.SetEndLevelFireLit();
            }
        }

        if(isHubFire) {
            LoadLastPrimordialFireLit();
        }

        base.Awake();

        fireOrbCollider = GetComponentInChildren<FireOrbCollider>();
    }

    protected override void Start() {
        LoadStats();

        if(!isHubFire) {
            base.Start();
        } else {
            
            GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
            GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
            GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;
            if(UICurrencyManager.PlayerInventoryUI != null) {
                UICurrencyManager.PlayerInventoryUI.OnCurrencyRemovedFromBag += PlayerInventoryUI_OnCurrencyRemovedFromBag;
                UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
            }
        }

        if (isMainFire) {
            fuelLevel = wildFuelTreshold - 1;
            Player.Instance.OnPlayerBackToTentToRespawn += Player_OnPlayerBackToTentToRespawn;
            PlayerCurrencies.Instance.OnEmberDropped += PlayerCurrencies_OnEmberDropped;
            Tent.Instance.OnStructureUpgraded += Tent_OnStructureUpgraded;
            ChangeState(State.calm);

        }

        if(isEndLevelFire) {
            fuelLevel = insaneFuelTreshold - 1;
            lerpDuration = 5f;
            ChangeState(State.wild);
        }

        if (isSecondaryFire) {
           ResetSecondaryFire();
        }

        if (isHubFire) {
            state = State.calm;
            ChangeState(State.calm);
            SetStructurePrimaryFunctionUnlocked(false);
        } else {
            SetFireCurrentMaxFuelTreshold();
        }

        if(isPrimordialFire && !isHubFire) {
            LevelManager.Instance.OnLevelSuccess += LevelManager_OnLevelSuccess;
        }
    }
    protected override void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);
        if(isSecondaryFire && fuelLevel >= 0) {
            ResetSecondaryFire();
        }
    }

    private void Update() {
        HandleFuelDecrease();
        HandleFuelFireCooldown();
        CheckNeedsRefillFromEngineer();

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
    private void ResetSecondaryFire() {
        fuelLevel = wildFuelTreshold - 1;
        ChangeState(State.calm);
        SetStructurePrimaryFunctionUnlocked(false);
        OnAnySecondaryFireReset?.Invoke(this, EventArgs.Empty);
    }
    private void LoadStats() {
        orbFuelValue = StructureStats.Instance.GetOrbFuelValue();

        if(isSecondaryFire) {
            fuelDepletionRate = StructureStats.Instance.GetSecondaryFireFuelDepletionRate();
            maxFuelTreshold = StructureStats.Instance.GetSecondaryFireMaxFuelTreshold();
            currentMaxFuelTreshold = maxFuelTreshold;
        } else {
            fuelDepletionRate = StructureStats.Instance.GetMainFireFuelDepletionRate();
            maxFuelTreshold = StructureStats.Instance.GetMainFireMaxFuelTreshold();
        }
    }

    private void LoadLastPrimordialFireLit() {
        primordialFireColor = ES3.Load("lastPrimordialFireLit", PrimordialFireColor.Orange);
    }

    private void LevelManager_OnLevelSuccess(object sender, EventArgs e) {
        ES3.Save("lastPrimordialFireLit", primordialFireColor);
        OnPrimordialFireLit?.Invoke(this, EventArgs.Empty);

    }

    [Button]
    public void InvokePrimordialFireLit() {

        OnPrimordialFireLit?.Invoke(this, EventArgs.Empty);
    }

    public void SetFuelLevel(float fuelLevel) {
        this.fuelLevel = fuelLevel;
        CheckFireStateUpgrade();
        CheckFireStateDowngrade();
        OnFuelLevelLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void SetAsEndFire() {
        isEndLevelFire = true;
    }

    public void SetAsSecondaryFire() {
        isSecondaryFire = true;
    }

    protected override void TriggerStructurePrimaryFunction() {
        base.TriggerStructurePrimaryFunction();

        if (isSecondaryFire) {

            fuelLevel = wildFuelTreshold - 1;
            ChangeState(State.calm);
            SetStructurePrimaryFunctionUnlocked(false);

        } else {

            StartCoroutine(OrbPaidToFuelCoroutine(.1f));

        }


    }

    private IEnumerator OrbPaidToFuelCoroutine(float delay) {
        yield return new WaitForSeconds(delay);

        FuelFire(orbFuelValue);

        if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            fuelFireOnCooldown = true;
            fuelFireNightTimer = 0;
        }

        StartCoroutine(SetJustFuelledFireFalseAfterDelay());
        justFuelledFire = true;
        OnFireFuelled?.Invoke(this, EventArgs.Empty);
        OnAnyFireFuelled?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!isHubFire) return;
        if (emberExtracted) return;
        if (emberExtractionDisabled) return;
        RefreshHubFireEmberExtractable();
    }

    private void PlayerInventoryUI_OnCurrencyRemovedFromBag(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!isHubFire) return;
        if (emberExtracted) return;
        if (emberExtractionDisabled) return;
        RefreshHubFireEmberExtractable();
    }

    public void RefreshHubFireEmberExtractable() {

        if (UICurrencyManager.PlayerInventoryUI == null) return;
        if (PlayerCurrencies.Instance.GetCarryingEmber()) {
            SetStructureSecondaryFunctionUnlocked(false);
            ActivateStructureSecondaryFunctionInteraction(false);
            return;
        };

        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfCategory(PlayerCurrencies.CurrencyCategory.gem).Count == 0) {
            SetStructureSecondaryFunctionUnlocked(true);
            ActivateStructureSecondaryFunctionInteraction(true);
            SetCurrentStructureInteractionType(StructureInteractionType.secondaryFunction);
        }
        else {
            SetStructureSecondaryFunctionUnlocked(false);
            ActivateStructureSecondaryFunctionInteraction(false);
        };
    }

    private void PlayerCurrencies_OnEmberDropped(object sender, EventArgs e) {
        emberExtracted = false;
        EnableMainFireEmberExtraction(false);
    }

    private void Tent_OnStructureUpgraded(object sender, EventArgs e) {
        if (!isMainFire) return;
        SetFireCurrentMaxFuelTreshold();
    }

    private void Player_OnPlayerBackToTentToRespawn(object sender, EventArgs e) {
        respawningPlayer = true;
        respawningPlayerTimer = 5f;
    }

    private void HandleFuelFireCooldown() {
        if (!fuelFireOnCooldown) return;

        fuelFireNightTimer += Time.deltaTime;
        if (fuelFireNightTimer >= fuelFireNightCooldown) {
            fuelFireOnCooldown = false;
            fuelFireNightTimer = 0;
        }
    }

    private void HandleFuelDecrease() {
        if (isSecondaryFire) return;

        if (extractingEmber) {
            extractingEmberTimer -= Time.deltaTime;
            fuelLevel -= Time.deltaTime * extractingEmberFuelRateDepletion;
            extractingEmberFuelUsed += Time.deltaTime * extractingEmberFuelRateDepletion;
            if (extractingEmberTimer < 0) {
                extractingEmber = false;
                StartCoroutine(ExtractEmber());
            }

        }

        else if (respawningPlayer) {
            if (lockFireInteractionFunctionsUpdate) return;

            respawningPlayerTimer -= Time.deltaTime;
            fuelLevel -= Time.deltaTime * respawningPlayerFuelRateDepletion;
            if (respawningPlayerTimer < 0) {
                respawningPlayer = false;
            }

            if(fuelLevel < 0) {
                OnFireExtinguishedByPlayerRespawning?.Invoke(this, EventArgs.Empty);
                respawningPlayer = false;
            }
        }

        else {
            if (justFuelledFire) return;
            if (lockFireInteractionFunctionsUpdate) return;

            if (fuelLevel > 0) {
                fuelLevel -= Time.deltaTime * fuelDepletionRate;
            }
        }

    }

    private void CheckNeedsRefillFromEngineer() {
        if(fuelLevel < (wildFuelTreshold - .1)) {
            needsRefill = true;
        } else {
            needsRefill = false;
        }
    }

    private void ChangeFireRadius(float fireRadius) {
        fireRadiusCollider.radius = fireRadius;
    }

    public void FuelFire(float fuelAmount) {
        if (fuelLevel + fuelAmount >= currentMaxFuelTreshold) {
            fuelLevel = currentMaxFuelTreshold - .1f;
        }
        else {
            fuelLevel += fuelAmount;
        }

        CheckFireStateUpgrade();
    }

    private IEnumerator SetJustFuelledFireFalseAfterDelay() {
        yield return new WaitForSeconds(.5f);
        justFuelledFire = false;
    }

    private IEnumerator ExtractEmber() {

        Ember ember = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.ember), emberSpawnPosition.position, Quaternion.identity).GetComponent<Ember>();
        ember.ApplyRandomForceRandomDir(4,8, true,4, 8, false);
        ember.SetCollectibleUnInteractable(1f);
        ember.SetCanNeverBePickedUpByWorker();
        ActivateStructureSecondaryFunctionInteraction(false);
        if(isHubFire) {
            SetHubFireEmberExtractable(false);
        }
        emberExtracted = true;
        OnFireEmberExtracted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2f);
        StopExtractingEmber();
    }

    private void CheckFireFeedable() {
        if (!isMainFire) return;
        if (lockFireInteractionFunctionsUpdate) return;
        if (extractingEmber) return;

        if(fuelLevel + fuelTickValue <= currentMaxFuelTreshold) {
            SetStructurePrimaryFunctionUnlocked(true);
        } else {
            SetStructurePrimaryFunctionUnlocked(false);
        }

        if (state == State.extinguished) {
            SetStructurePrimaryFunctionUnlocked(false);
        }
    }

    private void CheckSecondaryFireFeedable() {
        if (!isSecondaryFire) return;
        //if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

        if (fuelLevel < wildFuelTreshold - 1) {
            SetStructurePrimaryFunctionUnlocked(true);
        }
        else {
            SetStructurePrimaryFunctionUnlocked(false);
        }
    }

    private void CheckFireStateDowngrade() {

        if (fuelLevel < 0 && state == State.calm) {
            ChangeState(State.extinguished);
            if(isSecondaryFire) {
                SetStructurePrimaryFunctionUnlocked(true);
            }
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
        CheckSecondaryFireFeedable();
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
        if (emberExtractionDisabled) return;
        if (emberExtracted)return;
        if (extractingEmber) return;
        if (lockFireInteractionFunctionsUpdate) return;
        if (isEndLevelFire) return;
        if (isSecondaryFire) return;

        //Debug.Log("fuelLevel " + fuelLevel);
        //Debug.Log("currentMaxFuelTreshold - fuelTickValue*3 " + (currentMaxFuelTreshold - fuelTickValue * 3));
        if(fuelLevel > (currentMaxFuelTreshold - fuelTickValue*3)) {
            EnableMainFireEmberExtraction(true);

        } else {
            EnableMainFireEmberExtraction(false);
        }
    }

    private void EnableMainFireEmberExtraction(bool enabled) {
        if (mainFireEmberExtractable == enabled) return;

        mainFireEmberExtractable = enabled;

        SetStructureSecondaryFunctionUnlocked(enabled);
        ActivateStructureSecondaryFunctionInteraction(enabled);
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
        if (!isMainFire) return;

        State maxState = State.wild;

        if (Tent.Instance.GetStructureLevel() == 2) {
            maxState = State.insane;
        }

        if (maxState == State.calm) {
            currentMaxFuelTreshold = wildFuelTreshold;
        }
        if (maxState == State.mild) {
            currentMaxFuelTreshold = wildFuelTreshold;
        }
        if (maxState == State.wild) {
            currentMaxFuelTreshold = maxFuelTreshold;
        }
        if (maxState == State.insane) {
            currentMaxFuelTreshold = maxFuelTreshold;
        }

    }

    public void ManualSetFireCurrentMaxFuelTreshold(State state) {
        if (state == State.calm) {
            currentMaxFuelTreshold = mildFuelTreshold;
        }
        if (state == State.mild) {
            currentMaxFuelTreshold = wildFuelTreshold;
        }
        if (state == State.wild) {
            currentMaxFuelTreshold = insaneFuelTreshold;
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
            extractingEmberFuelUsed = 0;

        } else {

            if (fuelFireOnCooldown) return;
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
            StopExtractingEmber(true);
        }
    }

    protected void StopExtractingEmber(bool refundFuel = false) {
        extractingEmber = false;
        OnFireEmberExtractionStopped?.Invoke(this, EventArgs.Empty);
        OnAnyFireEmberExtractionStopped?.Invoke(this, EventArgs.Empty);

        if(refundFuel) {
            FuelFire(extractingEmberFuelUsed);
        }
    }

    public void ActivateInitialFire(bool forceSetCarryingEmber = true) {
        initialFireLit = true;
        OnInitialFireActivated?.Invoke(this, EventArgs.Empty);

        if(forceSetCarryingEmber) {
            PlayerCurrencies.Instance.SetCarryingEmber(false);
        }
    }

    [Button]
    public void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {

        fuelLevel -= (damage * damageToFuelConversionRate);
        if (fuelLevel < 0) fuelLevel = -0.1f;
        OnFireDamageTaken?.Invoke(this, EventArgs.Empty);
        CheckFireStateDowngrade();
    }

    public void Die(Transform damageSource = null) {

    }

    private IEnumerator ReactivateStructureLocationAfterDelay() {
        yield return new WaitForSeconds(2f);
        secondaryFireStructureLocation.ReActivateFireStructureLocation();
        Destroy(gameObject);
    }
    protected override void OnTriggerExit2D(Collider2D collision) {
        if(collision.GetComponent<Player>() != null) {
            if (extractingEmber) {
                StopExtractingEmber(true);
            }
        }

        base.OnTriggerExit2D(collision);
    }

    #region GET PARAMETERS
    public State GetState() {
        return state;
    }

    public float GetLevel1FireRadius() {
        return calmFireRadius;
    }
    public float GetMildFireRadius() {
        return mildFireRadius;
    }
    public float GetLevel2FireRadius() {
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

    public bool GetFireFuelLevelCritical() {
        return GetCurrentFuelLevel() <= criticalFuelTreshold;
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
        return currentMaxFuelTreshold;
    }

    public float GetLerpDuration() {
        return lerpDuration;
    }

    public void SetLerpDuration(float duration) {
        lerpDuration = duration;
    }

    public float GetCurrentFuelLevel() {
        return fuelLevel;
    }

    public bool GetInitialFireLit() {
        return initialFireLit;
    }


    public bool GetFuelFireOnCooldown() {
        return fuelFireOnCooldown;
    }
    public bool GetIsMainFire() {
        return isMainFire;
    }

    public bool GetIsHubFire() {
        return isHubFire;
    }
    public bool GetIsSecondaryFire() {
        return isSecondaryFire;
    }

    public bool GetIsPrimordialFire() {
        return isPrimordialFire;
    }

    public PrimordialFireColor GetPrimordialFireColor() {
        return primordialFireColor;
    }
    public bool GetIsEndLevelAreaFire() {
        return isEndLevelFire;
    }
    public float GetFuelFireCooldownTimerNormalized() {
        return fuelFireNightTimer / fuelFireNightCooldown;
    }
    public bool GetEmberExtracted() {
        return emberExtracted;
    }
    public Transform GetProjectileTarget() {
        return transform;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }
    #endregion

    #region SET PARAMETERS

    public void SetFireInteractionsUpdateLocked(bool locked) {
        Debug.Log("SetFireInteractionsUpdateLocked " + locked);

        SetStructurePrimaryFunctionUnlocked(!locked);
        SetStructureSecondaryFunctionUnlocked(!locked);
        lockFireInteractionFunctionsUpdate = locked;
    }
    public void SetHubFireEmberExtractable(bool extractable) {
        SetStructureSecondaryFunctionUnlocked(extractable);
        ActivateStructureSecondaryFunctionInteraction(extractable);
        if (extractable) {
            SetCurrentStructureInteractionType(StructureInteractionType.secondaryFunction);
        }

        emberExtractionDisabled = !extractable;
    }
    public void DisableEmberExtraction() {
        emberExtractionDisabled = true;
    }

    public void SetSecondaryFireStructureLocation(StructureLocation_SecondaryFire location) {
        secondaryFireStructureLocation = location;
    }

    #endregion


}
