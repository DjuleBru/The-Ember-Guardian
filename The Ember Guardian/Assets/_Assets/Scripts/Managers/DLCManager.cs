using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class DLCManager : MonoBehaviour {
    public static DLCManager Instance;

    public enum DLCType {
        None,
        SupporterEdition,
    }

    [System.Serializable]
    public class DLCDefinition {
        public DLCType type;
        public uint appId;
    }

    [SerializeField] private List<DLCDefinition> dlcDefinitions;

    private Dictionary<DLCType, uint> dlcAppIds = new();

    private void Awake() {
        Instance = this;

        foreach (var def in dlcDefinitions) {
            dlcAppIds[def.type] = def.appId;
        }
    }

    public bool HasDLC(DLCType type) {
        if (type == DLCType.None)
            return true;

        if (!SteamClient.IsValid)
            return false;

        if (!dlcAppIds.ContainsKey(type))
            return false;

        return SteamApps.IsDlcInstalled(dlcAppIds[type]);
    }
}