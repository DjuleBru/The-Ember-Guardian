using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSounds_AAGun : GunSounds
{
    [SerializeField] private AudioClip[] layMinesShootAudioClips;


    public override AudioClip[] GetShootAudioClips() {

        if(PlayerShoot.Instance.GetAAGunSpawnsMines()) {
            return layMinesShootAudioClips;
        } else {
            return gun.GetGunSO().shootGunSound;
        }

    }

}
