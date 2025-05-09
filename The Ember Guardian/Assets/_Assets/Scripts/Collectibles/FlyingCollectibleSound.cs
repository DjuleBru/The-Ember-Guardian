using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCollectibleSound : SoundObject
{
    [SerializeField] private AudioClip[] spawnFlyingCollectibleAudioClips;
    [SerializeField] private AudioClip[] destroyFlyingCollectibleAudioClips;
    [SerializeField] private FlyingCollectible flyingCollectible;
    [SerializeField] private float volumeMultiplier;


    protected override void Start() {
        base.Start();

        flyingCollectible.OnDestinationReached += FlyingCollectible_OnDestinationReached;
        PlaySound2D(spawnFlyingCollectibleAudioClips, volumeMultiplier);
    }

    private void FlyingCollectible_OnDestinationReached(object sender, System.EventArgs e) {
        PlaySound2D(destroyFlyingCollectibleAudioClips, volumeMultiplier);
    }
}
