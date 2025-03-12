using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBManager_Demo : MonoBehaviour
{
    private bool firstDemoHubEncounter;

    [SerializeField] private List<HubMerchant> functionalDemoHubMerchantList;
    [SerializeField] private List<HubMerchant> decorationalDemoHubMerchantList;

    private void Awake() {
        firstDemoHubEncounter = ES3.Load("firstDemoHubEncounter", true);
    }

    void Start() {
        HubMerchantTalkUI.OnAnyMerchantEndTalk += HubMerchantTalkUI_OnAnyMerchantEndTalk;
    }

    private void HubMerchantTalkUI_OnAnyMerchantEndTalk(object sender, System.EventArgs e) {
        //if (!firstDemoHubEncounter) return;

        Debug.Log("HubMerchantTalkUI_OnAnyMerchantEndTalk");
        HubMerchantTalkUI hubMerchantTalkUI = sender as HubMerchantTalkUI;
        HubMerchant hubMerchant = hubMerchantTalkUI.GetHubMerchant();

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GemMerchant) {
            ES3.Save("firstDemoHubEncounter", false);

            foreach(HubMerchant functionalHubMerchant in functionalDemoHubMerchantList) {
                functionalHubMerchant.SetHasTalkLinesToShow();
                functionalHubMerchant.SetDemoMerchantUnlocked();
            }

            foreach (HubMerchant decorationalHubMerchant in decorationalDemoHubMerchantList) {
                decorationalHubMerchant.SetHasTalkLinesToShow(false);
                decorationalHubMerchant.SetDemoMerchantUnlocked();
            }
        }

    }


    private void OnDestroy() {
        HubMerchantTalkUI.OnAnyMerchantEndTalk -= HubMerchantTalkUI_OnAnyMerchantEndTalk;
    }


}
