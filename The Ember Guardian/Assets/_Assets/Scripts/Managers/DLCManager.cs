using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Galaxy;
using Galaxy.Api;

public class DLCManager : MonoBehaviour {
    public static DLCManager Instance;

    public enum DLCType {
        None,
        SupporterEdition,
    }

    [System.Serializable]
    public class DLCDefinition {
        public DLCType type;
        public uint steamAppId;
        public uint gogProductId;
    }

    [SerializeField] private List<DLCDefinition> dlcDefinitions;
    private Dictionary<DLCType, DLCDefinition> dlcLookup = new();

    private void Awake() {
        Instance = this;
        foreach (var def in dlcDefinitions) {
            dlcLookup[def.type] = def;
        }
    }

    public bool HasDLC(DLCType type) {
        if (type == DLCType.None) {
            return true;
        }

        if (!dlcLookup.ContainsKey(type)) {
            return false;
        }

        DLCDefinition def = dlcLookup[type];

        if (HasDLC_ViaGogNative(def)) {
            return true;
        }

        if (HasDLC_ViaSteamworks(def)) {
            return true;
        }

        return false;
    }

    private bool HasDLC_ViaGogNative(DLCDefinition def) {
        try {
            return GalaxyInstance.Apps().IsDlcInstalled(def.gogProductId);
        }
        catch (Exception e) {
            Debug.Log("[DLCManager] GalaxyInstance.Apps().IsDlcInstalled a échoué pour " + def.type + " : " + e.Message);
            return false;
        }
    }

    private bool HasDLC_ViaSteamworks(DLCDefinition def) {
        if (!SteamClient.IsValid) {
            return false;
        }

        try {
            return SteamApps.IsDlcInstalled(def.steamAppId);
        }
        catch (Exception e) {
            Debug.Log("[DLCManager] IsDlcInstalled (Steamworks) a échoué pour " + def.type + " : " + e.Message);
            return false;
        }
    }
}