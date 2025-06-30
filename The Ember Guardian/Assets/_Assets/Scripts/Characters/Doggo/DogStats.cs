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
    private bool retreiverBiteAbilityUnlocked;
    private bool retreiverBuffWorkersAbilityUnlocked;
    private bool retreiverPickUpItemsAbilityUnlocked;
    private bool darkCompanionBiteAbilityUnlocked;
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

    [SerializeField] private int initialDarkCompanionBiteDamage;
    [SerializeField] private float initialDarkCompanionBiteCooldown;
    [SerializeField] private int initialDarkCompanionLaserDamage;
    [SerializeField] private float initialDarkCompanionLaserCooldown;
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

    private void Awake() {
        Instance = this;
        InitializeParameters();
    }
    private void InitializeParameters() {
        LoadSavedDogStats();
    }

    private void LoadSavedDogStats() {
        retreiverUnlocked = ES3.Load("retreiverUnlocked", false);
        darkCompanionUnlocked = ES3.Load("darkCompanionUnlocked", false);

        germanShepherdBiteAbilityUnlocked = ES3.Load("germanShepherdBiteAbilityUnlocked", false);
        germanShepherdDigResourceAbilityUnlocked = ES3.Load("germanShepherdDigResourceAbilityUnlocked", false);
        germanShepherdDetectAmbushAbilityUnlocked = ES3.Load("germanShepherdDetectAmbushAbilityUnlocked", false);
        retreiverBiteAbilityUnlocked = ES3.Load("retreiverBiteAbilityUnlocked", false);
        retreiverBuffWorkersAbilityUnlocked = ES3.Load("retreiverBuffWorkersAbilityUnlocked", false);
        retreiverPickUpItemsAbilityUnlocked = ES3.Load("retreiverPickUpItemsAbilityUnlocked", false);
        darkCompanionBiteAbilityUnlocked = ES3.Load("darkCompanionBiteAbilityUnlocked", false);
        darkCompanionLaserAbilityUnlocked = ES3.Load("darkCompanionLaserAbilityUnlocked", false);
        darkCompanionStompAbilityUnlocked = ES3.Load("darkCompanionStompAbilityUnlocked", false);

        germanShepherdBiteDamage = ES3.Load("germanShepherdBiteDamage", initialGermanShepherdBiteDamage);
        germanShepherdBiteCooldown = ES3.Load("germanShepherdBiteCooldown", initialGermanShepherdBiteCooldown);
        germanShepherdDigResourceCooldown = ES3.Load("germanShepherdDigResourceCooldown", initialGermanShepherdDigResourceCooldown);
        germanShepherdDigResourceProbability = ES3.Load("germanShepherdDigResourceProbability", initialGermanShepherdDigResourceProbability);
        germanShepherdDigResourceDoubleProbability = ES3.Load("germanShepherdDigResourceDoubleProbability", initialGermanShepherdDigResourceDoubleProbability);
        germanShepherdAmbushDetectionProbability = ES3.Load("germanShepherdAmbushDetectionProbability", initialGermanShepherdAmbushDetectionProbability);

        retreiverBiteDamage = ES3.Load("retreiverBiteDamage", initialRetreiverBiteDamage);
        retreiverBiteCooldown = ES3.Load("retreiverBiteCooldown", initialRetreiverBiteCooldown);
        retreiverBuffWorkersAmount = ES3.Load("retreiverBuffWorkersAmount", initialRetreiverBuffWorkersAmount);
        retreiverBuffWorkersRadius = ES3.Load("retreiverBuffWorkersRadius", initialRetreiverBuffWorkersRadius);

        darkCompanionBiteDamage = ES3.Load("darkCompanionBiteDamage", initialDarkCompanionBiteDamage);
        darkCompanionBiteCooldown = ES3.Load("darkCompanionBiteCooldown", initialDarkCompanionBiteCooldown);
        darkCompanionLaserDamage = ES3.Load("darkCompanionLaserDamage", initialDarkCompanionLaserDamage);
        darkCompanionLaserCooldown = ES3.Load("darkCompanionLaserCooldown", initialDarkCompanionLaserCooldown);
        darkCompanionStompDamage = ES3.Load("darkCompanionStompDamage", initialDarkCompanionStompDamage);
        darkCompanionStompCooldown = ES3.Load("darkCompanionStompCooldown", initialDarkCompanionStompCooldown);
        darkCompanionStompStunDuration = ES3.Load("darkCompanionStompStunDuration", initialDarkCompanionStompStunDuration);
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
    }
    public void UnlockDarkCompanion() {
        darkCompanionUnlocked = true;
        Dog.Instance.SetDogType(Dog.DogType.DarkCompanion);
        Debug.Log("UnlockDarkCompanion");
    }

    public void UnlockGermanShepherdBiteAbility() {
        germanShepherdBiteAbilityUnlocked = true;
    }
    public void UnlockGermanShepherdDigResourceAbility() {
        germanShepherdDigResourceAbilityUnlocked = true;
    }
    public void UnlockGermanShepherdDetectAmbushAbility() {
        germanShepherdDetectAmbushAbilityUnlocked = true;
    }
    public void UnlockRetreiverBiteAbility() {
        retreiverBiteAbilityUnlocked = true;
    }
    public void UnlockRetreiverBuffWorkersAbility() {
        retreiverBuffWorkersAbilityUnlocked = true;
    }
    public void UnlockRetreiverPickUpItemsAbility() {
        retreiverPickUpItemsAbilityUnlocked = true;
    }
    public void UnlockDarkCompanionBiteAbility() {
        darkCompanionBiteAbilityUnlocked = true;
    }
    public void UnlockDarkCompanionLaserAbility() {
        darkCompanionLaserAbilityUnlocked = true;
    }
    public void UnlockDarkCompanionStompAbility() {
        darkCompanionStompAbilityUnlocked = true;
    }

    public void BuffGermanShepherdAmbushDetectionProbability(float ambushDetectionProbabilityAbsoluteBuff) {
        this.germanShepherdAmbushDetectionProbability = initialGermanShepherdAmbushDetectionProbability + ambushDetectionProbabilityAbsoluteBuff;
    }
    public void BuffGermanShepherdBiteDamage(int biteDamageAbsoluteBuff) {
        this.germanShepherdBiteDamage = initialGermanShepherdBiteDamage + biteDamageAbsoluteBuff;
    }
    public void BuffGermanShepherdDigResourceCooldown(float digResourceCooldownAbsoluteBuff) {
        this.germanShepherdDigResourceCooldown = initialGermanShepherdDigResourceCooldown + digResourceCooldownAbsoluteBuff;
    }
    public void BuffGermanShepherdDigResourceProbability(float absoluteBuff) {
        this.germanShepherdDigResourceProbability = initialGermanShepherdDigResourceProbability + absoluteBuff;
    }
    public void BuffGermanShepherdDigResourceDoubleProbability(float absoluteBuff) {
        this.germanShepherdDigResourceDoubleProbability = initialGermanShepherdDigResourceDoubleProbability + absoluteBuff;
    }
    public void BuffGermanShepherdBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.germanShepherdBiteCooldown = initialGermanShepherdBiteCooldown + biteCooldownAbsoluteBuff;
    }

    public void BuffRetreiverBiteDamage(int biteDamageAbsoluteBuff) {
        this.retreiverBiteDamage = initialRetreiverBiteDamage + biteDamageAbsoluteBuff;
    }
    public void BuffRetreiverBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.retreiverBiteCooldown = initialRetreiverBiteCooldown + biteCooldownAbsoluteBuff;
    }
    public void BuffRetreiverBuffWorkersAmount(float absoluteBuff) {
        this.retreiverBuffWorkersAmount = initialRetreiverBuffWorkersAmount + absoluteBuff;
    }
    public void BuffRetreiverBuffWorkersRadius(float absoluteBuff) {
        this.retreiverBuffWorkersRadius = initialRetreiverBuffWorkersRadius + absoluteBuff;
    }

    public void BuffDarkCompanionBiteDamage(int biteDamageAbsoluteBuff) {
        this.darkCompanionBiteDamage = initialDarkCompanionBiteDamage + biteDamageAbsoluteBuff;
    }
    public void BuffDarkCompanionBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.darkCompanionBiteCooldown = initialDarkCompanionBiteCooldown + biteCooldownAbsoluteBuff;
    }
    public void BuffDarkCompanionLaserDamage(int absoluteBuff) {
        this.darkCompanionLaserDamage = initialDarkCompanionLaserDamage + absoluteBuff;
    }
    public void BuffDarkCompanionLaserCooldown(float absoluteBuff) {
        this.darkCompanionLaserCooldown = initialDarkCompanionLaserCooldown + absoluteBuff;
    }
    public void BuffDarkCompanionStompDamage(int absoluteBuff) {
        this.darkCompanionStompDamage = initialDarkCompanionStompDamage + absoluteBuff;
    }
    public void BuffDarkCompanionStompCooldown(float absoluteBuff) {
        this.darkCompanionStompCooldown = initialDarkCompanionStompCooldown + absoluteBuff;
    }
    public void BuffDarkCompanionStompStunDuration(float absoluteBuff) {
        this.darkCompanionStompStunDuration = initialDarkCompanionStompStunDuration + absoluteBuff;
    }
    #endregion

    #region SAVE PARAMETERS
    public void SaveDogStats() {
        ES3.Save("retreiverUnlocked", retreiverUnlocked);
        ES3.Save("darkCompanionUnlocked", darkCompanionUnlocked);

        ES3.Save("germanShepherdBiteAbilityUnlocked", germanShepherdBiteAbilityUnlocked);
        ES3.Save("germanShepherdDigResourceAbilityUnlocked", germanShepherdDigResourceAbilityUnlocked);
        ES3.Save("germanShepherdDetectAmbushAbilityUnlocked", germanShepherdDetectAmbushAbilityUnlocked);
        ES3.Save("retreiverBiteAbilityUnlocked", retreiverBiteAbilityUnlocked);
        ES3.Save("retreiverBuffWorkersAbilityUnlocked", retreiverBuffWorkersAbilityUnlocked);
        ES3.Save("retreiverPickUpItemsAbilityUnlocked", retreiverPickUpItemsAbilityUnlocked);
        ES3.Save("darkCompanionBiteAbilityUnlocked", darkCompanionBiteAbilityUnlocked);
        ES3.Save("darkCompanionLaserAbilityUnlocked", darkCompanionLaserAbilityUnlocked);
        ES3.Save("darkCompanionStompAbilityUnlocked", darkCompanionStompAbilityUnlocked);

        ES3.Save("germanShepherdBiteDamage", germanShepherdBiteDamage);
        ES3.Save("germanShepherdBiteCooldown", germanShepherdBiteCooldown);
        ES3.Save("germanShepherdDigResourceCooldown", germanShepherdDigResourceCooldown);
        ES3.Save("germanShepherdDigResourceProbability", germanShepherdDigResourceProbability);
        ES3.Save("germanShepherdAmbushDetectionProbability", germanShepherdAmbushDetectionProbability);
        ES3.Save("germanShepherdDigResourceDoubleProbability", germanShepherdDigResourceDoubleProbability);

        ES3.Save("retreiverBiteDamage", retreiverBiteDamage);
        ES3.Save("retreiverBiteCooldown", retreiverBiteCooldown);
        ES3.Save("retreiverBuffWorkersAmount", retreiverBuffWorkersAmount);
        ES3.Save("retreiverBuffWorkersRadius", retreiverBuffWorkersRadius);

        ES3.Save("darkCompanionLaserDamage", darkCompanionLaserDamage);
        ES3.Save("darkCompanionLaserCooldown", darkCompanionLaserCooldown);
        ES3.Save("darkCompanionStompDamage", darkCompanionStompDamage);
        ES3.Save("darkCompanionStompCooldown", darkCompanionStompCooldown);
        ES3.Save("darkCompanionStompStunDuration", darkCompanionStompStunDuration);

        ES3.Save("dogType", Dog.Instance.GetDogType());
    }
    #endregion
}
