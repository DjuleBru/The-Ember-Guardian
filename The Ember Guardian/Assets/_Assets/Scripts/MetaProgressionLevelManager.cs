using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionLevelManager : MonoBehaviour
{

    private bool emberlingsControlUnlocked;

    private void Awake() {
        emberlingsControlUnlocked = ES3.Load("emberlingsControlUnlocked", false);
    }

    private void Start() {
        bool isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;
        if (!isLevelScene) return;

        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        HubMerchant hubMerchant = (HubMerchant)sender;

        if(hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.WorkerMerchant) {

            if(LevelManager.Instance.GetLevelSO().levelObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.FindArchitectTable) {
                if (emberlingsControlUnlocked) return;
                UnlockEmberlingsControl();
            }

        }
    }

    private void UnlockEmberlingsControl() {
        ES3.Save("emberlingsControlUnlocked", true);

        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.ControlEmberlings.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantItemBought(HubMerchantItem_WatcherMerchantItem.WatcherItemType.ControlEmberlings.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.ControlEmberlings.ToString(), true);

        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.MaxFollowingWorkers.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.MaxFollowingWorkers.ToString(), true);

        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.InitialEmberlings.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.InitialEmberlings.ToString(), true);

        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.EmberlingArrivals.ToString(), true);
        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.EmberlingArrivals.ToString(), true);

        MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.WorkerMerchant, true);
        WorkerStats.Instance.SetInteractionWithWorkersUnlocked();
        VideoTipManager.Instance.PlayControlEmberlingsTip();
    }
}
