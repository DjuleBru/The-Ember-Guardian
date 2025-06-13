using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTowerSound : StructureSounds
{
    [SerializeField] private SpecialTower specialTower;
    [SerializeField] private AudioClip[] addAmmoAudioClips;
    [SerializeField] private AudioClip[] removeAmmoAudioClips;
    [SerializeField] private AudioClip playerClimbOnTower;
    [SerializeField] private float addAmmoVolumeMultiplier;
    [SerializeField] private float removeAmmoVolumeMultiplier;

    protected override void Start() {
        base.Start();
        specialTower.OnAmmoClipAdded += SpecialTower_OnAmmoClipAdded;
        specialTower.OnAmmoClipRemoved += SpecialTower_OnAmmoClipRemoved;
        specialTower.OnPlayerClimberOnSpecialTower += SpecialTower_OnPlayerClimberOnSpecialTower;
    }

    private void SpecialTower_OnPlayerClimberOnSpecialTower(object sender, System.EventArgs e) {
        PlaySound2D(playerClimbOnTower);
    }

    private void SpecialTower_OnAmmoClipRemoved(object sender, System.EventArgs e) {
        PlaySound2D(removeAmmoAudioClips, removeAmmoVolumeMultiplier);
    }

    private void SpecialTower_OnAmmoClipAdded(object sender, System.EventArgs e) {
        PlaySound2D(addAmmoAudioClips, addAmmoVolumeMultiplier);
    }
}
