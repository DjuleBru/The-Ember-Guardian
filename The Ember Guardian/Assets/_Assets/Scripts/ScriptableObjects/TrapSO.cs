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
    public StructureSO trapStructureSO;
    public TrapSO linkedTrapSO;
    public string TrapName;
    public int buyTrapPrice;
    public int trapDamage;
    public int trapSpecialStat;
    public int trapCooldown;
    public int trapUsesPerNight;
    public int trapReloadsBeforeBreaking;
    public int trapPriceToReload;
    public float trapActiveDuration;
    public bool trapHasAOEAttack;
    public Sprite Icon;
    public PlayerCurrencies.CurrencyType currencyTypeToPay;
    public AudioClip[] triggerTrapAudioClip;
    public AudioClip[] reloadTrapAudioClip;

    [LabelWidth(100)]
    [TextArea]
    public string Description;
}
