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

    [SerializeField] private int currentBlueprintAmount;
    [SerializeField] private int maxBlueprintAmount;
    private bool locked;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners(); // Supprime les anciens pour éviter les doublons
        button.onClick.AddListener(() => {
            TryAddStructureBlueprint();
        });

        structureIconImage.sprite = linkedStructureSO.structureSprite;
    }

    protected override void Start() {
        base.Start();
        LoadStructureUnlocked();

        structureNameText.text = LocalizationManager.Instance.GetLocalizedText(linkedStructureSO.structureNameLocalizationKey);

        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
        CampEditManager.Instance.OnStructureAdded += CampEditManager_OnStructureAdded;
        CampEditManager.Instance.OnStructureRemovedAnySituation += CampEditManager_OnStructureRemovedAnySituation;
        CampEditManager.Instance.OnLayoutResetToDefault += CampEditManager_OnLayoutResetToDefault;
        GameInput.Instance.OnEditCampDeselect += GameInput_OnEditCampDeselect;
        RefreshStructureAmounts();
    }

    private void GameInput_OnEditCampDeselect(object sender, EventArgs e) {
        if (locked) return;
        if(buttonHovered || buttonSelected) {
            CampEditManager.Instance.TryRemoveBlueprintFromCamp(linkedStructureSO);
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
        if(gemMerchantItem != null) {
            if (gemMerchantItem.GetStructureType() == linkedStructureSO.structureType) {
                SetStructureUnlocked();
            }
        }
    }

    private void LoadStructureUnlocked() {
        string saveString = linkedStructureSO.structureType.ToString() + (1);

        if (!linkedStructureSO.level1StructureInitiallyUnlocked && !MetaProgressionManager.Instance.GetMerchantItemBought(saveString)) {
            locked = true;
            //button.enabled = false;
            //button.interactable = false;
            structureIconImage.sprite = lockedStructureSprite;
            plusIcon.GetComponent<Image>().enabled = false;
            maxStructureAmountGameObject.SetActive(false);
            structureNameText.gameObject.SetActive(false);
        }
    }

    private void SetStructureUnlocked() {
        locked = false;
        //button.enabled = true;
        //button.interactable = true;
        structureIconImage.sprite = linkedStructureSO.structureSprite;
        plusIcon.GetComponent<Image>().enabled = true;
        maxStructureAmountGameObject.SetActive(true);
        structureNameText.gameObject.SetActive(true);
        RefreshStructureAmounts();
    }

    private void TryAddStructureBlueprint() {
        if (locked) return;
        if (currentBlueprintAmount >= maxBlueprintAmount) {

            return;
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
        maxBlueprintAmount = linkedStructureSO.maxStructureBlueprintAmount;

        if(currentBlueprintAmount < maxBlueprintAmount) {
            plusIcon.gameObject.SetActive(true);
            maxedOutStructureBlueprintText.gameObject.SetActive(false);
        } else {
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

    protected override void OnDestroy() {
        base.OnDestroy();
        HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
    }
}
