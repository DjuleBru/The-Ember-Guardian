using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : Structure { 

    public enum MerchantType {
        Skills,
        Guns,
        Seeds,
        Traps,
    }

    [SerializeField] protected MerchantType merchantType; 
    [SerializeField] protected bool useDebugItemAmountToDisplay;
    [SerializeField] protected int debugBigItemToDisplay;
    [SerializeField] protected int debugSmallItemToDisplay;

    // Listes d'objets disponibles à la vente pour chaque type de marchand
    protected List<MerchantItem> allItemsForSale = new List<MerchantItem>();
    protected List<MerchantItem> allMajorMerchantItems = new List<MerchantItem>();
    protected List<MerchantItem> allMinorMerchantItems = new List<MerchantItem>();

    protected List<MerchantItem> majorItemListForSale = new List<MerchantItem>();
    protected List<MerchantItem> minorItemListForSale = new List<MerchantItem>();

    protected MerchantItem currentHoveredItem;

    public event EventHandler OnPlayerOpenedMerchantShop;
    public event EventHandler OnPlayerClosedMerchantShop;
    public static event EventHandler OnAnyPlayerClosedMerchantShop;
    public static event EventHandler OnAnyPlayerOpenedMerchantShop;
    public event EventHandler<OnPlayerBoughtItemEventArgs> OnPlayerBoughtItem;

    public class OnPlayerBoughtItemEventArgs : EventArgs {
        public MerchantItem boughtItem;
    }

    protected bool shopOpened;
    protected bool playerHoldingDownInteract;
    protected bool currentSelectedItemAlreadyPurchased;
    protected bool playerJustTriggeredInteraction;
    protected bool playerPayedToRefreshShop;
    protected bool refreshAfterPlayerClosesShop;
    [SerializeField] protected int smallItemsToDisplayAmount = 2;
    [SerializeField] protected int bigItemsToDisplayAmount = 1;

    protected override void Start() {
        base.Start();

        InitializeMerchantItems();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerRollPerformed += GameInput_OnPlayerRollPerformed;
        GameInput.Instance.OnPlayerRunPerformed += GameInput_OnPlayerRunPerformed;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
    }


    private void Update() {
        //if (Input.GetKeyUp(KeyCode.R)) {
        //    RefreshShopItems();
        //    ActivateStructurePrimaryFunctionInteraction(true);
        //    playerPayedToRefreshShop = false;
        //}
    }

    protected override void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);
        refreshAfterPlayerClosesShop = true;

        if (!shopOpened) {
            RefreshShopItems();
        }

    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if(collision.GetComponent<Player>() != null) {
            Player.Instance.SetInMerchantTriggerArea(true);
        }
    }
    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D (collision);
        if (collision.GetComponent<Player>() != null) {
            Player.Instance.SetInMerchantTriggerArea(false);
        }
    }

    protected override void TriggerStructurePrimaryFunction() {

        if (!playerPayedToRefreshShop) {
            base.TriggerStructurePrimaryFunction();
            ActivateStructurePrimaryFunctionInteraction(false);

            playerPayedToRefreshShop = true;
            OpenCloseShop(true);

        } else {

            if (currentHoveredItem.itemType == MerchantItem.MerchantItemType.RefreshShopItems) {
                playerPayedToRefreshShop = false;
                RefreshShopItems();
                TriggerStructurePrimaryFunction();
                return; // pas besoin de continuer le traitement normal
            }

            OnPlayerBoughtItem?.Invoke(this, new OnPlayerBoughtItemEventArgs {
                boughtItem = currentHoveredItem
            });
        }

        playerJustTriggeredInteraction = true;
    }

    public void InvokeOnPlayerBoughtItem() {
        OnPlayerBoughtItem?.Invoke(this, new OnPlayerBoughtItemEventArgs {
            boughtItem = currentHoveredItem
        });
    }

    protected virtual void RefreshShopItems() {
        refreshAfterPlayerClosesShop = false;
        ActivateStructurePrimaryFunctionInteraction(true);
        playerPayedToRefreshShop = false;
    }

    protected virtual void InitializeMerchantItems() {
    }

    protected override void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
        playerJustTriggeredInteraction = false;
        playerHoldingDownInteract = false;
    }

    protected override void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        if(shopOpened) {
            // Player is trying to buy an item
            if (currentSelectedItemAlreadyPurchased) return;
        }

        playerInteracting = true;
        playerHoldingDownInteract = true;
        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!playerInteracting) return;
        if (playerJustTriggeredInteraction) return;
        if (!playerPayedToRefreshShop) return;

        if(playerHoldingDownInteract) {
            payCurrencyUI.SetPlayerInteracting(false);
        } else {
            shopOpened = !shopOpened;
            OpenCloseShop(shopOpened);
        }

    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, EventArgs e) {
        if (shopOpened) {
            shopOpened = false;
            OpenCloseShop(shopOpened);
        };
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (shopOpened) {
            shopOpened = false;
            OpenCloseShop(shopOpened);
        };

    }
    private void GameInput_OnPlayerRunPerformed(object sender, EventArgs e) {
        if (shopOpened) {
            shopOpened = false;
            OpenCloseShop(shopOpened);
        };
    }

    private void GameInput_OnPlayerRollPerformed(object sender, EventArgs e) {
        if (shopOpened) {
            shopOpened = false;
            OpenCloseShop(shopOpened);
        };
    }

    protected void OpenCloseShop(bool shopOpened) {
        if (!shopOpened) {

            // Stop interacting

            this.shopOpened = false;
            Player.Instance.StopInteractingWithMerchant();
            CameraManager.Instance.ZoomOut(true, .5f);

            if (refreshAfterPlayerClosesShop) {
                RefreshShopItems();
            }

            OnPlayerClosedMerchantShop?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerClosedMerchantShop?.Invoke(this, EventArgs.Empty);
            return;

        }
        else {

            // Start interacting

            this.shopOpened = true;
            CameraManager.Instance.ZoomIn(false, 1.2f, .5f);
            Player.Instance.StartInteractingWithMerchant();
            OnPlayerOpenedMerchantShop?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerOpenedMerchantShop?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual void SetItemSold(MerchantItem merchantItemBought) {
       
    }

    public bool GetShopOpen() {
        return shopOpened;
    }
    public bool GetPlayerPaidToRefreshShop() {
        return playerPayedToRefreshShop;
    }


    public List<MerchantItem> GetMajorItemListForSale() {
        return majorItemListForSale;
    }

    public List<MerchantItem> GetMinorItemListForSale() {
        return minorItemListForSale;
    }

    public List<MerchantItem> GetAllCurrentItemsForSale() {
        List<MerchantItem> allCurrentItemsForSale = new List<MerchantItem>();
        foreach(MerchantItem merchantItem in majorItemListForSale) {
            allCurrentItemsForSale.Add(merchantItem);
        }
        foreach (MerchantItem merchantItem in minorItemListForSale) {
            allCurrentItemsForSale.Add(merchantItem);
        }
        return allCurrentItemsForSale;
    }

    public List<MerchantItem> GetAllItemsForSale() {
        return allItemsForSale;
    }

    public void SetCurrentSelectedItemCanBeBought(bool bought) {
        currentSelectedItemAlreadyPurchased = bought;
    }

    public void SetCurrentHoveredItem(MerchantItem merchantItem) {
        currentHoveredItem = merchantItem;
    }
}
