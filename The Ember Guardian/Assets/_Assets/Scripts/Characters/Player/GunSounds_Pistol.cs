using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds_Pistol : GunSounds
{
    [SerializeField] private AudioClip[] silencedAudioClips;

    public override AudioClip[] GetShootAudioClips() {
        if(PlayerShoot.Instance.GetSilencerActive()) {
            return silencedAudioClips;
        } else {
            return gun.GetGunSO().shootGunSound;
        }

    }
}
