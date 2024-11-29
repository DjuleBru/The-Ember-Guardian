using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerWorldUITooltip : MonoBehaviour
{
    public static PlayerWorldUITooltip Instance;

    [SerializeField] private GameObject tooltipVisualGameObject;
    [SerializeField] private Animator tooltipAnimator;
    [SerializeField] private TextMeshProUGUI tooltipText;

    public event EventHandler OnTooltipShown;
    public event EventHandler OnTooltipHidden;

    private float tooltipDisplayTimer;
    private bool isActive;

    private void Awake() {
        Instance = this;
        tooltipVisualGameObject.SetActive(false);
    }

    private void Update() {
        if (isActive) {
            tooltipDisplayTimer -= Time.deltaTime;
            if(tooltipDisplayTimer <= 0) {
                StartCoroutine(HideTooltip());
                isActive = false;
            }
        }
    }


    public void ShowTooltip(string textToShow, float displayTime) {
        if (isActive) return;

        tooltipVisualGameObject.SetActive(true);
        tooltipText.text = textToShow;
        tooltipDisplayTimer = displayTime;
        isActive = true;
        OnTooltipShown?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator HideTooltip() {
        OnTooltipHidden?.Invoke(this, EventArgs.Empty);
        tooltipAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(.1f);

        tooltipVisualGameObject.SetActive(false);

    }
}
