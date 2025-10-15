using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeWeaponPanel : MonoBehaviour
{

    public static ChangeWeaponPanel Instance;

    private int totalWeaponAmount = 12;
    private bool panelOpen;
    private bool primaryWeaponSwap;
    private bool openedOnce;
    private bool isLevelScene;

    [SerializeField] private Transform changeWeaponSlotContainer;
    [SerializeField] private Transform changeWeaponSlotTemplate;
    [SerializeField] private Transform emptyWeaponSlotTemplate;
    private List<GameObject> changeWeaponButtons;

    private WeaponChangeButton lastChangeButtonThatOpenedThisPanel;

    public event EventHandler OnChangeWeaponPanelClosed;
    public event EventHandler OnChangeWeaponPanelOpened;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        Gun.OnAnyGunUnlocked += Gun_OnAnyGunUnlocked;
        PlayerShoot.Instance.OnPrimaryWeaponChanged += PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged += PlayerShoot_OnSecondaryWeaponChanged;
        PlayerShoot.Instance.OnPlayerWeaponReplaced += PlayerShoot_OnPlayerWeaponReplaced;

        isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;

        if (isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        }
        else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

        gameObject.SetActive(false);
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, EventArgs e) {
        if (panelOpen) {
            panelOpen = false;
            gameObject.SetActive(false);
            lastChangeButtonThatOpenedThisPanel = null;
            OnChangeWeaponPanelClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if(panelOpen) {
            ClosePanel();
        }
    }


    private void PlayerShoot_OnPlayerWeaponReplaced(object sender, EventArgs e) {
        UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
    }

    private void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        if (isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        }
        else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

    }

    private void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        if(isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        } else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

    }

    private void Gun_OnAnyGunUnlocked(object sender, System.EventArgs e) {
        if (isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        }
        else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

    }

    private void UpdateWeaponSlots(List<GunSO> gunSOList) {
        changeWeaponSlotTemplate.gameObject.SetActive(true);
        emptyWeaponSlotTemplate.gameObject.SetActive(true);
        changeWeaponButtons = new List<GameObject>();

        foreach (Transform child in changeWeaponSlotContainer) {
            if (child == changeWeaponSlotTemplate || child == emptyWeaponSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        int unlockedGunAmount = 0;
        foreach(GunSO gunSO in gunSOList) {
            WeaponReplaceButton weaponReplaceButton = Instantiate(changeWeaponSlotTemplate, changeWeaponSlotContainer).GetComponent<WeaponReplaceButton>();

            weaponReplaceButton.SetLinkedGunSO(gunSO);
            changeWeaponButtons.Add(weaponReplaceButton.gameObject);
            unlockedGunAmount++;
        }

        for(int i = 0; i < totalWeaponAmount - unlockedGunAmount; i++) {
            Instantiate(emptyWeaponSlotTemplate, changeWeaponSlotContainer);
        }

        changeWeaponSlotTemplate.gameObject.SetActive(false);
        emptyWeaponSlotTemplate.gameObject.SetActive(false);
    }

    public void SetPrimaryWeaponSwap(bool primaryWeaponSwap) {
        this.primaryWeaponSwap = primaryWeaponSwap;
    }
    public void SetLastWeaponChangeButton(WeaponChangeButton changeButton) {
        lastChangeButtonThatOpenedThisPanel = changeButton;
    }

    public void OpenClosePanel(bool calledFromPrimaryWeaponButton) {

        if(panelOpen) {

            if (!openedOnce || (primaryWeaponSwap && calledFromPrimaryWeaponButton) || (!primaryWeaponSwap && !calledFromPrimaryWeaponButton)) {
                lastChangeButtonThatOpenedThisPanel = null;
                OnChangeWeaponPanelClosed?.Invoke(this, EventArgs.Empty);
                panelOpen = false;
                gameObject.SetActive(false);
            }


        } else {

            SkillsDescriptionPanel.Instance.ClosePanel();
            openedOnce = true;
            if (changeWeaponButtons.Count > 0) {
                if (GameInput.Instance.IsUsingGamepad()) {
                    EventSystem.current.SetSelectedGameObject(changeWeaponButtons[0]);
                };

                OnChangeWeaponPanelOpened?.Invoke(this, EventArgs.Empty);
                panelOpen = true;
                gameObject.SetActive(true);
            }

        }
    }

    public bool GetJustPressedByOtherWeaponButton(WeaponChangeButton changeButton) {
        if (lastChangeButtonThatOpenedThisPanel == null) return false;

        return lastChangeButtonThatOpenedThisPanel != changeButton;
    }

    public bool GetPrimaryWeaponSwap() {
        return primaryWeaponSwap;
    }

    public void ClosePanel() {
        panelOpen = false;
        gameObject.SetActive(panelOpen);

        OnChangeWeaponPanelClosed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy() {
        Gun.OnAnyGunUnlocked -= Gun_OnAnyGunUnlocked;
        PlayerShoot.Instance.OnPrimaryWeaponChanged -= PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged -= PlayerShoot_OnSecondaryWeaponChanged;
        PlayerTabMenuUI.Instance.OnPlayerTabClosed -= PlayerTabMenuUI_OnPlayerTabClosed;
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}
