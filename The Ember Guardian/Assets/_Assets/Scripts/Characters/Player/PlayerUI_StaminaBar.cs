using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_StaminaBar : MonoBehaviour
{
    [SerializeField] private GameObject staminaBarGameObject;
    [SerializeField] private Image staminaBarFill_Right;
    [SerializeField] private Image staminaBarFill_Left;

    private RectTransform staminaBarRectTransform;
    private RectTransform staminaBarFillRightRect;
    private RectTransform staminaBarFillLeftRect;

    private CanvasGroup staminaBarCanvasGroup;
    private Coroutine fadeCoroutine;

    public float fadeOutDuration = .2f;  // Durée du fade-out
    public float fadeInDuration = .2f;  // Durée du fade-in

    private float initialMaxStamina;
    private float maxStamina;
    private Vector2 initialSize;

    private bool alwaysDisplay;

    private void Awake() {
        staminaBarCanvasGroup = staminaBarGameObject.GetComponent<CanvasGroup>();
        staminaBarCanvasGroup.alpha = 0f;

        staminaBarRectTransform = GetComponent<RectTransform>();
        staminaBarFillRightRect = staminaBarFill_Right.GetComponent<RectTransform>();
        staminaBarFillLeftRect = staminaBarFill_Left.GetComponent<RectTransform>();

        initialSize = staminaBarRectTransform.sizeDelta; // taille de base
    }

    private void Start() {
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStarted += PlayerMovement_OnPlayerAlmostExhaustionStarted;
        PlayerMovement.Instance.OnPlayerAlmostExhaustionStopped += PlayerMovement_OnPlayerAlmostExhaustionStopped;
        PlayerStats.Instance.OnPlayerMaxStaminaBuffed += PlayerStats_OnPlayerMaxStaminaBuffed;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;

        initialMaxStamina = PlayerStats.Instance.GetInitialMaxStamina();
        maxStamina = PlayerStats.Instance.GetMaxStamina();

        RefreshStaminaBarSize();
        RefreshAlwaysDisplay();
        SettingsManager.Instance.OnUIDisplayChanged += SettingsManager_OnUIDisplayChanged;
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, System.EventArgs e) {
        if (alwaysDisplay) {
            staminaBarCanvasGroup.alpha = 1f;
        }
        else {
            staminaBarCanvasGroup.alpha = 0f;
        }
    }

    private void SettingsManager_OnUIDisplayChanged(object sender, System.EventArgs e) {
        RefreshAlwaysDisplay();
    }

    private void RefreshAlwaysDisplay() {
        alwaysDisplay = SettingsManager.Instance.GetCurrentUIDisplayType() == SettingsManager.UIDisplayType.Persistent;
    }

    private void PlayerStats_OnPlayerMaxStaminaBuffed(object sender, System.EventArgs e) {
        maxStamina = PlayerStats.Instance.GetMaxStamina();
        RefreshStaminaBarSize();
    }

    private void RefreshStaminaBarSize() {
        float ratio = maxStamina / initialMaxStamina;
        //Debug.Log("RefreshStaminaBarSize " + ratio);

        // 1. Redimensionne la barre principale
        float newWidth = initialSize.x * ratio;
        staminaBarRectTransform.sizeDelta = new Vector2(newWidth, initialSize.y);

        // 2. Chaque moitié doit occuper exactement la moitié de la largeur

        staminaBarFillLeftRect.anchorMin = new Vector2(0f, 0f);
        staminaBarFillLeftRect.anchorMax = new Vector2(0.5f, 1f);
        staminaBarFillLeftRect.offsetMin = Vector2.zero;
        staminaBarFillLeftRect.offsetMax = Vector2.zero;

        staminaBarFillRightRect.anchorMin = new Vector2(0.5f, 0f);
        staminaBarFillRightRect.anchorMax = new Vector2(1f, 1f);
        staminaBarFillRightRect.offsetMin = Vector2.zero;
        staminaBarFillRightRect.offsetMax = Vector2.zero;
    }

    private void Update() {
        staminaBarFill_Right.fillAmount = 1 - PlayerMovement.Instance.GetStaminaTimerNormalized();
        staminaBarFill_Left.fillAmount = 1 - PlayerMovement.Instance.GetStaminaTimerNormalized();
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStarted(object sender, System.EventArgs e) {
        if (alwaysDisplay) return;
        StartFade(true);
    }

    private void PlayerMovement_OnPlayerAlmostExhaustionStopped(object sender, System.EventArgs e) {
        if (alwaysDisplay) return;
        StartFade(false);
    }

    private void StartFade(bool fadeIn) {
        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeRoutine(fadeIn));
    }

    private IEnumerator FadeRoutine(bool fadeIn) {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        float startAlpha = staminaBarCanvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float time = 0f;

        while (time < duration) {
            time += Time.deltaTime;
            staminaBarCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        staminaBarCanvasGroup.alpha = targetAlpha;
        fadeCoroutine = null;
    }

    private void OnDestroy() {
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
    }
}
