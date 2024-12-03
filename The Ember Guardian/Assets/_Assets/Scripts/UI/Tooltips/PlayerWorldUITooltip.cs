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

    private float tooltipDisplayTimer;
    private bool isActive;
    private bool hideTooltip;

    private void Awake() {
        tooltipVisualGameObject.SetActive(false);
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

    public void ShowTooltipInstruction(string text1ToShow, string text2ToShow, Sprite iconSprite, Sprite iconSprite2 = null, float displayTime = 0) {
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

        Debug.Log("ShowTooltipInstruction");

    }

    private IEnumerator HideTooltipCoroutine() {
        Debug.Log("HideTooltipCoroutine");
        OnTooltipHidden?.Invoke(this, EventArgs.Empty);
        tooltipAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(.1f);

        isActive = false;
        tooltipVisualGameObject.SetActive(false);
    }

    public void HideTooltip() {
        Debug.Log("HideTooltip");
        StartCoroutine(HideTooltipCoroutine());
    }
}
