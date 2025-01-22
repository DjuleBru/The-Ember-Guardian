using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HubMerchantItemStatModifierSO : ScriptableObject
{

    public List<float> statModifierList;

    public List<int> greenGemCostList;
    public List<int> redGemCostList;
    public List<int> blueGemCostList;
    public List<int> yellowGemCostList;
    public List<int> purleGemCostList;
}
