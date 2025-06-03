using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI_GunJam : MonoBehaviour
{
    [SerializeField] private GunJamSingleIconUI inputIconTemplate;
    [SerializeField] private Transform primaryGunInputIconContainer;
    [SerializeField] private Transform secondaryGunInputIconContainer;
    private Transform activeInputContainer;

    private void Awake() {
        inputIconTemplate.gameObject.SetActive(false); 
        primaryGunInputIconContainer.gameObject.SetActive(false);
    }

    private void Start() {
        GunJamHandler.OnAnyJamSequenceGenerated += GunJamHandler_OnJamSequenceGenerated;
        GunJamHandler.OnAnyJamSequenceCompleted += GunJamHandler_OnAnyJamSequenceCompleted;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        RefreshPrimaryOrSecondaryUI();
    }

    private void GunJamHandler_OnAnyJamSequenceCompleted(object sender, System.EventArgs e) {
        CleanUISequence();
    }

    private void GunJamHandler_OnJamSequenceGenerated(object sender, GunJamHandler.OnJamSequenceGeneratedEventArgs e) {
        RefreshPrimaryOrSecondaryUI();

        Queue<GameInput.Binding> inputSequence = e.inputSequence;

        int i = 0;
        foreach(GameInput.Binding inputBinding in inputSequence) {
            GunJamSingleIconUI inputIcon = Instantiate(inputIconTemplate.transform, activeInputContainer).GetComponent<GunJamSingleIconUI>();
            inputIcon.gameObject.SetActive(true);
            inputIcon.SetBinding(inputBinding);
            inputIcon.SetIndex(i);
            i++;
        }
    }

    private void RefreshPrimaryOrSecondaryUI() {
        primaryGunInputIconContainer.gameObject.SetActive(false);
        secondaryGunInputIconContainer.gameObject.SetActive(false);

        if (PlayerShoot.Instance.GetHeldGunSO() == PlayerShoot.Instance.GetPrimaryGunSO()) {
            activeInputContainer = primaryGunInputIconContainer;
        }
        else {
            activeInputContainer = secondaryGunInputIconContainer;
        }

        activeInputContainer.gameObject.SetActive(true);
    }

    private void CleanUISequence() {
        foreach(Transform child in activeInputContainer) {
            if (child == inputIconTemplate.transform) continue;
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy() {
        GunJamHandler.OnAnyJamSequenceGenerated -= GunJamHandler_OnJamSequenceGenerated;
        GunJamHandler.OnAnyJamSequenceCompleted -= GunJamHandler_OnAnyJamSequenceCompleted;
    }
}
