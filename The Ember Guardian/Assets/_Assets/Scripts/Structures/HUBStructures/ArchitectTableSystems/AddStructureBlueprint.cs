using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddStructureBlueprint : ButtonUI
{
    [SerializeField] private StructureSO linkedStructureSO;

    [SerializeField] private Image structureIconImage;
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material lockedStructureIconMaterial;
    [SerializeField] private Sprite lockedStructureSprite;
    [SerializeField] private TextMeshProUGUI structureNameText;
    [SerializeField] private GameObject maxStructureAmountGameObject;
    [SerializeField] private GameObject addStructureInstructionGameObject;
    [SerializeField] private GameObject removeStructureInstructionGameObject;
    [SerializeField] private GameObject backgroundImageGameObject;
    [SerializeField] private TextMeshProUGUI maxStructureBlueprintText;
    [SerializeField] private TextMeshProUGUI currentStructureBlueprintText;
    [SerializeField] private TextMeshProUGUI maxedOutStructureBlueprintText;
    [SerializeField] private GameObject plusIcon;
    [SerializeField] private GameObject newItemGO;

    [SerializeField] private int currentBlueprintAmount;
    [SerializeField] private int maxBlueprintAmount;
    [SerializeField] private int maxBlueprintAmount_HordeMode;

    [SerializeField] private TextMeshProUGUI gemBudgetText;
    [SerializeField] private GameObject gemBudgetGO;
    [SerializeField] private GameObject structureAmountGameObject;

    private bool locked = true;
    private bool newItem;
    private bool hordeMode;
    private Button button;

    public static event EventHandler OnAnyStructureBlueprintFailedAddedMaxAmount;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners(); // Supprime les anciens pour éviter les doublons
        button.onClick.AddListener(() => {
            TryAddStructureBlueprint();
        });

        structureIconImage.sprite = linkedStructureSO.structureSprite;

        if(!newItem) {
            newItemGO.SetActive(false);
        }

    }

    protected override void Start() {
        base.Start();

        hordeMode = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;
        maxedOutStructureBlueprintText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
        maxBlueprintAmount_HordeMode = linkedStructureSO.maxStructureBlueprintAmount_HordeMode;

        maxedOutStructureBlueprintText.text = LocalizationManager.Instance.GetLocalizedText("menu_MAX");

        if (!hordeMode) {
            structureAmountGameObject.SetActive(true);
            gemBudgetGO.SetActive(false);
        } else {
            structureAmountGameObject.SetActive(false);
            gemBudgetGO.SetActive(true);
            gemBudgetText.text = linkedStructureSO.hordeModeGemBudget.ToString();
        }

        if (locked) {

            if(hordeMode) {
                RefreshStructureUnlocked_HordeMode();
                HordeModeProgressionManager.Instance.OnHordeModeUnlockableUnlocked += HordeModePrMa_OnHordeModeUnlockableUnlocked;
            } else {
                LoadStructureUnlocked();
            }

        }

        var localizedResult = LocalizationManager.Instance.GetLocalized(linkedStructureSO.structureNameLocalizationKey);
        structureNameText.text = localizedResult.text;
        if (localizedResult.font != null) {
            structureNameText.font = localizedResult.font;
        }

        CampEditManager.Instance.OnStructureAdded += CampEditManager_OnStructureAdded;
        CampEditManager.Instance.OnStructureRemovedAnySituation += CampEditManager_OnStructureRemovedAnySituation;
        CampEditManager.Instance.OnLayoutResetToDefault += CampEditManager_OnLayoutResetToDefault;
        GameInput.Instance.OnEditCampDeselect += GameInput_OnEditCampDeselect;
        RefreshStructureAmounts();
    }

    private void HordeModePrMa_OnHordeModeUnlockableUnlocked(object sender, EventArgs e) {
        RefreshStructureUnlocked_HordeMode();
    }

    public void SubscribeToNewItemsEvents() {

        HubMerchantItem.OnAnyHubMerchantItemArchitectTableUnlocks += HubMerchantItem_OnAnyHubMerchantItemArchitectTableUnlocks;
        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;

    }

    private void GameInput_OnEditCampDeselect(object sender, EventArgs e) {
        if (locked) return;
        if(buttonHovered || buttonSelected) {
            CampEditManager.Instance.TryRemoveBlueprintFromCamp(linkedStructureSO);
        }
    }

    private void HubMerchantItem_OnAnyHubMerchantItemArchitectTableUnlocks(object sender, EventArgs e) {
        HubMerchantItem merchantItem = sender as HubMerchantItem;

        HubMerchantItem_WatcherMerchantItem watcherMerchantItem = merchantItem as HubMerchantItem_WatcherMerchantItem;
        if (watcherMerchantItem != null) {
            if (watcherMerchantItem.GetStructureType() == linkedStructureSO.structureType) {
                SetStructureUnlocked();
            }
        }
    }

    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, EventArgs e) {
        HubMerchantItem merchantItem = sender as HubMerchantItem;

        HubMerchantItem_ArchitectMerchantItem architectMerchantItem = merchantItem as HubMerchantItem_ArchitectMerchantItem;
        if (architectMerchantItem != null) {
            if(architectMerchantItem.GetArchitectItemCategory() == HubMerchantItem_ArchitectMerchantItem.ArchitectItemCategory.architectTableUpgrades) {
                RefreshStructureAmounts();
            }
        }

        HubMerchantItem_GemMerchantItem gemMerchantItem = merchantItem as HubMerchantItem_GemMerchantItem;
        if (gemMerchantItem != null) {
            if (gemMerchantItem.GetStructureType() == linkedStructureSO.structureType && gemMerchantItem.GetItemLevel() == 1) {
                Debug.Log("BOUGH GEM ITEM " + linkedStructureSO.structureType + " " + gemMerchantItem.GetItemLevel());
                SetStructureUnlocked();
            }
        }

        HubMerchantItem_WatcherMerchantItem watcherItem = merchantItem as HubMerchantItem_WatcherMerchantItem;
        if (watcherItem != null) {
            if (watcherItem.GetWatcherItemCategory() == HubMerchantItem_WatcherMerchantItem.WatcherItemCategory.newShrine) {
                // ONLY UNLOCKS ORB CONTAINERS
                if(watcherItem.GetStructureType() == linkedStructureSO.structureType) {
                    Debug.Log("BOUGH WATCHER ITEM " + linkedStructureSO.structureType + " " + watcherItem.GetItemLevel());
                    SetStructureUnlocked();
                }
                
            }
        }

    }

    private void LoadStructureUnlocked() {
        string saveString = linkedStructureSO.structureType.ToString() + (1);
        bool itemIsLocked = !linkedStructureSO.level1StructureInitiallyUnlocked && !MetaProgressionManager.Instance.GetMerchantItemBought(saveString);
        
        if(CampEditManager.Instance.GetStructureTypeUnlockedThisSession(linkedStructureSO.structureType)) {
            itemIsLocked = false;
        }

        if (DebugManager.Instance.GetAllStructuresUnlocked()) {
            itemIsLocked = false;
        }

        if (itemIsLocked) {
            locked = true;
            structureIconImage.sprite = lockedStructureSprite;
            plusIcon.GetComponent<Image>().enabled = false;
            maxStructureAmountGameObject.SetActive(false);
            structureNameText.gameObject.SetActive(false);
            gemBudgetGO.SetActive(false);

        } else {
            locked = false;
        }
    }
    private void RefreshStructureUnlocked_HordeMode() {
        bool hordeModeUnlocked = HordeModeProgressionManager.Instance.GetStructureUnlocked(linkedStructureSO.structureType);
        bool itemIsLocked = !linkedStructureSO.level1StructureInitiallyUnlocked && !hordeModeUnlocked;

        if (itemIsLocked) {
            locked = true;
            structureIconImage.sprite = lockedStructureSprite;
            plusIcon.GetComponent<Image>().enabled = false;
            maxStructureAmountGameObject.SetActive(false);
            structureNameText.gameObject.SetActive(false);
            gemBudgetGO.SetActive(false);
        }
        else {
            locked = false;
            structureIconImage.sprite = linkedStructureSO.structureSprite;
            plusIcon.GetComponent<Image>().enabled = true;
            maxStructureAmountGameObject.SetActive(true);
            structureNameText.gameObject.SetActive(true);
            gemBudgetGO.SetActive(true);
        }
    }


    private void SetStructureUnlocked() {
        if (this == null) return;
        //Debug.Log(this + " SetStructureUnlocked " + linkedStructureSO);
        //Debug.Log(this + " plusIcon " + plusIcon);

        locked = false;
        structureIconImage.sprite = linkedStructureSO.structureSprite;
        plusIcon.GetComponent<Image>().enabled = true;
        maxStructureAmountGameObject.SetActive(true);
        structureNameText.gameObject.SetActive(true);
        RefreshStructureAmounts();
        newItemGO.SetActive(true);
        newItem = true;
        ArchitectTable.Instance.GetArchitectTableHubMerchant().SetMerchantHasNewItems();

    }

    private void TryAddStructureBlueprint() {
        if (locked) return;

        if(hordeMode) {
            if (!ArchitectTable_MainMenu.Instance.GetHasEnoughBudget(linkedStructureSO.hordeModeGemBudget)) {
                maxedOutStructureBlueprintText.text = LocalizationManager.Instance.GetLocalizedText("menu_notEnoughBudget");
                OnAnyStructureBlueprintFailedAddedMaxAmount?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (currentBlueprintAmount >= maxBlueprintAmount_HordeMode) {
                maxedOutStructureBlueprintText.text = LocalizationManager.Instance.GetLocalizedText("menu_MAX");
                OnAnyStructureBlueprintFailedAddedMaxAmount?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
        else {
            if (currentBlueprintAmount >= maxBlueprintAmount) {
                maxedOutStructureBlueprintText.text = LocalizationManager.Instance.GetLocalizedText("menu_MAX");
                OnAnyStructureBlueprintFailedAddedMaxAmount?.Invoke(this, EventArgs.Empty);
                return;
            }
        }


        CampEditManager.Instance.AddStructure(linkedStructureSO);
    }

    private void CampEditManager_OnStructureRemovedAnySituation(object sender, System.EventArgs e) {
        RefreshStructureAmounts();

    }

    private void CampEditManager_OnStructureAdded(object sender, System.EventArgs e) {
        RefreshStructureAmounts();
    }

    private void CampEditManager_OnLayoutResetToDefault(object sender, EventArgs e) {
        RefreshStructureAmounts();
    }

    private void RefreshStructureAmounts() {
        currentBlueprintAmount = CampEditManager.Instance.GetPlacedStructureBlueprintAmountOfType(linkedStructureSO);

        if (hordeMode) {

            if (ArchitectTable.Instance.GetHasEnoughBudget(linkedStructureSO.hordeModeGemBudget) && currentBlueprintAmount < maxBlueprintAmount_HordeMode) {
                plusIcon.gameObject.SetActive(true);
                maxedOutStructureBlueprintText.gameObject.SetActive(false);
            } else {
                plusIcon.gameObject.SetActive(false);
                maxedOutStructureBlueprintText.gameObject.SetActive(true);
            }

        } else {
            maxBlueprintAmount = linkedStructureSO.maxStructureBlueprintAmount;

            if (currentBlueprintAmount < maxBlueprintAmount) {
                plusIcon.gameObject.SetActive(true);
                maxedOutStructureBlueprintText.gameObject.SetActive(false);
            }
            else {
                plusIcon.gameObject.SetActive(false);
                maxedOutStructureBlueprintText.gameObject.SetActive(true);
            }

            switch (linkedStructureSO.structureType) {

                case StructureSO.StructureType.ammoCrafter:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMaxAmmoCrafterAmount();
                    break;

                case StructureSO.StructureType.bearTrap:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMaxTrapSlotsAmount();
                    break;

                case StructureSO.StructureType.secondaryFire:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMaxSecondaryFireAmount();
                    break;

                case StructureSO.StructureType.tower:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMaxTowerAmount();
                    break;

                case StructureSO.StructureType.fastTravelTeleporter:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMaxFastTravelTPAmount();
                    break;

                case StructureSO.StructureType.sniperTower:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMaxSniperTowerAmount();
                    break;

                case StructureSO.StructureType.mortarTower:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMortarPositionsAmount();
                    break;

                case StructureSO.StructureType.machineGunTower:
                    maxBlueprintAmount = ArchitectTable.Instance.GetMachineGunTowerAmount();
                    break;
            }
            RefreshStructureAmountTexts();
        }
        

    }

    private void RefreshStructureAmountTexts() {
        currentStructureBlueprintText.text = currentBlueprintAmount.ToString();
        maxStructureBlueprintText.text = "/" + maxBlueprintAmount.ToString();

        if(currentBlueprintAmount == maxBlueprintAmount) {
            addStructureInstructionGameObject.SetActive(false);
        } else {
            addStructureInstructionGameObject.SetActive(true);
        }

        if(currentBlueprintAmount == 0) {
            removeStructureInstructionGameObject.SetActive(false);
        }
        else {
            removeStructureInstructionGameObject.SetActive(true);
        }
    }

    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        base.ButtonUI_OnAnyButtonSelected(sender, e);

        ButtonUI button = sender as ButtonUI;
        if (button != this) return;

        if (newItem) {
            newItemGO.SetActive(false);
            newItem = false;
        }
    }

    protected override void ButtonUI_OnAnyButtonHovered(object sender, EventArgs e) {
        base.ButtonUI_OnAnyButtonHovered(sender, e);

        ButtonUI button  = sender as ButtonUI;
        if (button != this) return;

        if (newItem) {
            newItemGO.SetActive(false);
            newItem = false;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemArchitectTableUnlocks -= HubMerchantItem_OnAnyHubMerchantItemArchitectTableUnlocks;
    }
}
