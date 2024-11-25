using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SkillSO : ScriptableObject
{
    public MerchantItem.MerchantItemType itemType;
    public string SkillName;
    public string Description;
    public string StatChanges;
    public Sprite Icon;
    public PlayerCurrencies.CurrencyType currencyTypeToPay;
    public int Price;
}
