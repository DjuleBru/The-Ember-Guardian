using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubChestUI : MonoBehaviour
{
    private HubChest hubChest;
    [SerializeField] private GameObject payCurrencyUI;
    private float openChestAnimationDelay = 3.3f;

    private Coroutine showUICoroutine;

    private void Awake() {
        hubChest = GetComponentInParent<HubChest>();
        payCurrencyUI.SetActive(false);
    }

    private void Start() {
        hubChest.OnChestOpened += HubChest_OnChestOpened;
        hubChest.OnChestClosed += HubChest_OnChestClosed;
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        StopCoroutine(showUICoroutine);
        payCurrencyUI.SetActive(false);

        if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfCategory(PlayerCurrencies.CurrencyCategory.gem).Count != 0) return;
    }

    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        showUICoroutine = StartCoroutine(SetUIActiveAfterDelay());
    }

    private IEnumerator SetUIActiveAfterDelay() {
        yield return new WaitForSeconds(openChestAnimationDelay);
        if(hubChest.GetPlayerInTriggerArea()) {
            payCurrencyUI.SetActive(true);
        }
    }
}
