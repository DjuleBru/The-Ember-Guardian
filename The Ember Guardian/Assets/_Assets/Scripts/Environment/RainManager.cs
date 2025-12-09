using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC.Utilities;
using UnityEngine;
using Water2D;
using static WindManager;

public class RainManager : MonoBehaviour
{
    public static RainManager Instance;


    private ModernWater2D modernWater2D;

    [SerializeField] private ParticleSystem rainPS_GroundCollisions;
    [SerializeField] private ParticleSystem rainPS_NoCollisions;
    [SerializeField] private SpriteRenderer rainFrontFogSpriteRenderer;

    [SerializeField] private ParticleSystem rainPS_SubEmitter;
    private Transform cameraTransform;

    public enum RainIntensity {
        none,
        sparse,
        medium,
        strong,
        extreme,
    }

    private List<RainIntensity> allRainLevels = new List<RainIntensity>();
    private List<RainIntensity> rainIntensitiesAllowedInLevel;
    private RainIntensity currentRainIntensity;
    private bool hasRain;
    private bool debugMode;

    public event EventHandler OnRainIntensityChanged;

    private float sparseRainFrontFogAlpha = .15f;
    private float mediumRainFrontFogAlpha = .23f;
    private float highRainFrontFogAlpha = .4f;
    private float extremeRainFrontFogAlpha = .4f;

    private int sparseRainPSEmission = 30;
    private int mediumRainPSEmission = 70;
    private int highRainPSEmission = 150;
    private int extremeRainPSEmission = 300;

    private float sparseRainPSSize = .03f;
    private float mediumRainPSSize = .04f;
    private float highRainPSSize = .06f;
    private float extremeRainPSSize = .08f;

    private float sparseRainSubEmitterPSSize = .03f;
    private float mediumRainSubEmitterPSSize = .04f;
    private float highRainSubEmitterPSSize = .05f;
    private float extremeRainSubEmitterPSSize = .06f;

    private float sparseRainSpeed = .3f;
    private float mediumRainSpeed = .75f;
    private float highRainSpeed = 1f;
    private float extremeRainSpeed = 1.5f;

    private float sparseRainStrength = .3f;
    private float mediumRainStrength = .45f;
    private float highRainStrength = .5f;
    private float extremeRainStrength = .6f;


    private void Awake()
    {
        Instance = this;
        InitializeRainLevels();

        rainFrontFogSpriteRenderer.material.SetFloat("_Alpha", 0);
        ParticleSystem.EmissionModule emission_Collisions = rainPS_GroundCollisions.emission;
        ParticleSystem.EmissionModule emission_NoCollisions = rainPS_NoCollisions.emission;
        emission_Collisions.rateOverTime = 0;
        emission_NoCollisions.rateOverTime = 0;
    }

    private void Start()
    {
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) return;

        modernWater2D = WaterManager.Instance.GetComponent<ModernWater2D>();
        debugMode = DebugManager.Instance.GetDebugMode_RainManager();

        cameraTransform = Camera.main.transform;

        hasRain = LevelManager.Instance.GetLevelSO().hasRain;

        if (hasRain)
        {
            rainIntensitiesAllowedInLevel = LevelManager.Instance.GetLevelSO().rainIntensitiesAllowedInLevel;
        }
        else
        {
            rainIntensitiesAllowedInLevel = new List<RainIntensity> { RainIntensity.none };
        }

        if(debugMode) {
            rainIntensitiesAllowedInLevel = allRainLevels;
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
    }

