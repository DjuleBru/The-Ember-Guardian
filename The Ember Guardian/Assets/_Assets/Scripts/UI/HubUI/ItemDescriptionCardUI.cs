using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemDescriptionCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI redGemAmountText;
    [SerializeField] private TextMeshProUGUI greenGemAmountText;

    public void SetDescriptionCardText(string itemName, string itemDescription, int greenGem, int redGem) {
        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        redGemAmountText.text = redGem.ToString();
        greenGemAmountText.text = greenGem.ToString();
    }
}
