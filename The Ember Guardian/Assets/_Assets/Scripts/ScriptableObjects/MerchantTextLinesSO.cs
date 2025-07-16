using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MerchantTextLinesSO : ScriptableObject
{
    public List<string> merchantTextLinesLocalizationKeys;
    public MerchantTextLinesSO nextTextLineSO_LinkedLevelNotCompleted;
    public MerchantTextLinesSO nextTextLineSO_LinkedLevelCompleted;
    public bool showShopAfterDialog;
}
