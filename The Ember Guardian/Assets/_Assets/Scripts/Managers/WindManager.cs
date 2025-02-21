using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance;

    public enum WindStrength {
        none,
        soft,
        medium,
        strong,
        extreme,
    }

    private bool hasWind;
    private bool debugMode;
    private WindStrength currentWindStrength = WindStrength.none;
    private float currentWindDir;
    private List<WindStrength> windStrengthAllowedInLevel;

    public event EventHandler OnWindStrengthChanged;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;

        hasWind = LevelManager.Instance.GetLevelSO().hasWind;

        if(hasWind) {
            windStrengthAllowedInLevel = LevelManager.Instance.GetLevelSO().windStrengthsAllowedInLevel;
        } else {
            windStrengthAllowedInLevel = new List<WindStrength> { WindStrength.none };
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        currentWindStrength = LevelManager.Instance.GetLevelSO().initialWindStrength;
        RandomizeWindDir();

        debugMode = DebugManager.Instance.GetDebugMode_WindManager();
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        if(currentWindStrength != WindStrength.none) {
            currentWindStrength = WindStrength.none;
            OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update() {
        if(debugMode) {
            HandleDebugWindInput();
        }
    }

    private void HandleDebugWindInput() {
        if (Input.GetKeyDown(KeyCode.F)) {
            RandomizeWindDir();
            currentWindStrength = WindStrength.none;
            OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            RandomizeWindDir();
            currentWindStrength = WindStrength.soft;
            OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            RandomizeWindDir();
            currentWindStrength = WindStrength.medium;
            OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            RandomizeWindDir();
            currentWindStrength = WindStrength.strong;
            OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            RandomizeWindDir();
            currentWindStrength = WindStrength.extreme;
            OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        currentWindStrength = windStrengthAllowedInLevel[UnityEngine.Random.Range(0, windStrengthAllowedInLevel.Count)];
        RandomizeWindDir();

        OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RandomizeWindDir() {
        int windDir = UnityEngine.Random.Range(-1, 1);
        currentWindDir = windDir;

        if (windDir == 0) {
            currentWindDir = 1;
        }
    }

    public WindStrength GetWindStrength() {
        return currentWindStrength;
    }

    public float GetWindDir() {
        return currentWindDir;
    }

    public float GetWindStrengthImpactOnSpeed() {


        if (currentWindStrength == WindStrength.medium) {
            return 1.1f;
        }
        if (currentWindStrength == WindStrength.strong) {
            return 1.2f;
        }
        if (currentWindStrength == WindStrength.extreme) {
            return 1.3f;
        }

        return 1;
    }

    private void OnDestroy() {
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
    }

}
