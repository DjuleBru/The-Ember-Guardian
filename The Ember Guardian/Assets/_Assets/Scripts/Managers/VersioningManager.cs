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
    [SerializeField] protected float demoBuildVersion;
    [SerializeField] protected float buildVersion;
    [SerializeField] protected float latestCompatibleBuildVersion;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        string versionString = buildVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if(isDemo) {
            versionString = demoBuildVersion.ToString(System.Globalization.CultureInfo.InvariantCulture);
            versioningText.text = "Demo version " + versionString;
        } else {
            versioningText.text = "Pre-alpha version " + versionString;
        }
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

            string key = "buildVersion_" + buildVersion + "_saveFileDeleted";
            ES3.Save(key, true);

            return true;
        } else {
            return false;
        }

    }

    public bool GetIsDemo() {
        return isDemo;
    }

}
