using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : Structure { 

    public enum MerchantType {
        Skills,
        Guns,
        Seeds
    }

    [SerializeField] protected MerchantType merchantType;

    // Listes d'objets disponibles à la vente pour chaque type de marchand
    protected List<MerchantItem> allItemsForSale = new List<MerchantItem>();
    protected List<MerchantItem> allMajorMerchantItems = new List<MerchantItem>();
    protected List<MerchantItem> allMinorMerchantItems = new List<MerchantItem>();

    protected List<MerchantItem> majorItemListForSale = new List<MerchantItem>();
    protected List<MerchantItem> minorItemListForSale = new List<MerchantItem>();

    protected MerchantItem currentHoveredItem;

    public event EventHandler OnPlayerOpenedMerchantShop;
    public event EventHandler OnPlayerClosedMerchantShop;
    public event EventHandler<OnPlayerBoughtItemEventArgs> OnPlayerBoughtItem;

    public class OnPlayerBoughtItemEventArgs : EventArgs {
        public MerchantItem boughtItem;
    }

    protected bool shopOpened;
    protected bool currentSelectedItemAlreadyPurchased;
    protected bool playerJustTriggeredInteraction;
    protected bool playerPayedToRefreshShop;
    protected int smallItemsToDisplayAmount = 2;
    protected int bigItemsToDisplayAmount = 1;

    protected override void Start() {
        base.Start();
        InitializeMerchantItems();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.R)) {
            RefreshShopItems();
            ActivateStructurePrimaryFunctionInteraction(true);
            playerPayedToRefreshShop = false;
        }
    }

    protected override void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);

        RefreshShopItems();
        ActivateStructurePrimaryFunctionInteraction(true);
        playerPayedToRefreshShop = false;
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        Player.Instance.SetInMerchantTriggerArea(false);
    }

    protected override void TriggerStructurePrimaryFunction() {

        if(!playerPayedToRefreshShop) {
            base.TriggerStructurePrimaryFunction();
            ActivateStructurePrimaryFunctionInteraction(false);

            playerPayedToRefreshShop = true;
            OpenCloseShop(true);

        } else {

            OnPlayerBoughtItem?.Invoke(this, new OnPlayerBoughtItemEventArgs {
                boughtItem = currentHoveredItem
            });
        }

        playerJustTriggeredInteraction = true;
    }

    protected virtual void RefreshShopItems() {
    }

    protected virtual void InitializeMerchantItems() {
    }

    protected override void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        playerJustTriggeredInteraction = false;
    }

    protected override void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        if(shopOpened) {
            // Player is trying to buy an item
            if (currentSelectedItemAlreadyPurchased) return;
        }

        playerInteracting = true;
        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerPayedToRefreshShop) return;
        if (!playerInTriggerArea) return;
        if (!playerInteracting) return;
        if (playerJustTriggeredInteraction) return;

        if(payCurrencyUI.GetPlayerInteracting()) {
            payCurrencyUI.SetPlayerInteracting(false);
        } else {
            shopOpened = !shopOpened;
            OpenCloseShop(shopOpened);
        }
    }

    protected void OpenCloseShop(bool shopOpened) {
        if (!shopOpened) {

            // Stop interacting

            this.shopOpened = false;
            Player.Instance.StopInteractingWithMerchant();
            CameraManager.Instance.ZoomOut(true, .5f);
            OnPlayerClosedMerchantShop?.Invoke(this, EventArgs.Empty);
            return;

        }
        else {

            // Start interacting

            this.shopOpened = true;
            CameraManager.Instance.ZoomIn(false, 1.2f, .5f);
            Player.Instance.StartInteractingWithMerchant();
            OnPlayerOpenedMerchantShop?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual void SetItemSold(MerchantItem merchantItemBought) {
       
    }

    public bool GetShopOpen() {
        return shopOpened;
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

    public void SetCurrentSelectedItemPurchased(bool bought) {
        currentSelectedItemAlreadyPurchased = bought;
    }

    public void SetCurrentHoveredItem(MerchantItem merchantItem) {
        currentHoveredItem = merchantItem;
    }

}
