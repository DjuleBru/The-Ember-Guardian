using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TrapSO : ScriptableObject
{
    public MerchantItem.MerchantItemType itemType;
    public TrapItem.TrapType trapType;
    public TrapUpgradeSO trapUpgradeSO;
    public TrapSO linkedTrapSO;
    public int buyTrapPrice;
    public int reloadTrapPrice;
    public string TrapName;
    public string TrapStats;
    public Sprite Icon;
    public PlayerCurrencies.CurrencyType currencyTypeToPay;
    public AudioClip triggerTrapAudioClip;
    public AudioClip reloadTrapAudioClip;

    [LabelWidth(100)]
    [TextArea]
    public string Description;
}
