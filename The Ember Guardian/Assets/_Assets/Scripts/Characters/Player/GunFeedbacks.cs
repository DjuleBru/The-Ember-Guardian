using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFeedbacks : MonoBehaviour
{
    [SerializeField] protected GunMeleeAttackCollider meleeAttackCollider;
    [SerializeField] protected MMF_Player mmfPlayer;
    [SerializeField] protected MMF_Player meleeAttackFeedbacks;
    [SerializeField] protected ParticleSystem shellOutPS;

    [SerializeField] protected ParticleSystem loadGunPS1;
    [SerializeField] protected ParticleSystem loadGunPS2;
    [SerializeField] protected ParticleSystem dmgBuffInFirePS;
    [SerializeField] protected ParticleSystem dmgBuffOutFirePS;
    [SerializeField] protected ParticleSystem dmgBuffSurgePS;

    protected Gun gun;

    protected void Awake() {
        gun = GetComponentInParent<Gun>();
    }

    protected virtual void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnShotStartedLoading += PlayerShoot_OnShotStartedLoading;
        PlayerShoot.Instance.OnPlayerShootStopped += PlayerShoot_OnPlayerShootStopped;
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

    protected void PlayerSHoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        if (gun.GetDamageSurgeBuffed() && gun.GetGunActive() && PlayerShoot.Instance.GetCurrentBullets() != 0) {
            dmgBuffSurgePS.Play();
        }
    }

    protected void PlayerShoot_OnBulletsChanged(object sender, System.EventArgs e) {
        if (gun.GetDamageSurgeBuffed()) {
            if(PlayerShoot.Instance.GetCurrentBullets() == 0) {
                dmgBuffSurgePS.Stop();
            }
        }
    }

    protected void PlayerShoot_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        if(gun.GetDamageSurgeBuffed()) {
            dmgBuffSurgePS.Play();
        }
    }

    protected void Gun_OnPerfectQTEDamageBuffEnded(object sender, System.EventArgs e) {
        dmgBuffSurgePS.Stop();
    }

    protected void Gun_OnPerfectQTEDamageBuff(object sender, System.EventArgs e) {
        dmgBuffSurgePS.Play();
    }

    protected void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if(e.skillTypeDeactivated == SkillItem.SkillType.activeMagmaShotBullet) {
            dmgBuffInFirePS.Stop();
        }
    }

    protected void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeMagmaShotBullet) {
            dmgBuffInFirePS.Play();
        }
    }

    protected void PlayerSkills_OnPlayerOutFireLightDebuffedDmg(object sender, System.EventArgs e) {
        dmgBuffOutFirePS.Stop();
    }

    protected void PlayerSkills_OnPlayerInFireLightDebuffedDmg(object sender, System.EventArgs e) {
        dmgBuffInFirePS.Stop();
    }

    protected void PlayerSkills_OnPlayerOutFireLightBuffed(object sender, System.EventArgs e) {
        dmgBuffOutFirePS.Play();
    }

    protected void PlayerSkills_OnPlayerInFireLightBuffedDmg(object sender, System.EventArgs e) {
        dmgBuffInFirePS.Play();
    }

    protected void MeleeAttackCollider_OnGunMeleeAttackHit(object sender, System.EventArgs e) {
        meleeAttackFeedbacks.PlayFeedbacks();
    }

    protected virtual void PlayerShoot_OnShotStartedLoading(object sender, System.EventArgs e) {
        if(loadGunPS1 != null) {
            loadGunPS1.Play();
        }
        if (loadGunPS2 != null) {
            loadGunPS2.Play();
        }
    }

    protected virtual void PlayerShoot_OnPlayerShootStopped(object sender, System.EventArgs e) {
        if (loadGunPS1 != null) {
            if(loadGunPS1.isPlaying) {
                loadGunPS1.Stop();
            }
        }
        if (loadGunPS2 != null) {
            if (loadGunPS2.isPlaying) {
                loadGunPS2.Stop();
            }
        }
    }


    protected void PlayerShoot_OnPlayerCooldownTrigger(object sender, System.EventArgs e) {
        if(!gun.GetGunActive()) return;
        shellOutPS.Emit(1);
    }

    protected virtual void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        if (!gun.GetGunSO().triggersShootSFXOnEachBuller) return;
        mmfPlayer.PlayFeedbacks();
    }

    private void OnDestroy() {
        PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated -= PlayerSkills_OnActiveSkillDeactivated;
    }
}
