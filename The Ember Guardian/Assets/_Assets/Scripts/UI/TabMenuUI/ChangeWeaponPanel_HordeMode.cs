using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeWeaponPanel_HordeMode : ChangeWeaponPanel
{
    protected override void Start() {
        UpdateWeaponSlots(PlayerSave.Instance.GetUnlockedGunSOList());

        gameObject.SetActive(false);
    }

    public override void OpenClosePanel(bool calledFromPrimaryWeaponButton) {
        if (panelOpen) {

            panelOpen = false;
            gameObject.SetActive(false);

        }
        else {

            panelOpen = true;
            gameObject.SetActive(true);

            if(GameInput.Instance.IsUsingGamepad()) {
                EventSystem.current.SetSelectedGameObject(changeWeaponButtons[0]);
            }

        }
    }

    protected override void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}
