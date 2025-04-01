using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MerchantTextLinesSO : ScriptableObject
{
    public List<string> merchantTextLines;
    public List<string> merchantTextLinesLocalizationKeys;
    public bool showShopAfterDialog;
}
