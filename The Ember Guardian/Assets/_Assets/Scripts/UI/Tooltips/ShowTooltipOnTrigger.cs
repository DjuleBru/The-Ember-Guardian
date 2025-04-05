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
    [SerializeField] private float tooltipShowDuration = 999f;
    [SerializeField] private int numberOfTimesToShowTooltip = 2;
    [SerializeField] private bool hideTooltipOnTriggerExit;

    private int amountShown;
    private bool tooltipShown;
    private bool tooltipBeingShown;
    private bool showTooltips = true;
    private bool playerInTriggerArea;

    private void Awake() {
        amountShown = ES3.Load(amountShownSaveKey, 0);
        if(amountShown >= numberOfTimesToShowTooltip) {
            tooltipShown = true;
        }
        Debug.Log(amountShownSaveKey + " " + amountShown);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (!showTooltips) return;
        playerInTriggerArea = true;
        Debug.Log("amountShown " + amountShown);
        if (tooltipShown) return;

        if(isControlTooltip) {
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
        Debug.Log(amountShownSaveKey + " " + amountShown);
        ES3.Save(amountShownSaveKey, amountShown);
    }

    public void SetShowTooltips(bool showTooltips) {
        this.showTooltips = showTooltips;
    }
}
