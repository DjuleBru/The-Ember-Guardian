using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTooltipOnTrigger : MonoBehaviour
{
    [SerializeField] private bool showLeftTooltip;
    [SerializeField] private bool showRightTooltip;
    [SerializeField] private string text1ToShowLocalizationKey = "menu_Press";
    [SerializeField] private string text2ToShowLocalizationKey = "tooltip_ammoTip";
    [SerializeField] private string amountShownSaveKey;
    [SerializeField] private bool isControlTooltip;
    [SerializeField] private InputControlIcons.Control control;
    [SerializeField] private float tooltipShowDuration = 5f;
    [SerializeField] private int numberOfTimesToShowTooltip = 2;
    [SerializeField] private bool hideTooltipOnTriggerExit;
    [SerializeField] private bool hideTooltipAtNight;

    private int amountShown;
    private bool tooltipShown;
    private bool tooltipBeingShown;
    private bool showTooltips = true;
    private bool playerInTriggerArea;

    public static event EventHandler OnAnyTooltipAmountShownIncreased;

    private void Awake() {
        amountShown = ES3.Load(amountShownSaveKey, 0);

        RefreshTooltipShown();

        OnAnyTooltipAmountShownIncreased += ShowTooltipOnTrigger_OnAnyTooltipAmountShownIncreased;
    }

    private void ShowTooltipOnTrigger_OnAnyTooltipAmountShownIncreased(object sender, EventArgs e) {
        ShowTooltipOnTrigger showTooltipOnTrigger = sender as ShowTooltipOnTrigger;

        if(showTooltipOnTrigger != null && showTooltipOnTrigger != this) {
            if(amountShownSaveKey == showTooltipOnTrigger.GetAmountShownSaveKey()) {
                amountShown++;
                RefreshTooltipShown();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!showTooltips) return;
        playerInTriggerArea = true;
        if (tooltipShown) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;
        if (hideTooltipAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

        if (isControlTooltip) {
            tooltipBeingShown = true;
            string text1 = LocalizationManager.Instance.GetLocalizedText(text1ToShowLocalizationKey);
            string text2 = LocalizationManager.Instance.GetLocalizedText(text2ToShowLocalizationKey);
            if (showLeftTooltip) {
                PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(text1, text2, control, tooltipShowDuration);
            } else {
                PlayerTooltipManager.Instance.GetTooltipRight().ShowTooltipInstruction(text1, text2, control, tooltipShowDuration);
            }
        }

    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!showTooltips) return;
        playerInTriggerArea = false;
        if (!tooltipBeingShown) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;
        if (hideTooltipAtNight && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

        if (isControlTooltip) {
            tooltipBeingShown = false;
            if (showLeftTooltip) {
                PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            }
            else {
                PlayerTooltipManager.Instance.GetTooltipRight().HideTooltip();
            }
        }
    }

    public void HideTooltipShown() {
        if (!tooltipBeingShown) return;
        if (!showTooltips) return;
        if (tooltipShown) return;

        if (isControlTooltip) {
            tooltipBeingShown = false;
            if (showLeftTooltip) {
                PlayerTooltipManager.Instance.GetTooltipLeft().HideTooltip();
            }
            else {
                PlayerTooltipManager.Instance.GetTooltipRight().HideTooltip();
            }
        }

        amountShown++;
        OnAnyTooltipAmountShownIncreased?.Invoke(this, EventArgs.Empty);
        ES3.Save(amountShownSaveKey, amountShown);
        RefreshTooltipShown();
    }

    private void RefreshTooltipShown() {
        if (amountShown >= numberOfTimesToShowTooltip) {
            tooltipShown = true;
        }
    }

    public void SetShowTooltips(bool showTooltips) {
        this.showTooltips = showTooltips;
    }

    public string GetAmountShownSaveKey() {
        return amountShownSaveKey;
    }

    private void OnDestroy() {
        OnAnyTooltipAmountShownIncreased -= ShowTooltipOnTrigger_OnAnyTooltipAmountShownIncreased;
    }
}
