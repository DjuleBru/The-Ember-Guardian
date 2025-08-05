using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks : MonoBehaviour
{
    [SerializeField] private GunMeleeAttackCollider meleeAttackCollider;
    [SerializeField] private MMF_Player mmfPlayer;
    [SerializeField] private MMF_Player meleeAttackFeedbacks;
    [SerializeField] private ParticleSystem shellOutPS;

    [SerializeField] private ParticleSystem loadGunPS1;
    [SerializeField] private ParticleSystem loadGunPS2;
    [SerializeField] private ParticleSystem dmgBuffInFirePS;
    [SerializeField] private ParticleSystem dmgBuffOutFirePS;
    [SerializeField] private ParticleSystem dmgBuffSurgePS;

    private Gun gun;

    private void Awake() {
        gun = GetComponentInParent<Gun>();
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerStartedShot += PlayerShoot_OnPlayerStartedShot;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoot_OnPlayerCooldownTrigger;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnBulletsChanged += PlayerShoot_OnBulletsChanged;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerSHoot_OnPlayerSwappedGun;
        PlayerSkills.Instance.OnPlayerInFireLightBuffedDmg += PlayerSkills_OnPlayerInFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightDebuffedDmg += PlayerSkills_OnPlayerInFireLightDebuffedDmg;
        PlayerSkills.Instance.OnPlayerOutFireLightBuffedDmg += PlayerSkills_OnPlayerOutFireLightBuffed;
        PlayerSkills.Instance.OnPlayerOutFireLightDebuffedDmg += PlayerSkills_OnPlayerOutFireLightDebuffedDmg;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;

        if(meleeAttackCollider != null) {
            meleeAttackCollider.OnGunMeleeAttackHit += MeleeAttackCollider_OnGunMeleeAttackHit;
        }

        gun.OnPerfectQTEDamageBuff += Gun_OnPerfectQTEDamageBuff;
        gun.OnPerfectQTEDamageBuffEnded += Gun_OnPerfectQTEDamageBuffEnded;
    }

    private void PlayerSHoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if (gun.GetDamageSurgeBuffed() && gun.GetGunActive() && PlayerShoot.Instance.GetCurrentBullets() != 0) {
            dmgBuffSurgePS.Play();
        }
    }

    private void PlayerShoot_OnBulletsChanged(object sender, System.EventArgs e) {
        if (gun.GetDamageSurgeBuffed()) {
            if(PlayerShoot.Instance.GetCurrentBullets() == 0) {
                dmgBuffSurgePS.Stop();
            }
        }
    }

    private void PlayerShoot_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        if(gun.GetDamageSurgeBuffed()) {
            dmgBuffSurgePS.Play();
        }
    }

    private void Gun_OnPerfectQTEDamageBuffEnded(object sender, System.EventArgs e) {
        dmgBuffSurgePS.Stop();
    }

    private void Gun_OnPerfectQTEDamageBuff(object sender, System.EventArgs e) {
        dmgBuffSurgePS.Play();
    }

    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if(e.skillTypeDeactivated == SkillItem.SkillType.activeMagmaShotBullet) {
            dmgBuffInFirePS.Stop();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeMagmaShotBullet) {
            dmgBuffInFirePS.Play();
        }
    }

    private void PlayerSkills_OnPlayerOutFireLightDebuffedDmg(object sender, System.EventArgs e) {
        dmgBuffOutFirePS.Stop();
    }

    private void PlayerSkills_OnPlayerInFireLightDebuffedDmg(object sender, System.EventArgs e) {
        dmgBuffInFirePS.Stop();
    }

    private void PlayerSkills_OnPlayerOutFireLightBuffed(object sender, System.EventArgs e) {
        dmgBuffOutFirePS.Play();
    }

    private void PlayerSkills_OnPlayerInFireLightBuffedDmg(object sender, System.EventArgs e) {
        dmgBuffInFirePS.Play();
    }

    private void MeleeAttackCollider_OnGunMeleeAttackHit(object sender, System.EventArgs e) {
        meleeAttackFeedbacks.PlayFeedbacks();
    }

    private void PlayerShoot_OnPlayerStartedShot(object sender, System.EventArgs e) {
        if(loadGunPS1 != null) {
            loadGunPS1.Play();
        }
        if (loadGunPS2 != null) {
            loadGunPS2.Play();
        }
    }

    private void PlayerShoot_OnPlayerCooldownTrigger(object sender, System.EventArgs e) {
        if(!gun.GetGunActive()) return;
        shellOutPS.Emit(1);
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;
        mmfPlayer.PlayFeedbacks();
    }
}
