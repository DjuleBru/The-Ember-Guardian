using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogStats : MonoBehaviour {

    public static DogStats Instance;

    private bool biteAbilityUnlocked;
    private bool digResourceAbilityUnlocked;
    private bool detectAmbushAbilityUnlocked;

    private int biteDamage;
    private float biteCooldown;
    private float digResourceCooldown;
    private float digResourceProbability;
    private float digResourceDoubleProbability;
    private float ambushDetectionProbability;

    [SerializeField] private int initialBiteDamage;
    [SerializeField] private float initialBiteCooldown;
    [SerializeField] private float initialDigResourceCooldown;
    [SerializeField] private float initialDigResourceProbability;
    [SerializeField] private float initialDigResourceDoubleProbability;
    [SerializeField] private float initialAmbushDetectionProbability;

    [SerializeField] private bool debugUnlockBiteAbility;
    [SerializeField] private bool debugUnlockDigResourceAbility;
    [SerializeField] private bool debugUnlockDetectAmbushAbility;

    private void Awake() {
        Instance = this;
        InitializeParameters();
    }
    private void InitializeParameters() {
        LoadSavedDogStats();
    }

    private void LoadSavedDogStats() {
        biteAbilityUnlocked = ES3.Load("biteAbilityUnlocked", false);
        digResourceAbilityUnlocked = ES3.Load("digResourceAbilityUnlocked", false);
        detectAmbushAbilityUnlocked = ES3.Load("detectAmbushAbilityUnlocked", false);

        biteDamage = ES3.Load("biteDamage", initialBiteDamage);
        biteCooldown = ES3.Load("biteCooldown", initialBiteCooldown);
        digResourceCooldown = ES3.Load("digResourceCooldown", initialDigResourceCooldown);
        digResourceProbability = ES3.Load("digResourceProbability", initialDigResourceProbability);
        digResourceDoubleProbability = ES3.Load("digResourceDoubleProbability", initialDigResourceDoubleProbability);
        ambushDetectionProbability = ES3.Load("ambushDetectionProbability", initialAmbushDetectionProbability);
    }

    #region GET INITIAL PARAMETERS
    public int GetInitialBiteDamage() {
        return initialBiteDamage;
    }
    public float GetInitialBiteCooldown() {
        return initialBiteCooldown;
    }
    public float GetInitialDigResourceCooldown() {
        return initialDigResourceCooldown;
    }
    public float GetInitialDigResourceProbability() {
        return initialDigResourceProbability;
    }
    public float GetInitialDigResourceDoubleProbability() {
        return initialDigResourceDoubleProbability;
    }
    public float GetInitialAmbushDetectionProbability() {
        return initialAmbushDetectionProbability;
    }
    #endregion

    #region GET PARAMETERS
    public bool GetBiteAbilityUnlocked() {
        return biteAbilityUnlocked || debugUnlockBiteAbility;
    }
    public bool GetdigResourceAbilityUnlocked() {
        return digResourceAbilityUnlocked || debugUnlockDigResourceAbility;
    }
    public bool GetDetectAmbushAbilityUnlocked() {
        return detectAmbushAbilityUnlocked || debugUnlockDetectAmbushAbility;
    }

    public int GetBiteDamage() {
        return biteDamage;
    }

    public float GetBiteCooldown() {
        return biteCooldown;
    }

    public float GetDigResourceCooldown() {
        return digResourceCooldown;
    }
    public float GetDigResourceDoubleProbability() {
        return digResourceDoubleProbability;
    }
    public float GetDetectAmbushProbability() {
        return ambushDetectionProbability;
    }
    public float GetDigResourceProbility() {
        return digResourceProbability;
    }
    #endregion

    #region SET PARAMETERS
    public void UnlockBiteAbility() {
        biteAbilityUnlocked = true;
    }
    public void UnlockDigResourceAbility() {
        digResourceAbilityUnlocked = true;
    }
    public void UnlockDetectAmbushAbility() {
        detectAmbushAbilityUnlocked = true;
    }

    public void BuffAmbushDetectionProbability(float ambushDetectionProbabilityAbsoluteBuff) {
        this.ambushDetectionProbability = initialAmbushDetectionProbability + ambushDetectionProbabilityAbsoluteBuff;
    }
    public void BuffBiteDamage(int biteDamageAbsoluteBuff) {
        this.biteDamage = initialBiteDamage + biteDamageAbsoluteBuff;
    }
    public void BuffDigResourceCooldown(float digResourceCooldownAbsoluteBuff) {
        this.digResourceCooldown = initialDigResourceCooldown + digResourceCooldownAbsoluteBuff;
    }
    public void BuffDigResourceProbability(float digResourceProbabilityAbsoluteBuff) {
        this.digResourceProbability = initialDigResourceProbability + digResourceProbabilityAbsoluteBuff;
    }
    public void BuffDigResourceDoubleProbability(float digResourceDoubleProbabilityAbsoluteBuff) {
        this.digResourceDoubleProbability = initialDigResourceDoubleProbability + digResourceDoubleProbabilityAbsoluteBuff;
    }
    public void BuffBiteCooldown(float biteCooldownAbsoluteBuff) {
        this.biteCooldown = initialBiteCooldown + biteCooldownAbsoluteBuff;
    }
    #endregion

    #region SAVE PARAMETERS
    public void SaveDogStats() {
        ES3.Save("biteAbilityUnlocked", biteAbilityUnlocked);
        ES3.Save("digResourceAbilityUnlocked", digResourceAbilityUnlocked);
        ES3.Save("detectAmbushAbilityUnlocked", detectAmbushAbilityUnlocked);

        ES3.Save("biteDamage", biteDamage);
        ES3.Save("biteCooldown", biteCooldown);
        ES3.Save("digResourceCooldown", digResourceCooldown);
        ES3.Save("digResourceProbability", digResourceProbability);
        ES3.Save("ambushDetectionProbability", ambushDetectionProbability);
        ES3.Save("digResourceDoubleProbability", digResourceDoubleProbability);
    }
    #endregion
}
