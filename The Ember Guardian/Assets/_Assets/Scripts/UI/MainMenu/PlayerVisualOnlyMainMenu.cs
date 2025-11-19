using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualOnlyMainMenu : MonoBehaviour
{

    [SerializeField] private Sprite femaleSprite;
    [SerializeField] private Sprite maleSprite;
    [SerializeField] private SpriteRenderer characterSpriteRenderer;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private SpriteRenderer weaponLightsSpriteRenderer;

    private void Start() {
        CharacterSelectUI.Instance.OnCharacterChanged += CharacterSelectUI_OnCharacterChanged;
        HordeModeUI.Instance.OnWeaponSelected += HordeModeUI_OnWeaponSelected;

        SetCharacterVisuals();

        GunSO.GunType lastGunTypeUsed = PlayerSave.Instance.GetPrimaryActiveGunType();
        SetGunVisuals(lastGunTypeUsed);
    }

    private void HordeModeUI_OnWeaponSelected(object sender, System.EventArgs e) {
        SetGunVisuals(HordeModeUI.Instance.GetSelectedGunType());
    }

    private void CharacterSelectUI_OnCharacterChanged(object sender, System.EventArgs e) {
        SetCharacterVisuals();
    }

    private void SetCharacterVisuals() {
        bool chosenCharacterIsFemale = CharacterSelectUI.Instance.GetChosenCharacterIsFemale();

        if (chosenCharacterIsFemale) {
            characterSpriteRenderer.sprite = femaleSprite;
        } else {
            characterSpriteRenderer.sprite = maleSprite;
        }
    }

    private void SetGunVisuals(GunSO.GunType gunType) {
        GunSO gunSO = GetGunSO(gunType);

        weaponSpriteRenderer.sprite = gunSO.gunSprite_mainMenu;
        weaponLightsSpriteRenderer.sprite = gunSO.gunSprite_mainMenuLights;
    }

    private GunSO GetGunSO(GunSO.GunType gunType) {
        foreach(GunSO gunSO in PlayerSave.Instance.GetAllGunSOs()) {
            if (gunSO.gunType == gunType) return gunSO;
        }
        return PlayerSave.Instance.GetAllGunSOs()[0];
    }
}
