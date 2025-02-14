using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    [SerializeField] private bool allowDebugInputs_CreaturesSpawnManager;
    [SerializeField] private bool allowDebugInputs_DayNightManager;
    [SerializeField] private bool allowDebugInputs_CurrencyUIManager;
    [SerializeField] private bool debugMode_PlayerWeapons;
    [SerializeField] private bool debugMode_Portals;
    [SerializeField] private bool debugMode_HUBManager;
    [SerializeField] private bool debugMode_HUBMerchants;
    [SerializeField] private bool debugMode_Tutorial;
    [SerializeField] private bool debugMode_PlayerCamp;
    [SerializeField] private bool debugMode_Progression;
    [SerializeField] private bool debugMode_DontShowVideoTip;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.T)) {
            //HUBManager.Instance.SaveHub();
        }
    }
    public bool GetAllowDebugInputs_CreaturesSpawnManager() {
        return allowDebugInputs_CreaturesSpawnManager;
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
    public bool GetDebugMode_Progression() {
        return debugMode_Progression;
    }
    public bool GetDebugMode_DontShowVideoTips() {
        return debugMode_DontShowVideoTip;
    }
}
