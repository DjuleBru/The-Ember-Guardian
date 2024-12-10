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
    }
    private void Start() {
        hubMerchant.OnPlayerTriggeredIn += HubMerchant_OnPlayerTriggeredIn;
        hubMerchant.OnPlayerTriggeredOut += HubMerchant_OnPlayerTriggeredOut;
    }

    private void HubMerchant_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = emptyMaterial;
    }

    private void HubMerchant_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = hoveredMaterial;

    }
}
