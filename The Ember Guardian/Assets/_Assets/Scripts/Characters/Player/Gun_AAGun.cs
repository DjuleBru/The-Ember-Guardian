using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_AAGun : Gun
{

    protected override void Shoot() {
        base.Shoot();
        if(PlayerShoot.Instance.GetAAGunSpawnsChildBullets()) {
            currentBullet--;
        }
    }

}
