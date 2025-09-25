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
        HubMerchant.OnPlayerOpenedAnyHubMerchantShop += HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        VideoTipUI.Instance.OnVideoTipPanelOpened += VideoTipUI_OnVideoTipPanelOpened;
    }

    private void VideoTipUI_OnVideoTipPanelOpened(object sender, System.EventArgs e) {
        blurVolumeAnimator.ResetTrigger("Hide");
        blurVolumeAnimator.SetTrigger("Show");
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        blurVolumeAnimator.ResetTrigger("Show");
        blurVolumeAnimator.SetTrigger("Hide");

    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, System.EventArgs e) {
        blurVolumeAnimator.ResetTrigger("Show");
        blurVolumeAnimator.SetTrigger("Hide");
    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, System.EventArgs e) {
        blurVolumeAnimator.ResetTrigger("Hide");
        blurVolumeAnimator.SetTrigger("Show");
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        blurVolumeAnimator.SetTrigger("Hide");
    }

    private void HubMerchant_OnPlayerInteractedWithAnyHubMerchant(object sender, System.EventArgs e) {
        blurVolumeAnimator.SetTrigger("Show");
    }

    private void OnDestroy() {
        HubMerchant.OnPlayerOpenedAnyHubMerchantShop -= HubMerchant_OnPlayerInteractedWithAnyHubMerchant;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }
}
