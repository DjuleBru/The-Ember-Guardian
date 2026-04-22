using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    [SerializeField] private bool allowDebugInputs_CreaturesSpawnManager;
    [SerializeField] private bool debugMode_DontSpawnWavesAtNight;
    [SerializeField] private bool allowDebugInputs_DayNightManager;
    [SerializeField] private bool allowDebugInputs_CurrencyUIManager;
    [SerializeField] private bool allowDebugInputs_gunJam;
    [SerializeField] private bool debugMode_PlayerWeapons;
    [SerializeField] private bool debugMode_Portals;
    [SerializeField] private bool debugMode_HUBManager;
    [SerializeField] private bool debugMode_HUBMerchants;
    [SerializeField] private bool debugMode_Tutorial;
    [SerializeField] private bool debugMode_PlayerCamp;
    [SerializeField] private bool debugMode_AllStructuresUnlocked;
    [SerializeField] private bool debugMode_AllStructureUpgradeUnlocked;
    [SerializeField] private bool debugMode_FlagCarry;
    [SerializeField] private bool debugMode_WorkerInteractions;
    [SerializeField] private bool debugMode_DontShowVideoTip;
    [SerializeField] private bool debugMode_WindManager;
    [SerializeField] private bool debugMode_RainManager;
    [SerializeField] private bool debugMode_AllSkillsUnlocked;
    [SerializeField] private bool debugMode_AllTrapsUnlocked;
    [SerializeField] private bool disableCreatureDetection;
    [SerializeField] private bool showMobDestinationGizmos;
    [SerializeField] private bool allItemsUnlocked;
    [SerializeField] private bool dropRedOrbsUnlocked;
    [SerializeField] private bool takeScreenshotsContinuous;
    [SerializeField] private bool takeScreenshotsOnKeyPressed;
    [SerializeField] private bool saveAfterEachLevel;
    [SerializeField] private bool debugShowLaser;
    [SerializeField] private bool debugSurgeReload;
    [SerializeField] private bool logNightWaveData;
    [SerializeField] private bool hordeModeActiveDebug;
    [SerializeField] private bool hordeModeAllUnlockedDebug;
    [SerializeField] private bool debugMode_Credits;
    [SerializeField] private bool debugMode_EventSystem;

    private GameObject lastSelected;

    int i = 0;
    private float screenshotTakeTimer;
    private float screenshotTakeCooldown = .5f;
    private bool takingScreenshots;

    private void Awake() {
        Instance = this;
        screenshotTakeTimer = screenshotTakeCooldown;
    }

    private void Update() {

        if(debugMode_EventSystem) {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != lastSelected) {
                Debug.Log(
                            "[EventSystem] Selection changed\n" +
                            "FROM: " + (lastSelected != null ? lastSelected.name : "NULL") + "\n" +
                            "TO: " + (current != null ? current.name : "NULL") + "\n" +
                            System.Environment.StackTrace
                        );

                lastSelected = current;
            }
        }

        if(Input.GetKeyDown(KeyCode.S)) {
            takingScreenshots = !takingScreenshots;
        }

        if (takeScreenshotsContinuous) {
            if (!takingScreenshots) return;
            screenshotTakeTimer -= Time.deltaTime;
            if (screenshotTakeTimer < 0) {
                screenshotTakeTimer = screenshotTakeCooldown*3;
                i++;

                ScreenCapture.CaptureScreenshot("screenshot_" + i + ".png");
                Debug.Log("A screenshot was taken!");
            }
        }

        if (takeScreenshotsOnKeyPressed) {
            if (Input.GetKeyDown(KeyCode.W)) {
                i++;

                ScreenCapture.CaptureScreenshot("screenshot_" + i + ".png", 1) ;
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

    public bool GetDebugMode_FlagCarry() {
        return debugMode_FlagCarry;
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
    public bool GetDebugMode_AllSkillsUnlocked() {
        return debugMode_AllSkillsUnlocked;
    }
    public bool GetDebugMode_AllTrapsUnlocked() {
        return debugMode_AllTrapsUnlocked;
    }
    public bool GetDisableCreatureDetection() {
        return disableCreatureDetection;
    }
    public bool GetShowMobDestinationGyzmos() {
        return disableCreatureDetection;
    }
    public bool GetAllItemsUnlockedInDemo() {
        return allItemsUnlocked;
    }
    public bool GetDropRedOrbsUnlocked() {
        return dropRedOrbsUnlocked;
    }

    public bool GetGunJamDebugInputsAllowed() {
        return allowDebugInputs_gunJam;
    }
    public bool GetSaveAfterEachLevelDebug() {
        return saveAfterEachLevel;
    }

    public bool GetLogNightWavesData() {
        return logNightWaveData;
    }
    public bool GetDebugShowLaser() {
        return debugShowLaser;
    }
    public bool GetDebugSurgeReload() {
        return debugSurgeReload;
    }

    public bool GetDebugMode_Credits() {
        return debugMode_Credits;
    }

    public bool GetHordeModeActiveDebug() {
        return hordeModeActiveDebug;
    }
    public bool GetHordeModeAllUnlockedDebug() {
        return hordeModeAllUnlockedDebug;
    }
}
