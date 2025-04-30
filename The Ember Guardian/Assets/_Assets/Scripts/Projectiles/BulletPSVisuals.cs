using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPSVisuals : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private ParticleSystem bulletPS;
    [SerializeField] private ParticleSystem trailPS;

    private void Start() {
        gun.OnBuffedLastBulletShot += Gun_OnBuffedLastBulletShot;
        gun.OnDebuffLastBulletShot += Gun_OnDebuffLastBulletShot;
    }

    private void Gun_OnDebuffLastBulletShot(object sender, System.EventArgs e) {

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.white;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = Color.white;
        
    }

    private void Gun_OnBuffedLastBulletShot(object sender, System.EventArgs e) {
        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.red;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = Color.red;
    }
}
