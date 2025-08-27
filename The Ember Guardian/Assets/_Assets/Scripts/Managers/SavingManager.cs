using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    private void Start() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            if(DebugManager.Instance.GetSaveAfterEachLevelDebug()) {
                string saveFileName = "SaveFile";
                string original = Application.persistentDataPath + "/" + saveFileName + ".es3";

                string lastLevelCompletedString = MetaProgressionManager.Instance.GetLastLevelCompletedString();

                string newSaveFile = Application.persistentDataPath + "/" + saveFileName + "_" + lastLevelCompletedString + ".es3";

                if (File.Exists(original)) {
                    File.Copy(original, newSaveFile, true); // true = overwrite si backup existe déjà
                }
            }
        }
    }
}
