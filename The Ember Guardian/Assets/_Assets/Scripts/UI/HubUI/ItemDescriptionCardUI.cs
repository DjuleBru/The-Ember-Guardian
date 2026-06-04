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
    [SerializeField] private Material modifiedItemStatMaterial_JP;
    [SerializeField] private Material initialItemStatMaterial_JP;
    [SerializeField] private Material redFontMaterial_JP;
    [SerializeField] private Material modifiedItemStatMaterial_CH;
    [SerializeField] private Material initialItemStatMaterial_CH;
    [SerializeField] private Material redFontMaterial_CH;
    [SerializeField] private Color modifiedItemColor;
    [SerializeField] private Color redItemColor;

    [SerializeField] private TextMeshProUGUI itemStatDescriptionText;
    [SerializeField] private TextMeshProUGUI redGemAmountText;
    [SerializeField] private TextMeshProUGUI greenGemAmountText;
    [SerializeField] private TextMeshProUGUI blueGemAmountText;
    [SerializeField] private TextMeshProUGUI yellowGemAmountText;
    [SerializeField] private TextMeshProUGUI purpleGemAmountText;
    [SerializeField] private TextMeshProUGUI cyanGemAmountText;

    [SerializeField] private GameObject greenGemCostGO;
    [SerializeField] private GameObject redGemCostGO;
    [SerializeField] private GameObject blueGemCostGO;
    [SerializeField] private GameObject yellowGemCostGO;
    [SerializeField] private GameObject purpleGemCostGO;
    [SerializeField] private GameObject cyanGemCostGO;
    [SerializeField] private TextMeshProUGUI maxLevelText;
    [SerializeField] private GameObject foreGround;

    private bool gemGODisabled;
    private int greenGem;
    private int redGem;
    private int blueGem;
    private int yellowGem;
    private int purpleGem;
    private int cyanGem;

    public void SetDescriptionCardFonts() {
        TMP_FontAsset fontAsset = LocalizationManager.Instance.GetCurrentFont();

        itemNameText.font = fontAsset;
        itemDescriptionText.font = fontAsset;
        itemStatTemplateText.font = fontAsset;
        itemStatTemplateValue.font = fontAsset;
        itemStatDescriptionText.font = fontAsset;
        redGemAmountText.font = fontAsset;
        greenGemAmountText.font = fontAsset;
        blueGemAmountText.font = fontAsset;
        yellowGemAmountText.font = fontAsset;
        purpleGemAmountText.font = fontAsset;
        cyanGemAmountText.font= fontAsset;

        redGemAmountText.color = Color.white;
        greenGemAmountText.color = Color.white;
        blueGemAmountText.color = Color.white;
        yellowGemAmountText.color = Color.white;
        purpleGemAmountText.color = Color.white;
        cyanGemAmountText.color = Color.white;

        redGemAmountText.color = Color.white;
        greenGemAmountText.color = Color.white;
        blueGemAmountText.color = Color.white;
        yellowGemAmountText.color = Color.white;
        purpleGemAmountText.color = Color.white;
        cyanGemAmountText.color = Color.white;

        maxLevelText.font = fontAsset;
    }

    public void SetDescriptionCardText(string itemName, bool constantUnlockDescription, List<string> itemStatDescriptionList, string itemDescription,List<string> itemStatList = null, List<bool> itemModifiersBools = null) {
        itemNameText.text = LocalizationManager.Instance.GetLocalizedText(itemName);
        itemDescriptionText.text = LocalizationManager.Instance.GetLocalizedText(itemName + "_Description");

        if (constantUnlockDescription) {

            itemStatDescriptionTemplate.gameObject.SetActive(false);
            itemStatDescriptionText.gameObject.SetActive(true);
            itemStatDescriptionText.text = LocalizationManager.Instance.GetLocalizedText(itemName + "_UnlockDescription");

        } else {
            itemStatDescriptionTemplate.gameObject.SetActive(true);
            itemStatDescriptionText.gameObject.SetActive(false);
            RefreshItemStatDescription(itemStatDescriptionList, itemStatList, itemModifiersBools);
        }

        maxLevelText.gameObject.SetActive(false);
    }

    public void SetDescriptionCardCost(int greenGem, int redGem, int blueGem, int yellowGem, int purpleGem, int cyanGem) {
        if (gemGODisabled) return;

        this.greenGem = greenGem;
        this.redGem = redGem;
        this.blueGem = blueGem;
        this.yellowGem = yellowGem;
        this.purpleGem = purpleGem;
        this.cyanGem = cyanGem;

        greenGemCostGO.SetActive(true);
        redGemCostGO.SetActive(true);
        blueGemCostGO.SetActive(true);
        yellowGemCostGO.SetActive(true);
        purpleGemCostGO.SetActive(true);
        cyanGemCostGO.SetActive(true);

        redGemAmountText.text = redGem.ToString();
        greenGemAmountText.text = greenGem.ToString();
        blueGemAmountText.text = blueGem.ToString();
        yellowGemAmountText.text = yellowGem.ToString();
        purpleGemAmountText.text = purpleGem.ToString();
        cyanGemAmountText.text = cyanGem.ToString();

        RefreshDescriptionCardCostColors();

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

        if (cyanGem == 0) {
            cyanGemCostGO.SetActive(false);
        }
       
    }

    public void RefreshDescriptionCardCostColors() {

        //Debug.Log("RefreshDescriptionCardCostColors");

        int playerGreenGems = 0;
        int playerRedGems = 0;
        int playerCyanGems = 0;
        int playerBlueGems = 0;
        int playerYellowGems = 0;
        int playerPurpleGems = 0;

        if (HUBManager.Instance != null) {
            playerGreenGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
            playerRedGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
            playerCyanGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.cyanGem).Count;
            playerBlueGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
            playerYellowGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
            playerPurpleGems = UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;
        }
        else {
            playerGreenGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count;
            playerRedGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count;
            playerCyanGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.cyanGem).Count;
            playerBlueGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count;
            playerYellowGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count;
            playerPurpleGems = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count;
        }


        if (greenGem > playerGreenGems) {
            //greenGemAmountText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
            greenGemAmountText.color = redItemColor;
        }
        else {
            greenGemAmountText.color = Color.white;
        }

        if (redGem > playerRedGems) {
            //redGemAmountText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
            redGemAmountText.color = redItemColor;
        }
        else {
            redGemAmountText.color = Color.white;
        }
        if (cyanGem > playerCyanGems) {
            //cyanGemAmountText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
            cyanGemAmountText.color = redItemColor;
        }
        else {
            cyanGemAmountText.color = Color.white;
        }
        if (blueGem > playerBlueGems) {
            //blueGemAmountText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
            blueGemAmountText.color = redItemColor;
        }
        else {
            blueGemAmountText.color = Color.white;
        }
        if (yellowGem > playerYellowGems) {
            //yellowGemAmountText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
            yellowGemAmountText.color = redItemColor;
        }
        else {
            yellowGemAmountText.color = Color.white;
        }
        if (purpleGem > playerPurpleGems) {
            //purpleGemAmountText.fontMaterial = LocalizationManager.Instance.GetRedGlowMaterial();
            purpleGemAmountText.color = redItemColor;
        }
        else {
            purpleGemAmountText.color = Color.white;
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
                SetGreenFontMaterial(itemStatTemplateValue);
            } else {
                SetInitialItemStatMaterial();
            }

            RectTransform template = Instantiate(itemStatDescriptionTemplate, itemStatDescriptionContainer).GetComponent<RectTransform>();

            if (itemStatList[i] == "") {
                template.Find("StatText").GetComponent<ContentSizeFitter>().enabled = true;
                float size = 250f;
                if(SceneLoader.Instance.IsSteamDeck()) {
                    size = 350f;
                }
                template.Find("StatText").GetComponent<RectTransform>().sizeDelta = new Vector2(size, itemStatTemplateText.GetComponent<RectTransform>().sizeDelta.y);
            }
            else {
                template.Find("StatText").GetComponent<ContentSizeFitter>().enabled = false;
            }
            i++;
        }

        itemStatDescriptionTemplate.gameObject.SetActive(false);
    }

    public void CheckDescriptionCardFree(int greenGem, int redGem, int blueGem, int yellowGem, int purpleGem, int cyanGem) {
        if (greenGem == 0 && redGem == 0 && blueGem == 0 && yellowGem == 0 && purpleGem == 0 && cyanGem == 0) {
            SetGreenFontMaterial(maxLevelText);
            maxLevelText.text = LocalizationManager.Instance.GetLocalizedText("card_free");
            DisableGemCostGO();

            maxLevelText.gameObject.SetActive(true);
        }
    }

    public void SetDescriptionCardMaxlevel() {
        SetGreenFontMaterial(maxLevelText);
        maxLevelText.text = LocalizationManager.Instance.GetLocalizedText("card_maxLevel");
        DisableGemCostGO();

        maxLevelText.gameObject.SetActive(true);
    }

    public void SetLockedByDLC() {
        SetRedFontMaterial();
        maxLevelText.text = LocalizationManager.Instance.GetLocalizedText("supporterEdition");

        DisableGemCostGO();

        maxLevelText.gameObject.SetActive(true);
        foreGround.SetActive(true);
    }

    public void SetDescriptionCardItemLockedInDemo() {
        SetRedFontMaterial();
        maxLevelText.text = LocalizationManager.Instance.GetLocalizedText("card_lockedInDemo");

        DisableGemCostGO();

        maxLevelText.gameObject.SetActive(true);
        foreGround.SetActive(true);
    }

    public void SetDescriptionCardItemUnlockableOnlyInLevel() {
        SetRedFontMaterial();
        maxLevelText.text = LocalizationManager.Instance.GetLocalizedText("card_FindInLevel");

        DisableGemCostGO(); 
        
        maxLevelText.gameObject.SetActive(true);
        foreGround.SetActive(true);
    }

    public void SetDescriptionCardBought() {
        maxLevelText.text = LocalizationManager.Instance.GetLocalizedText("card_unlocked");
        SetGreenFontMaterial(maxLevelText);
        DisableGemCostGO();
        maxLevelText.gameObject.SetActive(true);
    }

    private void DisableGemCostGO() {
        gemGODisabled = true;
        greenGemCostGO.SetActive(false);
        redGemCostGO.SetActive(false);
        blueGemCostGO.SetActive(false);
        yellowGemCostGO.SetActive(false);
        purpleGemCostGO.SetActive(false);
        cyanGemCostGO.SetActive(false);

    }
   
    private void SetGreenFontMaterial(TextMeshProUGUI text) {
        LocalizationManager.Language currentLanguage = SettingsManager.Instance.GetLanguage();

        text.color = modifiedItemColor;

        if (currentLanguage == LocalizationManager.Language.Japanese) {

            text.fontMaterial = modifiedItemStatMaterial_JP;
            return;
        }

        if (currentLanguage == LocalizationManager.Language.Chinese) {

            text.fontMaterial = modifiedItemStatMaterial_CH;

            return;
        }

        text.fontMaterial = modifiedItemStatMaterial;
    }

    private void SetInitialItemStatMaterial() {
        LocalizationManager.Language currentLanguage = SettingsManager.Instance.GetLanguage();
        itemStatTemplateValue.color = Color.white;

        if (currentLanguage == LocalizationManager.Language.Japanese) {

            itemStatTemplateValue.fontMaterial = initialItemStatMaterial_JP;
            return;
        }

        if (currentLanguage == LocalizationManager.Language.Chinese) {

            itemStatTemplateValue.fontMaterial = initialItemStatMaterial_CH;

            return;
        }

        itemStatTemplateValue.fontMaterial = initialItemStatMaterial;

    }

    private void SetRedFontMaterial() {
        LocalizationManager.Language currentLanguage = SettingsManager.Instance.GetLanguage();
        maxLevelText.fontSize = 40;

        if (currentLanguage == LocalizationManager.Language.Japanese) {

            maxLevelText.fontMaterial = redFontMaterial_JP;
            return;
        }

        if (currentLanguage == LocalizationManager.Language.Chinese) {

            itemStatTemplateValue.fontMaterial = redFontMaterial_CH;

            return;
        }

        maxLevelText.fontMaterial = redFontMaterial;
    }

}
