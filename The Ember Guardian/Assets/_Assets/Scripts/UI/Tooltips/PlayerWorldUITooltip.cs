using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWorldUITooltip : MonoBehaviour
{

    [SerializeField] private GameObject tooltipVisualGameObject;
    [SerializeField] private Animator tooltipAnimator;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [SerializeField] private GameObject constrolInstructionGameObject;
    [SerializeField] private TextMeshProUGUI constrolInstructionText1;
    [SerializeField] private TextMeshProUGUI constrolInstructionText2;
    [SerializeField] private Image constrolInstructionIconImage;
    [SerializeField] private Image constrolInstructionIcon2Image;

    public static event EventHandler OnTooltipShown;
    public static event EventHandler OnTooltipHidden;

    private InputControlIcons.Control currentControl;

    private float tooltipDisplayTimer;
    private bool isActive;
    private bool hideTooltip;

    private void Awake() {
        tooltipVisualGameObject.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        RefreshInputIcon();
    }

    private void Update() {
        if (isActive && hideTooltip) {
            tooltipDisplayTimer -= Time.deltaTime;
            if(tooltipDisplayTimer <= 0) {
                StartCoroutine(HideTooltipCoroutine());
                isActive = false;
            }
        }
    }

    public void ShowTooltip(string textToShow, float displayTime) {
        if (isActive) return;

        constrolInstructionGameObject.SetActive(false);
        tooltipText.gameObject.SetActive(true);

        tooltipVisualGameObject.SetActive(true);
        tooltipText.text = textToShow;
        tooltipDisplayTimer = displayTime;
        isActive = true;
        hideTooltip= true;
        OnTooltipShown?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshInputIcon() {
        List<Sprite> spriteList = InputControlIcons.Instance.GetControlIconSprite(currentControl);

        Sprite iconSprite = spriteList[0];
        Sprite iconSprite2 = null;

        if (spriteList.Count == 2) {
            iconSprite2 = spriteList[1];
        }

        constrolInstructionIconImage.sprite = iconSprite;

        if (iconSprite2 != null) {
            constrolInstructionIcon2Image.gameObject.SetActive(true);
            constrolInstructionIcon2Image.sprite = iconSprite2;
        }
        else {
            constrolInstructionIcon2Image.gameObject.SetActive(false);
        }

    }

    public void ShowTooltipInstruction(string text1ToShow, string text2ToShow, InputControlIcons.Control controlType, float displayTime = 0) {
        currentControl = controlType;
        List<Sprite> spriteList = InputControlIcons.Instance.GetControlIconSprite(controlType);

        Sprite iconSprite = spriteList[0];
        Sprite iconSprite2 = null;

        if (spriteList.Count == 2) {
            iconSprite2 = spriteList[1];
        }

        StartCoroutine(ShowTooltipInstructionCoroutine(text1ToShow, text2ToShow, iconSprite, iconSprite2, displayTime));
    }

    private IEnumerator ShowTooltipInstructionCoroutine(string text1ToShow, string text2ToShow, Sprite iconSprite, Sprite iconSprite2 = null, float displayTime = 0) {
        if (isActive) {
            HideTooltip();
            yield return new WaitForSeconds(.5f);
        };

        if (displayTime > 0) {
            hideTooltip = true;
        }

        constrolInstructionGameObject.SetActive(true);
        tooltipText.gameObject.SetActive(false);

        tooltipVisualGameObject.SetActive(true);
        constrolInstructionText1.text = text1ToShow;
        constrolInstructionText2.text = text2ToShow;
        constrolInstructionIconImage.sprite = iconSprite;

        if (iconSprite2 != null) {
            constrolInstructionIcon2Image.gameObject.SetActive(true);
            constrolInstructionIcon2Image.sprite = iconSprite2;
        }
        else {
            constrolInstructionIcon2Image.gameObject.SetActive(false);
        }

        tooltipDisplayTimer = displayTime;
        isActive = true;
        OnTooltipShown?.Invoke(this, EventArgs.Empty);

    }

    private IEnumerator HideTooltipCoroutine(float delay = 0f) {
        yield return new WaitForSeconds(delay);

        OnTooltipHidden?.Invoke(this, EventArgs.Empty);
        tooltipAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(.1f);

        isActive = false;
        tooltipVisualGameObject.SetActive(false);
    }

    public void HideTooltip(float delay = 0f) {
        StartCoroutine(HideTooltipCoroutine(delay));
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
