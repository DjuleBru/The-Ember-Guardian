using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeWeaponPanel : MonoBehaviour
{

    public static ChangeWeaponPanel Instance;

    protected int totalWeaponAmount = 12;
    protected bool panelOpen;
    protected bool primaryWeaponSwap;
    protected bool openedOnce;
    protected bool isLevelScene;

    [SerializeField] protected Transform changeWeaponSlotContainer;
    [SerializeField] protected Transform changeWeaponSlotTemplate;
    [SerializeField] protected Transform emptyWeaponSlotTemplate;
    protected List<GameObject> changeWeaponButtons;

    protected WeaponChangeButton lastChangeButtonThatOpenedThisPanel;

    public event EventHandler OnChangeWeaponPanelClosed;
    public event EventHandler OnChangeWeaponPanelOpened;

    protected void Awake() {
        Instance = this;
    }

    protected virtual void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;

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

    protected void PlayerTabMenuUI_OnPlayerTabClosed(object sender, EventArgs e) {
        if (panelOpen) {
            panelOpen = false;
            gameObject.SetActive(false);
            lastChangeButtonThatOpenedThisPanel = null;
            OnChangeWeaponPanelClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if(panelOpen) {
            ClosePanel();
        }
    }

    protected void PlayerShoot_OnPlayerWeaponReplaced(object sender, EventArgs e) {
        UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
    }

    protected void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        if (isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        }
        else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

    }

    protected void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        if(isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        } else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

    }

    protected void Gun_OnAnyGunUnlocked(object sender, System.EventArgs e) {
        if (isLevelScene) {
            UpdateWeaponSlots(PlayerShoot.Instance.GetGunSOInStock());
        }
        else {
            UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
        }

    }

    protected void UpdateWeaponSlots(List<GunSO> gunSOList) {
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

    public virtual void OpenClosePanel(bool calledFromPrimaryWeaponButton) {

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

        EventSystem.current.SetSelectedGameObject(lastChangeButtonThatOpenedThisPanel.gameObject);

        OnChangeWeaponPanelClosed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnDestroy() {
        Gun.OnAnyGunUnlocked -= Gun_OnAnyGunUnlocked;
        PlayerShoot.Instance.OnPrimaryWeaponChanged -= PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged -= PlayerShoot_OnSecondaryWeaponChanged;
        PlayerTabMenuUI.Instance.OnPlayerTabClosed -= PlayerTabMenuUI_OnPlayerTabClosed;
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}
