using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Water2D;

public class RainManager : MonoBehaviour
{
    public enum RainLevel {
        none,
        sparse,
        medium,
        high,
        extreme,
    }

    private List<RainLevel> allRainLevels = new List<RainLevel>();
    private RainLevel currentRainLevel;

    [SerializeField] private ModernWater2D modernWater2D;

    [SerializeField] private ParticleSystem rainPS_GroundCollisions;
    [SerializeField] private ParticleSystem rainPS_NoCollisions;

    [SerializeField] private ParticleSystem rainPS_SubEmitter;

    private int sparseRainPSEmission = 15;
    private int mediumRainPSEmission = 40;
    private int highRainPSEmission = 75;
    private int extremeRainPSEmission = 150;

    private float sparseRainPSSize = .05f;
    private float mediumRainPSSize = .06f;
    private float highRainPSSize = .07f;
    private float extremeRainPSSize = .08f;

    private float sparseRainSubEmitterPSSize = .03f;
    private float mediumRainSubEmitterPSSize = .04f;
    private float highRainSubEmitterPSSize = .05f;
    private float extremeRainSubEmitterPSSize = .06f;

    private float sparseRainSpeed = .6f;
    private float mediumRainSpeed = 1f;
    private float highRainSpeed = 1.5f;
    private float extremeRainSpeed = 2f;

    private float sparseRainStrength = .5f;
    private float mediumRainStrength = .6f;
    private float highRainStrength = .7f;
    private float extremeRainStrength = .8f;

    [SerializeField] private bool enableRain;

    private int rainLevelIndex;

    private void Awake() {
        InitializeRainLevels();
    }

    private void Start() {
        SetRainLevel(allRainLevels[rainLevelIndex]);
    }

    private void Update() {
        FollowPlayer();
        if(Input.GetKeyDown(KeyCode.T)) {
            rainLevelIndex++;
            currentRainLevel = allRainLevels[rainLevelIndex];
            SetRainLevel(currentRainLevel);
        }
    }

    private void FollowPlayer() {
        Vector3 newRainPSPosition = new Vector3(Player.Instance.transform.position.x, rainPS_GroundCollisions.transform.position.y, 0);
        rainPS_GroundCollisions.transform.position = newRainPSPosition;
        rainPS_NoCollisions.transform.position = newRainPSPosition;
    }

    private void InitializeRainLevels() {
        allRainLevels.Add(RainLevel.none);
        allRainLevels.Add(RainLevel.sparse);
        allRainLevels.Add(RainLevel.medium);
        allRainLevels.Add(RainLevel.high);
        allRainLevels.Add(RainLevel.extreme);
    }

    private void SetRainLevel(RainLevel rainLevel) {
        WaterSimulationAdvanced waterSim = modernWater2D.waterSimulation as WaterSimulationAdvanced;

        if (rainLevel != RainLevel.none) {
            enableRain = true;
        } else {
            enableRain = false;
        }

        waterSim.EnableRain(enableRain);

        ParticleSystem.EmissionModule emission_Collisions = rainPS_GroundCollisions.emission;
        ParticleSystem.MainModule main_Collisions = rainPS_GroundCollisions.main;
        ParticleSystem.EmissionModule emission_NoCollisions = rainPS_NoCollisions.emission;
        ParticleSystem.MainModule main_NoCollisions = rainPS_NoCollisions.main;
        ParticleSystem.MainModule main_SubEmitter = rainPS_SubEmitter.main;

        if (currentRainLevel == RainLevel.none) {
            emission_Collisions.rateOverTime = 0;
            emission_NoCollisions.rateOverTime = 0;
            return;
        }

        if (currentRainLevel == RainLevel.sparse) {
            emission_Collisions.rateOverTime = sparseRainPSEmission;
            emission_NoCollisions.rateOverTime = sparseRainPSEmission;
            main_Collisions.startSize = sparseRainPSSize;
            main_NoCollisions.startSize = sparseRainPSSize;
            main_SubEmitter.startSize = sparseRainSubEmitterPSSize;

            waterSim.SetRainSpeed(sparseRainSpeed);
            waterSim.SetRainWaveH(sparseRainStrength);
            return;
        }

        if (currentRainLevel == RainLevel.medium) {
            emission_Collisions.rateOverTime = mediumRainPSEmission;
            emission_NoCollisions.rateOverTime = mediumRainPSEmission;
            main_Collisions.startSize = mediumRainPSSize;
            main_NoCollisions.startSize = mediumRainPSSize;
            main_SubEmitter.startSize = mediumRainSubEmitterPSSize;

            waterSim.SetRainSpeed(mediumRainSpeed);
            waterSim.SetRainWaveH(mediumRainStrength);
            return;
        }

        if (currentRainLevel == RainLevel.high) {
            emission_Collisions.rateOverTime = highRainPSEmission;
            emission_NoCollisions.rateOverTime = highRainPSEmission;
            main_Collisions.startSize = highRainPSSize;
            main_NoCollisions.startSize = highRainPSSize;
            main_SubEmitter.startSize = highRainSubEmitterPSSize;

            waterSim.SetRainSpeed(highRainSpeed);
            waterSim.SetRainWaveH(highRainStrength);
            return;
        }

        if (currentRainLevel == RainLevel.extreme) {
            emission_Collisions.rateOverTime = extremeRainPSEmission;
            emission_NoCollisions.rateOverTime = extremeRainPSEmission;
            main_Collisions.startSize = extremeRainPSSize;
            main_NoCollisions.startSize = extremeRainPSSize;
            main_SubEmitter.startSize = extremeRainSubEmitterPSSize;

            waterSim.SetRainSpeed(extremeRainSpeed);
            waterSim.SetRainWaveH(extremeRainStrength);
            return;
        }

    }

    private void EnableRainInWaterSimulation(bool enableRain) {
        WaterSimulationAdvanced waterSim = modernWater2D.waterSimulation as WaterSimulationAdvanced;
    }


}
