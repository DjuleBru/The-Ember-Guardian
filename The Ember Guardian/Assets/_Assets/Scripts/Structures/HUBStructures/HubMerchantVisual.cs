using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantVisual : MonoBehaviour
{
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private GameObject newItemsForSaleGameObject;

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
        hubMerchant.OnMerchantHasNewTalkLines += HubMerchant_OnMerchantHasNewTalkLines;
        hubMerchant.OnMerchantHideExclamationMark += HubMerchant_OnMerchantHideExclamationMark;

        if (hubMerchant.GetMerchantHasNewItems() || hubMerchant.GetMerchantJustArrivedInHub() || hubMerchant.GetMerchantIsLevelNPC() || hubMerchant.GetMerchantHasNewTalkLinkes()) {
            newItemsForSaleGameObject.gameObject.SetActive(true);
        }
    }

    private void HubMerchant_OnMerchantHideExclamationMark(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(false);
    }

    private void HubMerchant_OnMerchantHasNewTalkLines(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(true);
    }

    private void HubMerchant_OnPlayerStartedTalkingWithHubMerchant(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(false);
    }

    private void HubMerchant_OnPlayerInteractedWithHubMerchant(object sender, System.EventArgs e) {
        newItemsForSaleGameObject.gameObject.SetActive(false);
    }

    private void HubMerchant_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = emptyMaterial;
    }

    private void HubMerchant_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = hoveredMaterial;
    }
}
