using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Transform itemStatDescriptionContainer;
    [SerializeField] private Transform itemStatDescriptionTemplate;
    [SerializeField] private AdjustContentSizeFitterParentSize itemStatDescriptionAdjustSize;
    [SerializeField] private TextMeshProUGUI itemStatTemplateText;
    [SerializeField] private TextMeshProUGUI itemStatTemplateValue;
    [SerializeField] private Material modifiedItemStatMaterial;
    [SerializeField] private Material initialItemStatMaterial;
    [SerializeField] private Material redFontMaterial;
    [SerializeField] private Color modifiedItemColor;

    [SerializeField] private TextMeshProUGUI itemStatDescriptionText;
    [SerializeField] private TextMeshProUGUI redGemAmountText;
    [SerializeField] private TextMeshProUGUI greenGemAmountText;
    [SerializeField] private TextMeshProUGUI blueGemAmountText;
    [SerializeField] private TextMeshProUGUI yellowGemAmountText;
    [SerializeField] private TextMeshProUGUI purpleGemAmountText;

    [SerializeField] private GameObject greenGemCostGO;
    [SerializeField] private GameObject redGemCostGO;
    [SerializeField] private GameObject blueGemCostGO;
    [SerializeField] private GameObject yellowGemCostGO;
    [SerializeField] private GameObject purpleGemCostGO;
    [SerializeField] private TextMeshProUGUI maxLevelText;
    [SerializeField] private GameObject foreGround;

    public void SetDescriptionCardText(string itemName, bool constantUnlockDescription, List<string> itemStatDescriptionList, string itemDescription, List<string> itemStatList = null, List<bool> itemModifiersBools = null) {

        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;

        if(constantUnlockDescription) {

            itemStatDescriptionTemplate.gameObject.SetActive(false);
            itemStatDescriptionText.gameObject.SetActive(true);
            itemStatDescriptionText.text = itemStatDescriptionList[0];

        } else {
            itemStatDescriptionTemplate.gameObject.SetActive(true);
            itemStatDescriptionText.gameObject.SetActive(false);
            RefreshItemStatDescription(itemStatDescriptionList, itemStatList, itemModifiersBools);
        }

        maxLevelText.gameObject.SetActive(false);
    }

    public void SetDescriptionCardCost(int greenGem, int redGem, int blueGem, int yellowGem, int purpleGem) {

        redGemAmountText.text = redGem.ToString();
        greenGemAmountText.text = greenGem.ToString();
        blueGemAmountText.text = blueGem.ToString();
        yellowGemAmountText.text = yellowGem.ToString();
        purpleGemAmountText.text = purpleGem.ToString();

        if (greenGem == 0) {
            greenGemCostGO.SetActive(false);
        }

        if (redGem == 0) {
            redGemCostGO.SetActive(false);
        }

        if (blueGem == 0) {
            blueGemCostGO.SetActive(false);
        }

        if (yellowGem == 0) {
            yellowGemCostGO.SetActive(false);
        }

        if (purpleGem == 0) {
            purpleGemCostGO.SetActive(false);
        }
    }

    private void RefreshItemStatDescription(List<string> itemStatDescriptionList, List<string> itemStatList, List<bool> itemStatModifiersBools) {

        foreach(Transform child in itemStatDescriptionContainer) {
            if (child == itemStatDescriptionTemplate) continue;
            Destroy(child.gameObject);
        }

        itemStatDescriptionTemplate.gameObject.SetActive(true);

        int i = 0;
        foreach(string itemStatName in itemStatDescriptionList) {
            itemStatTemplateText.text = itemStatName;
            itemStatTemplateValue.text = itemStatList[i];

            if (itemStatModifiersBools[i] == true) {
                itemStatTemplateValue.fontMaterial = modifiedItemStatMaterial;
                itemStatTemplateValue.color = modifiedItemColor;
            } else {
                itemStatTemplateValue.fontMaterial = initialItemStatMaterial;
                itemStatTemplateValue.color = Color.white;
            }

            Instantiate(itemStatDescriptionTemplate, itemStatDescriptionContainer);
            i++;
        }

        itemStatDescriptionTemplate.gameObject.SetActive(false);
    }

    public void SetDescriptionCardMaxlevel() {
        maxLevelText.text = "MAX LEVEL"; 
        DisableGemCostGO();

        maxLevelText.gameObject.SetActive(true);
    }
    public void SetDescriptionCardItemLockedInDemo() {
        maxLevelText.text = "LOCKED IN DEMO";
        maxLevelText.fontMaterial = redFontMaterial;
        maxLevelText.fontSize = 40;

        DisableGemCostGO();

        maxLevelText.gameObject.SetActive(true);
        foreGround.SetActive(true);
    }

    public void SetDescriptionCardBought() {
        maxLevelText.text = "UNLOCKED";
        DisableGemCostGO();
        maxLevelText.gameObject.SetActive(true);
    }

    private void DisableGemCostGO() {
        greenGemCostGO.SetActive(false);
        redGemCostGO.SetActive(false);
        blueGemCostGO.SetActive(false);
        yellowGemCostGO.SetActive(false);
        purpleGemCostGO.SetActive(false);

    }

}
