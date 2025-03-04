using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    [SerializeField] private bool allowDebugInputs_CreaturesSpawnManager;
    [SerializeField] private bool debugMode_DontSpawnWavesAtNight;
    [SerializeField] private bool allowDebugInputs_DayNightManager;
    [SerializeField] private bool allowDebugInputs_CurrencyUIManager;
    [SerializeField] private bool debugMode_PlayerWeapons;
    [SerializeField] private bool debugMode_Portals;
    [SerializeField] private bool debugMode_HUBManager;
    [SerializeField] private bool debugMode_HUBMerchants;
    [SerializeField] private bool debugMode_Tutorial;
    [SerializeField] private bool debugMode_PlayerCamp;
    [SerializeField] private bool debugMode_AllStructuresUnlocked;
    [SerializeField] private bool debugMode_AllStructureUpgradeUnlocked;
    [SerializeField] private bool debugMode_WorkerInteractions;
    [SerializeField] private bool debugMode_DontShowVideoTip;
    [SerializeField] private bool debugMode_WindManager;
    [SerializeField] private bool debugMode_RainManager;
    [SerializeField] private bool takeScreenshots;

    int i = 0;
    private float screenshotTakeTimer;
    private float screenshotTakeCooldown = 3f;

    private void Awake() {
        Instance = this;
        screenshotTakeTimer = screenshotTakeCooldown;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.T)) {
            //HUBManager.Instance.SaveHub();
        }

        if (takeScreenshots) {
            screenshotTakeTimer -= Time.deltaTime;
            if (screenshotTakeTimer < 0) {
                screenshotTakeTimer = screenshotTakeCooldown;
                i++;

                ScreenCapture.CaptureScreenshot("screenshot_" + i + ".png");
                Debug.Log("A screenshot was taken!");
            }
        }
    }

    public bool GetAllowDebugInputs_CreaturesSpawnManager() {
        return allowDebugInputs_CreaturesSpawnManager;
    }
    public bool GetAllStructuresUnlocked() {
        return debugMode_AllStructuresUnlocked;
    }
    public bool GetAllStructureUpgradesUnlocked() {
        return debugMode_AllStructureUpgradeUnlocked;
    }
    public bool GetDebugDontSpawnAtNight() {
        return debugMode_DontSpawnWavesAtNight;
    }
    public bool GetAllowDebugInputs_DayNightManager() {
        return allowDebugInputs_DayNightManager;
    }
    public bool GetAllowDebugInputs_CurrencyUIManager() {
        return allowDebugInputs_CurrencyUIManager;
    }
    public bool GetDebugMode_PlayerWeapons() {
        return debugMode_PlayerWeapons;
    }
    public bool GetDebugMode_Portals() {
        return debugMode_Portals;
    }
    public bool GetDebugMode_HUBManager() {
        return debugMode_HUBManager;
    }
    public bool GetDebugMode_HUBMerchants() {
        return debugMode_HUBMerchants;
    }
    public bool GetDebugMode_Tutorial() {
        return debugMode_Tutorial;
    }

    public bool GetDebugMode_PlayerCamp() {
        return debugMode_PlayerCamp;
    }
    public bool GetDebugMode_WorkerInteractions() {
        return debugMode_WorkerInteractions;
    }
    public bool GetDebugMode_DontShowVideoTips() {
        return debugMode_DontShowVideoTip;
    }
    public bool GetDebugMode_WindManager() {
        return debugMode_WindManager;
    }
    public bool GetDebugMode_RainManager()
    {
        return debugMode_RainManager;
    }
}
