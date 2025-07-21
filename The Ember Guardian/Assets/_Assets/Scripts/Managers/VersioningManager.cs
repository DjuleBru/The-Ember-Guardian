using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class VersioningManager : MonoBehaviour
{
    public static VersioningManager Instance;

    private bool saveFileDeleted;
    [SerializeField] protected TextMeshProUGUI versioningText;
    [SerializeField] protected bool saveFileIncompatible;
    [SerializeField] protected bool isDemo;
    [SerializeField] protected string prefixText;
    [SerializeField] private int state = 0;
    [SerializeField] private int major = 9;
    [SerializeField] private int minor = 1;
    [SerializeField] private int patch = 2;

    [SerializeField] private int demoState = 0;
    [SerializeField] private int demoMajor = 9;
    [SerializeField] private int demoMinor = 0;
    [SerializeField] private int demoPatch = 1;

    [SerializeField] protected float latestCompatibleBuildVersion;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        int vMajor = isDemo ? demoState : state;
        int vMinor = isDemo ? demoMajor : major;
        int vPatch = isDemo ? demoMinor : minor;
        int vBuild = isDemo ? demoPatch : patch;

        string versionString = FormatVersion(vMajor, vMinor, vPatch, vBuild);
        versioningText.text = prefixText + " v." + versionString;
    }

    public static string FormatVersion(int major, int minor, int patch, int build) {
        if (patch == 0 && build == 0)
            return $"{major}.{minor}";
        else if (build == 0)
            return $"{major}.{minor}.{patch}";
        else
            return $"{major}.{minor}.{patch}.{build}";
    }

    public bool CheckIncompatibleSaveFile() {
        return false;
        float version = ReconstructFloatVersion(state, major, minor, patch);
        string key = "buildVersion_" + version + "_saveFileDeleted";
        string latestBuildSavedKey = "latestBuildSaved";

        float latestBuildSaved = ES3.Load(latestBuildSavedKey, version);
        ES3.Save(latestBuildSavedKey, version);

        if (!saveFileIncompatible) return false;

        if (!MetaProgressionManager.Instance.GetSavedOnce()) {
            return false;
        };

        saveFileDeleted = ES3.Load(key, false);

        if (latestBuildSaved > latestCompatibleBuildVersion) return false;
        if (saveFileDeleted) return false;

        MainMenuUI_StartupMessagePanel.Instance.OpenPanel();
        MainMenuUI_StartupMessagePanel.Instance.SetErasedSaveFilePanel();
        ES3.DeleteFile();
        ES3.Save(key, true);

        return true;
    }
    public static float ReconstructFloatVersion(int major, int minor, int patch, int build) {
        return major + (minor * 0.01f) + (patch * 0.0001f) + (build * 0.000001f);
    }
    public bool CheckNewSaveFile() {
        Debug.Log("ES3.FileExists " + ES3.FileExists());
        if (!ES3.FileExists()) {

            if(MainMenuUI_StartupMessagePanel.Instance != null) {
                MainMenuUI_StartupMessagePanel.Instance.OpenPanel();
                MainMenuUI_StartupMessagePanel.Instance.SetNewTesterPanel();
            } 

            if(AdjustGammaUI.Instance != null) {
                AdjustGammaUI.Instance.OpenPanel(true);
            }

            MusicManager.Instance.PauseMusic();
            MainMenuUI.Instance.HideAllMenuUI();
            string key = "buildVersion_" + ReconstructFloatVersion(state, major, minor, patch) + "_saveFileDeleted";
            ES3.Save(key, true);

            return true;
        } else {
            return false;
        }

    }

    public bool GetNewSaveFile() {
        if (!ES3.FileExists()) {
            return true;
        }
        else {
            return false;
        }
    }

    public bool GetIsDemo() {
        return isDemo;
    }

}
