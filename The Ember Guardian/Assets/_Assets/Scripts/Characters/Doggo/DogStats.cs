using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogStats : MonoBehaviour {

    public static DogStats Instance;

    private bool retreiverUnlocked;
    private bool darkCompanionUnlocked;

    private bool germanShepherdBiteAbilityUnlocked;
    private bool germanShepherdDigResourceAbilityUnlocked;
    private bool germanShepherdDetectAmbushAbilityUnlocked;
    private bool retreiverBiteAbilityUnlocked = true;
    private bool retreiverBuffWorkersAbilityUnlocked;
    private bool retreiverPickUpItemsAbilityUnlocked;
    private bool darkCompanionBiteAbilityUnlocked = true;
    private bool darkCompanionLaserAbilityUnlocked;
    private bool darkCompanionStompAbilityUnlocked;

    private int germanShepherdBiteDamage;
    private float germanShepherdBiteCooldown;
    private float germanShepherdDigResourceCooldown;
    private float germanShepherdDigResourceProbability;
    private float germanShepherdDigResourceDoubleProbability;
    private float germanShepherdAmbushDetectionProbability;

    private int retreiverBiteDamage;
    private float retreiverBiteCooldown;
    private float retreiverBuffWorkersAmount;
    private float retreiverBuffWorkersRadius;

    private int darkCompanionBiteDamage;
    private float darkCompanionBiteCooldown;
    private int darkCompanionLaserDamage;
    private float darkCompanionLaserCooldown;
    private int darkCompanionStompDamage;
    private float darkCompanionStompCooldown;
    private float darkCompanionStompStunDuration;

    [SerializeField] private int initialGermanShepherdBiteDamage;
    [SerializeField] private float initialGermanShepherdBiteCooldown;
    [SerializeField] private float initialGermanShepherdDigResourceCooldown;
    [SerializeField] private float initialGermanShepherdDigResourceProbability;
    [SerializeField] private float initialGermanShepherdDigResourceDoubleProbability;
    [SerializeField] private float initialGermanShepherdAmbushDetectionProbability;

    [SerializeField] private int initialRetreiverBiteDamage;
    [SerializeField] private float initialRetreiverBiteCooldown;
    [SerializeField] private float initialRetreiverBuffWorkersAmount;
    [SerializeField] private float initialRetreiverBuffWorkersRadius;
    [SerializeField] private float initialRetreiverCurrenciesDetectionRange;

    [SerializeField] private int initialDarkCompanionBiteDamage;
    [SerializeField] private float initialDarkCompanionBiteCooldown;
    [SerializeField] private int initialDarkCompanionLaserDamage;
    [SerializeField] private float initialDarkCompanionLaserCooldown;
    [SerializeField] private float initialDarkCompanionLaserTickCooldown;
    [SerializeField] private int initialDarkCompanionStompDamage;
    [SerializeField] private float initialDarkCompanionStompCooldown;
    [SerializeField] private float initialDarkCompanionStompStunDuration;

    [SerializeField] private bool debugUnlockBiteAbility;
    [SerializeField] private bool debugUnlockDigResourceAbility;
    [SerializeField] private bool debugUnlockDetectAmbushAbility;
    [SerializeField] private bool debugUnlockBuffWorkersAbility;
    [SerializeField] private bool debugUnlockPickUpItemsAbility;
    [SerializeField] private bool debugUnlockLaserAbility;
    [SerializeField] private bool debugUnlockStompAbility;

    public event EventHandler OnNewDogUnlocked;
    public event EventHandler OnNewAbilityUnlocked;
    public event EventHandler OnAbilityUpgraded;

    [SerializeField] private Sprite germanShepherdIcon;
    [SerializeField] private Sprite retreiverIcon;
    [SerializeField] private Sprite darkCompanionIcon;

    private void Awake() {
        Instance = this;
        InitializeParameters();
    }
    private void InitializeParameters() {
        LoadSavedDogStats();
    }

    private void LoadSavedDogStats() {
        // Anciennes clés restées hors batch
        retreiverUnlocked = ES3.Load("retreiverUnlocked", false);
        darkCompanionUnlocked = ES3.Load("darkCompanionUnlocked", false);

        // Initialisation par défaut
        germanShepherdBiteAbilityUnlocked = false;
        germanShepherdDigResourceAbilityUnlocked = false;
        germanShepherdDetectAmbushAbilityUnlocked = false;

        retreiverBiteAbilityUnlocked = true;
        retreiverBuffWorkersAbilityUnlocked = false;
        retreiverPickUpItemsAbilityUnlocked = false;

        darkCompanionBiteAbilityUnlocked = true;
        darkCompanionLaserAbilityUnlocked = false;
        darkCompanionStompAbilityUnlocked = false;

        germanShepherdBiteDamage = initialGermanShepherdBiteDamage;
        germanShepherdBiteCooldown = initialGermanShepherdBiteCooldown;
        germanShepherdDigResourceCooldown = initialGermanShepherdDigResourceCooldown;
        germanShepherdDigResourceProbability = initialGermanShepherdDigResourceProbability;
        germanShepherdDigResourceDoubleProbability = initialGermanShepherdDigResourceDoubleProbability;
        germanShepherdAmbushDetectionProbability = initialGermanShepherdAmbushDetectionProbability;

        retreiverBiteDamage = initialRetreiverBiteDamage;
        retreiverBiteCooldown = initialRetreiverBiteCooldown;
        retreiverBuffWorkersAmount = initialRetreiverBuffWorkersAmount;
        retreiverBuffWorkersRadius = initialRetreiverBuffWorkersRadius;

        darkCompanionBiteDamage = initialDarkCompanionBiteDamage;
        darkCompanionBiteCooldown = initialDarkCompanionBiteCooldown;
        darkCompanionLaserDamage = initialDarkCompanionLaserDamage;
        darkCompanionLaserCooldown = initialDarkCompanionLaserCooldown;
        darkCompanionStompDamage = initialDarkCompanionStompDamage;
        darkCompanionStompCooldown = initialDarkCompanionStompCooldown;
        darkCompanionStompStunDuration = initialDarkCompanionStompStunDuration;

        // Dog type par défaut
        Dog.DogType loadedDogType = Dog.DogType.GermanShepherd;

        if (!ES3.KeyExists("DogStats") && Dog.Instance != null) {
            Dog.Instance.SetDogType(loadedDogType);
            return;
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level && LevelManager.Instance.IsHordeMode()) return;

        var dogData = ES3.Load<Dictionary<string, object>>("DogStats");

        // Abilities
        germanShepherdBiteAbilityUnlocked = GetValue(dogData, "germanShepherdBiteAbilityUnlocked", false);
        germanShepherdDigResourceAbilityUnlocked = GetValue(dogData, "germanShepherdDigResourceAbilityUnlocked", false);
        germanShepherdDetectAmbushAbilityUnlocked = GetValue(dogData, "germanShepherdDetectAmbushAbilityUnlocked", false);

        retreiverBiteAbilityUnlocked = GetValue(dogData, "retreiverBiteAbilityUnlocked", true);
        retreiverBuffWorkersAbilityUnlocked = GetValue(dogData, "retreiverBuffWorkersAbilityUnlocked", false);
        retreiverPickUpItemsAbilityUnlocked = GetValue(dogData, "retreiverPickUpItemsAbilityUnlocked", false);

        darkCompanionBiteAbilityUnlocked = GetValue(dogData, "darkCompanionBiteAbilityUnlocked", true);
        darkCompanionLaserAbilityUnlocked = GetValue(dogData, "darkCompanionLaserAbilityUnlocked", false);
        darkCompanionStompAbilityUnlocked = GetValue(dogData, "darkCompanionStompAbilityUnlocked", false);

        // Stats
        germanShepherdBiteDamage = GetValue(dogData, "germanShepherdBiteDamage", initialGermanShepherdBiteDamage);
        germanShepherdBiteCooldown = GetValue(dogData, "germanShepherdBiteCooldown", initialGermanShepherdBiteCooldown);
        germanShepherdDigResourceCooldown = GetValue(dogData, "germanShepherdDigResourceCooldown", initialGermanShepherdDigResourceCooldown);
        germanShepherdDigResourceProbability = GetValue(dogData, "germanShepherdDigResourceProbability", initialGermanShepherdDigResourceProbability);
        germanShepherdDigResourceDoubleProbability = GetValue(dogData, "germanShepherdDigResourceDoubleProbability", initialGermanShepherdDigResourceDoubleProbability);
        germanShepherdAmbushDetectionProbability = GetValue(dogData, "germanShepherdAmbushDetectionProbability", initialGermanShepherdAmbushDetectionProbability);

        retreiverBiteDamage = GetValue(dogData, "retreiverBiteDamage", initialRetreiverBiteDamage);
        retreiverBiteCooldown = GetValue(dogData, "retreiverBiteCooldown", initialRetreiverBiteCooldown);
        retreiverBuffWorkersAmount = GetValue(dogData, "retreiverBuffWorkersAmount", initialRetreiverBuffWorkersAmount);
        retreiverBuffWorkersRadius = GetValue(dogData, "retreiverBuffWorkersRadius", initialRetreiverBuffWorkersRadius);

        darkCompanionBiteDamage = GetValue(dogData, "darkCompanionBiteDamage", initialDarkCompanionBiteDamage);
        darkCompanionBiteCooldown = GetValue(dogData, "darkCompanionBiteCooldown", initialDarkCompanionBiteCooldown);
        darkCompanionLaserDamage = GetValue(dogData, "darkCompanionLaserDamage", initialDarkCompanionLaserDamage);
        darkCompanionLaserCooldown = GetValue(dogData, "darkCompanionLaserCooldown", initialDarkCompanionLaserCooldown);
        darkCompanionStompDamage = GetValue(dogData, "darkCompanionStompDamage", initialDarkCompanionStompDamage);
        darkCompanionStompCooldown = GetValue(dogData, "darkCompanionStompCooldown", initialDarkCompanionStompCooldown);
        darkCompanionStompStunDuration = GetValue(dogData, "darkCompanionStompStunDuration", initialDarkCompanionStompStunDuration);

        // Dog type (enum direct)
        loadedDogType = GetValue(dogData, "dogType", Dog.DogType.GermanShepherd);

        if(Dog.Instance != null) {
            Dog.Instance.SetDogType(loadedDogType);
        }

    }


    private T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue) {
        if (dict.ContainsKey(key) && dict[key] is T value)
            return value;
        return defaultValue;
    }

    #region GET INITIAL PARAMETERS
    public int GetInitialGermanShepherdBiteDamage() {
        return initialGermanShepherdBiteDamage;
    }
    public float GetGermanShepherdInitialBiteCooldown() {
        return initialGermanShepherdBiteCooldown;
    }
    public float GetGermanShepherdInitialDigResourceCooldown() {
        return initialGermanShepherdDigResourceCooldown;
    }
    public float GetGermanShepherdInitialDigResourceProbability() {
        return initialGermanShepherdDigResourceProbability;
    }
    public float GetGermanShepherdInitialDigResourceDoubleProbability() {
        return initialGermanShepherdDigResourceDoubleProbability;
    }
    public float GetGermanShepherdInitialAmbushDetectionProbability() {
        return initialGermanShepherdAmbushDetectionProbability;
    }
    public float GetRetreiverInitialResourceDetectionRange() {
        return initialRetreiverCurrenciesDetectionRange;
    }

    public int GetInitialRetreiverBiteDamage() {
        return initialRetreiverBiteDamage;
    }
    public float GetInitialRetreiverBiteCooldown() {
        return initialRetreiverBiteCooldown;
    }
    public float GetInitialRetreiverBuffWorkersAmount() {
        return initialRetreiverBuffWorkersAmount;
    }
    public float GetInitialRetreiverBuffWorkersRadius() {
        return initialRetreiverBuffWorkersRadius;
    }

    public int GetInitialDarkCompanionBiteDamage() {
        return initialDarkCompanionBiteDamage;
    }
    public float GetInitialDarkCompanionBiteCooldown() {
        return initialDarkCompanionBiteCooldown;
    }
    public float GetInitialDarkCompanionLaserDamage() {
        return initialDarkCompanionLaserDamage;
    }
    public float GetInitialDarkCompanionLaserCooldown() {
        return initialDarkCompanionLaserCooldown;
    }
    public float GetInitialDarkCompanionStompDamage() {
        return initialDarkCompanionStompDamage;
    }
    public float GetInitialDarkCompanionStompCooldown() {
        return initialDarkCompanionStompCooldown;
    }
    public float GetInitialDarkCompanionStompStunDuration() {
        return initialDarkCompanionStompStunDuration;
    }
    #endregion

    #region GET PARAMETERS
    public bool GetRetreiverUnlocked() {
        return retreiverUnlocked;
    }
    public bool GetDarkCompanionUnlocked() {
        return darkCompanionUnlocked;
    }

    public bool GetGermanShepherdBiteAbilityUnlocked() {
        return germanShepherdBiteAbilityUnlocked || debugUnlockBiteAbility;
    }
    public bool GetGermanShepherdDigResourceAbilityUnlocked() {
        return germanShepherdDigResourceAbilityUnlocked || debugUnlockDigResourceAbility;
    }
    public bool GetGermanShepherdDetectAmbushAbilityUnlocked() {
        return germanShepherdDetectAmbushAbilityUnlocked || debugUnlockDetectAmbushAbility;
    }
    public bool GetRetreiverBiteAbilityUnlocked() {
        return retreiverBiteAbilityUnlocked || debugUnlockBiteAbility;
    }
    public bool GetRetreiverBuffWorkersAbilityUnlocked() {
        return retreiverBuffWorkersAbilityUnlocked || debugUnlockBuffWorkersAbility;
    }
    public bool GetRetreiverPickUpItemsAbilityUnlocked() {
        return retreiverPickUpItemsAbilityUnlocked || debugUnlockPickUpItemsAbility;
    }
    public bool GetDarkCompanionBiteAbilityUnlocked() {
        return darkCompanionBiteAbilityUnlocked || debugUnlockBiteAbility;
    }
    public bool GetDarkCompanionLaserAbilityUnlocked() {
        return darkCompanionLaserAbilityUnlocked || debugUnlockLaserAbility;
    }
    public bool GetDarkCompanionStompAbilityUnlocked() {
        return darkCompanionStompAbilityUnlocked || debugUnlockStompAbility;
    }

    public int GetGermanShepherdBiteDamage() {
        return germanShepherdBiteDamage;
    }

    public float GetGermanShepherdBiteCooldown() {
        return germanShepherdBiteCooldown;
    }

    public float GetGermanShepherdDigResourceCooldown() {
        return germanShepherdDigResourceCooldown;
    }
    public float GetGermanShepherdDigResourceDoubleProbability() {
        return germanShepherdDigResourceDoubleProbability;
    }
    public float GetGermanShepherdDetectAmbushProbability() {
        return germanShepherdAmbushDetectionProbability;
    }
    public float GetGermanShepherdDigResourceProbility() {
        return germanShepherdDigResourceProbability;
    }

    public int GetRetreiverBiteDamage() {
        return retreiverBiteDamage;
    }
    public float GetRetreiverBiteCooldown() {
        return retreiverBiteCooldown;
    }
    public float GetRetreiverBuffWorkersAmount() {
        return retreiverBuffWorkersAmount;
    }
    public float GetRetreiverBuffWorkersRadius() {
        return retreiverBuffWorkersRadius;
    }

    public int GetDarkCompanionBiteDamage() {
        return darkCompanionBiteDamage;
    }
    public float GetDarkCompanionBiteCooldown() {
        return darkCompanionBiteCooldown;
    }
    public int GetDarkCompanionLaserDamage() {
        return darkCompanionLaserDamage;
    }
    public float GetDarkCompanionLaserTickCooldown() {
        return initialDarkCompanionLaserTickCooldown;
    }
    public float GetDarkCompanionLaserCooldown() {
        return darkCompanionLaserCooldown;
    }
    public int GetDarkCompanionStompDamage() {
        return darkCompanionStompDamage;
    }
    public float GetDarkCompanionStompCooldown() {
        return darkCompanionStompCooldown;
    }
    public float GetDarkCompanionStompStunDuration() {
        return darkCompanionStompStunDuration;
    }
    #endregion

    #region SET PARAMETERS

    public void UnlockRetreiver() {
        retreiverUnlocked = true;
        Dog.Instance.SetDogType(Dog.DogType.GoldenRetreiver);
        OnNewDogUnlocked?.Invoke(this, EventArgs.Empty);
        ES3.Save("retreiverUnlocked", retreiverUnlocked);
    }
    public void UnlockDarkCompanion() {
        darkCompanionUnlocked = true;
        Dog.Instance.SetDogType(Dog.DogType.DarkCompanion);
        OnNewDogUnlocked?.Invoke(this, EventArgs.Empty);
        ES3.Save("darkCompanionUnlocked", darkCompanionUnlocked);
    }

    public void UnlockGermanShepherdBiteAbility() {
        germanShepherdBiteAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockGermanShepherdDigResourceAbility() {
        germanShepherdDigResourceAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockGermanShepherdDetectAmbushAbility() {
        germanShepherdDetectAmbushAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockRetreiverBiteAbility() {
        retreiverBiteAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockRetreiverBuffWorkersAbility() {
        retreiverBuffWorkersAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockRetreiverPickUpItemsAbility() {
        retreiverPickUpItemsAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockDarkCompanionBiteAbility() {
        darkCompanionBiteAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockDarkCompanionLaserAbility() {
        darkCompanionLaserAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }
    public void UnlockDarkCompanionStompAbility() {
        darkCompanionStompAbilityUnlocked = true;
        OnNewAbilityUnlocked?.Invoke(this, EventArgs.Empty);
    }

    public void BuffGermanShepherdAmbushDetectionProbability(float ambushDetectionProbabilityAbsoluteBuff) {
        this.germanShepherdAmbushDetectionProbability = initialGermanShepherdAmbushDetectionProbability + ambushDetectionProbabilityAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffGermanShepherdBiteDamage(int biteDamageAbsoluteBuff) {
        this.germanShepherdBiteDamage = initialGermanShepherdBiteDamage + biteDamageAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffGermanShepherdDigResourceCooldown(float digResourceCooldownAbsoluteBuff) {
        this.germanShepherdDigResourceCooldown = initialGermanShepherdDigResourceCooldown + digResourceCooldownAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffGermanShepherdDigResourceProbability(float absoluteBuff) {
        this.germanShepherdDigResourceProbability = initialGermanShepherdDigResourceProbability + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffGermanShepherdDigResourceDoubleProbability(float absoluteBuff) {
        this.germanShepherdDigResourceDoubleProbability = initialGermanShepherdDigResourceDoubleProbability + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffGermanShepherdBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.germanShepherdBiteCooldown = initialGermanShepherdBiteCooldown + biteCooldownAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void BuffRetreiverBiteDamage(int biteDamageAbsoluteBuff) {
        this.retreiverBiteDamage = initialRetreiverBiteDamage + biteDamageAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffRetreiverBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.retreiverBiteCooldown = initialRetreiverBiteCooldown + biteCooldownAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffRetreiverBuffWorkersAmount(float absoluteBuff) {
        this.retreiverBuffWorkersAmount = initialRetreiverBuffWorkersAmount + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffRetreiverBuffWorkersRadius(float absoluteBuff) {
        this.retreiverBuffWorkersRadius = initialRetreiverBuffWorkersRadius + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void BuffDarkCompanionBiteDamage(int biteDamageAbsoluteBuff) {
        this.darkCompanionBiteDamage = initialDarkCompanionBiteDamage + biteDamageAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffDarkCompanionBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.darkCompanionBiteCooldown = initialDarkCompanionBiteCooldown + biteCooldownAbsoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffDarkCompanionLaserDamage(int absoluteBuff) {
        this.darkCompanionLaserDamage = initialDarkCompanionLaserDamage + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffDarkCompanionLaserCooldown(float absoluteBuff) {
        this.darkCompanionLaserCooldown = initialDarkCompanionLaserCooldown + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffDarkCompanionStompDamage(int absoluteBuff) {
        this.darkCompanionStompDamage = initialDarkCompanionStompDamage + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffDarkCompanionStompCooldown(float absoluteBuff) {
        this.darkCompanionStompCooldown = initialDarkCompanionStompCooldown + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    public void BuffDarkCompanionStompStunDuration(float absoluteBuff) {
        this.darkCompanionStompStunDuration = initialDarkCompanionStompStunDuration + absoluteBuff;
        OnAbilityUpgraded?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    public bool GetDogUnlocked(Dog.DogType dogType) {
        if (dogType == Dog.DogType.GermanShepherd) return true;
        if (dogType == Dog.DogType.GoldenRetreiver) return retreiverUnlocked;
        if (dogType == Dog.DogType.DarkCompanion) return darkCompanionUnlocked;
        return false;
    }

    public Sprite GetDogIconSprite(Dog.DogType type) {
        if(type == Dog.DogType.GermanShepherd) {
            return germanShepherdIcon;
        }

        if(type == Dog.DogType.GoldenRetreiver) {
            return retreiverIcon;
        }

        if(type == Dog.DogType.DarkCompanion) {
            return darkCompanionIcon;
        }
        return germanShepherdIcon;
    }

    #region SAVE PARAMETERS
    public void SaveDogStats() {
        var dogData = new Dictionary<string, object>();

        // Abilities
        dogData["germanShepherdBiteAbilityUnlocked"] = germanShepherdBiteAbilityUnlocked;
        dogData["germanShepherdDigResourceAbilityUnlocked"] = germanShepherdDigResourceAbilityUnlocked;
        dogData["germanShepherdDetectAmbushAbilityUnlocked"] = germanShepherdDetectAmbushAbilityUnlocked;

        dogData["retreiverBiteAbilityUnlocked"] = retreiverBiteAbilityUnlocked;
        dogData["retreiverBuffWorkersAbilityUnlocked"] = retreiverBuffWorkersAbilityUnlocked;
        dogData["retreiverPickUpItemsAbilityUnlocked"] = retreiverPickUpItemsAbilityUnlocked;

        dogData["darkCompanionBiteAbilityUnlocked"] = darkCompanionBiteAbilityUnlocked;
        dogData["darkCompanionLaserAbilityUnlocked"] = darkCompanionLaserAbilityUnlocked;
        dogData["darkCompanionStompAbilityUnlocked"] = darkCompanionStompAbilityUnlocked;

        // Stats
        dogData["germanShepherdBiteDamage"] = germanShepherdBiteDamage;
        dogData["germanShepherdBiteCooldown"] = germanShepherdBiteCooldown;
        dogData["germanShepherdDigResourceCooldown"] = germanShepherdDigResourceCooldown;
        dogData["germanShepherdDigResourceProbability"] = germanShepherdDigResourceProbability;
        dogData["germanShepherdAmbushDetectionProbability"] = germanShepherdAmbushDetectionProbability;
        dogData["germanShepherdDigResourceDoubleProbability"] = germanShepherdDigResourceDoubleProbability;

        dogData["retreiverBiteDamage"] = retreiverBiteDamage;
        dogData["retreiverBiteCooldown"] = retreiverBiteCooldown;
        dogData["retreiverBuffWorkersAmount"] = retreiverBuffWorkersAmount;
        dogData["retreiverBuffWorkersRadius"] = retreiverBuffWorkersRadius;

        dogData["darkCompanionBiteDamage"] = darkCompanionBiteDamage;
        dogData["darkCompanionBiteCooldown"] = darkCompanionBiteCooldown;
        dogData["darkCompanionLaserDamage"] = darkCompanionLaserDamage;
        dogData["darkCompanionLaserCooldown"] = darkCompanionLaserCooldown;
        dogData["darkCompanionStompDamage"] = darkCompanionStompDamage;
        dogData["darkCompanionStompCooldown"] = darkCompanionStompCooldown;
        dogData["darkCompanionStompStunDuration"] = darkCompanionStompStunDuration;

        // Dog type (enum direct)
        dogData["dogType"] = Dog.Instance.GetDogType();

        ES3.Save("DogStats", dogData);
    }
    #endregion
}
