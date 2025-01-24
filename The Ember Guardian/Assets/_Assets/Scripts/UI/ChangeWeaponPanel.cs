using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWeaponPanel : MonoBehaviour
{

    public static ChangeWeaponPanel Instance;

    private bool panelOpen;
    private bool primaryWeaponSwap;

    [SerializeField] private Transform changeWeaponSlotContainer;
    [SerializeField] private Transform changeWeaponSlotTemplate;

    private WeaponChangeButton lastChangeButtonThatOpenedThisPanel;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;

        Gun.OnAnyGunUnlocked += Gun_OnAnyGunUnlocked;
        PlayerShoot.Instance.OnPrimaryWeaponChanged += PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged += PlayerShoot_OnSecondaryWeaponChanged;

        UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());

        gameObject.SetActive(false);
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        panelOpen = false;
        gameObject.SetActive(false);
        lastChangeButtonThatOpenedThisPanel = null;
    }

    private void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
    }

    private void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
    }

    private void Gun_OnAnyGunUnlocked(object sender, System.EventArgs e) {
        UpdateWeaponSlots(PlayerShoot.Instance.GetUnlockedAndUnequippedGunSOList());
    }

    private void UpdateWeaponSlots(List<GunSO> gunSOList) {
        changeWeaponSlotTemplate.gameObject.SetActive(true);

        foreach (Transform child in changeWeaponSlotContainer) {
            if (child == changeWeaponSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(GunSO gunSO in gunSOList) {
            WeaponReplaceButton weaponReplaceButton = Instantiate(changeWeaponSlotTemplate, changeWeaponSlotContainer).GetComponent<WeaponReplaceButton>();

            weaponReplaceButton.SetLinkedGunSO(gunSO);
        }

        changeWeaponSlotTemplate.gameObject.SetActive(false);
    }

    public void SetPrimaryWeaponSwap(bool primaryWeaponSwap) {
        this.primaryWeaponSwap = primaryWeaponSwap;
    }
    public void SetLastWeaponChangeButton(WeaponChangeButton changeButton) {
        lastChangeButtonThatOpenedThisPanel = changeButton;
    }

    public void OpenClosePanel() {
        panelOpen = !panelOpen;
        gameObject.SetActive(panelOpen);

        if(!panelOpen) {
            lastChangeButtonThatOpenedThisPanel = null;
        }
    }

    public bool GetJustPressedByOtherWeaponButton(WeaponChangeButton changeButton) {
        if (lastChangeButtonThatOpenedThisPanel == null) return false;

        return lastChangeButtonThatOpenedThisPanel != changeButton;
    }

    public bool GetPrimaryWeaponSwap() {
        return primaryWeaponSwap;
    }

    private void OnDestroy() {
        Gun.OnAnyGunUnlocked -= Gun_OnAnyGunUnlocked;
        PlayerShoot.Instance.OnPrimaryWeaponChanged -= PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged -= PlayerShoot_OnSecondaryWeaponChanged;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened -= PlayerTabMenuUI_OnPlayerTabOpened;
    }
}
