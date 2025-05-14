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
    public ActiveSkillEffectSO activeSkillEffect;
    public string SkillName;
    public string StatChanges;
    public string StatChangePrefix;
    public string StatChangeUnit;
    public Sprite Icon;
    public PlayerCurrencies.CurrencyType currencyTypeToPay;
    public int maxLevel;
    public AudioClip activateSkillAudioClip;

    [LabelWidth(100)]
    [TextArea]
    public string Description;
}
