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
        sniperTower,
        machineGunTower,
        mortarTower,
        secondaryFire,
        fastTravelTeleporter,
        fireEjector,
        spikes,
        orbExtractor,
        engineerShrine,
        currencyStorage_BigOrb,
        currencyStorage_Objective,
        currencyStorage_SmallOrb,
        currencyStorage_Ammo,
        currencyStorage_SpecialAmmo,
        fastTravelTeleporter_World,
    }
    public enum StructureCategory {
        core,
        tower,
        trap,
        util,
        storage,
    }

    public StructureType structureType;
    public StructureCategory structureCategory;

    public Transform structureLocationPrefab;
    public Transform structurePrefab;
    public Sprite structureSprite;

    public string structureNameLocalizationKey;
    public int widthInCells;
    public bool structurePositionMovable;
    public bool structurePositionRemovable;
    public int maxStructureBlueprintAmount;
    public bool level1StructureInitiallyUnlocked;
    public bool buildableAtNight;
    public bool upgradeableAtNight;
    public bool functionUsableAtNight;
    public bool playerCanAlwaysInteract;
    public bool engineerCanWorkByDay;
    public bool engineerCanWorkByNight;
    public bool workingEngineerHideTool;
    public bool workingEngineerHideVisual;
    [PropertyRange(0,10)]
    public int engineerWorkingPriority;
    public int maxEngineersAssignedWorking;
    public int maxEngineersAssignedRefilling;
    public PlayerCurrencies.CurrencyType refillCurrencyTypeNeeded;

    public bool upgradeable = true;
    [ShowIf("upgradeable")]
    public int maxLevel;

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
