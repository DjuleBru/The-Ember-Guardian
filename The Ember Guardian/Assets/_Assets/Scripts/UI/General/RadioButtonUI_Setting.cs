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
        ScreenMode,
        Language,

        AimAssist,
        AutoAlignPlayerWithMoveDir,
        GamepadVibrations,
        AutoReload,
        AdjustGamma,
        StreamerMode,
        ShowDamageNumbers,
        WaterReflections,
        UIDisplay,
        Difficulty,
        Photosensitivity,
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
        SettingsManager.Instance.OnFullScreenChanged += SettingsManager_OnFullScreenChanged;
        SettingsManager.Instance.OnLanguageChanged += SettingsManager_OnLanguageChanged;
        SettingsManager.Instance.OnAutoReloadChanged += SettingsManager_OnAutoReloadChanged;
        SettingsManager.Instance.OnSteamerModeChanged += SettingsManager_OnSteamerModeChanged;
        SettingsManager.Instance.OnShowDamageNumbersChanged += SettingsManager_OnShowDamageNumbersChanged;
        SettingsManager.Instance.OnWaterReflectionsChanged += SettingsManager_OnWaterSimulationChanged;
        SettingsManager.Instance.OnUIDisplayChanged += SettingsManager_OnUIDisplayChanged;
        SettingsManager.Instance.OnDifficultyChanged += SettingsManager_OnDifficultyChanged;
        SettingsManager.Instance.OnPhotosensitivityChanged += SettingsManager_OnPhotosensitivityChanged;

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
            SettingsManager.Instance.ChangeAutoAimMode();
        }
        if (settingType == SettingType.AutoAlignPlayerWithMoveDir) {
            SettingsManager.Instance.ChangeAutoAlignAimWithMovement();
        }
        if (settingType == SettingType.GamepadVibrations) {
            SettingsManager.Instance.ChangeControllerVibrations();
        }
        if (settingType == SettingType.ScreenMode) {
            SettingsManager.Instance.ChangeScreenMode();
        }
        if (settingType == SettingType.Language) {
            SettingsManager.Instance.ChangeLanguage();
        }
        if (settingType == SettingType.AutoReload) {
            SettingsManager.Instance.ChangeAutoReload();
        }
        if (settingType == SettingType.StreamerMode) {
            SettingsManager.Instance.ChangeStreamerMode();
        }
        if (settingType == SettingType.ShowDamageNumbers) {
            SettingsManager.Instance.ChangeShowDamageNumbers();
        }
        if (settingType == SettingType.AdjustGamma) {
            AdjustGammaUI.Instance.OpenPanel(false);
            SettingsMenuUI.Instance.HideSettingsPanel();
        }
        if (settingType == SettingType.WaterReflections) {
            SettingsManager.Instance.ChangeWaterReflections();
        }

        if (settingType == SettingType.UIDisplay) {
            SettingsManager.Instance.ChangeUIDisplayType();
        }

        if (settingType == SettingType.Difficulty) {
            SettingsManager.Instance.ChangeDifficulty();
        }
        if (settingType == SettingType.Photosensitivity) {
            SettingsManager.Instance.ChangePhotosensitivityMode();
        }
    }

    private void SettingsManager_OnPhotosensitivityChanged(object sender, System.EventArgs e) {
        RefreshVisual();

    }
    private void SettingsManager_OnWaterSimulationChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnDifficultyChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnUIDisplayChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }
    private void SettingsManager_OnLanguageChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }
    private void SettingsManager_OnAutoReloadChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnSteamerModeChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnShowDamageNumbersChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void RefreshVisual() {
        if(toggledText != null) {
            TMP_FontAsset font = LocalizationManager.Instance.GetCurrentFont();
            toggledText.font = font;
        }


        if (settingType == SettingType.HoldToRun) {
            if(SettingsManager.Instance.GetHoldToRun()) {
                toggledText.text = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            } else {
                toggledText.text = LocalizationManager.Instance.GetLocalizedText("menu_toggle");
            }
        }

        if (settingType == SettingType.UIDisplay) {
            string displayTypeText = "menu_" + SettingsManager.Instance.GetCurrentUIDisplayType().ToString();
            toggledText.text = LocalizationManager.Instance.GetLocalizedText(displayTypeText);
        }

        if (settingType == SettingType.Difficulty) {

            SettingsManager.Difficulty currentDifficulty = SettingsManager.Instance.GetDifficulty();

            if (LevelManager.Instance != null && LevelManager.Instance.IsHordeMode()) {
                currentDifficulty = SettingsManager.Instance.GetHordeDifficulty();
            }

            string displayTypeText = "difficulty_" + currentDifficulty;

            if(currentDifficulty == SettingsManager.Difficulty.Easy) {
                displayTypeText = "difficulty_easy";
            }
            if (currentDifficulty == SettingsManager.Difficulty.Medium) {
                displayTypeText = "difficulty_medium";
            }
            if (currentDifficulty == SettingsManager.Difficulty.Hard) {
                displayTypeText = "difficulty_hard";
            }

            toggledText.text = LocalizationManager.Instance.GetLocalizedText(displayTypeText);
        }

        if (settingType == SettingType.ScreenMode) {
            SettingsManager.ScreenMode screenMode = SettingsManager.Instance.GetCurrentScreenMode();
            string localizationKey = "menu_" + screenMode;
            toggledText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
        }

        if (settingType == SettingType.AutoToggleLight) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetAutoSwitchLight());
        }

        if (settingType == SettingType.AimAssist) {
            SettingsManager.AutoAimMode autoAimMode = SettingsManager.Instance.GetAutoAimMode();
            string localizationKey = autoAimMode.ToString();
            toggledText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
        }

        if (settingType == SettingType.AutoAlignPlayerWithMoveDir) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetAlignAimWithMovement());
        }
        if (settingType == SettingType.AutoReload) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetAutoReload());
        }
        if (settingType == SettingType.GamepadVibrations) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetControllerVibrations());
        }
        if (settingType == SettingType.Language) {
            toggledText.text = LocalizationManager.Instance.GetLocalizedText(SettingsManager.Instance.GetLanguage().ToString());
        }
        if (settingType == SettingType.AdjustGamma) {
            toggledText.text = LocalizationManager.Instance.GetLocalizedText("menu_adjust");
        }
        if (settingType == SettingType.StreamerMode) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetStreamerMode());
        }
        if (settingType == SettingType.ShowDamageNumbers) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetShowDamageNumbers());
        }
        if (settingType == SettingType.WaterReflections) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetWaterReflectionsActive());
        }
        if (settingType == SettingType.Photosensitivity) {
            toggledImageGameObject.SetActive(SettingsManager.Instance.GetPhotosensitivityMode());
        }
    }

    private void SettingsManagerOnAimAssistChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void SettingsManager_OnFullScreenChanged(object sender, System.EventArgs e) {
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
