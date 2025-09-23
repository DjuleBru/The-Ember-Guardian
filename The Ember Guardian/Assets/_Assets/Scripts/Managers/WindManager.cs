using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
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

    [SerializeField] private ParticleSystem windPS_singlePixels;
    [SerializeField] private Color greenPSStartColor;
    [SerializeField] private Color greenPSEndColor;
    [SerializeField] private Color bluePSStartColor;
    [SerializeField] private Color bluePSEndColor;
    [SerializeField] private Color redPSStartColor;
    [SerializeField] private Color redPSEndColor;

    private bool hasWind;
    private bool debugMode;
    private WindStrength currentWindStrength = WindStrength.none;
    private WindStrength windStrengthOutside = WindStrength.none;
    private float currentWindDir;
    private List<WindStrength> windStrengthAllowedInLevel;
    private WindStrength previousWindStrength = WindStrength.none;
    private Transform cameraTransform;

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

        SetWindStrength(LevelManager.Instance.GetLevelSO().initialWindStrength);
        windStrengthOutside = currentWindStrength;
        RandomizeWindDir();
        SetWindPSColor();

        debugMode = DebugManager.Instance.GetDebugMode_WindManager();
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        if(currentWindStrength != WindStrength.none) {
            SetWindStrength(WindStrength.none);
            windStrengthOutside = WindStrength.none;
        }
    }

    private void Update() {
        WindFollowCamera();

        if(debugMode) {
            HandleDebugWindInput();
        }
    }

    private void SetWindPSColor() {
        LevelSO.LevelEnvironment environment = LevelManager.Instance.GetLevelSO().environmentType;
        var main = windPS_singlePixels.main;

        switch (environment) {

            case (LevelSO.LevelEnvironment.TheVerdantGraveyard):
                main.startColor = new ParticleSystem.MinMaxGradient(greenPSStartColor,greenPSEndColor);

                break;
            case (LevelSO.LevelEnvironment.TheLostGreens):
                main.startColor = new ParticleSystem.MinMaxGradient(greenPSStartColor, greenPSEndColor);
                break;
            case (LevelSO.LevelEnvironment.TheLumenHollow):
                main.startColor = new ParticleSystem.MinMaxGradient(bluePSStartColor, bluePSEndColor);
                break;
            case (LevelSO.LevelEnvironment.CorruptedCity):
                main.startColor = new ParticleSystem.MinMaxGradient(redPSStartColor, redPSEndColor);
                break;
        }

    }
    private void WindFollowCamera() {
        cameraTransform = Camera.main.transform;
        Vector3 newPosition = new Vector3(cameraTransform.position.x, cameraTransform.position.y-4f, 0);

        windPS_singlePixels.transform.position = newPosition;
    }

    private void HandleDebugWindInput() {
        if (Input.GetKeyDown(KeyCode.F)) {
            RandomizeWindDir();
            SetWindStrength(WindStrength.none);
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            RandomizeWindDir();
            SetWindStrength(WindStrength.soft);
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            RandomizeWindDir();
            SetWindStrength(WindStrength.medium);
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            RandomizeWindDir();
            SetWindStrength(WindStrength.strong);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            RandomizeWindDir();
            SetWindStrength(WindStrength.extreme);
        }

    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (!hasWind) return;
        if (windStrengthAllowedInLevel.Count == 0) return;

        RandomizeWindDir();

        if (previousWindStrength == WindStrength.strong || previousWindStrength == WindStrength.extreme) {
            SetWindStrength(WindStrength.none);
        }
        else {
            WindStrength newWindStrength = windStrengthAllowedInLevel[UnityEngine.Random.Range(0, windStrengthAllowedInLevel.Count)];
            SetWindStrength(newWindStrength);
        }

        windStrengthOutside = currentWindStrength;
        previousWindStrength = currentWindStrength;
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

    public void DisableWind() {
        hasWind = false;
    }

    public float GetWindDir() {
        return currentWindDir;
    }

    private void SetWindStrength(WindStrength windStrength) {
        currentWindStrength = windStrength;
        OnWindStrengthChanged?.Invoke(this, EventArgs.Empty);

        SetWindPS(windStrength);

        SetWindPSForce();
    }

    private void SetWindPS(WindStrength windStrength) {
        ParticleSystem.EmissionModule emission = windPS_singlePixels.emission;

        switch (windStrength) {
            case WindStrength.none:
                emission.rateOverTime = 0;
            break;
            case WindStrength.soft:
                emission.rateOverTime = 2;
                break;
            case WindStrength.medium:
                emission.rateOverTime = 5;
                break;
            case WindStrength.strong:
                emission.rateOverTime = 10;
                break;
            case WindStrength.extreme:
                emission.rateOverTime = 15;
                break;

        }
    }

    private void SetWindPSForce() {
        var force = windPS_singlePixels.forceOverLifetime;
        force.enabled = true;

        // On récupère le signe du vent
        float sign = Mathf.Sign(currentWindDir);

        // Ici, tu définis les bornes min/max de la force.
        // Exemple : entre 0.5 et 1.0, multiplié par le signe.
        float min = GetMinPSForce() * sign;
        float max = GetMaxPSForce() * sign;

        force.x = new ParticleSystem.MinMaxCurve(min, max);
    }

    public float GetMinPSForce() {
        float minForce = 0f;
        switch (currentWindStrength) {
            case WindStrength.none:
                minForce = 0;
                break;
            case WindStrength.soft:
                minForce = 5;
                break;
            case WindStrength.medium:
                minForce = 10;
                break;
            case WindStrength.strong:
                minForce = 15;
                break;
            case WindStrength.extreme:
                minForce = 25;
                break;
        }
        return minForce;
    }

    public float GetMaxPSForce() {
        float maxForce = 0f;
        switch (currentWindStrength) {
            case WindStrength.none:
                maxForce = 0;
                break;
            case WindStrength.soft:
                maxForce = 10;
                break;
            case WindStrength.medium:
                maxForce = 20;
                break;
            case WindStrength.strong:
                maxForce = 30;
                break;
            case WindStrength.extreme:
                maxForce = 50;
                break;
        }
        return maxForce;
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

    public void SetInCavern(bool inCavern) {
        if (inCavern) {
            SetWindStrength(WindStrength.none);
        }
        else {
            SetWindStrength(windStrengthOutside);
        }
    }

    private void OnDestroy() {
        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
            DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
        }
    }

}
