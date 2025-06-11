using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservationTowerSounds : StructureSounds {

    [SerializeField] private AudioClip activeTowerAudioClip;
    [SerializeField] private ObservationTower observationTower;

    protected override void Start() {
        base.Start();
        observationTower.OnObservationTowerActivated += ObservationTower_OnObservationTowerActivated;
        observationTower.OnObservationTowerDeActivated += ObservationTower_OnObservationTowerDeActivated;

        audioSource2D.clip = activeTowerAudioClip;
        audioSource2D.loop = true;
    }

    private void ObservationTower_OnObservationTowerDeActivated(object sender, System.EventArgs e) {
        audioSource2D.Stop();
    }

    private void ObservationTower_OnObservationTowerActivated(object sender, System.EventArgs e) {
        audioSource2D.Play();
    }
}
