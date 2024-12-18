using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GunStatModifierSO : ScriptableObject
{
   public enum StatModifier {
        bulletDamage,
        reloadTime,
        cooldown,
        shotsPerClip,
        maxAmmo,
        critChance,
   }

    public StatModifier statModifier;
    public int maxStatModifierLevel;

    public List<int> intStatModifierList;
    public List<float> floatStatModifierList;

    public List<int> greenGemCostList;
    public List<int> redGemCostList;
}
