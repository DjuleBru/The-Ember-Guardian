using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillItem : MerchantItem {

    public enum SkillType {
        passiveMoveSpeedBuff,
        passiveRunMaxTimeBuff,
        passiveRunAccelerationFactorBuff,
        passiveChanceToDoubleXPDrop,
        passiveShieldGenerator,
        passiveHealthRegen,
        passiveMaxHPIncrease,
        passiveAmmoGenerator,
        activeMoveSpeedBuff,
        activeTeleportation,
        activeShootSpeedBuff,
    }

    public SkillSO skillSO { get; private set; }
    public SkillType skillType { get; private set; }
    public int CustomPrice { get; private set; } // Prix modifié selon le marchand
    public int maxLevel;
    public string effectDescription;

    public override void Initialize(ScriptableObject data) {
        SkillSO SkillDataSO = data as SkillSO;

        if (SkillDataSO != null) {
            skillSO = SkillDataSO;

            if(skillSO.itemType == MerchantItemType.PassiveSkill) {
                price = skillSO.passiveSkillEffect.GetPriceAtLevel(1);
            } else {
                price = skillSO.activeSkillEffect.GetPriceAtLevel(1);
            }

            itemName = skillSO.SkillName;
            itemStatChanges = skillSO.StatChanges;
            itemDescription = skillSO.Description;
            itemType = skillSO.itemType;
            skillType = skillSO.skillType;
            icon = skillSO.Icon;
            maxLevel = skillSO.maxLevel;
            currencyTypeToPay = skillSO.currencyTypeToPay;
        }
    }

    public bool UpgradeSkill() {
        if (currentLevel < maxLevel) {
            currentLevel++;
            UpdateEffectDescription();
            return true;
        }
        return false;
    }

    private void UpdateEffectDescription() {
        // Logique pour mettre à jour la description en fonction du niveau
        effectDescription = $"Level {currentLevel}: Improved Effect!";
    }

    public override void Purchase() {
        base.Purchase();

        if (itemType == MerchantItemType.ActiveSkill) {
            PlayerSkills.Instance.AddActiveSkill(this);
        }

        if (itemType == MerchantItemType.PassiveSkill) {
            PlayerSkills.Instance.AddPassiveSkill(this);
        }
    }

    public SkillSO GetSkillSO() {
        return skillSO;
    }
}
