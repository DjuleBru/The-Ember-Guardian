using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TrapUpgradeSO : ScriptableObject
{
    public enum TrapUpgradeType {
        damage,
        cooldown,
        usesPerNight,
        totalUses,
        priceToReload,
        special,
    }

    public TrapItem.TrapType linkedTrapType; // Le type de skill
    public TrapUpgradeType trapUpgradeType;
    public List<float> valuesByLevel; // Les valeurs pour chaque niveau
    public List<int> pricesBylevel; // Les prix pour chaque niveau

    public float GetValueAtLevel(int level) {
        if (level >= 0 && level < valuesByLevel.Count) {
            return valuesByLevel[level]; // Retourne la valeur pour le niveau donné
        }

        return 0f; // Retourne 0 si le niveau est invalide
    }

    public int GetPriceAtLevel(int level) {
        if (level >= 0 && level < pricesBylevel.Count) {
            return pricesBylevel[level]; // Retourne la valeur pour le niveau donné
        }

        return 0; // Retourne 0 si le niveau est invalide
    }
}
