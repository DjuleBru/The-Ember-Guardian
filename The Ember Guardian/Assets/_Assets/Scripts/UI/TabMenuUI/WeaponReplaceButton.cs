using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponReplaceButton : ButtonUI {

    [SerializeField] private Image gunIconImage;

    private Button button;
    private GunSO linkedGunSO;

    public static event EventHandler OnWeaponReplaceButtonPressed;
    public static event EventHandler OnAnyWeaponReplaceButtonHovered;
    public static event EventHandler OnAnyWeaponReplaceButtonUnhovered;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SwapWeapon();
        });
    }

    private void SwapWeapon() {
        bool primaryWeaponSwap = ChangeWeaponPanel.Instance.GetPrimaryWeaponSwap();
        bool levelReplace = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;

        if (levelReplace) {
            PlayerShoot.Instance.ReplaceWeaponSO(linkedGunSO, primaryWeaponSwap);

        } else {

            if (primaryWeaponSwap) {
                PlayerShoot.Instance.SetPrimaryWeaponSO(linkedGunSO);
            }
            else {
                PlayerShoot.Instance.SetSecondaryWeaponSO(linkedGunSO);
            }

        }

        ChangeWeaponPanel.Instance.OpenClosePanel();
        OnWeaponReplaceButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    public void SetLinkedGunSO(GunSO gunSO) {
        linkedGunSO = gunSO;
        UpdateGunIconImage(gunSO);
    }

    private void UpdateGunIconImage(GunSO gunSO) {

        if (gunSO != null) {

            gunIconImage.sprite = gunSO.gunSprite;
            gunIconImage.color = Color.white;

        }
        else {
            Color transparentColor = Color.white;
            transparentColor.a = 0f;
            gunIconImage.color = transparentColor;
        }
    }

    public GunSO GetLinkedGunSO() {
        return linkedGunSO;
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        if (GameInput.Instance.IsUsingGamepad()) return;

        base.OnPointerEnter(eventData);
        OnAnyWeaponReplaceButtonHovered?.Invoke(this, EventArgs.Empty);
    }

    public override void OnPointerExit(PointerEventData eventData) {
        if (GameInput.Instance.IsUsingGamepad()) return;

        base.OnPointerExit(eventData);
        OnAnyWeaponReplaceButtonUnhovered?.Invoke(this, EventArgs.Empty);
    }

    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;

        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            OnAnyWeaponReplaceButtonHovered?.Invoke(this, EventArgs.Empty);
        }
    }
}
