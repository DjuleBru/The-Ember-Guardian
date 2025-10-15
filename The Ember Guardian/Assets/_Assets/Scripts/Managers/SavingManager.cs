using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    private Dictionary<string, int> levelAttempts = new Dictionary<string, int>();

    private void Awake() {
        levelAttempts = ES3.Load("levelAttempts", new Dictionary<string, int>());
    }

    private void Start() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
            Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;
        }
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        //Debug.Log("Portal_OnAnyTeleporterTeleportedPlayerOut");

        if (DebugManager.Instance.GetSaveAfterEachLevelDebug()) {
            string saveFileName = "SaveFile";
            string original = Application.persistentDataPath + "/" + saveFileName + ".es3";

            string lastLevelCompletedString = MetaProgressionManager.Instance.GetLastLevelCompletedString();

            if (lastLevelCompletedString == "") return;

            string newSaveFile = Application.persistentDataPath + "/" + saveFileName + "_After_" + lastLevelCompletedString + "_Completed.es3";

            if (File.Exists(original)) {
                File.Copy(original, newSaveFile, true); // true = overwrite si backup existe déjà
            }

        }
    }

    private void LevelManager_OnLevelFailed(object sender, System.EventArgs e) {
        // Récupérer le niveau courant
        string currentLevel = LevelManager.Instance.GetLevelSO().ToString();

        // Si on n’a pas encore d’entrée pour ce niveau, on commence à 0
        if (!levelAttempts.ContainsKey(currentLevel)) {
            levelAttempts[currentLevel] = 0;
        }

        // Incrémenter le nombre d'essais
        levelAttempts[currentLevel]++;

        int attemptNumber = levelAttempts[currentLevel];

        string saveFileName = "SaveFile";
        string original = Application.persistentDataPath + "/" + saveFileName + ".es3";
        string newSaveFile = Application.persistentDataPath + "/" + saveFileName + "_" + currentLevel + "_Attempt_" + attemptNumber + "_Failed.es3";

        if (File.Exists(original)) {
            File.Copy(original, newSaveFile, true);
        }

        SaveLevelAttempts();
    }

    private void Portal_OnAnyPlayerTeleported(object sender, System.EventArgs e) {
        Debug.Log("Portal_OnAnyPlayerTeleported");
        if (DebugManager.Instance.GetSaveAfterEachLevelDebug()) {
            Portal portal = sender as Portal;
            string saveFileName = "SaveFile";
            string original = Application.persistentDataPath + "/" + saveFileName + ".es3";

            string levelStartingString = portal.GetLinkedLevelSO().ToString();

            // Déterminer le numéro d’attempt pour ce niveau
            int attemptNumber = 1;
            if (levelAttempts.ContainsKey(levelStartingString)) {
                attemptNumber = levelAttempts[levelStartingString] + 1;
            }

            string newSaveFile = Application.persistentDataPath + "/" + saveFileName + "_Before_" + levelStartingString + "_Attempt_" + attemptNumber + ".es3";

            if (File.Exists(original)) {
                File.Copy(original, newSaveFile, true);
            }

            SaveLevelAttempts();
        }
    }

    private void SaveLevelAttempts() {
        ES3.Save("levelAttempts", levelAttempts);
    }

    private void OnDestroy() {
        Portal.OnAnyPlayerTeleported -= Portal_OnAnyPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
    }
}
