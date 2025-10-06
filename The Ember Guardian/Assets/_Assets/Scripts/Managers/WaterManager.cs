using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Water2D;

public class WaterManager : MonoBehaviour
{
    public static WaterManager Instance;

    private ModernWater2D modernWater2D;

    [SerializeField] private float reflectionLevel = 1f;
    private bool perspectiveActive;

    public event EventHandler OnReflectionLevelChanged;
    private ES3Settings settingsSaveFileSettings;

    private void Awake() {
        Instance = this;

        modernWater2D = GetComponent<ModernWater2D>();

        settingsSaveFileSettings = new ES3Settings("Settings.es3");
        reflectionLevel = ES3.Load("reflectionLevel", 1f, settingsSaveFileSettings);

    }

    private void Start() {
        //RefreshPerspectiveActive();

        SettingsManager.Instance.OnWaterPerspectiveChanged += SettingsManager_OnWaterPerspectiveChanged;
        RefreshReflectionsLevel();
    }

    private void SettingsManager_OnWaterPerspectiveChanged(object sender, System.EventArgs e) {
        //RefreshPerspectiveActive();
    }

    private void RefreshPerspectiveActive() {
        perspectiveActive = SettingsManager.Instance.GetWaterPerspectiveActive();

        modernWater2D.settings._reflectionsSettings.usePerspective.value = perspectiveActive;

        modernWater2D.reflectionsManagerPlatformer.UpdateSettings(
            modernWater2D.settings._reflectionsSettings,
            false
        );

    }

    private void RefreshReflectionsLevel() {

        // Liste complète des layers disponibles
        List<int> allLayers = new List<int> { 0, 6, 11, 12, 13, 14, 15, 16, 17, 18, 19, 25, 28, 29, 30 };

        // Ordre de suppression (les derniers seront enlevés en premier)
        List<int> removalPriority = new List<int> { 25, 17, 14, 13, 15, 30, 16, 11, 19, 12, 18, 6, 0, 28, 29 };

        // Calcul combien de layers garder
        int totalLayers = allLayers.Count;
        int layersToKeep = Mathf.RoundToInt(reflectionLevel * totalLayers);

        if(layersToKeep <= 2) {
            layersToKeep = 2;
        }

        // Crée une nouvelle liste à partir de toutes les layers
        List<int> activeLayers = new List<int>(allLayers);

        // Supprime les layers selon la priorité jusqu'à avoir le bon nombre
        foreach (int layer in removalPriority) {
            if (activeLayers.Count <= layersToKeep) break;
            activeLayers.Remove(layer);
        }

        // Applique au water system
        modernWater2D.settings._reflectionsSettings.layers = activeLayers;
        modernWater2D.reflectionsManagerPlatformer.UpdateSettings(
            modernWater2D.settings._reflectionsSettings,
            false
        );
        modernWater2D.RefreshReflections();
    }
    public float GetWaterReflectionLevel() {
        return reflectionLevel;
    }

    public void SetReflectionLevel(float newReflectionLevel) {
        reflectionLevel = newReflectionLevel;
        ES3.Save("reflectionLevel", reflectionLevel, settingsSaveFileSettings);

        OnReflectionLevelChanged?.Invoke(this, EventArgs.Empty);

        RefreshReflectionsLevel();
    }
}
