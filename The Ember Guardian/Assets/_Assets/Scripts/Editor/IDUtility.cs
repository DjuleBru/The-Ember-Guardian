using UnityEditor;
using UnityEngine;

public static class IDUtility {
    [MenuItem("Tools/Regenerate IDs in Scene")]
    public static void RegenerateIDs() {
        int spawnerCount = 0;
        int obstacleCount = 0;
        int scavengableCount = 0;
        int trialAreasCount = 0;
        int chestCount = 0;

        // MobSpawners
        foreach (var spawner in GameObject.FindObjectsOfType<MobSpawner>()) {
            spawner.ForceNewID();
            EditorUtility.SetDirty(spawner);
            spawnerCount++;
        }

        // Obstacles
        foreach (var obstacle in GameObject.FindObjectsOfType<Obstacle>()) {
            obstacle.ForceNewID();
            EditorUtility.SetDirty(obstacle);
            obstacleCount++;
        }        
        
        // Chests
        foreach (var chest in GameObject.FindObjectsOfType<Chest>()) {
            chest.ForceNewID();
            EditorUtility.SetDirty(chest);
            chestCount++;
        }

        // Scav
        foreach (var scavengable in GameObject.FindObjectsOfType<Scavengable>()) {
            scavengable.ForceNewID();
            EditorUtility.SetDirty(scavengable);
            scavengableCount++;
        }

        // Trial Area
        foreach (var trialArea in GameObject.FindObjectsOfType<TrialArea>()) {
            trialArea.ForceNewID();
            EditorUtility.SetDirty(trialArea);
            trialAreasCount++;
        }

        Debug.Log($"IDs regenerated: {spawnerCount} spawners, {obstacleCount} obstacles,  {chestCount} chests, {scavengableCount} scavengables,  {trialAreasCount} trialAreas");
    }
}
