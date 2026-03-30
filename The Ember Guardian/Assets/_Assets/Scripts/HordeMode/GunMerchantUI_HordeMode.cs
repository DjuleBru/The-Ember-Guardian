using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GunMerchantUI_HordeMode : MonoBehaviour
{
    [SerializeField] private HubMerchant gunMerchant;

    [SerializeField] private GameObject rifleItems;
    [SerializeField] private GameObject smgItems;
    [SerializeField] private GameObject shotgunItems;
    [SerializeField] private GameObject sniperItems;
    [SerializeField] private GameObject LMGItems;
    [SerializeField] private GameObject revolverItems;
    [SerializeField] private GameObject pistolItems;
    [SerializeField] private GameObject AAItems;
    [SerializeField] private GameObject ARItems;
    [SerializeField] private GameObject RLItems;
    [SerializeField] private GameObject GLItems;
    [SerializeField] private GameObject minigunItems;

    [SerializeField] private Button rifleButton;
    [SerializeField] private Button smgButton;
    [SerializeField] private Button shotgunButton;
    [SerializeField] private Button sniperButton;
    [SerializeField] private Button LMGButton;
    [SerializeField] private Button revolverButton;
    [SerializeField] private Button pistolButton;
    [SerializeField] private Button AAButton;
    [SerializeField] private Button ARButton;
    [SerializeField] private Button RLButton;
    [SerializeField] private Button GLButton;
    [SerializeField] private Button minigunButton;

    private List<GunSO> gunList = new List<GunSO>();

    private void Start() {
        PlayerShoot.Instance.OnPrimaryWeaponChanged += PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged += PlayerShoot_OnSecondaryWeaponChanged;
        PlayerShoot.Instance.OnPlayerWeaponReplaced += PlayerShoot_OnPlayerWeaponReplaced;

        gunMerchant.OnPlayerOpenedHubMerchantShop += GunMerchant_OnPlayerOpenedHubMerchantShop;

        StartCoroutine(RefreshUIAfterFrame());
    }

    private void GunMerchant_OnPlayerOpenedHubMerchantShop(object sender, System.EventArgs e) {
        StartCoroutine(SetSelectedButtonAfterDelay());
    }

    private void PlayerShoot_OnPlayerWeaponReplaced(object sender, System.EventArgs e) {
        RefreshUI();
    }

    private void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        RefreshUI();
    }

    private void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        RefreshUI();
    }

    private IEnumerator SetSelectedButtonAfterDelay() {
        yield return new WaitForSeconds(.15f);

        if (GameInput.Instance.IsUsingGamepad()) {
            EventSystem.current.SetSelectedGameObject(GetGunItemsButton(gunList[0]).gameObject);
            GetGunItemsButton(gunList[0]).GetComponent<ItemButtonUI>().ShowTree();
        }

    }

    private void RefreshUI() {
        RefreshGunList();
        RefreshActiveItems();
        RefreshNavigation();
    }

    private IEnumerator RefreshUIAfterFrame() {
        yield return new WaitForEndOfFrame();
        RefreshUI();
    }

    private void RefreshGunList() {
        GunSO primaryGunSO = PlayerShoot.Instance.GetPrimaryGunSO();
        if (primaryGunSO != null && !gunList.Contains(primaryGunSO)) {
            gunList.Add(primaryGunSO);
        }

        GunSO secondaryGunSO = PlayerShoot.Instance.GetSecondaryGunSO();
        if (secondaryGunSO != null && !gunList.Contains(secondaryGunSO)) {
            gunList.Add(secondaryGunSO);
        }

        foreach (GunSO gunSO in PlayerShoot.Instance.GetGunSOInStock()) {
            if (!gunList.Contains(gunSO)) {
                gunList.Add(gunSO);
            }
        }
    }

    private void RefreshActiveItems() {
        rifleItems.SetActive(false);
        smgItems.SetActive(false);
        shotgunItems.SetActive(false);
        sniperItems.SetActive(false);       
        LMGItems.SetActive(false);
        revolverItems.SetActive(false);
        pistolItems.SetActive(false);
        AAItems.SetActive(false);
        ARItems.SetActive(false);
        RLItems.SetActive(false);
        GLItems.SetActive(false);
        minigunItems.SetActive(false);

        foreach (GunSO gunSO in gunList) {
            GetGunItemsGO(gunSO).SetActive(true);
        }

        // ---- ORDRE D'AFFICHAGE ----
        // Les premiers de la liste sont placés au début du Horizontal Layout Group
        for (int i = 0; i < gunList.Count; i++) {
            Transform t = GetGunItemsGO(gunList[i]).transform;
            t.SetSiblingIndex(i);
        }
    }

    private void RefreshNavigation() {
        List<Button> orderedButtons = new List<Button>();

        // Récupérer les boutons actifs dans l'ordre visuel (dans le layout)
        for (int i = 0; i < gunList.Count; i++) {
            GameObject itemGO = GetGunItemsGO(gunList[i]);
            if (itemGO.activeSelf) {
                Button btn = itemGO.GetComponentInChildren<Button>();
                if (btn != null)
                    orderedButtons.Add(btn);
            }
        }

        // Appliquer la navigation
        for (int i = 0; i < orderedButtons.Count; i++) {
            Button btn = orderedButtons[i];

            Navigation nav = btn.navigation;        // récupère la navigation déjà existante
            nav.mode = Navigation.Mode.Explicit;    // obligatoire pour que gauche/droite fonctionne

            nav.selectOnLeft = i > 0 ? orderedButtons[i - 1] : null;
            nav.selectOnRight = i < orderedButtons.Count - 1 ? orderedButtons[i + 1] : null;

            btn.navigation = nav;                  // réapplique sans toucher haut/bas
        }
    }

    private GameObject GetGunItemsGO(GunSO gunSO) {
        if(gunSO.gunType == GunSO.GunType.Rifle) {
            return rifleItems;
        }
        if (gunSO.gunType == GunSO.GunType.SMG) {
            return smgItems;
        }
        if (gunSO.gunType == GunSO.GunType.Shotgun) {
            return shotgunItems;
        }
        if (gunSO.gunType == GunSO.GunType.Revolver) {
            return revolverItems;
        }
        if (gunSO.gunType == GunSO.GunType.LMG) {
            return LMGItems;
        }
        if (gunSO.gunType == GunSO.GunType.Pistol) {
            return pistolItems;
        }
        if (gunSO.gunType == GunSO.GunType.GrenadeLauncher) {
            return GLItems;
        }
        if (gunSO.gunType == GunSO.GunType.AssaultRifle) {
            return ARItems;
        }
        if (gunSO.gunType == GunSO.GunType.AAGun) {
            return AAItems;
        }
        if (gunSO.gunType == GunSO.GunType.RocketLauncher) {
            return RLItems;
        }
        if (gunSO.gunType == GunSO.GunType.Sniper) {
            return sniperItems;
        }
        if (gunSO.gunType == GunSO.GunType.MiniGun) {
            return minigunItems;
        }
        return rifleItems;
    }

    private Button GetGunItemsButton(GunSO gunSO) {
        if (gunSO.gunType == GunSO.GunType.Rifle) {
            return rifleButton;
        }
        if (gunSO.gunType == GunSO.GunType.SMG) {
            return smgButton;
        }
        if (gunSO.gunType == GunSO.GunType.Shotgun) {
            return shotgunButton;
        }
        if (gunSO.gunType == GunSO.GunType.Revolver) {
            return revolverButton;
        }
        if (gunSO.gunType == GunSO.GunType.LMG) {
            return LMGButton;
        }
        if (gunSO.gunType == GunSO.GunType.Pistol) {
            return pistolButton;
        }
        if (gunSO.gunType == GunSO.GunType.GrenadeLauncher) {
            return GLButton;
        }
        if (gunSO.gunType == GunSO.GunType.AssaultRifle) {
            return ARButton;
        }
        if (gunSO.gunType == GunSO.GunType.AAGun) {
            return AAButton;
        }
        if (gunSO.gunType == GunSO.GunType.RocketLauncher) {
            return RLButton;
        }
        if (gunSO.gunType == GunSO.GunType.Sniper) {
            return sniperButton;
        }
        if (gunSO.gunType == GunSO.GunType.MiniGun) {
            return minigunButton;
        }
        return rifleButton;
    }
}
