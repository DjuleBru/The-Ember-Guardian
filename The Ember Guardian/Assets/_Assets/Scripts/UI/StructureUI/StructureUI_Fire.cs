using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI_Fire : StructureUI
{
    public static StructureUI_Fire Instance;

    [SerializeField] private List<PayCurrencyTemplateWorldUI> rebuildFirePayOrbsUI;
    [SerializeField] private PayCurrencyTemplateWorldUI repairFirePayOrbsUI;
    [SerializeField] private PayCurrencyUI payCurrencyUI;
    [SerializeField] private GameObject fireProgressBarGameObject;
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarContainer;
    [SerializeField] private RectTransform progressBarTemplate;
    [SerializeField] private RectTransform progressBarBackgroundContainer;
    [SerializeField] private RectTransform progressBarBackgroundTemplate;
    [SerializeField] private Animator fireUIAnimator;

    [SerializeField] private Image fuelFireOrbBackground;
    [SerializeField] private Image fuelFireOrbOutline;

    private CanvasGroup progressBarCanvasGroup;
    [SerializeField] private float displayDuration = 2f; // Durée pendant laquelle le progressBar est visible avant le fade out
    [SerializeField] private float fadeOutDuration = .5f; // Durée de la transition de fade out
    private float displayTimer;
    private bool isDisplaying;
    private bool isFadingOut;
    private float fadeOutTimer;

    private float progressTemplateWidth = .45f;
    private float progressTemplateHeight = .1f;
    private int barAmountPerOrb = 3;

    private float currentFuelLowLimit;
    private float currentFuelHighLimit;
    private float currentFuelDelta;
    private float tickFuelValue = 10f/3f;
    private int currentBarAmount;
    private int targetBarAmount;
    private int maxBarAmount;
    private int barsLeftToRemove;

    private bool isRefuelling;

    private Fire fire;
    private Coroutine refillProgressBarCoroutine;

    public static event EventHandler<OnFireTickRemovedEventArgs> OnMainFireTickRemoved;
    public static event EventHandler OnMainCricitalFireTickRemoved;
    public static event EventHandler OnMainFireMaxBarAmountChanged;

    public event EventHandler<OnFireTickRemovedEventArgs> OnFireTickRemoved;
    public event EventHandler<OnFireTickRemovedEventArgs> OnFireTickAdded;
    public event EventHandler OnCricitalFireTickRemoved;
    public event EventHandler OnFireMaxBarAmountChanged;

    public class OnFireTickRemovedEventArgs : EventArgs {
        public int currentBars;
    }

    protected override void Awake() {
        base.Awake();
        fire = GetComponentInParent<Fire>();
        progressBarCanvasGroup = fireProgressBarGameObject.GetComponent<CanvasGroup>();

        if(fire.GetIsMainFire()) {
            Instance = this;
        }
    }

    protected override void Start() {
        base.Start();

        StructureStats.Instance.OnStructureStatsUpdated += StructureStats_OnStructureStatsUpdated;

        RefreshRepairOrRebuildSecondaryFireUI();
        fire.OnPlayerTriggeredOut += Fire_OnPlayerTriggeredOut;
        fire.OnFuelLevelLoaded += Fire_OnFuelLevelLoaded;
    }


    private void Fire_OnFuelLevelLoaded(object sender, EventArgs e) {

        if(refillProgressBarCoroutine != null) {
            StopCoroutine(refillProgressBarCoroutine);
        }
        // Force recalcul des bornes max/min et du nombre de barres
        RefreshBarState();

        // Calcul du nombre de barres en fonction du fuel actuel
        UpdateTargetBarAmount();

        foreach (Transform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < targetBarAmount; i++) {
            PlayerUI_TickTemplate tick = Instantiate(progressBarTemplate, progressBarContainer).GetComponent<PlayerUI_TickTemplate>();
            tick.gameObject.SetActive(true);
        }

        // Aligner les compteurs
        currentBarAmount = targetBarAmount;

        // On déclenche aussi les events pour notifier de l'état
        OnFireMaxBarAmountChanged?.Invoke(this, EventArgs.Empty);
        if (fire.GetIsMainFire()) {
            OnMainFireMaxBarAmountChanged?.Invoke(this, EventArgs.Empty);
        }

        if(Fire.Instance.GetFireFuelLevelCritical()) {
            if (fire.GetIsMainFire()) {
                OnMainCricitalFireTickRemoved?.Invoke(this, EventArgs.Empty);
            }
            OnCricitalFireTickRemoved?.Invoke(this, EventArgs.Empty);
            fireUIAnimator.SetBool("FuelCritical", true);
        }
        isRefuelling = false;
    }

    protected void Update() {
        HandleUIDisplay();
        HandleFuelFireCooldownVisuals();
        UpdateTargetBarAmount();

        if (currentBarAmount != targetBarAmount) {

            DisplayProgressBar();

            int barDifference = targetBarAmount - currentBarAmount;
            if (barDifference > 0) {
                isRefuelling = true;
                RefreshProgressBar(targetBarAmount);
            }

            else {
                if (isRefuelling) return;
                RefreshProgressBar(targetBarAmount);
            }

            currentBarAmount = targetBarAmount;
        }
    }

    private void StructureStats_OnStructureStatsUpdated(object sender, EventArgs e) {
        RefreshBarState();
        if(refillProgressBarCoroutine != null) {
            StopCoroutine(refillProgressBarCoroutine);
            refillProgressBarCoroutine = null;
        }
        refillProgressBarCoroutine = StartCoroutine(RefillProgressBar(targetBarAmount));
    }

    private void RefreshRepairOrRebuildSecondaryFireUI() {
        if (!fire.GetIsSecondaryFire()) return;

        List<PayCurrencyTemplateWorldUI> orbTemplateList = new List<PayCurrencyTemplateWorldUI>();
        orbTemplateList.Add(repairFirePayOrbsUI);

        if (fire.GetCurrentFuelLevel() < 0) {
            foreach (PayCurrencyTemplateWorldUI worldTemplate in rebuildFirePayOrbsUI) {
                worldTemplate.gameObject.SetActive(true);
                orbTemplateList.Add(worldTemplate);
            }
        } else {
            foreach (PayCurrencyTemplateWorldUI worldTemplate in rebuildFirePayOrbsUI) {
                worldTemplate.gameObject.SetActive(false);
            }
        }

        payCurrencyUI.SetOrbTemplateUIList(orbTemplateList);
    }

    private void DisplayProgressBar() {
        isDisplaying = true;
        fireProgressBarGameObject.SetActive(true);
        progressBarCanvasGroup.alpha = 1.0f;
        displayTimer = 0;
    }

    private void HandleUIDisplay() {
        if (structure.GetPlayerInTriggerArea()) return;

        if (isDisplaying) {
            displayTimer += Time.deltaTime;

            if (displayTimer >= displayDuration) {
                // Fin de l'affichage, commencer le fade-out

                isDisplaying = false;
                isFadingOut = true;
                fadeOutTimer = 0f; // Réinitialiser le timer pour le fade-out
            }
        }
        else if (isFadingOut) {

            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
            progressBarCanvasGroup.alpha = alpha;

            // Vérifier si le fade-out est terminé
            if (fadeOutTimer >= fadeOutDuration) {
                isFadingOut = false;
                fireProgressBarGameObject.SetActive(false); // Masquer l'objet après le fade-out
            }
        }
    }

    private void HandleFuelFireCooldownVisuals() {
        if (!fire.GetFuelFireOnCooldown()) return;

        float fuelFireCooldownNormalized = fire.GetFuelFireCooldownTimerNormalized();
        fuelFireOrbBackground.fillAmount = fuelFireCooldownNormalized;
        fuelFireOrbOutline.fillAmount = fuelFireCooldownNormalized;
    }

    private void UpdateTargetBarAmount() {
        float currentStateFuelLevel = fire.GetCurrentFuelLevel() - currentFuelLowLimit;

        // low level maxBarAmount = 6
        if(maxBarAmount > 6) {
            targetBarAmount = Mathf.Clamp(Mathf.FloorToInt(currentStateFuelLevel / tickFuelValue) + 1, 0, maxBarAmount);
        } else {
            targetBarAmount = Mathf.FloorToInt(currentStateFuelLevel / tickFuelValue) + 1;
        }

    }

    private void RefreshBarState() {

        foreach (RectTransform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        if (fire.GetState() == Fire.State.calm) {
            currentFuelLowLimit = fire.GetCalmFireTreshold();
            currentFuelHighLimit = fire.GetWildFireTreshold();
        }

        if (fire.GetState() == Fire.State.mild) {
            currentFuelLowLimit = fire.GetCalmFireTreshold();
            currentFuelHighLimit = fire.GetWildFireTreshold();
        }

        if (fire.GetState() == Fire.State.wild) {
            currentFuelLowLimit = fire.GetWildFireTreshold();
            currentFuelHighLimit = fire.GetMaxFireTreshold();
        }

        if (fire.GetState() == Fire.State.insane) {
            currentFuelLowLimit = fire.GetWildFireTreshold();
            currentFuelHighLimit = fire.GetMaxFireTreshold();
        }

        currentFuelDelta = currentFuelHighLimit - currentFuelLowLimit;

        maxBarAmount = Mathf.FloorToInt(currentFuelDelta / tickFuelValue);
        //maxBarAmount = Mathf.FloorToInt(currentFuelDelta / tickFuelValue) + 1;
        progressBar.sizeDelta = new Vector2(progressTemplateWidth, maxBarAmount * progressTemplateHeight);

        RefreshBackgroundProgressBar(maxBarAmount);
        RefreshFireSlotVisuals();

        //Debug.Log("currentFuelHighLimit " + currentFuelHighLimit);
        //Debug.Log("currentFuelLowLimit " + currentFuelLowLimit);
        //Debug.Log("currentFuelDelta " + currentFuelDelta);
        //Debug.Log("maxBarAmount " + maxBarAmount);

        OnFireMaxBarAmountChanged?.Invoke(this, EventArgs.Empty);

        if(fire.GetIsMainFire()) {
            OnMainFireMaxBarAmountChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RefreshProgressBar(int targetBarAmount) {
        progressBarTemplate.gameObject.SetActive(true);

        int barDifference = targetBarAmount - currentBarAmount;

        if (barDifference > 0) {
            refillProgressBarCoroutine = StartCoroutine(RefillProgressBar(barDifference));
        }
        else {
            StopCoroutine(RemoveProgressBars(barDifference));
            int barsToRemove = barDifference - barsLeftToRemove;
            StartCoroutine(RemoveProgressBars(-barsToRemove));
        }

        progressBarTemplate.gameObject.SetActive(false);
    }

    private IEnumerator RefillProgressBar(int barAmount) {

        for (int i = 0; i < barAmount; i++) {

            PlayerUI_TickTemplate[] fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>(true);
            if (fireTickArray.Length > maxBarAmount) {
                RefreshBarState();

                // Wait for the end of the next frame to avoid negative values on BarContainer up
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                currentBarAmount = targetBarAmount;
            }

            yield return new WaitForSeconds(.05f);

            PlayerUI_TickTemplate fireTick = Instantiate(progressBarTemplate, progressBarContainer).GetComponent<PlayerUI_TickTemplate>();

            fireTick.gameObject.SetActive(true);
            fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
            fireTickArray[0].AddTick();

            OnFireTickAdded?.Invoke(this, new OnFireTickRemovedEventArgs {
                currentBars = currentBarAmount
            });

            yield return new WaitForSeconds(.05f);

        }

        if(!Fire.Instance.GetFireFuelLevelCritical() && fire.GetIsMainFire()) {
            fireUIAnimator.SetBool("FuelCritical", false);
        }

        isRefuelling = false;
    }

    private void Fire_OnPlayerTriggeredOut(object sender, EventArgs e) {
        PlayerUI_TickTemplate[] fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        foreach(PlayerUI_TickTemplate tick in fireTickArray) {
            tick.StopInFeedbacks();
        }
    }

    private IEnumerator RemoveProgressBars(int barAmount) {
        barsLeftToRemove = barAmount;

        for (int i = 0; i < barAmount; i++) {
            PlayerUI_TickTemplate[] fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>(true);

            if (fireTickArray.Length < 2) yield break;

            fireTickArray[1].RemoveTick();
            fireTickArray[1].transform.SetParent(this.transform);
            barsLeftToRemove -= 1;

            if(Fire.Instance.GetFireFuelLevelCritical()) {
                if (fire.GetIsMainFire()) {
                    OnMainCricitalFireTickRemoved?.Invoke(this, EventArgs.Empty);
                }
                OnCricitalFireTickRemoved?.Invoke(this, EventArgs.Empty);
                fireUIAnimator.SetBool("FuelCritical", true);
            }

            fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>(true);

            if (fireTickArray.Length == 1) {
                RefreshBarState();

                // Wait for the end of the next frame to avoid negative values on BarContainer up
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                currentBarAmount = targetBarAmount;

                for (int j = 0; j == maxBarAmount; j++) {
                    Instantiate(progressBarTemplate, progressBarContainer);
                }

                 
                OnFireTickRemoved?.Invoke(this, new OnFireTickRemovedEventArgs {
                    currentBars = currentBarAmount
                });

                if(fire.GetIsMainFire()) {
                    OnMainFireTickRemoved?.Invoke(this, new OnFireTickRemovedEventArgs {
                        currentBars = currentBarAmount
                    });
                }

            } else {

                
                OnFireTickRemoved?.Invoke(this, new OnFireTickRemovedEventArgs {
                    currentBars = currentBarAmount
                });
                if (fire.GetIsMainFire()) {
                    OnMainFireTickRemoved?.Invoke(this, new OnFireTickRemovedEventArgs {
                        currentBars = currentBarAmount - 1
                    });
                }
            }


            yield return new WaitForSeconds(.05f);
        }
    }

    private void RefreshBackgroundProgressBar(int barAmount) {
        progressBarBackgroundTemplate.gameObject.SetActive(true);

        foreach (RectTransform child in progressBarBackgroundContainer) {
            if (child == progressBarBackgroundTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < barAmount; i++) {
            Instantiate(progressBarBackgroundTemplate, progressBarBackgroundContainer);
        }

        progressBarBackgroundTemplate.gameObject.SetActive(false);
    }

    private void RefreshFireSlotVisuals() {

        foreach(GameObject go in levelSlotVisualContainerList) {
            go.SetActive(false);
        }

        if(fire.GetState() == Fire.State.calm) {
            levelSlotVisualContainerList[0].SetActive(true);
        }
        if (fire.GetState() == Fire.State.mild) {
            levelSlotVisualContainerList[1].SetActive(true);
        }
        if (fire.GetState() == Fire.State.wild) {
            levelSlotVisualContainerList[2].SetActive(true);
        }
        if (fire.GetState() == Fire.State.insane) {
            levelSlotVisualContainerList[3].SetActive(true);
        }
    }

    protected override void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredOut(sender, e);
        fireProgressBarGameObject.SetActive(false);
    }

    protected override void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredIn(sender, e);
        DisplayProgressBar();
        RefreshRepairOrRebuildSecondaryFireUI();
    }

    public int GetMaxBarAmount() {
        return maxBarAmount;
    }

    public int GetCurrentBarAmount() {
        return currentBarAmount;
    }
}
