using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubChestVisualIndicator : MonoBehaviour
{
    private HubChest hubChest;
    [SerializeField] private GameObject chestIndicator;

    private void Awake() {
        hubChest = GetComponentInParent<HubChest>();

        chestIndicator.gameObject.SetActive(false);
    }

    private void Start() {
        hubChest.OnChestOpened += HubChest_OnChestOpened;
        hubChest.OnChestClosed += HubChest_OnChestClosed;
        hubChest.OnChestSetCanOpen += HubChest_OnChestSetCanOpen;
    }

    private void HubChest_OnChestSetCanOpen(object sender, System.EventArgs e) {
        chestIndicator.gameObject.SetActive(true);
        Debug.Log("HubChest_OnChestSetCanOpen");
    }

    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        if (!HubChest.Instance.GetCanOpenChest()) return;
        chestIndicator.gameObject.SetActive(false);
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        if (!HubChest.Instance.GetCanOpenChest()) return;
        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfCategory(PlayerCurrencies.CurrencyCategory.gem).Count == 0) return;

        chestIndicator.gameObject.SetActive(true);
    }

}
