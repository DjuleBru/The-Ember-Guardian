using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponChangeButton : ButtonUI
{
    private GunSO linkedGunSO;
    private Button button;

    [SerializeField] private Image gunIconImage;
    [SerializeField] private bool isPrimaryWeaponButton;

    public static event EventHandler OnAnyWeaponChangeButtonPressed;

    private void Awake() {
        button = GetComponent<Button>();
    }

    protected override void Start() {
        PlayerShoot.Instance.OnPrimaryWeaponChanged += PlayerShoot_OnPrimaryWeaponChanged;
        PlayerShoot.Instance.OnSecondaryWeaponChanged += PlayerShoot_OnSecondaryWeaponChanged;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            //button.enabled = false;
            button.onClick.AddListener(() => {
                WeaponButtonPressLevel();
            });

        } else {

            button.onClick.AddListener(() => {
                WeaponButtonPressHub();
            });
        }

    }

    private void WeaponButtonPressHub() {
        OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);

        if (linkedGunSO != null) {
            PlayerShoot.Instance.SetActiveGun(linkedGunSO.gunType, isPrimaryWeaponButton);
        }

        if (PlayerShoot.Instance.GetUnlockedGunSOList().Count <= 1) return;

        ChangeWeaponPanel.Instance.SetPrimaryWeaponSwap(isPrimaryWeaponButton);

        if (ChangeWeaponPanel.Instance.GetJustPressedByOtherWeaponButton(this)) {
            ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
            return;
        };

        ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
        ChangeWeaponPanel.Instance.OpenClosePanel();
        ;
    }

    private void WeaponButtonPressLevel() {
        OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);

        if (PlayerShoot.Instance.GetGunSOInStock() == null) return;

        ChangeWeaponPanel.Instance.SetPrimaryWeaponSwap(isPrimaryWeaponButton);

        if (ChangeWeaponPanel.Instance.GetJustPressedByOtherWeaponButton(this)) {
            ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
            return;
        };

        ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
        ChangeWeaponPanel.Instance.OpenClosePanel();
    }

    private void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        if (isPrimaryWeaponButton) return;
        UpdateGunIconImage(PlayerShoot.Instance.GetSecondaryGunSO());
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        if (!isPrimaryWeaponButton) return;
        UpdateGunIconImage(PlayerShoot.Instance.GetPrimaryGunSO());
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void UpdateGunIconImage(GunSO gunSO) {
        if(gunSO != null) {
            
            gunIconImage.sprite = gunSO.gunSprite;
            gunIconImage.color = Color.white;

        } else {
            Color transparentColor = Color.white;
            transparentColor.a = 0f;
            gunIconImage.color = transparentColor;
        }
    }

    public void SetLinkedGunSO(GunSO gunSO) {
        linkedGunSO = gunSO;

        UpdateGunIconImage(linkedGunSO);
    }

    public GunSO GetLinkedGunSO() {
        return linkedGunSO;
    }
}
