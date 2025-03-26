using Sirenix.OdinInspector;
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
        bearTrap,
        bladeTrap,
        shockerEjector,
        spikeEjector,
        smokeEjector,
    }

    public StructureType structureType;

    public Transform structurePrefab;
    public Sprite structureSprite;

    public bool level1StructureInitiallyUnlocked;
    public bool buildableAtNight;
    public bool functionUsableAtNight;
    public bool playerCanAlwaysInteract;

    public bool upgradeable = true;
    [ShowIf("upgradeable")]
    public int tentLevelRequiredForLevel2 = 2;
    [ShowIf("upgradeable")]
    public int tentLevelRequiredForLevel3 = 3;
    [ShowIf("upgradeable")]
    public int tentLevelRequiredForLevel4 = 4;

    [ShowIf("upgradeable")]
    public List<Sprite> buildingUpgradeSpriteList;

    public AudioClip buildAudioClip;
    public float buildVolumeMultiplier = 1f;
    public AudioClip useFunctionAudioClip;
    public float useFunctionVolumeMultiplier = 1f;
    [ShowIf("upgradeable")]
    public AudioClip upgradeAudioClip;
    [ShowIf("upgradeable")]
    public float upgradeVolumeMultiplier = 1f;
}
