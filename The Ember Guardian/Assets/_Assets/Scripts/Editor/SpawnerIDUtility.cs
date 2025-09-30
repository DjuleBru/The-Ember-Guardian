using UnityEditor;
using UnityEngine;

public static class SpawnerIDUtility {
    [MenuItem("Tools/Regenerate Spawner IDs in Scene")]
    public static void RegenerateIDs() {
        foreach (var spawner in GameObject.FindObjectsOfType<MobSpawner>()) {
            spawner.ForceNewID();
            EditorUtility.SetDirty(spawner);
        }

        Debug.Log("Spawner IDs regenerated.");
    }
}
