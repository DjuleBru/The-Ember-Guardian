using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private Chest chest;
    [SerializeField] private GameObject uiGO;

    private void Start() {
        chest.OnPlayerTriggeredIn += Chest_OnPlayerTriggeredIn;
        chest.OnPlayerTriggeredOut += Chest_OnPlayerTriggeredOut;
        chest.OnChestPricePaid += Chest_OnChestPricePaid;
        uiGO.SetActive(false);
    }

    private void Chest_OnChestPricePaid(object sender, System.EventArgs e) {
        uiGO.SetActive(false);
    }

    private void Chest_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (chest.GetChestOpened()) return;
        if (!chest.GetPayToOpenChest()) return;
        uiGO.SetActive(false);
    }

    private void Chest_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (chest.GetChestOpened()) return;
        if (!chest.GetPayToOpenChest()) return;
        uiGO.SetActive(true);
    }
}
