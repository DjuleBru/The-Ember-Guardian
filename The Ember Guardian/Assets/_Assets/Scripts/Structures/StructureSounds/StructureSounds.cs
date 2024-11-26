using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSounds : MonoBehaviour
{
    protected float sfxVolume;

    protected virtual void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
    }

    protected virtual void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }
}
