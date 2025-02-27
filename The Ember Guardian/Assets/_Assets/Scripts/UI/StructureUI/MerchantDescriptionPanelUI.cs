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
        descriptionPanelItemName.text = merchantItem.itemName + " " + merchantItem.currentLevel.ToString();
        descriptionPanelItemDescription.text = merchantItem.itemDescription;

        SetStatChangesText(merchantItem);
    }

    private void SetStatChangesText(MerchantItem merchantItem) {

        if(merchantItem is SkillItem) {
            SkillItem skillItem = (SkillItem)merchantItem;

            if(skillItem.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                activeSkillItemStatChanges.SetActive(false);
                passiveSkillItemStatChanges.SetActive(true);
                trapStatValues.SetActive(false);

                SetPassiveSkillStatsDescription(skillItem);
            } 

            if(skillItem.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                activeSkillItemStatChanges.SetActive(true);
                passiveSkillItemStatChanges.SetActive(false);
                trapStatValues.SetActive(false);

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

        switch (skillSO.skillType) {
            case SkillItem.SkillType.passiveMaxHPIncrease:
                previousStatText = "(+" + (int)(previousStatValue) + ")";
                currentStatText = "+" + ((int)(currentBuffValue)).ToString();
                break;

            case SkillItem.SkillType.passiveMoveSpeedBuff:
                previousStatText = "(+" + (int)(previousStatValue) + "%)";
                currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "%";
                break;

            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                previousStatText = "(" + (int)(PlayerStats.Instance.GetMaxStamina()) + "s)";
                currentStatText = ((int)(PlayerStats.Instance.GetMaxStamina() + relativeStatValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                previousStatText = "(+" + (int)(previousStatValue) + "%)";
                currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "%";
                break;

            case SkillItem.SkillType.passiveAmmoGenerator:
                previousStatText = "(" + (int)previousStatValue + "s)";
                currentStatText = "" + ((int)(currentBuffValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveHealthRegen:
                previousStatText = "(" + (int)(previousStatValue) + "s)"; ;
                currentStatText = "" + ((int)(currentBuffValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveChanceToDoubleXPDrop:
                previousStatText = "(+" + (int)(previousStatValue) + "%)"; ;
                currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "%";
                break;

            case SkillItem.SkillType.passiveShieldGenerator:
                previousStatText = "(" + previousStatValue + "s)"; ;
                currentStatText = ((int)(currentBuffValue)).ToString() + "s";
                break;
        }

        passiveItemStatValue.text = currentStatText;
        passiveItemStatChangesDescription.text = skillSO.StatChanges + " " + previousStatText;
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

        switch (skillSO.skillType) {
            case SkillItem.SkillType.activeMoveSpeedBuff:

                if (skillItem.currentLevel > 1) {
                    previousStatText = " (" + previousStatValue.ToString() + "s)";
                }
                absoluteStatText = currentStatValue + "s";
                absoluteStatDescriptionText = "Duration" + previousStatText;
                cooldownStatText = currentCooldownValue.ToString() + "s";

            break;

            case SkillItem.SkillType.activeShootSpeedBuff:

                if (skillItem.currentLevel > 1) {
                    previousStatText = " (" + previousStatValue.ToString() + "%)";
                }

                absoluteStatText = "+" + currentStatValue + "%";
                absoluteStatDescriptionText = "Fire Rate" + previousStatText;
                cooldownStatText = currentCooldownValue.ToString() + "s";
            break;

            case SkillItem.SkillType.activeTeleportation:

                if (skillItem.currentLevel > 1) {
                    previousStatText = " (" + previousStatValue.ToString() + "m)";
                }

                absoluteStatText = "+" + currentStatValue + "m";
                absoluteStatDescriptionText = "Warp distance" + previousStatText;
                cooldownStatText = currentCooldownValue.ToString() + "s";
            break;
        }

        activeItemStatValue.text = absoluteStatText;
        activeItemStatChangesDescription.text = absoluteStatDescriptionText;
        activeItemCooldownValue.text = cooldownStatText;
        activeItemCooldownChangesText.text = "Cooldown " + previousCooldownStatText;
    }
    private void SetTrapStatsDescription(TrapItem trapItem) {
        TrapSO trapSO = trapItem.trapSO;

        trapSpecialStatValueGameObject.SetActive(false);

        if(trapSO.trapType == TrapItem.TrapType.bearTrap || trapSO.trapType == TrapItem.TrapType.shockerEjector || trapSO.trapType == TrapItem.TrapType.smokeEjector) {
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

                trapSpecialStatDescription.text = "Immobilization duration";
                trapSpecialStatValue.text += "s";

                break;

            case TrapItem.TrapType.shockerEjector:
                trapSpecialStatDescription.text = "Slow down effect";

                trapSpecialStatValue.text += "%";

                break;

            case TrapItem.TrapType.smokeEjector:
                trapSpecialStatDescription.text = "Poison duration";

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
        int totalMaxReloads = trapSO.trapUsesPerNight + trapMaxReloadsUpgrade;
        trapMaxReloadsStatValue.text = totalMaxReloads.ToString();
        if (trapMaxReloadsUpgrade != 0) {
            trapMaxReloadsStatValue.fontMaterial = UpgradeFontMaterial;
        }
        else {
            trapMaxReloadsStatValue.fontMaterial = cleanFontMaterial;
        }

        int trapReloadPriceUpgrade = (int)TrapManager.Instance.GetCurrentUpgradeValue(trapItem.trapType, TrapUpgradeSO.TrapUpgradeType.priceToReload);
        int totalReloadPrice = trapSO.trapPriceToReload - trapReloadPriceUpgrade;
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
                passiveItemStatChangesDescription.text = " Damage ";
                break;

            case TrapUpgradeSO.TrapUpgradeType.cooldown:
                previousStatText = "(-" + (int)(previousStatValue) + "s)";
                currentStatText = "-" + ((int)(currentBuffValue)).ToString() + "s";
                passiveItemStatChangesDescription.text = " Cooldown ";
                break;


            case TrapUpgradeSO.TrapUpgradeType.priceToReload:
                previousStatText = "(-" + (int)(previousStatValue) + ")";
                currentStatText = "-" + ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " Rearm cost";
                break;

            case TrapUpgradeSO.TrapUpgradeType.usesPerNight:
                previousStatText = "(+" + (int)(previousStatValue) + ")";
                currentStatText = ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " Uses/night ";
                break;

            case TrapUpgradeSO.TrapUpgradeType.totalUses:
                previousStatText = "(+" + (int)previousStatValue + ")";
                currentStatText = "+" + ((int)(currentBuffValue)).ToString();
                passiveItemStatChangesDescription.text = " Total Rearms ";
                break;

            case TrapUpgradeSO.TrapUpgradeType.special:

                switch(trapSO.linkedTrapSO.trapType) {

                    case TrapItem.TrapType.bearTrap:

                        previousStatText = "(-" + (int)previousStatValue + "s)";
                        currentStatText = "-" + ((int)(currentBuffValue)).ToString() + "s";
                        passiveItemStatChangesDescription.text = " Immobilization duration ";

                    break;

                    case TrapItem.TrapType.shockerEjector:

                        previousStatText = "(+" + (int)previousStatValue + "%)";
                        currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "%";
                        passiveItemStatChangesDescription.text = " Slow down Amount ";

                    break;

                    case TrapItem.TrapType.smokeEjector:

                        previousStatText = "(+" + (int)previousStatValue + "/s)";
                        currentStatText = "+" + ((int)(currentBuffValue)).ToString() + "/s";
                        passiveItemStatChangesDescription.text = " Poison Damage ";

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
