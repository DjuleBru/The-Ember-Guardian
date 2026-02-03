using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUI_Fire : MonoBehaviour
{
    [SerializeField] private GameObject fireUIGameObject;

    [SerializeField] private Animator fireAnimator;
    [SerializeField] private Animator criticalFuelAnimator;
    [SerializeField] private RectTransform progressBarContainer;
    [SerializeField] private RectTransform progressBarTemplate;
    [SerializeField] private RectTransform progressBarBackgroundContainer;
    [SerializeField] private RectTransform progressBarBackgroundTemplate;

    private CanvasGroup fireUICanvasGroup;

    private float displayDuration = 3f; // Durée pendant laquelle le progressBar est visible avant le fade out
    private float fadeOutDuration = 1f; // Durée de la transition de fade out
    private float displayTimer;
    private bool isDisplaying;
    private bool isFadingOut;
    private bool fuelLevelCritical;
    private float fadeOutTimer;
    private float minDistanceToFireToShowUI = 18f;
    private bool alwaysDisplay;

    private void Awake() {
        fireUICanvasGroup = fireUIGameObject.GetComponent<CanvasGroup>();
    }

    private void Start() {
        StructureUI_Fire.Instance.OnFireMaxBarAmountChanged += StructureUI_Fire_OnFireMaxBarAmountChanged;
        StructureUI_Fire.Instance.OnFireTickRemoved += StructureUI_Fire_OnFireTickRemoved1;
        StructureUI_Fire.Instance.OnFireTickAdded += StructureUI_OnFireTickAdded;
        Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;
        Fire.Instance.OnFireChangedState += Fire_OnFireChangedState;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;

        fireUIGameObject.SetActive(false);

        RefreshAlwaysDisplay();
        SettingsManager.Instance.OnUIDisplayChanged += SettingsManager_OnUIDisplayChanged;
    }


    private void SettingsManager_OnUIDisplayChanged(object sender, System.EventArgs e) {
        RefreshAlwaysDisplay();
    }

    private void RefreshAlwaysDisplay() {
        alwaysDisplay = SettingsManager.Instance.GetCurrentUIDisplayType() == SettingsManager.UIDisplayType.Persistent;

        if (alwaysDisplay) {
            fireUICanvasGroup.alpha = 1f;
            fireUIGameObject.SetActive(true);
            RefreshBackgroundProgressBar();
            int currentBars = StructureUI_Fire.Instance.GetCurrentBarAmount();
            RefreshProgressBarInstant(currentBars);
            RefreshStateVisual(Fire.Instance.GetState(), true);
        }
        else {
            fireUICanvasGroup.alpha = 0f;
        }
    }

    private void StructureUI_Fire_OnFireMaxBarAmountChanged(object sender, System.EventArgs e) {
        RefreshBackgroundProgressBar();
    }

    private void Update() {
        if (alwaysDisplay) return;

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
            if (fuelLevelCritical) return;

            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
            fireUICanvasGroup.alpha = alpha;

            // Vérifier si le fade-out est terminé
            if (fadeOutTimer >= fadeOutDuration) {
                isFadingOut = false;
                fireUIGameObject.SetActive(false); // Masquer l'objet après le fade-out
            }
        }
    }

    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {

        int currentBars = StructureUI_Fire.Instance.GetCurrentBarAmount();
        RefreshProgressBar(currentBars);

        if (!Fire.Instance.GetFireFuelLevelCritical()) {
            criticalFuelAnimator.SetBool("FuelCritical", false);
            fuelLevelCritical = false;
        }
    }

    private void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {
        RefreshStateVisual(e.newState);
        RefreshBackgroundProgressBar();
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        RefreshStateVisual(Fire.Instance.GetState());
        RefreshBackgroundProgressBar();
    }

    private void RefreshStateVisual(Fire.State e, bool disableUIIfExtinguished = false) {
        if (e == Fire.State.extinguished) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Extinguished");
            if(disableUIIfExtinguished) {
                fireAnimator.gameObject.SetActive(false);
                progressBarBackgroundContainer.gameObject.SetActive(false);
                progressBarContainer.gameObject.SetActive(false);
            }


            return;
        }

        fireAnimator.gameObject.SetActive(true);
        progressBarBackgroundContainer.gameObject.SetActive(true);
        progressBarContainer.gameObject.SetActive(true);

        if (e == Fire.State.wild) {
            fireAnimator.SetTrigger("Wild");
        }

        if (e == Fire.State.mild) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Mild");
        }

        if (e == Fire.State.insane) {
            fireAnimator.SetTrigger("Insane");
        }

        if (e == Fire.State.calm) {
            fireAnimator.SetTrigger("Calm");
        }

    }

    private void StructureUI_Fire_OnFireTickRemoved1(object sender, StructureUI_Fire.OnFireTickRemovedEventArgs e) {
        //Debug.Log(Fire.Instance.GetFireFuelLevelCritical());
        if (Fire.Instance.GetFireFuelLevelCritical()) {
            DisplayFireUI();
            RefreshProgressBar(e.currentBars);
            criticalFuelAnimator.SetBool("FuelCritical", true);
            fuelLevelCritical = true;
            return;
        }

        if (Mathf.Abs(Player.Instance.transform.position.x - Fire.Instance.transform.position.x) < minDistanceToFireToShowUI && !alwaysDisplay) return;

        DisplayFireUI();
        RefreshProgressBar(e.currentBars);


    }

    private void StructureUI_OnFireTickAdded(object sender, StructureUI_Fire.OnFireTickRemovedEventArgs e) {
        if (Fire.Instance.GetFireFuelLevelCritical()) {
            criticalFuelAnimator.SetBool("FuelCritical", true);
            fuelLevelCritical = true;
            DisplayFireUI();
            RefreshProgressBarInstant(e.currentBars);
            return;
        }

        if (Mathf.Abs(Player.Instance.transform.position.x - Fire.Instance.transform.position.x) < minDistanceToFireToShowUI && !alwaysDisplay) return;

        DisplayFireUI();
        RefreshProgressBarInstant(e.currentBars);

    }

    private void RefreshBackgroundProgressBar() {

        progressBarBackgroundTemplate.gameObject.SetActive(true);
        foreach (RectTransform child in progressBarBackgroundContainer) {
            if (child == progressBarBackgroundTemplate) continue;
            Destroy(child.gameObject);
        }
        

        int maxBars = StructureUI_Fire.Instance.GetMaxBarAmount();
        for(int i = 0; i < maxBars; i++) {
            Instantiate(progressBarBackgroundTemplate, progressBarBackgroundContainer);
        }

        progressBarBackgroundTemplate.gameObject.SetActive(false);
    }

    private void RefreshProgressBar(int currentBarAmount) {
        progressBarTemplate.gameObject.SetActive(true);

        foreach (RectTransform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        int maxBars = StructureUI_Fire.Instance.GetMaxBarAmount();

        for (int i = 0; i <= currentBarAmount-1; i++) {
           Instantiate(progressBarTemplate, progressBarContainer);
        }

        PlayerUI_TickTemplate[] tickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        PlayerUI_TickTemplate tick = tickArray[tickArray.Length - 1];

        if(tick != null) {
            tick.RemoveTick(1);
            tick.GetComponent<Rigidbody2D>().gravityScale = 2f;
            tick.transform.SetParent(fireUIGameObject.transform, true);
        }


        if(currentBarAmount == maxBars) {
            Instantiate(progressBarTemplate, progressBarContainer);
        }

        progressBarTemplate.gameObject.SetActive(false);
    }

    private void RefreshProgressBarInstant(int currentBarAmount) {
        progressBarTemplate.gameObject.SetActive(true);

        foreach (RectTransform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        int maxBars = StructureUI_Fire.Instance.GetMaxBarAmount();

        for (int i = 0; i <= currentBarAmount; i++) {
            Instantiate(progressBarTemplate, progressBarContainer);
        }

        progressBarTemplate.gameObject.SetActive(false);
    }

    private void DisplayFireUI() {
        isDisplaying = true;
        fireUIGameObject.SetActive(true);
        fireUICanvasGroup.alpha = 1.0f;
        displayTimer = 0;
    }

    private void OnDestroy() {
        Fire.Instance.OnFireFuelled -= Fire_OnFireFuelled;
        Fire.Instance.OnFireChangedState -= Fire_OnFireChangedState;
    }
}
