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
        orbProcessor,
    }

    public StructureType structureType;

    public Transform structurePrefab;

    public int maxLevel = 1;
    public int level2UpgradeTentNecessaryLevel;
    public int level3UpgradeTentNecessaryLevel;
    public int level4UpgradeTentNecessaryLevel;
    public bool buildableAtNight;
    public bool functionUsableAtNight;

    public List<Sprite> buildingUpgradeSpriteList;

    public AudioClip buildAudioClip;
    public AudioClip useFunctionAudioClip;
    public AudioClip upgradeAudioClip;
}
