using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SkillSO : ScriptableObject
{
    public MerchantItem.MerchantItemType itemType;
    public SkillItem.SkillType skillType;
    public PassiveSkillEffectSO passiveSkillEffect;
    public string SkillName;
    public string StatChanges;
    public Sprite Icon;
    public PlayerCurrencies.CurrencyType currencyTypeToPay;
    public int Price;
    public int maxLevel;

    [LabelWidth(100)]
    [TextArea]
    public string Description;
}
