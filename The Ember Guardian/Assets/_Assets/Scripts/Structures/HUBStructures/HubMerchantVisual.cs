using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantVisual : MonoBehaviour
{
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private GameObject newItemsForSaleGameObject;
    [SerializeField] private Animator bodyAnimator;

    [SerializeField] private Animator interactInputAnimator;

    private HubMerchant hubMerchant;

    private void Awake() {
        hubMerchant = GetComponentInParent<HubMerchant>();

        newItemsForSaleGameObject.gameObject.SetActive(false);
    }
    private void Start() {
        hubMerchant.OnPlayerTriggeredIn += HubMerchant_OnPlayerTriggeredIn;
        hubMerchant.OnPlayerTriggeredOut += HubMerchant_OnPlayerTriggeredOut;
        hubMerchant.OnPlayerOpenedHubMerchantShop += HubMerchant_OnPlayerInteractedWithHubMerchant;
        hubMerchant.OnPlayerStartedTalkingWithHubMerchant += HubMerchant_OnPlayerStartedTalkingWithHubMerchant;
        hubMerchant.OnPlayerStoppedInteractingWithHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithHubMerchant;
        hubMerchant.OnMerchantHasNewInteraction += HubMerchant_OnMerchantHasNewTalkLines;
        hubMerchant.OnMerchantHideExclamationMark += HubMerchant_OnMerchantHideExclamationMark;
        hubMerchant.OnMerchantFadeOutStarted += HubMerchant_OnMerchantFadeOutStarted;


        if (hubMerchant.GetMerchantHasNewItems() || hubMerchant.GetMerchantJustArrivedInHub() || hubMerchant.GetMerchantIsLevelNPC() || hubMerchant.GetMerchantHasNewTalkLinkes()) {
            newItemsForSaleGameObject.gameObject.SetActive(true);
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        StartCoroutine(CheckNewItemsForSaleActiveAfterFrame());
    }

    private IEnumerator CheckNewItemsForSaleActiveAfterFrame() {
        yield return new WaitForEndOfFrame();
        if (!hubMerchant.GetMerchantHasNewItems()) {
            newItemsForSaleGameObject.gameObject.SetActive(false);
        }
        else {
            newItemsForSaleGameObject.gameObject.SetActive(true);
        }
    }

    private void HubMerchant_OnMerchantFadeOutStarted(object sender, System.EventArgs e) {
        bodyAnimator.SetTrigger("FadeOut");
    }

    private void HubMerchant_OnMerchantHideExclamationMark(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(false);
    }

    private void HubMerchant_OnMerchantHasNewTalkLines(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(true);
    }

    private void HubMerchant_OnPlayerStartedTalkingWithHubMerchant(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(false);
        ShowInputIcon(false);
    }

    private void HubMerchant_OnPlayerInteractedWithHubMerchant(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(false);
       
        ShowInputIcon(false);
    }

    private void HubMerchant_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = emptyMaterial;
        ShowInputIcon(false);
    }

    private void HubMerchant_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (hubMerchant.GetMerchantIsDecorationalDemoMerchant()) return;
        if (hubMerchant.GetMerchantIsLevelNPC() && !hubMerchant.GetMerchantHasNewTalkLinkes()) return;

        bodySpriteRenderer.material = hoveredMaterial;

        if(hubMerchant.GetPlayerCanInteractWithMerchant()) {
            ShowInputIcon(true);
        }

    }

    private void ShowInputIcon(bool show) {

        if(show) {
            interactInputAnimator.ResetTrigger("Hide");
            interactInputAnimator.SetTrigger("Show");
        } else {
            interactInputAnimator.ResetTrigger("Show");
            interactInputAnimator.SetTrigger("Hide");
        }

    }
}