    private void Update() {
        RainFollowCamera();

        if (!debugMode) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            RandomizeRainDir();
            currentRainIntensity = RainIntensity.none;
            SetRainLevel(currentRainIntensity);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            RandomizeRainDir();
            currentRainIntensity = RainIntensity.sparse;
            SetRainLevel(currentRainIntensity);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            RandomizeRainDir();
            currentRainIntensity = RainIntensity.medium;
            SetRainLevel(currentRainIntensity);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            RandomizeRainDir();
            currentRainIntensity = RainIntensity.strong;
            SetRainLevel(currentRainIntensity);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            RandomizeRainDir();
            currentRainIntensity = RainIntensity.extreme;
            SetRainLevel(currentRainIntensity);
        }
    }
    private void RainFollowCamera()
    {
        cameraTransform = Camera.main.transform;
        Vector3 newPosition = new Vector3(cameraTransform.position.x, cameraTransform.position.y, 0);

        rainPS_GroundCollisions.transform.position = newPosition;
        rainPS_NoCollisions.transform.position = newPosition;
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e)
    {
        if (!hasRain) return;
        if (rainIntensitiesAllowedInLevel.Count == 0) return;

        if(DayNightManager.Instance.GetCurrentDay() == 0) {
            currentRainIntensity = LevelManager.Instance.GetLevelSO().initialRainIntensity;
        }
        else {
            currentRainIntensity = rainIntensitiesAllowedInLevel[UnityEngine.Random.Range(0, rainIntensitiesAllowedInLevel.Count)];
        }

        RandomizeRainDir();
        SetRainLevel(currentRainIntensity);
    }

    public void SetRandomRainLevel() {
        // Poids : plus l'intensité est forte, plus le poids est faible
        Dictionary<RainIntensity, int> weights = new Dictionary<RainIntensity, int>()
        {
        { RainIntensity.sparse, 40 },
        { RainIntensity.medium, 30 },
        { RainIntensity.strong, 20 },
        { RainIntensity.extreme, 10 },
    };

        int totalWeight = 0;
        foreach (var w in weights.Values)
            totalWeight += w;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int accum = 0;

        RainIntensity selected = RainIntensity.sparse; // fallback sûr

        foreach (var kvp in weights) {
            accum += kvp.Value;
            if (roll < accum) {
                selected = kvp.Key;
                break;
            }
        }

        currentRainIntensity = selected;
        RandomizeRainDir();
        SetRainLevel(currentRainIntensity);
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e)
    {
        if (currentRainIntensity != RainIntensity.none)
        {
            currentRainIntensity = RainIntensity.none;
            SetRainLevel(currentRainIntensity);
        }
    }

    private void InitializeRainLevels() {
        allRainLevels.Add(RainIntensity.none);
        allRainLevels.Add(RainIntensity.sparse);
        allRainLevels.Add(RainIntensity.medium);
        allRainLevels.Add(RainIntensity.strong);
        allRainLevels.Add(RainIntensity.extreme);
    }

    private void SetRainLevel(RainIntensity rainLevel) {
        WaterSimulationAdvanced waterSim = modernWater2D.waterSimulation as WaterSimulationAdvanced;
        bool enableRain;

        if (rainLevel != RainIntensity.none) {
            enableRain = true;
        } else {
            enableRain = false;
        }

        waterSim.EnableRain(enableRain);
        float targetAlpha = 0f;

        ParticleSystem.EmissionModule emission_Collisions = rainPS_GroundCollisions.emission;
        ParticleSystem.MainModule main_Collisions = rainPS_GroundCollisions.main;
        ParticleSystem.EmissionModule emission_NoCollisions = rainPS_NoCollisions.emission;
        ParticleSystem.MainModule main_NoCollisions = rainPS_NoCollisions.main;
        ParticleSystem.MainModule main_SubEmitter = rainPS_SubEmitter.main;

        if (rainLevel == RainIntensity.none) {
            emission_Collisions.rateOverTime = 0;
            emission_NoCollisions.rateOverTime = 0;

            targetAlpha = 0;
        }

        if (rainLevel == RainIntensity.sparse) {
            emission_Collisions.rateOverTime = sparseRainPSEmission;
            emission_NoCollisions.rateOverTime = sparseRainPSEmission;
            main_Collisions.startSize = sparseRainPSSize;
            main_NoCollisions.startSize = sparseRainPSSize;
            main_SubEmitter.startSize = sparseRainSubEmitterPSSize;

            targetAlpha = sparseRainFrontFogAlpha;

            waterSim.SetRainSpeed(sparseRainSpeed);
            waterSim.SetRainWaveH(sparseRainStrength);
        }

        if (rainLevel == RainIntensity.medium) {
            emission_Collisions.rateOverTime = mediumRainPSEmission;
            emission_NoCollisions.rateOverTime = mediumRainPSEmission;
            main_Collisions.startSize = mediumRainPSSize;
            main_NoCollisions.startSize = mediumRainPSSize;
            main_SubEmitter.startSize = mediumRainSubEmitterPSSize;

            targetAlpha = mediumRainFrontFogAlpha;

            waterSim.SetRainSpeed(mediumRainSpeed);
            waterSim.SetRainWaveH(mediumRainStrength);
        }

        if (rainLevel == RainIntensity.strong) {
            emission_Collisions.rateOverTime = highRainPSEmission;
            emission_NoCollisions.rateOverTime = highRainPSEmission;
            main_Collisions.startSize = highRainPSSize;
            main_NoCollisions.startSize = highRainPSSize;
            main_SubEmitter.startSize = highRainSubEmitterPSSize;

            targetAlpha = highRainFrontFogAlpha;

            waterSim.SetRainSpeed(highRainSpeed);
            waterSim.SetRainWaveH(highRainStrength);
        }

        if (rainLevel == RainIntensity.extreme) {
            emission_Collisions.rateOverTime = extremeRainPSEmission;
            emission_NoCollisions.rateOverTime = extremeRainPSEmission;
            main_Collisions.startSize = extremeRainPSSize;
            main_NoCollisions.startSize = extremeRainPSSize;
            main_SubEmitter.startSize = extremeRainSubEmitterPSSize;

            targetAlpha = extremeRainFrontFogAlpha;

            waterSim.SetRainSpeed(extremeRainSpeed);
            waterSim.SetRainWaveH(extremeRainStrength);
        }

        StartCoroutine(TransitionFogAlpha(targetAlpha, 2f));
        OnRainIntensityChanged?.Invoke(this, EventArgs.Empty);
    }
    private IEnumerator TransitionFogAlpha(float targetAlpha, float duration) {
        Material mat = rainFrontFogSpriteRenderer.material;
        float startAlpha = mat.GetFloat("_Alpha");
        float time = 0f;

        while (time < duration) {
            time += Time.deltaTime;
            float t = time / duration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            mat.SetFloat("_Alpha", newAlpha);
            yield return null;
        }

        mat.SetFloat("_Alpha", targetAlpha);
    }

    private void RandomizeRainDir()
    {
        float rainDir = UnityEngine.Random.Range(-1f, 1f);
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime_GroundCollision = rainPS_GroundCollisions.velocityOverLifetime;
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime_NoCollision = rainPS_NoCollisions.velocityOverLifetime;

        if (rainDir < 0) {
            velocityOverLifetime_GroundCollision.x = new ParticleSystem.MinMaxCurve(-12f, -10f);
            velocityOverLifetime_NoCollision.x = new ParticleSystem.MinMaxCurve(-12f, -10f);
        } else {
            velocityOverLifetime_GroundCollision.x = new ParticleSystem.MinMaxCurve(10f, 12f);
            velocityOverLifetime_NoCollision.x = new ParticleSystem.MinMaxCurve(10f, 12f);
        }
    }

    public RainIntensity GetRainIntensity()
    {
        return currentRainIntensity;
    }

    public void SetInCavern(bool inCavern) {
        if(inCavern) {
            SetRainLevel(RainIntensity.none);
        } else {
            SetRainLevel(currentRainIntensity);
        }
    }

}
