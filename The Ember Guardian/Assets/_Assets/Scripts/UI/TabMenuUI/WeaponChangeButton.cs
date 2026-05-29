using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponChangeButton : ButtonUI
{
    private GunSO linkedGunSO;
    private Button button;

    [SerializeField] private Image gunIconImage;
    [SerializeField] private bool isPrimaryWeaponButton;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image gunAmmoTypeImage;
    [SerializeField] private Sprite specialAmmoSprite;
    private Sprite standardAmmoSprite;

    public static event EventHandler OnAnyWeaponChangeButtonPressed;

    private void Awake() {
        button = GetComponent<Button>();

        standardAmmoSprite = gunAmmoTypeImage.sprite;
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

        weaponNameText.font = LocalizationManager.Instance.GetCurrentFont();
    }

    private void WeaponButtonPressHub() {
        OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);

        if (linkedGunSO != null) {
            PlayerShoot.Instance.SetActiveGun(linkedGunSO.gunType, isPrimaryWeaponButton);
        }

        if (PlayerShoot.Instance.GetUnlockedGunSOList().Count <= 1) return;

        ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
        ChangeWeaponPanel.Instance.OpenClosePanel(isPrimaryWeaponButton);
        ChangeWeaponPanel.Instance.SetPrimaryWeaponSwap(isPrimaryWeaponButton);

        ;
    }

    private void WeaponButtonPressLevel() {
        OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);

        if (PlayerShoot.Instance.GetGunSOInStock() == null) return;


        ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
        ChangeWeaponPanel.Instance.OpenClosePanel(isPrimaryWeaponButton);
        ChangeWeaponPanel.Instance.SetPrimaryWeaponSwap(isPrimaryWeaponButton);
    }

    private void PlayerShoot_OnSecondaryWeaponChanged(object sender, System.EventArgs e) {
        if (isPrimaryWeaponButton) return;
        UpdateGunIconImage(PlayerShoot.Instance.GetSecondaryGunSO());

        if (!GameInput.Instance.IsUsingGamepad()) return;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void PlayerShoot_OnPrimaryWeaponChanged(object sender, System.EventArgs e) {
        if (!isPrimaryWeaponButton) return;
        UpdateGunIconImage(PlayerShoot.Instance.GetPrimaryGunSO());

        if (!GameInput.Instance.IsUsingGamepad()) return;
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

        if(linkedGunSO != null) {
            weaponNameText.text = LocalizationManager.Instance.GetLocalizedText(gunSO.gunType.ToString());
            gunAmmoTypeImage.gameObject.SetActive(true);
        } else {
            weaponNameText.text = "";
            gunAmmoTypeImage.gameObject.SetActive(false);
        }

        if (gunSO != null && gunSO.ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special) {
            gunAmmoTypeImage.sprite = specialAmmoSprite;
        } else {
            gunAmmoTypeImage.sprite = standardAmmoSprite;
        }
    }

    public GunSO GetLinkedGunSO() {
        return linkedGunSO;
    }
}
