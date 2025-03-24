using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSound : StructureSounds
{
    [SerializeField] private Tower tower;
    [SerializeField] private AudioClip hunterAssignAudioClip;

    protected override void Awake() {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Start() {
        base.Start();

        tower.OnHunterGarrisoned += Tower_OnHunterAssigned;
        tower.OnPlayerClimbedOnTower += Tower_OnPlayerClimbedOnTower;
    }

    private void Tower_OnPlayerClimbedOnTower(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(hunterAssignAudioClip, sfxVolume);
    }

    private void Tower_OnHunterAssigned(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(hunterAssignAudioClip, sfxVolume);
    }
}
