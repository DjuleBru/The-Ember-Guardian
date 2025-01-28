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
        merchant_skills,
        merchant_guns,
        merchant_plants,
        merchant_traps,
        savingShrine,
        observationTower,
    }

    public StructureType structureType;

    public Transform structurePrefab;

    public bool level1StructureInitiallyUnlocked;
    public bool buildableAtNight;
    public bool functionUsableAtNight;

    public int tentLevelRequiredForLevel2 = 2;
    public int tentLevelRequiredForLevel3 = 3;
    public int tentLevelRequiredForLevel4 = 4;

    public List<Sprite> buildingUpgradeSpriteList;

    public AudioClip buildAudioClip;
    public AudioClip useFunctionAudioClip;
    public AudioClip upgradeAudioClip;
}
