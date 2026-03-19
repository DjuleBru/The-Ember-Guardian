using Sirenix.OdinInspector;
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
    [SerializeField] private Color poisonedPSColor;

    [SerializeField] private float greenHueShift = 55;
    [SerializeField] private float redHueShift = 55;
    [SerializeField] private float yellowHueShift = 55;

    private bool outLightDamageBuffed;
    private bool lastBulletShotDamageBuffed;

    private void Start() {
        gun.OnBuffedLastBulletShot += Gun_OnBuffedLastBulletShot;
        gun.OnDebuffLastBulletShot += Gun_OnDebuffLastBulletShot;

        PlayerSkills.Instance.OnPlayerOutFireLightBuffedDmg += PlayerSkills_OnPlayerOutFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerOutFireLightDebuffedDmg += PlayerSkills_OnPlayerOutFireLightDebuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightBuffedDmg += PlayerSkills_OnPlayerInFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightDebuffedDmg += PlayerSkills_OnPlayerInFireLightDebuffedDmg;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;

    }


    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if (e.skillTypeDeactivated == SkillItem.SkillType.activeMagmaShotBullet || e.skillTypeDeactivated == SkillItem.SkillType.activeFeedFireOnKills) {
            DeActivateFireBulletFeedbacks();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeMagmaShotBullet || e.skillItemAdded.skillType == SkillItem.SkillType.activeFeedFireOnKills) {
            ActivateFireBulletFeedbacks();
        }
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
        outLightDamageBuffed = false;
        DeActivateBuffedDamageFeedbacks();
    }

    private void PlayerSkills_OnPlayerOutFireLightBuffedDmg(object sender, System.EventArgs e) {
        outLightDamageBuffed = true;
        ActivateBuffedDamageFeedbacks();
    }

    private void Gun_OnDebuffLastBulletShot(object sender, System.EventArgs e) {
        lastBulletShotDamageBuffed = false;
        DeActivateBuffedDamageFeedbacks();
    }

    private void Gun_OnBuffedLastBulletShot(object sender, System.EventArgs e) {
        lastBulletShotDamageBuffed = true;
        ActivateBuffedDamageFeedbacks();
    }

    [Button]
    private void ActivateBuffedDamageFeedbacks() {
        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.red;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(outFireLightBuffedBulletDmgStartColor, outFireLightBuffedBulletDmg);
    }

    [Button]
    private void DeActivateBuffedDamageFeedbacks() {
        if (lastBulletShotDamageBuffed || outLightDamageBuffed) return;

        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.white;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(initialBulletPSColor, initialBulletPSColor);
    }

    [Button]
    private void ActivateFireBulletFeedbacks() {
        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = fireLightBuffedBulletDmg;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(fireLightBuffedBulletDmgStartColor, fireLightBuffedBulletDmg);
    }

    [Button]
    private void DeActivateFireBulletFeedbacks() {
        ParticleSystem.MainModule trailMainModule = trailPS.main;
        trailMainModule.startColor = Color.white;

        if (bulletPS == null) return;
        ParticleSystem.MainModule bulletModule = bulletPS.main;
        bulletModule.startColor = new ParticleSystem.MinMaxGradient(initialBulletPSColor, initialBulletPSColor);
    }

    private void OnDestroy() {
        PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated -= PlayerSkills_OnActiveSkillDeactivated;
    }
}
