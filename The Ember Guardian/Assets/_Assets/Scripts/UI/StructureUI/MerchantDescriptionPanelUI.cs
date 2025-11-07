using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MerchantDescriptionPanelUI : MonoBehaviour
{

    [SerializeField] protected RectTransform descriptionPanelRectTransform;
    [SerializeField] protected GameObject activeSkillItemStatChanges;
    [SerializeField] protected GameObject passiveSkillItemStatChanges;
    [SerializeField] protected GameObject trapStatValues;

    [SerializeField] protected Animator descriptionPanelAnimator;
    [SerializeField] protected Image descriptionPanelItemIcon;
    [SerializeField] protected TextMeshProUGUI descriptionPanelItemName;
    [SerializeField] protected TextMeshProUGUI descriptionPanelItemDescription;

    [SerializeField] protected TextMeshProUGUI passiveItemStatValue;
    [SerializeField] protected TextMeshProUGUI passiveItemStatChangesDescription;

    [SerializeField] protected TextMeshProUGUI activeItemStatValue;
    [SerializeField] protected TextMeshProUGUI activeItemStatChangesDescription;
    [SerializeField] protected TextMeshProUGUI activeItemCooldownValue;
    [SerializeField] protected TextMeshProUGUI activeItemCooldownChangesText;

    [SerializeField] protected TextMeshProUGUI trapDamageStatValue;
    [SerializeField] protected TextMeshProUGUI trapCooldownStatValue;
    [SerializeField] protected TextMeshProUGUI trapUsesPerNightStatValue;
    [SerializeField] protected TextMeshProUGUI trapMaxReloadsStatValue;
    [SerializeField] protected TextMeshProUGUI trapReloadPriceStatValue;

    [SerializeField] protected GameObject trapSpecialStatValueGameObject;
    [SerializeField] protected TextMeshProUGUI trapSpecialStatValue;
    [SerializeField] protected TextMeshProUGUI trapSpecialStatDescription;

    [SerializeField] protected Material cleanFontMaterial;
    [SerializeField] protected Material UpgradeFontMaterial;

    private void Start() {
        TMP_FontAsset font = LocalizationManager.Instance.GetCurrentFont();

        descriptionPanelItemName.font = font;
        descriptionPanelItemDescription.font = font;

        if (passiveItemStatValue != null) {
            passiveItemStatValue.font = font;
            passiveItemStatChangesDescription.font = font;
            activeItemStatValue.font = font;
            activeItemStatChangesDescription.font = font;
            activeItemCooldownValue.font = font;
            activeItemCooldownChangesText.font = font;
        }

        if(trapDamageStatValue != null) {
            trapDamageStatValue.font = font;
            trapCooldownStatValue.font = font;
            trapUsesPerNightStatValue.font = font;
            trapMaxReloadsStatValue.font = font;
            trapReloadPriceStatValue.font = font;
            trapSpecialStatValue.font = font;
            trapSpecialStatDescription.font = font;
        }

    }

    public void SetPanelPosition(RectTransform rectTransform) {
        //Check if we switched from big item to small item
        descriptionPanelRectTransform.position = rectTransform.position;
    }

    public void OpenPanel() {
        descriptionPanelRectTransform.gameObject.SetActive(true);
        descriptionPanelAnimator.SetTrigger("Open");
    }
    public void ClosePanel() {
        descriptionPanelRectTransform.gameObject.SetActive(false);   
    }
    public void UpdateDescriptionPanelVisuals(MerchantItem merchantItem) {
        descriptionPanelItemIcon.sprite = merchantItem.icon;
        descriptionPanelItemName.text = LocalizationManager.Instance.GetLocalizedText(merchantItem.itemName);

        if(merchantItem is SkillItem) {
            descriptionPanelItemName.text += " " + merchantItem.currentLevel.ToString();
        }

        descriptionPanelItemDescription.text = LocalizationManager.Instance.GetLocalizedText(merchantItem.itemName + "_ItemDescription");

        SetStatChangesText(merchantItem);

        if(merchantItem.itemType == MerchantItem.MerchantItemType.RefreshShopItems) {
            descriptionPanelItemName.text = LocalizationManager.Instance.GetLocalizedText(merchantItem.itemName);
        }
    }

    private void SetStatChangesText(MerchantItem merchantItem) {

        if(merchantItem is SkillItem) {
            SkillItem skillItem = (SkillItem)merchantItem;

            if(skillItem.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                activeSkillItemStatChanges.SetActive(false);
                passiveSkillItemStatChanges.SetActive(true);

                SetPassiveSkillStatsDescription(skillItem);
            } 

            if(skillItem.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                activeSkillItemStatChanges.SetActive(true);
                passiveSkillItemStatChanges.SetActive(false);

                SetActiveSkillStatsDescription(skillItem);
            }

            if (skillItem.itemType == MerchantItem.MerchantItemType.RefreshShopItems) {
                activeSkillItemStatChanges.SetActive(false);
                passiveSkillItemStatChanges.SetActive(false);

                SetActiveSkillStatsDescription(skillItem);
            }
        }

        if(merchantItem is TrapItem) {
            TrapItem trapItem = (TrapItem)merchantItem;

            if (trapItem.itemType == MerchantItem.MerchantItemType.TrapUpgrade) {
                activeSkillItemStatChanges.SetActive(false);
                passiveSkillItemStatChanges.SetActive(true);
                trapStatValues.SetActive(false);

                SetTrapUpgradeStatsDescription(trapItem);
            }

            if (trapItem.itemType == MerchantItem.MerchantItemType.Trap) {
                trapStatValues.SetActive(true);
                activeSkillItemStatChanges.SetActive(false);
                passiveSkillItemStatChanges.SetActive(false);

                SetTrapStatsDescription(trapItem);
            }
        }
    }

    private void SetPassiveSkillStatsDescription(SkillItem skillItem) {
        SkillSO skillSO = skillItem.skillSO;

        PassiveSkillEffectSO skillEffect = skillSO.passiveSkillEffect;
        string previousStatText = "";
        string currentStatText = "";

        float currentBuffValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float relativeStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);

        if (skillItem.currentLevel > 1) {
            previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
            relativeStatValue = currentBuffValue - previousStatValue;
            previousStatText = "";
        }
        else {
            previousStatValue = 0;
        }

        previousStatText = "(" + skillSO.StatChangePrefix + (int)(previousStatValue) + skillSO.StatChangeUnit + ")";
        currentStatText = skillSO.StatChangePrefix + ((int)(currentBuffValue)).ToString() + skillSO.StatChangeUnit;

        switch (skillSO.skillType) {
            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                previousStatText = "(" + (int)(PlayerStats.Instance.GetMaxStamina()) + "s)";
                currentStatText = ((int)(PlayerStats.Instance.GetMaxStamina() + relativeStatValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveHealthRegen:
                previousStatValue = (int)PlayerStats.Instance.GetHpRegenTime();
                relativeStatValue = currentBuffValue - previousStatValue;
                previousStatText = "(" + (int)(PlayerStats.Instance.GetHpRegenTime()) + "s)";
                currentStatText = ((int)(PlayerStats.Instance.GetHpRegenTime() + relativeStatValue)).ToString() + "s";
                break;

        }

        passiveItemStatValue.text = currentStatText;
        passiveItemStatChangesDescription.text = LocalizationManager.Instance.GetLocalizedText(skillSO.StatChanges) + " " + previousStatText;
    }

    private void SetActiveSkillStatsDescription(SkillItem skillItem) {
        SkillSO skillSO = skillItem.skillSO;
        ActiveSkillEffectSO skillEffect = skillSO.activeSkillEffect;
        string absoluteStatText = "";
        string previousStatText = "";
        string absoluteStatDescriptionText = "";
        string cooldownStatText = "";
        string previousCooldownStatText = "";
        string cooldownStatDescriptionText = "";

        float currentStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float currentCooldownValue = skillEffect.GetCooldownAtLevel(skillItem.currentLevel);
        float previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);

        if (skillItem.currentLevel > 1) {
            previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
            float previousCooldownValue = skillEffect.GetCooldownAtLevel(skillItem.currentLevel -1);

            previousCooldownStatText = " (" + previousCooldownValue.ToString() + "s)";
            absoluteStatText = "";

            // Stat fonts
            if(previousStatValue != currentStatValue) {
                activeItemStatValue.fontMaterial = UpgradeFontMaterial;
            } else {
                activeItemStatValue.fontMaterial = cleanFontMaterial;
            }

            if (previousCooldownValue != currentCooldownValue) {
                activeItemCooldownValue.fontMaterial = UpgradeFontMaterial;
            }
            else {
                activeItemCooldownValue.fontMaterial = cleanFontMaterial;
            }

        } else {
            previousCooldownStatText = "";
        }

        if (skillItem.currentLevel > 1) {
            previousStatText = " (" + previousStatValue.ToString() + skillSO.StatChangeUnit.ToString() + ")";
        }
        absoluteStatText = currentStatValue + skillSO.StatChangeUnit.ToString();
        cooldownStatText = currentCooldownValue.ToString() + "s";

        activeItemStatValue.text = skillSO.StatChangePrefix + absoluteStatText;

        absoluteStatDescriptionText = LocalizationManager.Instance.GetLocalizedText(skillSO.StatChanges) + previousStatText;
        activeItemStatChangesDescription.text = absoluteStatDescriptionText;
        activeItemCooldownValue.text = cooldownStatText;
        activeItemCooldownChangesText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown") + previousCooldownStatText;
    }
    private void SetTrapStatsDescription(TrapItem trapItem) {
        TrapSO trapSO = trapItem.trapSO;

        trapSpecialStatValueGameObject.SetActive(false);

        if(trapSO.trapType == TrapItem.TrapType.bearTrap || trapSO.trapType == TrapItem.TrapType.shockerEjector || trapSO.trapType == TrapItem.TrapType.smokeEjector || trapSO.trapType == TrapItem.TrapType.fireEjector) {
            trapSpecialStatValueGameObject.SetActive(true);

            float trapSpecialUpgrade = TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.special);
            float totalSpecial = trapSO.trapSpecialStat + trapSpecialUpgrade;
            trapSpecialStatValue.text = totalSpecial.ToString();
            if (trapSpecialUpgrade != 0) {
                trapSpecialStatValue.fontMaterial = UpgradeFontMaterial;
            }
            else {
                trapSpecialStatValue.fontMaterial = cleanFontMaterial;
            }
        }

        switch (trapSO.trapType) {
            case TrapItem.TrapType.bearTrap:

                trapSpecialStatDescription.text = LocalizationManager.Instance.GetLocalizedText("Immobilization Duration");
                trapSpecialStatValue.text += "s";

                break;

            case TrapItem.TrapType.shockerEjector:
                trapSpecialStatDescription.text = LocalizationManager.Instance.GetLocalizedText("Slow Down Effect");

                trapSpecialStatValue.text += "%";

                break;

            case TrapItem.TrapType.smokeEjector:
                trapSpecialStatDescription.text = LocalizationManager.Instance.GetLocalizedText("Poison Duration");

                trapSpecialStatValue.text += "/s";

                break;

            case TrapItem.TrapType.fireEjector:
                trapSpecialStatDescription.text = LocalizationManager.Instance.GetLocalizedText("Burn Duration");

                trapSpecialStatValue.text += "/s";

                break;

        }

        int trapDamageUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.damage);
        int totalTrapDamage = trapSO.trapDamage + trapDamageUpgrade;
        trapDamageStatValue.text = totalTrapDamage.ToString();
        if(trapDamageUpgrade != 0) {
            trapDamageStatValue.fontMaterial = UpgradeFontMaterial;
        } else {
            trapDamageStatValue.fontMaterial = cleanFontMaterial;
        }

        float trapCooldownUpgrade = TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.cooldown);
        float totalCooldown = trapSO.trapCooldown - trapCooldownUpgrade;
        trapCooldownStatValue.text = totalCooldown.ToString("F1") + "s";
        if (trapCooldownUpgrade != 0) {
            trapCooldownStatValue.fontMaterial = UpgradeFontMaterial;
        }
        else {
            trapCooldownStatValue.fontMaterial = cleanFontMaterial;
        }

        int trapUsesPerNightUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.usesPerNight);
        int totalUsesPerNight = trapSO.trapUsesPerNight + trapUsesPerNightUpgrade;
        trapUsesPerNightStatValue.text = totalUsesPerNight.ToString();
        if (trapUsesPerNightUpgrade != 0) {
            trapUsesPerNightStatValue.fontMaterial = UpgradeFontMaterial;
        }
        else {
            trapUsesPerNightStatValue.fontMaterial = cleanFontMaterial;
        }

        int trapMaxReloadsUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.totalUses);
        int totalMaxReloads = trapSO.maxRearmsBeforeBreaking + trapMaxReloadsUpgrade;
        trapMaxReloadsStatValue.text = totalMaxReloads.ToString();
        if (trapMaxReloadsUpgrade != 0) {
            trapMaxReloadsStatValue.fontMaterial = UpgradeFontMaterial;
        }
        else {
            trapMaxReloadsStatValue.fontMaterial = cleanFontMaterial;
        }

        int trapReloadPriceUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.priceToReload);
        int totalReloadPrice = trapSO.rearmPrice - trapReloadPriceUpgrade;
        trapReloadPriceStatValue.text = totalReloadPrice.ToString();
        if (trapReloadPriceUpgrade != 0) {
            trapReloadPriceStatValue.fontMaterial = UpgradeFontMaterial;
        }
        else {
            trapReloadPriceStatValue.fontMaterial = cleanFontMaterial;
        }
    }

    private void SetTrapUpgradeStatsDescription(TrapItem trapItem) {
        TrapSO trapSO = trapItem.trapSO;
        TrapSO linkedTrapSO = trapItem.trapSO.linkedTrapSO;

        TrapUpgradeSO upgradeEffect = trapItem.trapSO.trapUpgradeSO;
        string previousStatText = "";
        string currentStatText = "";

        float currentBuffValue = upgradeEffect.GetValueAtLevel(trapItem.currentLevel);
        float previousStatValue = upgradeEffect.GetValueAtLevel(trapItem.currentLevel);
        float relativeStatValue = upgradeEffect.GetValueAtLevel(trapItem.currentLevel);

        if (trapItem.currentLevel > 1) {
            previousStatValue = upgradeEffect.GetValueAtLevel(trapItem.currentLevel - 1);
            relativeStatValue = currentBuffValue - previousStatValue;
            previousStatText = "";
        }
        else {
            previousStatValue = 0;
        }

        switch (trapSO.trapUpgradeSO.trapUpgradeType) {
            case TrapUpgradeSO.TrapUpgradeType.damage:
                previousStatText = "(+" + (int)(previousStatValue) + ")";
                currentStatText = "+" + ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("card_damage");
                break;

            case TrapUpgradeSO.TrapUpgradeType.cooldown:
                previousStatText = "(-" + (int)(previousStatValue) + "s)";
                currentStatText = "-" + ((int)(currentBuffValue)).ToString() + "s";
                passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                break;


            case TrapUpgradeSO.TrapUpgradeType.priceToReload:
                previousStatText = "(-" + (int)(previousStatValue) + ")";
                currentStatText = "-" + ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("Rearm Price");
                break;

            case TrapUpgradeSO.TrapUpgradeType.usesPerNight:
                previousStatText = "(+" + (int)(previousStatValue) + ")";
                currentStatText = ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("Uses Per Night");
                break;

            case TrapUpgradeSO.TrapUpgradeType.totalUses:
                previousStatText = "(+" + (int)previousStatValue + ")";
                currentStatText = "+" + ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("Total Rearms");
                break;

            case TrapUpgradeSO.TrapUpgradeType.special:

                switch(trapSO.linkedTrapSO.trapType) {

                    case TrapItem.TrapType.bearTrap:

                        previousStatText = "(-" + (int)previousStatValue + "s)";
                        currentStatText = "-" + ((int)(currentBuffValue)).ToString() + "s";
                        passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("Immobilization Duration");

                        break;

                    case TrapItem.TrapType.shockerEjector:

                        previousStatText = "(+" + (int)previousStatValue + "%)";
                        currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "%";
                        passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("Slow Down Amount");

                        break;

                    case TrapItem.TrapType.smokeEjector:

                        previousStatText = "(+" + (int)previousStatValue + "/s)";
                        currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "/s";
                        passiveItemStatChangesDescription.text = " " + LocalizationManager.Instance.GetLocalizedText("Poison Damage");

                        break;
                }

                break;


        }

        if(trapItem.currentLevel != 1) {
            passiveItemStatChangesDescription.text += previousStatText;
        }
        passiveItemStatValue.text = currentStatText;
    }
}
