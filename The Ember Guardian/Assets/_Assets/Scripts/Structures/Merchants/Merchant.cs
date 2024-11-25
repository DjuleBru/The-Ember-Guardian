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
    [SerializeField] protected List<SkillSO> merchantSkillSOList;
    protected List<MerchantItem> allItemsForSale = new List<MerchantItem>();
    protected List<MerchantItem> allMajorMerchantItems = new List<MerchantItem>();
    protected List<MerchantItem> allMinorMerchantItems = new List<MerchantItem>();

    protected MerchantItem majorItemForSale;
    protected List<MerchantItem> minorItemsForSale = new List<MerchantItem>();

    public event EventHandler OnPlayerOpenedMerchantShop;
    public event EventHandler OnPlayerClosedMerchantShop;
    public event EventHandler OnPlayerBoughtItem;

    protected bool shopOpened;
    protected bool currentSelectedItemAlreadyPurchased;
    protected bool playerJustTriggeredInteraction;
    protected bool playerPayedToRefreshShop;
    protected int smallItemsToDisplayAmount = 3;

    protected override void Start() {
        base.Start();
        InitializeMerchantItems();

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    protected override void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        base.DayNightManager_OnDawnStart(sender, e);

        RefreshCurrentMajorItemForSale();
        RefreshCurrentMinorItemListForSale();
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        Player.Instance.SetCanDropOrbOnTheFloor(false);
    }

    protected override void TriggerStructurePrimaryFunction() {

        if(!playerPayedToRefreshShop) {
            base.TriggerStructurePrimaryFunction();
            ActivateStructurePrimaryFunctionInteraction(false);

            playerPayedToRefreshShop = true;
            OpenCloseShop(true);

        } else {

            OnPlayerBoughtItem?.Invoke(this, EventArgs.Empty);
        }

        playerJustTriggeredInteraction = true;
    }

    protected void InitializeMerchantItems() {
        allItemsForSale = new List<MerchantItem>();

        switch (merchantType) {
            case MerchantType.Skills:
                InitializeSkillItems();
                break;
        }

        RefreshCurrentMajorItemForSale();
        RefreshCurrentMinorItemListForSale();
    }

    protected void InitializeSkillItems() {
        foreach (SkillSO skillSO in merchantSkillSOList) {
            var skillItem = new SkillItem();
            skillItem.Initialize(skillSO);
            allItemsForSale.Add(skillItem);

            if(skillSO.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                Debug.Log("major " + skillItem.itemName);
                allMajorMerchantItems.Add(skillItem);
            }

            if (skillSO.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                Debug.Log("minor " + skillItem.itemName);
                allMinorMerchantItems.Add(skillItem);
            }
        }
    }

    protected void RefreshCurrentMajorItemForSale() {
        majorItemForSale = allMajorMerchantItems[UnityEngine.Random.Range(0, allMajorMerchantItems.Count)];
    }

    protected void RefreshCurrentMinorItemListForSale() {
        minorItemsForSale = allMinorMerchantItems;
    }

    protected override void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        playerJustTriggeredInteraction = false;
    }

    protected override void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (currentSelectedItemAlreadyPurchased) return;

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

    public bool GetShopOpen() {
        return shopOpened;
    }

    public int GetSmallItemsToDisplayAmount() {
        return smallItemsToDisplayAmount;
    }

    public MerchantItem GetMajorItemForSale() {
        return majorItemForSale;
    }

    public List<MerchantItem> GetMinorItemListForSale() {
        return minorItemsForSale;
    }

    public List<MerchantItem> GetAllItemsForSale() {
        return allItemsForSale;
    }

    public void SetCurrentSelectedItemPurchased(bool bought) {
        currentSelectedItemAlreadyPurchased = bought;
    }

}
