using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponButtonContainer : MonoBehaviour
{
    [SerializeField] private WeaponChangeButton primaryWeaponButton;
    [SerializeField] private WeaponChangeButton secondaryWeaponButton;


    private void Start() {
        PlayerShoot.Instance.OnPrimaryWeaponChanged += PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged += PlayerShoot_OnSecondaryWeaponChanged;

        PlayerStats.Instance.OnCanHold2WeaponsUnlocked += PlayerShoot_OnCanHold2WeaponsUnlocked;

        UpdateWeaponButtonGuns();
    }

    private void PlayerShoot_OnCanHold2WeaponsUnlocked(object sender, System.EventArgs e) {
        UpdateWeaponButtonGuns();
    }

    private void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        UpdateWeaponButtonGuns();
    }

    private void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        UpdateWeaponButtonGuns();
    }

    private void UpdateWeaponButtonGuns() {
        GunSO primaryGunSO = PlayerShoot.Instance.GetPrimaryGunSO();
        primaryWeaponButton.SetLinkedGunSO(primaryGunSO);

        bool secondaryWeaponUnlocked = PlayerStats.Instance.GetCanHold2WeaponsUnlocked();

        if (secondaryWeaponUnlocked) {
            GunSO secondaryGunSO = PlayerShoot.Instance.GetSecondaryGunSO();
            secondaryWeaponButton.SetLinkedGunSO(secondaryGunSO);
        }
        else {
            secondaryWeaponButton.gameObject.SetActive(false);
        }
    }
}
