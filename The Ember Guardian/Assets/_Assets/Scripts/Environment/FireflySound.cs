using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflySound : SoundObject
{
    [SerializeField] private FireFlies fireflies;
    [SerializeField] private AudioClip[] firefliesSpawnAudioClips;


    protected override void Start() {
        base.Start();
        fireflies.OnFirefliesSpawned += Fireflies_OnFirefliesSpawned;
    }

    private void Fireflies_OnFirefliesSpawned(object sender, System.EventArgs e) {
        PlaySound2D(firefliesSpawnAudioClips);
    }

}
