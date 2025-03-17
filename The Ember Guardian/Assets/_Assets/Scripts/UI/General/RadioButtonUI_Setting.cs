using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadioButtonUI_Setting : RadioButtonUI
{
    public enum SettingType {
        HoldToRun,
        AutoToggleLight,

        AimAssist,
        AutoAlignPlayerWithMoveDir,
        GamepadVibrations,
    }

    [SerializeField] private SettingType settingType;
    [SerializeField] private GameObject toggledImageGameObject;
    [SerializeField] private TextMeshProUGUI toggledText;

    private Button button;
     private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            ChangeLinkedSetting();
            RefreshVisual();
            InvokeOnAnyButtonPressed();
        });
    }

    protected override void Start() {
        base.Start();

        SettingsManager.Instance.OnHoldToggleRunChanged += SettingsManager_OnHoldToggleRunChanged;
        SettingsManager.Instance.OnAutoSwitchLightGunChanged += SettingsManager_OnAutoSwitchLightGunChanged;
        SettingsManager.Instance.OnControllerVibrationsChanged += SettingsManager_OnControllerVibrationsChanged;
        SettingsManager.Instance.OnAutoAlignAimWithMovementChanged += SettingsManager_OnAutoAlignAimWithMovementChanged;
        SettingsManager.Instance.OnAimAssistChanged += SettingsManagerOnAimAssistChanged;

        RefreshVisual();
    }

    private void ChangeLinkedSetting() {
        if (settingType == SettingType.HoldToRun) {
            SettingsManager.Instance.ChangeHoldToggleRun();
        }
        if (settingType == SettingType.AutoToggleLight) {
            SettingsManager.Instance.ChangeAutoSwitchGunLight();
        }
        if (settingType == SettingType.AimAssist) {
            SettingsManager.Instance.ChangeAimAssist();
        }
        if (settingType == SettingType.AutoAlignPlayerWithMoveDir) {
            SettingsManager.Instance.ChangeAutoAlignAimWithMovement();
        }
        if (settingType == SettingType.GamepadVibrations) {
            SettingsManager.Instance.ChangeControllerVibrations();
        }
    }

    private void RefreshVisual() {
        if (settingType == SettingType.HoldToRun) {
            if(SettingsManager.Instance.GetHoldToRun()) {
                toggledText.text = "Hold";
            } else {
                toggledText.text = "Toggle";
            }
        }

        if (settingType == SettingType.AutoToggleLight) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetAutoSwitchLight());
        }
        if (settingType == SettingType.AimAssist) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetAimAssist());
        }
        if (settingType == SettingType.AutoAlignPlayerWithMoveDir) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetAlignAimWithMovement());
        }
        if (settingType == SettingType.GamepadVibrations) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetControllerVibrations());
        }
    }

    private void SettingsManagerOnAimAssistChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnAutoAlignAimWithMovementChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnControllerVibrationsChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnAutoSwitchLightGunChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnHoldToggleRunChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }
}
