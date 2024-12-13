using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemStatDescriptionText;
    [SerializeField] private TextMeshProUGUI redGemAmountText;
    [SerializeField] private TextMeshProUGUI greenGemAmountText;
    [SerializeField] private GameObject greenGemCostGO;
    [SerializeField] private GameObject redGemCostGO;

    public void SetDescriptionCardText(string itemName, string itemStatDescription, string itemDescription, int greenGem, int redGem) {
        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        itemStatDescriptionText.text = itemStatDescription;
        redGemAmountText.text = redGem.ToString();
        greenGemAmountText.text = greenGem.ToString();

        if(greenGem == 0) {
            greenGemCostGO.SetActive(false);
        }

        if(redGem == 0) {
            redGemCostGO.SetActive(false);
        }
    }
}
