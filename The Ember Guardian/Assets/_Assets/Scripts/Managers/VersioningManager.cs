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
    [SerializeField] protected float demoBuildVersion;
    [SerializeField] protected float buildVersion;
    [SerializeField] protected float latestCompatibleBuildVersion;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        string versionString = buildVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if(isDemo) {
            int major = (int)demoBuildVersion;

            // On arrondit à 2 décimales pour éviter les imprécisions binaires
            int minorAndPatch = (int)Mathf.Round((demoBuildVersion - major) * 100);

            int minor = minorAndPatch / 10;
            int patch = minorAndPatch % 10;

            if (minor == 0 && patch == 0) {
                versionString = $"{major}";
            }
            else if (patch == 0) {
                versionString = $"{major}.{minor}";
            }
            else {
                versionString = $"{major}.{minor}.{patch}";
            }
        }
        versioningText.text = prefixText + " v." + versionString;
    }

    public bool CheckIncompatibleSaveFile() {
        return false;
        string key = "buildVersion_" + buildVersion + "_saveFileDeleted";
        string latestBuildSavedKey = "latestBuildSaved";

        float latestBuildSaved = ES3.Load(latestBuildSavedKey, buildVersion);
        ES3.Save(latestBuildSavedKey, buildVersion);

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

    public bool CheckNewSaveFile() {

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
            string key = "buildVersion_" + buildVersion + "_saveFileDeleted";
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
