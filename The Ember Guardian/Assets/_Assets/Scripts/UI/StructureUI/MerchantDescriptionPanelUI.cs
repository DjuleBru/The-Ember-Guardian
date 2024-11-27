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
        string absoluteStatText = "";
        string relativeStatText = "";

        float buffAbsoluteValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);

        float relativeBuffValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        if (skillItem.currentLevel > 1) {
            relativeBuffValue -= skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
            previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
            absoluteStatText = "";
        }
        else {
            previousStatValue = 0;
        }

        switch (skillSO.skillType) {
            case SkillItem.SkillType.passiveMaxHPIncrease:
                absoluteStatText = "(" + (int)(PlayerStats.Instance.GetPlayerMaxHP()) + ")";
                relativeStatText = "+" + ((int)(relativeBuffValue)).ToString();
                break;

            case SkillItem.SkillType.passiveMoveSpeedBuff:
                absoluteStatText = "(" + (int)(previousStatValue) + "%)";
                relativeStatText = "+" + ((int)(relativeBuffValue)).ToString() + "%";
                break;

            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                absoluteStatText = "(" + (int)(PlayerStats.Instance.GetRunMaxTime()) + "s)";
                relativeStatText = "+" + ((int)(relativeBuffValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                absoluteStatText = "(" + (int)(previousStatValue) + "%)";
                relativeStatText = "+" + ((int)(relativeBuffValue)).ToString() + "%";
                break;

            case SkillItem.SkillType.passiveAmmoGenerator:
                absoluteStatText = "(" + (int)previousStatValue + "s)";
                relativeStatText = "" + ((int)(buffAbsoluteValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveHealthRegen:
                absoluteStatText = "(" + (int)(previousStatValue) + "s)"; ;
                relativeStatText = "" + ((int)(buffAbsoluteValue)).ToString() + "s";
                break;

            case SkillItem.SkillType.passiveChanceToDoubleXPDrop:
                absoluteStatText = "(" + (int)(previousStatValue) + "%)"; ;
                relativeStatText = "+" + ((int)(relativeBuffValue)).ToString() + "%";
                break;

            case SkillItem.SkillType.passiveShieldGenerator:
                absoluteStatText = "(" + (int)(PlayerStats.Instance.GetSkillStat(skillItem)) + "s)"; ;
                relativeStatText = ((int)(relativeBuffValue)).ToString() + "s";
                break;
        }

        passiveItemStatValue.text = relativeStatText;
        passiveItemStatChangesDescription.text = skillSO.StatChanges + " " + absoluteStatText;
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

        float buffAbsoluteValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float currentCooldownValue = skillEffect.GetCooldownAtLevel(skillItem.currentLevel);

        float relativeBuffValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);

        if (skillItem.currentLevel > 1) {
            relativeBuffValue -= skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
            float previousStatValue = skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
            float previousCooldownValue = skillEffect.GetCooldownAtLevel(skillItem.currentLevel -1);

            previousStatText = " (" + previousStatValue.ToString() + ")";
            previousCooldownStatText = " (" + previousCooldownValue.ToString() + "s)";
            absoluteStatText = "";
        } else {
            previousCooldownStatText = "";
        }

        switch (skillSO.skillType) {
            case SkillItem.SkillType.activeMoveSpeedBuff:
                absoluteStatText = "+ " + buffAbsoluteValue + "%";
                absoluteStatDescriptionText = "Move Speed Boost" + previousStatText;
                cooldownStatText = currentCooldownValue.ToString() + "s";
            break;
        }

        activeItemStatValue.text = absoluteStatText;
        activeItemStatChangesDescription.text = absoluteStatDescriptionText;
        activeItemCooldownValue.text = cooldownStatText;
        activeItemCooldownChangesText.text = "Cooldown " + previousCooldownStatText;
    }

}
