using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowCollider : MonoBehaviour
{
    [SerializeField] private bool isControlEmberlingsCollider;

    private bool playerCollided;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (playerCollided) return;

        //if(isControlEmberlingsCollider) {
        //    MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.ControlEmberlings.ToString(), true);
        //    MetaProgressionManager.Instance.SetHubMerchantItemBought(HubMerchantItem_WatcherMerchantItem.WatcherItemType.ControlEmberlings.ToString(), true);
        //    MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.ControlEmberlings.ToString(), true);

        //    MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.MaxFollowingWorkers.ToString(), true);
        //    MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.MaxFollowingWorkers.ToString(), true);

        //    MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.InitialEmberlings.ToString(), true);
        //    MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.InitialEmberlings.ToString(), true);

        //    MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.EmberlingArrivals.ToString(), true);
        //    MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HubMerchantItem_WatcherMerchantItem.WatcherItemType.EmberlingArrivals.ToString(), true);

        //    MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.WorkerMerchant, true);
        //    WorkerStats.Instance.SetInteractionWithWorkersUnlocked();
        //    playerCollided = true;
        //}
    }
}
