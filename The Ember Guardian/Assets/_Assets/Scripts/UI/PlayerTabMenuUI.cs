using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerTabMenuUI : MonoBehaviour
{
    public static PlayerTabMenuUI Instance;

    [SerializeField] private GameObject tabMenuPanel;
    [SerializeField] private GameObject firstButtonSelected;

    private bool canCloseTab = true;
    private bool tabMenuOpen;

    private CanvasGroup canvasGroup;
    private Animator panelAnimator;

    public event EventHandler OnPlayerTabOpened;

    private void Awake() {
        Instance = this;

        panelAnimator = GetComponent<Animator>();

    }

    private void Start() {
        GameInput.Instance.OnPlayerOpenPlayerTabPerformed += GameInput_OnPlayerOpenPlayerTabPerformed;
        HubMerchantUI.OnAnyHubMerchantOpenUIPanel += HubMerchantUI_OnAnyHubMerchantOpenUIPanel;
        HubMerchantUI.OnAnyHubMerchantCloseUIPanel += HubMerchantUI_OnAnyHubMerchantCloseUIPanel;
    }

    private void HubMerchantUI_OnAnyHubMerchantCloseUIPanel(object sender, System.EventArgs e) {
        HubMerchantUI hubMerchantUI = (HubMerchantUI)sender;
        if (hubMerchantUI.GetHubMerchant().GetHubMerchantType() == HubMerchant.HubMerchantType.GemMerchant) return;

        canCloseTab = true; 
        FadeOutTab();
    }

    private void HubMerchantUI_OnAnyHubMerchantOpenUIPanel(object sender, System.EventArgs e) {
        HubMerchantUI hubMerchantUI = (HubMerchantUI)sender;
        if (hubMerchantUI.GetHubMerchant().GetHubMerchantType() == HubMerchant.HubMerchantType.GemMerchant) return;

        canCloseTab = false;
        FadeInTab();
    }

    private void GameInput_OnPlayerOpenPlayerTabPerformed(object sender, System.EventArgs e) {
        OpenCloseTab();

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            EventSystem.current.SetSelectedGameObject(firstButtonSelected);
        }
    }

    private void FadeInTab() {
        tabMenuOpen = true;

        panelAnimator.ResetTrigger("Hide");
        panelAnimator.SetTrigger("Show");
    }

    private void FadeOutTab() {
        tabMenuOpen = false;

        panelAnimator.ResetTrigger("Show");
        panelAnimator.SetTrigger("Hide");
    }

    private void OpenCloseTab() {
        if (!canCloseTab) return;
        tabMenuOpen = !tabMenuOpen;

        if(tabMenuOpen) {
            FadeInTab();
            Player.Instance.DisableControlInputs();
            OnPlayerTabOpened?.Invoke(this, EventArgs.Empty);
        } else {
            FadeOutTab();
            Player.Instance.EnableControlInputs();
        }
    }

    public void SetCanCloseTab(bool canClose) {
        this.canCloseTab = canClose;
    }

    private void OnDestroy() {
        HubMerchantUI.OnAnyHubMerchantOpenUIPanel -= HubMerchantUI_OnAnyHubMerchantOpenUIPanel;
    }
}
