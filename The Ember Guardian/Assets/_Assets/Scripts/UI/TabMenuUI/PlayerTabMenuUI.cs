using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerTabMenuUI : MonoBehaviour
{
    public static PlayerTabMenuUI Instance;

    [SerializeField] private GameObject tabMenuPanel;
    [SerializeField] private GameObject firstButtonSelected;
    [SerializeField] private GameObject firstSkillButtonSelected;

    private bool canCloseTab = true;
    private bool tabMenuOpen;
    private bool changeWeaponPanelOpen;
    private bool skillDescriptionPanelOpen;

    private CanvasGroup canvasGroup;
    private Animator panelAnimator;

    public event EventHandler OnPlayerTabOpened;
    public event EventHandler OnPlayerTabClosed;

    private void Awake() {
        Instance = this;

        panelAnimator = GetComponent<Animator>();

    }

    private void Start() {
        GameInput.Instance.OnPlayerOpenPlayerTabPerformed += GameInput_OnPlayerOpenPlayerTabPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;
        HubMerchantUI.OnAnyHubMerchantOpenUIPanel += HubMerchantUI_OnAnyHubMerchantOpenUIPanel;
        HubMerchantUI.OnAnyHubMerchantCloseUIPanel += HubMerchantUI_OnAnyHubMerchantCloseUIPanel;
        ChangeWeaponPanel.Instance.OnChangeWeaponPanelOpened += ChangeWeaponPanel_OnChangeWeaponPanelOpened;
        ChangeWeaponPanel.Instance.OnChangeWeaponPanelClosed += ChangeWeaponPanel_OnChangeWeaponPanelClosed;

        SkillsDescriptionPanel.Instance.OnNewSkillsDescriptionPanelOpened += SkillsDescriptionPanel_OnNewSkillsDescriptionPanelOpened;
        SkillsDescriptionPanel.Instance.OnSkillsDescriptionPanelClosed += SkillsDescriptionPanel_OnSkillsDescriptionPanelClosed;
    }


    private void GameInput_OnPlayerPausePerformed(object sender, EventArgs e) {
        if (!tabMenuOpen) return;
        if (!canCloseTab) return;

        StartCoroutine(OpenCloseTabAfterFrame());
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!tabMenuOpen) return;
        if (!canCloseTab) return;

        StartCoroutine(OpenCloseTabAfterFrame());
    }

    private void SkillsDescriptionPanel_OnSkillsDescriptionPanelClosed(object sender, EventArgs e) {
        skillDescriptionPanelOpen = false;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            if (!GameInput.Instance.IsUsingGamepad()) return;
            EventSystem.current.SetSelectedGameObject(firstSkillButtonSelected);
        }
    }

    private void SkillsDescriptionPanel_OnNewSkillsDescriptionPanelOpened(object sender, EventArgs e) {
        skillDescriptionPanelOpen = true;
    }
    private void ChangeWeaponPanel_OnChangeWeaponPanelOpened(object sender, EventArgs e) {
        changeWeaponPanelOpen = true;
    }

    private void ChangeWeaponPanel_OnChangeWeaponPanelClosed(object sender, EventArgs e) {
        changeWeaponPanelOpen = false; 
        
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            if(!GameInput.Instance.IsUsingGamepad()) return;
            EventSystem.current.SetSelectedGameObject(firstButtonSelected);
        }
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
        if (hubMerchantUI.GetHubMerchant().GetHubMerchantType() == HubMerchant.HubMerchantType.ArchitectTable) return;

        canCloseTab = false;
        FadeInTab();
    }

    private void GameInput_OnPlayerOpenPlayerTabPerformed(object sender, System.EventArgs e) {
        if (PauseMenuUI.Instance.isPaused) return;
        OpenCloseTab();

        if (!GameInput.Instance.IsUsingGamepad()) return;
        EventSystem.current.SetSelectedGameObject(firstButtonSelected);
    }

    private void FadeInTab() {
        tabMenuOpen = true;

        panelAnimator.ResetTrigger("Hide");
        panelAnimator.SetTrigger("Show");
        OnPlayerTabOpened?.Invoke(this, EventArgs.Empty);
    }

    private void FadeOutTab() {
        tabMenuOpen = false;

        panelAnimator.ResetTrigger("Show");
        panelAnimator.SetTrigger("Hide");
        OnPlayerTabClosed?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator OpenCloseTabAfterFrame() {
        yield return new WaitForEndOfFrame();
        OpenCloseTab();
    }

    private void OpenCloseTab() {
        if (!canCloseTab) return;
        tabMenuOpen = !tabMenuOpen;

        if(tabMenuOpen) {
            FadeInTab();
        } else {
            FadeOutTab();
        }
    }

    public void SetCanCloseTab(bool canClose) {
        this.canCloseTab = canClose;
    }

    public GameObject GetFirstSelectedButton() {
        return firstButtonSelected;
    }

    public void SelectFirstButtonSelected() {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        EventSystem.current.SetSelectedGameObject(firstButtonSelected);
    }

    public void SetWeaponDownNavigationTarget(Button target) {
        
    }

    public bool GetTabMenuOpen() {
        return tabMenuOpen;
    }
    private void OnDestroy() {
        GameInput.Instance.OnPlayerOpenPlayerTabPerformed -= GameInput_OnPlayerOpenPlayerTabPerformed;
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerPausePerformed -= GameInput_OnPlayerPausePerformed;
        HubMerchantUI.OnAnyHubMerchantOpenUIPanel -= HubMerchantUI_OnAnyHubMerchantOpenUIPanel;
        HubMerchantUI.OnAnyHubMerchantCloseUIPanel -= HubMerchantUI_OnAnyHubMerchantCloseUIPanel;
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
        ChangeWeaponPanel.Instance.OnChangeWeaponPanelOpened -= ChangeWeaponPanel_OnChangeWeaponPanelOpened;
        ChangeWeaponPanel.Instance.OnChangeWeaponPanelClosed -= ChangeWeaponPanel_OnChangeWeaponPanelClosed;
    }
}
