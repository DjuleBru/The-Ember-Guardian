using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DemoSaveImportManager : MonoBehaviour {
    public static DemoSaveImportManager Instance;

    [Header("Folder Names (must match exactly)")]
    [SerializeField] private string demoFolderName = "The Ember Guardian Demo";
    [SerializeField] private string fullGameFolderName = "The Ember Guardian";

    private bool hasDemoSaveToImport;

    private void Awake() {
        Instance = this;

        hasDemoSaveToImport = HasDemoSave() && !HasFullGameSave();
    }

    public bool HasDemoSave() {
        string demoPath = GetDemoSavePath();
        if (string.IsNullOrEmpty(demoPath)) return false;

        string demoSaveFile = Path.Combine(demoPath, "SaveFile.es3");
        return File.Exists(demoSaveFile);
    }

    public bool HasFullGameSave() {
        string fullPath = Application.persistentDataPath;
        string saveFilePath = Path.Combine(fullPath, "SaveFile.es3");

        Debug.Log("HasFullGameSave " + saveFilePath + " " + File.Exists(saveFilePath));

        return File.Exists(saveFilePath);
    }

    public bool ImportDemoSave(bool overwriteExistingFiles = false) {
        string demoPath = GetDemoSavePath();
        string fullPath = Application.persistentDataPath;

        if (string.IsNullOrEmpty(demoPath)) {
            Debug.LogWarning("Demo save path not found.");
            return false;
        }

        if (!Directory.Exists(demoPath)) {
            Debug.LogWarning("Demo save folder does not exist.");
            return false;
        }

        if (!Directory.Exists(fullPath)) {
            Directory.CreateDirectory(fullPath);
        }

        string[] demoFiles = Directory.GetFiles(demoPath);

        if (demoFiles.Length == 0) {
            Debug.LogWarning("Demo save folder is empty.");
            return false;
        }

        foreach (string file in demoFiles) {
            string fileName = Path.GetFileName(file);
            string destinationFile = Path.Combine(fullPath, fileName);

            File.Copy(file, destinationFile, true);
        }

        StartCoroutine(ImportAchievementsAfterDelay());

        Debug.Log("Demo save successfully imported.");
        return true;
    }

    private IEnumerator ImportAchievementsAfterDelay() {
        yield return new WaitForSeconds(1f);


        AchievementsManager.Instance.ImportAchievementsFromSave();

    }

    private string GetDemoSavePath() {
        DirectoryInfo currentDir = new DirectoryInfo(Application.persistentDataPath);
        DirectoryInfo parentDir = currentDir.Parent;

        Debug.Log(parentDir);
        if (parentDir == null) {
            Debug.LogWarning("PersistentDataPath has no parent directory.");
            return null;
        }

        string demoPath = Path.Combine(parentDir.FullName, demoFolderName);

        return demoPath;
    }

    public bool HasDemoSaveToImport() {
        return hasDemoSaveToImport;
    }
}