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
        pelletsPerBullet,
        shootConeAngle,
   }

    public StatModifier statModifier;

    public List<float> statModifierList;

    public List<int> greenGemCostList;
    public List<int> redGemCostList;
}
