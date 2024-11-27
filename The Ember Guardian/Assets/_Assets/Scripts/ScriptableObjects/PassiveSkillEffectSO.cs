using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PassiveSkillEffectSO : ScriptableObject
{
    public enum Unit {
        percentBuff,
        absoluteBuff,
        timerMaxValue,
        timerValueBuff,
    }

    public SkillItem.SkillType skillType; // Le type de skill
    public Unit skillPassiveUnit;
    public List<float> valuesByLevel; // Les valeurs pour chaque niveau
    public List<int> pricesBylevel; // Les prix pour chaque niveau

    public float GetValueAtLevel(int level) {
        if (level > 0 && level <= valuesByLevel.Count) {
            return valuesByLevel[level - 1]; // Retourne la valeur pour le niveau donné
        }

        Debug.LogWarning($"Invalid level {level} for skill type {skillType}");
        return 0f; // Retourne 0 si le niveau est invalide
    }

    public int GetPriceAtLevel(int level) {
        if (level > 0 && level <= pricesBylevel.Count) {
            return pricesBylevel[level - 1]; // Retourne la valeur pour le niveau donné
        }

        Debug.LogWarning($"Invalid level {level} for skill type {skillType}");
        return 0; // Retourne 0 si le niveau est invalide
    }
}
