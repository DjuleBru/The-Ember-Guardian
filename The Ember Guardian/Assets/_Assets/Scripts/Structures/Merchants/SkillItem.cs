using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillItem : MerchantItem
{
    public SkillSO skillData { get; private set; }
    public int CustomPrice { get; private set; } // Prix modifié selon le marchand

    public override void Initialize(ScriptableObject data) {
        SkillSO SkillDataSO = data as SkillSO;

        if (SkillDataSO != null) {
            skillData = SkillDataSO;

            itemName = skillData.name;
            itemStatChanges = skillData.StatChanges;
            itemDescription = skillData.Description;
            itemType = skillData.itemType;
            price = skillData.Price;
            icon = skillData.Icon;
            currencyTypeToPay = skillData.currencyTypeToPay;
        }
    }

    public override void Purchase() {
        base.Purchase();
        //player.LearnSkill(SkillData);
    }
}
