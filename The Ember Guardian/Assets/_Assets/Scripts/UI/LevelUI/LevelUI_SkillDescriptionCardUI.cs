using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI_SkillDescriptionCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Transform itemStatDescriptionContainer;
    [SerializeField] private Transform itemStatDescriptionTemplate;

    [SerializeField] private TextMeshProUGUI itemStatTemplateText;
    [SerializeField] private TextMeshProUGUI itemStatTemplateValue;

    public void SetDescriptionCardText(string itemName, List<string> itemStatDescriptionList, List<string> itemStatList, string itemDescription) {

        itemNameText.text = LocalizationManager.Instance.GetLocalizedText(itemName);
        itemDescriptionText.text = LocalizationManager.Instance.GetLocalizedText(itemName + "_Description");


        itemStatDescriptionTemplate.gameObject.SetActive(true);
        RefreshItemStatDescription(itemStatDescriptionList, itemStatList);
    }

    private void RefreshItemStatDescription(List<string> itemStatDescriptionList, List<string> itemStatList) {
        foreach (Transform child in itemStatDescriptionContainer) {
            if (child == itemStatDescriptionTemplate) continue;
            Destroy(child.gameObject);
        }

        itemStatDescriptionTemplate.gameObject.SetActive(true);

        int i = 0;
        foreach (string itemStatName in itemStatDescriptionList) {


            itemStatTemplateText.text = itemStatName;
            itemStatTemplateValue.text = itemStatList[i];

            RectTransform template = Instantiate(itemStatDescriptionTemplate, itemStatDescriptionContainer).GetComponent<RectTransform>();

            if (itemStatList[i] == "") {
                template.Find("StatText").GetComponent<ContentSizeFitter>().enabled = true;
                template.Find("StatText").GetComponent<RectTransform>().sizeDelta = new Vector2(250f, itemStatTemplateText.GetComponent<RectTransform>().sizeDelta.y);
            }
            else {
                template.Find("StatText").GetComponent<ContentSizeFitter>().enabled = false;
            }
            i++;
        }

        itemStatDescriptionTemplate.gameObject.SetActive(false);
    }
}
