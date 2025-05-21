using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddStructureBlueprint : MonoBehaviour
{
    [SerializeField] private StructureSO linkedStructureSO;

    [SerializeField] private Image structureIconImage;
    [SerializeField] private TextMeshProUGUI maxStructureBlueprintText;
    [SerializeField] private TextMeshProUGUI currentStructureBlueprintText;
    [SerializeField] private TextMeshProUGUI maxedOutStructureBlueprintText;
    [SerializeField] private GameObject plusIcon;

    [SerializeField] private int currentBlueprintAmount;
    [SerializeField] private int maxBlueprintAmount;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners(); // Supprime les anciens pour éviter les doublons
        button.onClick.AddListener(() => {
            TryAddStructureBlueprint();
        });

        structureIconImage.sprite = linkedStructureSO.structureSprite;
    }

    private void Start() {
        CampEditManager.Instance.OnStructureAdded += CampEditManager_OnStructureAdded;
        CampEditManager.Instance.OnStructureRemoved += CampEditManager_OnStructureRemoved;
        RefreshStructureAmounts();
    }

    private void TryAddStructureBlueprint() {
        if (currentBlueprintAmount >= maxBlueprintAmount) {

            return;
        }

        CampEditManager.Instance.AddStructure(linkedStructureSO);
    }

    private void CampEditManager_OnStructureRemoved(object sender, System.EventArgs e) {
        RefreshStructureAmounts();
    }

    private void CampEditManager_OnStructureAdded(object sender, System.EventArgs e) {
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
    }

}
