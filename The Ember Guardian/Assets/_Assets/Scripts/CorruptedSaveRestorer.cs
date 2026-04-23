using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class CorruptedSaveRestorer : MonoBehaviour
{
    private const string mainFile = "SaveFile.es3";
    private const string backupFile = "SaveFile_backup.es3";

    private void Awake() {
        RecoverIfNeeded();
    }

    private void RecoverIfNeeded() {
        string path = Path.Combine(Application.persistentDataPath, mainFile);
        string backupPath = Path.Combine(Application.persistentDataPath, backupFile);

        if (IsFileValid(path)) {
            return;
        }

        Debug.LogWarning("Save corrompue détectée, tentative de récupération backup");

        if (File.Exists(backupPath) && IsFileValid(backupPath)) {
            File.Copy(backupPath, path, true);
            Debug.Log("Backup restauré avec succès");
        }
        else {
            Debug.LogError("Backup invalide ou absent, reset complet");

            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    private bool IsFileValid(string path) {
        if (!File.Exists(path)) {
            return false;
        }

        try {
            // test simple : essayer de lire une clé connue
            if (ES3.KeyExists("emberlingsControlUnlocked", path)) {
                ES3.Load<bool>("emberlingsControlUnlocked", path);
            }

            return true;
        }
        catch (Exception e) {
            Debug.LogWarning("Fichier invalide : " + e.Message);
            return false;
        }
    }
}
