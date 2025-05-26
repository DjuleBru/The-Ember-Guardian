using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralEditionButtons : ButtonUI
{
   public enum ButtonType {
        RevertChanges,
        RemoveAllStructures,
        ResetToDefault,
        SaveLayout,
    }

    [SerializeField] private ButtonType buttonType;
    [SerializeField] private TextMeshProUGUI buttonFunctionText;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Color disabledButtonIconColor;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            TriggerButtonFunction();
        });
    }

    protected override void Start() {
        base.Start();
        buttonFunctionText.text = LocalizationManager.Instance.GetLocalizedText(buttonType.ToString());

        CampEditManager.Instance.OnAnyChangeMade += CampEditManager_OnAnyChangeMade;
        CampEditManager.Instance.OnAllStructuresRemoved += CampEditManager_OnAllStructuresRemoved;
        CampEditManager.Instance.OnLayoutResetToDefault += CampEditManager_OnLayoutResetToDefault;
        CampEditManager.Instance.OnLayoutSaved += CampEditManager_OnLayoutSaved;

        if(buttonType == ButtonType.SaveLayout || buttonType == ButtonType.RevertChanges) {
            //SetButtonEnabled(false);
        }

        if(!CampEditManager.Instance.GetCampLayoutCustomized()) {
            if (buttonType == ButtonType.ResetToDefault) {
                //SetButtonEnabled(false);
            }
        }

    }
    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            buttonSelected = true;
        }

        if (this != buttonUI && buttonSelected) {
            buttonSelected = false;
        }
    }
    private void CampEditManager_OnLayoutSaved(object sender, System.EventArgs e) {
        if (buttonType == ButtonType.SaveLayout || buttonType == ButtonType.RevertChanges) {
            //SetButtonEnabled(false);
        }
    }

    private void CampEditManager_OnLayoutResetToDefault(object sender, System.EventArgs e) {
        if (buttonType == ButtonType.ResetToDefault) {
            //SetButtonEnabled(false);
        }
    }

    private void CampEditManager_OnAllStructuresRemoved(object sender, System.EventArgs e) {
        if (buttonType == ButtonType.RemoveAllStructures) {
            //SetButtonEnabled(false);
        }
    }

    private void CampEditManager_OnAnyChangeMade(object sender, System.EventArgs e) {
        if (buttonType == ButtonType.SaveLayout || buttonType == ButtonType.RevertChanges || buttonType == ButtonType.ResetToDefault || buttonType == ButtonType.RemoveAllStructures) {
            //SetButtonEnabled(true);
        }
    }

    private void SetButtonEnabled(bool enabled) {
        button.enabled = enabled;

        if(enabled) {
            buttonIcon.color = Color.white;
        }
        else {
            buttonIcon.color = disabledButtonIconColor;
        }
    }

    private void TriggerButtonFunction() {
        if(buttonType == ButtonType.RevertChanges) {
            CampEditManager.Instance.LoadCampLayout();
            CampEditManager.Instance.InitializeCampLayout();
        }

        if(buttonType == ButtonType.RemoveAllStructures) {
            CampEditManager.Instance.RemoveAllStructure();
        }

        if(buttonType == ButtonType.ResetToDefault) {
            CampEditManager.Instance.ResetToDefault();
        }

        if (buttonType == ButtonType.SaveLayout) {
            CampEditManager.Instance.SaveCampLayout();
        }
    }
}
