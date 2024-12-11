using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BlurBackgroundUI : MonoBehaviour
{
    private Volume blurVolume;
    private Animator blurVolumeAnimator;

    private void Awake() {
        blurVolume = GetComponent<Volume>();
        blurVolumeAnimator = GetComponent<Animator>();

        blurVolume.enabled = false;
    }

    private void Start() {
        HubMerchant.OnPlayerInteractedWithAnyHubMerchant += HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        blurVolumeAnimator.SetTrigger("Hide");
    }

    private void HubMerchant_OnPlayerInteractedWithAnyHubMerchant(object sender, System.EventArgs e) {
        blurVolumeAnimator.SetTrigger("Show");
    }
}
