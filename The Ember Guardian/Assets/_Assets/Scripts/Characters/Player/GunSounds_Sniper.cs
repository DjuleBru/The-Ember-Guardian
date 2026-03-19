using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds_Sniper : GunSounds
{
    [SerializeField] private AudioClip[] piercingRoundsAudioClips;

    public override AudioClip[] GetShootAudioClips() {
        if(PlayerShoot.Instance.GetSniperPiercingRoundsActive()) {
            return piercingRoundsAudioClips;
        } else {
            return gun.GetGunSO().shootGunSound;
        }
    }

}
