using System;
using System.Collections;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class CorruptedSaveRestorer : MonoBehaviour
{
    private const string mainFile = "SaveFile.es3";
    private const string backupFile = "SaveFile_backup.es3";

    private const string hordeMainFile = "SaveFile_HordeMode.es3";
    private const string hordeBackupFile = "SaveFile_HordeMode_backup.es3";

    private bool corruptedSaveDetected;

    public static CorruptedSaveRestorer Instance;

    private void Awake() {
        Instance = this;
        RecoverIfNeeded();
    }

    private void RecoverIfNeeded() {

        RecoverFile(mainFile, backupFile, "emberlingsControlUnlocked");
        RecoverFile(hordeMainFile, hordeBackupFile, "HordeModeXP");
    }

    private void RecoverFile(string file, string backup, string validationKey) {

        string path = Path.Combine(Application.persistentDataPath, file);
        string backupPath = Path.Combine(Application.persistentDataPath, backup);

        if (IsFileValid(path, validationKey)) {
            return;
        }

        Debug.LogWarning("Save corrompue détectée (" + file + "), tentative backup");

        if (File.Exists(backupPath) && IsFileValid(backupPath, validationKey)) {

            File.Copy(backupPath, path, true);
            Debug.Log("Backup restauré : " + file);
            corruptedSaveDetected = true;

        }
        else {

            Debug.LogError("Backup invalide (" + file + "), reset");

            if (!File.Exists(path) && !File.Exists(backupPath)) return; // New Game

            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    private bool IsFileValid(string path, string validationKey) {

        if (!File.Exists(path)) {
            return false;
        }

        try {

            if (ES3.KeyExists(validationKey, path)) {

                ES3.Load<object>(validationKey, path);
            }

            return true;
        }
        catch (Exception e) {

            Debug.LogWarning("Fichier invalide : " + e.Message);
            return false;
        }
    }


    public bool GetCorruptedSaveFileDetected() {
        return corruptedSaveDetected;
    }

}
