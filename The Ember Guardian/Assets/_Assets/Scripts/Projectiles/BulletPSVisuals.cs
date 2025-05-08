using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPSVisuals : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private ParticleSystem bulletPS;
    [SerializeField] private ParticleSystem trailPS;
    [SerializeField] private Color fireLightBuffedBulletDmg;
    [SerializeField] private Color fireLightBuffedBulletDmgStartColor;
    [SerializeField] private Color outFireLightBuffedBulletDmg;
    [SerializeField] private Color outFireLightBuffedBulletDmgStartColor;
    [SerializeField] private Color initialBulletPSColor;

    private void Start() {
        gun.OnBuffedLastBulletShot += Gun_OnBuffedLastBulletShot;
        gun.OnDebuffLastBulletShot += Gun_OnDebuffLastBulletShot;

        PlayerSkills.Instance.OnPlayerOutFireLightBuffedDmg += PlayerSkills_OnPlayerOutFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerOutFireLightDebuffedDmg += PlayerSkills_OnPlayerOutFireLightDebuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightBuffedDmg += PlayerSkills_OnPlayerInFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightDebuffedDmg += PlayerSkills_OnPlayerInFireLightDebuffedDmg;
    }

    private void PlayerSkills_OnPlayerInFireLightDebuffedDmg(object sender, System.EventArgs e) {

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.white;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(initialBulletPSColor, initialBulletPSColor);
    }

    private void PlayerSkills_OnPlayerInFireLightBuffedDmg(object sender, System.EventArgs e) {

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = fireLightBuffedBulletDmg;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(fireLightBuffedBulletDmgStartColor, fireLightBuffedBulletDmg);
    }

    private void PlayerSkills_OnPlayerOutFireLightDebuffedDmg(object sender, System.EventArgs e) {

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.white;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(initialBulletPSColor, initialBulletPSColor);
    }

    private void PlayerSkills_OnPlayerOutFireLightBuffedDmg(object sender, System.EventArgs e) {

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.red;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(outFireLightBuffedBulletDmgStartColor, outFireLightBuffedBulletDmg);
    }

    private void Gun_OnDebuffLastBulletShot(object sender, System.EventArgs e) {

        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = Color.white;

        if (bulletPS == null) return;
        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.white;
    }

    private void Gun_OnBuffedLastBulletShot(object sender, System.EventArgs e) {

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.red;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = Color.red;
    }
}
