using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI_Fire : StructureUI
{
    public static StructureUI_Fire Instance;

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
    private float tickFuelValue;
    private int currentBarAmount;
    private int targetBarAmount;
    private int maxBarAmount;
    private int barsLeftToRemove;

    private bool isRefuelling;

    private Fire fire;

    public static event EventHandler<OnFireTickRemovedEventArgs> OnFireTickRemoved;
    public static event EventHandler OnCricitalFireTickRemoved;
    public static event EventHandler OnFireMaxBarAmountChanged;

    public class OnFireTickRemovedEventArgs : EventArgs {
        public int currentBars;
    }

    protected override void Awake() {
        base.Awake();
        Instance = this;
        fire = GetComponentInParent<Fire>();
        progressBarCanvasGroup = fireProgressBarGameObject.GetComponent<CanvasGroup>();
    }

    protected override void Start() {
        base.Start();
        tickFuelValue = fire.GetOrbFuelValue() / barAmountPerOrb;
        fire.OnPlayerTriggeredOut += Fire_OnPlayerTriggeredOut;
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

    private void DisplayProgressBar() {
        isDisplaying = true;
        fireProgressBarGameObject.SetActive(true);
        progressBarCanvasGroup.alpha = 1.0f;
        displayTimer = 0;
    }

    private void HandleUIDisplay() {
        if (playerInTriggerArea) return;

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
        targetBarAmount = Mathf.FloorToInt(currentStateFuelLevel / tickFuelValue) +1;
    }

    private void RefreshBarState() {

        foreach (RectTransform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        if (fire.GetState() == Fire.State.calm) {
            currentFuelLowLimit = fire.GetCalmFireTreshold();
            currentFuelHighLimit = fire.GetMildFireTreshold();
        }

        if (fire.GetState() == Fire.State.mild) {
            currentFuelLowLimit = fire.GetMildFireTreshold();
            currentFuelHighLimit = fire.GetWildFireTreshold();
        }

        if (fire.GetState() == Fire.State.wild) {
            currentFuelLowLimit = fire.GetWildFireTreshold();
            currentFuelHighLimit = fire.GetInsaneFireTreshold();
        }

        if (fire.GetState() == Fire.State.insane) {
            currentFuelLowLimit = fire.GetInsaneFireTreshold();
            currentFuelHighLimit = fire.GetMaxFireTreshold();
        }

        currentFuelDelta = currentFuelHighLimit - currentFuelLowLimit;

        maxBarAmount = Mathf.FloorToInt(currentFuelDelta / fire.GetOrbFuelValue()) * barAmountPerOrb;
        progressBar.sizeDelta = new Vector2(progressTemplateWidth, maxBarAmount * progressTemplateHeight);
        RefreshBackgroundProgressBar(maxBarAmount);
        RefreshFireSlotVisuals();
        OnFireMaxBarAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshProgressBar(int targetBarAmount) {
        progressBarTemplate.gameObject.SetActive(true);

        int barDifference = targetBarAmount - currentBarAmount;

        if (barDifference > 0) {
            StartCoroutine(RefillProgressBar(barDifference));
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

            yield return new WaitForSeconds(.05f);

        }

        if(!Fire.Instance.GetFireFuelLevelCritical()) {
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

            } else {

                OnFireTickRemoved?.Invoke(this, new OnFireTickRemovedEventArgs {
                    currentBars = currentBarAmount - 1
                });
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
    }

    public int GetMaxBarAmount() {
        return maxBarAmount;
    }

    public int GetCurrentBarAmount() {
        return currentBarAmount;
    }
}
