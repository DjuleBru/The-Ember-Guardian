using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTowerSound : StructureSounds
{
    [SerializeField] private SpecialTower specialTower;
    [SerializeField] private List<SpecialTower_Manner> manners;
    [SerializeField] private AudioClip[] addAmmoAudioClips;
    [SerializeField] private AudioClip[] removeAmmoAudioClips;
    [SerializeField] private AudioClip[] shootAudioClips;
    [SerializeField] private AudioClip[] reloadAudioClips;
    [SerializeField] private AudioClip[] cooldownAudioClips;
    [SerializeField] private AudioClip playerClimbOnTower;
    [SerializeField] private float shootDelayToPlaySound = 0;
    [SerializeField] private float addAmmoVolumeMultiplier;
    [SerializeField] private float removeAmmoVolumeMultiplier;
    [SerializeField] private float shootVolumeMultiplier;
    [SerializeField] private float reloadVolumeMultiplier;
    [SerializeField] private float cooldownVolumeMultiplier;

    protected override void Start() {
        base.Start();
        specialTower.OnAmmoClipAdded += SpecialTower_OnAmmoClipAdded;
        specialTower.OnAmmoClipRemoved += SpecialTower_OnAmmoClipRemoved;
        specialTower.OnPlayerClimberOnSpecialTower += SpecialTower_OnPlayerClimberOnSpecialTower;
        specialTower.OnEngineerEnteredTower += SpecialTower_OnEngineerEnteredTower;
        specialTower.OnEngineerExitedTower += SpecialTower_OnEngineerExitedTower;

        foreach(SpecialTower_Manner manner in  manners) {
            manner.OnMannerShot += Manner_OnMannerShot;
            manner.OnMannerReloadingStarted += Manner_OnMannerReloadingStarted;
            manner.OnMannerCooldownEventTriggered += Manner_OnMannerCooldownEventTriggered;
        }
    }

    private void Manner_OnMannerCooldownEventTriggered(object sender, System.EventArgs e) {
        PlaySound2D(cooldownAudioClips, cooldownVolumeMultiplier);
    }

    private void Manner_OnMannerReloadingStarted(object sender, System.EventArgs e) {
        PlaySound2D(reloadAudioClips, reloadVolumeMultiplier);
    }

    private void Manner_OnMannerShot(object sender, System.EventArgs e) {
        PlaySFXAfterDelay(shootAudioClips, shootDelayToPlaySound, shootVolumeMultiplier);
    }

    private void SpecialTower_OnEngineerExitedTower(object sender, System.EventArgs e) {
        PlaySound2D(playerClimbOnTower);
    }

    private void SpecialTower_OnEngineerEnteredTower(object sender, System.EventArgs e) {
        PlaySound2D(playerClimbOnTower);
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
