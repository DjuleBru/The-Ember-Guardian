using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_BulletPistol : GunProjectile_Bullet
{
    protected void Start() {
        if(PlayerShoot.Instance.GetPistolExplosiveBulletsActive()) {
            explodeOnContact = true;
            dealDirectDamage = false;
        }
    }
}
