using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class StructureSO : ScriptableObject
{

    public enum StructureType {
        tent,
        fire,
        ammoCrafter,
        barricade,
        hunterShrine,
        minerShrine,
        guardShrine,
        tower,
    }

    public StructureType structureType;

    public Transform structurePrefab;

    public int maxLevel = 1;

    public List<Sprite> buildingUpgradeSpriteList;
}
