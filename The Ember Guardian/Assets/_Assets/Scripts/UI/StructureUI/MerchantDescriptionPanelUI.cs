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

    [SerializeField] protected Material cleanFontMaterial;
    [SerializeField] protected Material UpgradeFontMaterial;

    public void SetPanelPosition(RectTransform rectTransform) {
        //Check if we switched from big item to small item
        descriptionPanelRectTransform.position = rectTransform.position;
    }

    public void OpenPanel() {
        descriptionPanelAnimator.SetTrigger("Open");
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

                SetPassiveSkillStatsDescription(skillItem);
            } 

            if(skillItem.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                activeSkillItemStatChanges.SetActive(true);
                passiveSkillItemStatChanges.SetActive(false);

                SetActiveSkillStatsDescription(skillItem);
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
                previousStatText = "(" + (int)(PlayerStats.Instance.GetRunMaxTime()) + "s)";
                currentStatText = ((int)(PlayerStats.Instance.GetRunMaxTime() + relativeStatValue)).ToString() + "s";
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

}
