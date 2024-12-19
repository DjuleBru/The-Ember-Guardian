using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Transform itemStatDescriptionContainer;
    [SerializeField] private Transform itemStatDescriptionTemplate;
    [SerializeField] private TextMeshProUGUI itemStatTemplateText;
    [SerializeField] private TextMeshProUGUI itemStatTemplateValue;
    [SerializeField] private Material modifiedItemStatMaterial;
    [SerializeField] private Material initialItemStatMaterial;
    [SerializeField] private Color modifiedItemColor;

    [SerializeField] private TextMeshProUGUI itemStatDescriptionText;
    [SerializeField] private TextMeshProUGUI redGemAmountText;
    [SerializeField] private TextMeshProUGUI greenGemAmountText;

    [SerializeField] private GameObject greenGemCostGO;
    [SerializeField] private GameObject redGemCostGO;
    [SerializeField] private TextMeshProUGUI maxLevelText;

    public void SetDescriptionCardText(string itemName, bool constantUnlockDescription, List<string> itemStatDescriptionList, string itemDescription, int greenGem, int redGem, List<string> itemStatList = null, List<bool> itemModifiersBools = null) {

        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        redGemAmountText.text = redGem.ToString();
        greenGemAmountText.text = greenGem.ToString();

        if(greenGem == 0) {
            greenGemCostGO.SetActive(false);
        }

        if(redGem == 0) {
            redGemCostGO.SetActive(false);
        }

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
        greenGemCostGO.SetActive(false);
        redGemCostGO.SetActive(false);
        maxLevelText.gameObject.SetActive(true);
    }

    public void SetDescriptionCardBought() {
        maxLevelText.text = "UNLOCKED";
        greenGemCostGO.SetActive(false);
        redGemCostGO.SetActive(false);
        maxLevelText.gameObject.SetActive(true);
    }

}
