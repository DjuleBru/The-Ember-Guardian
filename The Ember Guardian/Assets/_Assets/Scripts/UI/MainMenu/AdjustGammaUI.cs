using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class AdjustGammaUI : MonoBehaviour
{
    public static AdjustGammaUI Instance;

    [SerializeField] private Slider slider;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject languageSettingGO;
    [SerializeField] private Button languageButton;
    private LiftGammaGain liftGammaGain;

    private bool panelOpen;

    private void Awake() {
        panel.SetActive(false);
        panelOpen = false;

        Instance = this;
    }

    private void Start() {
        slider.onValueChanged.AddListener((value) => {
            AdjustGamma();
        });

        if (!globalVolume.profile.TryGet(out liftGammaGain)) throw new System.NullReferenceException(nameof(liftGammaGain));

        slider.value = .5f;

        GameInput.Instance.OnPlayerBackPerformed += Gameinput_OnPlayerBackPerformed;

    }
    private void Gameinput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!panelOpen) return;
        ClosePanel();
    }

    private void AdjustGamma() {
        float gammaValue = Mathf.Lerp(-.5f, .5f, slider.value); // conversion 0–1 vers -.5–.5
        liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, gammaValue));

        SettingsManager.Instance.SetGammaLevel(gammaValue);
    }

    public void OpenPanel(bool startupOpenPanel) {
        panel.gameObject.SetActive(true);

        if (startupOpenPanel) {
            languageSettingGO.SetActive(true);
            EventSystem.current.SetSelectedGameObject(languageButton.gameObject);
        } else {
            languageSettingGO.SetActive(false);
            EventSystem.current.SetSelectedGameObject(slider.gameObject);
        }

        if (MainMenuUI.Instance != null) {

            MainMenuUI.Instance.HideAllMenuUI();
            MainMenuUI.Instance.HideMainMenuButtons();

        }

        panelOpen = true;

    }

    public void ClosePanel() {
        panel.gameObject.SetActive(false);

        if(MainMenuUI.Instance != null) {

            if (!CharacterSelectUI.Instance.HasChosenCharacter()) {
                CharacterSelectUI.Instance.OpenPanel();
            }
            else {
                MainMenuUI.Instance.ShowAllMenuUI();
                MainMenuUI.Instance.ShowMainMenuButtons();
                if (!MusicManager.Instance.GetPlayingMusic()) {
                    MusicManager.Instance.PlayMusic();
                }
            }
        }
        panelOpen = false;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= Gameinput_OnPlayerBackPerformed;
    }
}
