using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPSVisuals : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private ParticleSystem bulletPS;
    [SerializeField] private SpriteRenderer bulletSR;
    [SerializeField] private ParticleSystem trailPS;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color fireLightBuffedBulletDmg;
    [SerializeField] private Color fireLightBuffedBulletDmgStartColor;
    [SerializeField] private Color outFireLightBuffedBulletDmg;
    [SerializeField] private Color outFireLightBuffedBulletDmgStartColor;
    [SerializeField] private Color initialBulletPSColor;
    [SerializeField] private Color poisonedPSColor;

    [SerializeField] private Gradient initialTrailRendererGradient;

    [SerializeField] private float greenHueShift = 55;
    [SerializeField] private float redHueShift = 55;
    [SerializeField] private float yellowHueShift = 55;

    [SerializeField] private bool isOnProjectileVisual;

    private bool outLightDamageBuffed;
    private bool lastBulletShotDamageBuffed;

    private void Awake() {
        if(trailRenderer != null) {
            initialTrailRendererGradient = trailRenderer.colorGradient;
        }

    }

    private void Start() {
        if(gun != null) {
            gun.OnBuffedLastBulletShot += Gun_OnBuffedLastBulletShot;
            gun.OnDebuffLastBulletShot += Gun_OnDebuffLastBulletShot;
        }

        PlayerSkills.Instance.OnPlayerOutFireLightBuffedDmg += PlayerSkills_OnPlayerOutFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerOutFireLightDebuffedDmg += PlayerSkills_OnPlayerOutFireLightDebuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightBuffedDmg += PlayerSkills_OnPlayerInFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightDebuffedDmg += PlayerSkills_OnPlayerInFireLightDebuffedDmg;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;

        if(isOnProjectileVisual) {
            if(PlayerShoot.Instance.GetHeldGun().GetLastBulletShot()) {
                ActivateBuffedDamageFeedbacks();
            }

            if (PlayerSkills.Instance.GetDamageOutFireLightCurrentlyBuffed()) {
                ActivateBuffedDamageFeedbacks();
            }

            if(PlayerSkills.Instance.GetDamageInFireLightCurrentlyBuffed() || PlayerSkills.Instance.GetMagmaBulletActive() || PlayerSkills.Instance.GetFuelFireOnKills()) {
                ActivateInFireBuffedFeedbacks();
            }

        }
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
        ActivateInFireBuffedFeedbacks();
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

    private void ActivateInFireBuffedFeedbacks() {
        if (trailPS != null) {
            ParticleSystem.MainModule trailMainModule = trailPS.main;
            trailMainModule.startColor = fireLightBuffedBulletDmg;
        }


        if (bulletPS != null) {
            ParticleSystem.MainModule bulletModule = bulletPS.main;
            bulletModule.startColor = new ParticleSystem.MinMaxGradient(fireLightBuffedBulletDmgStartColor, fireLightBuffedBulletDmg);
        };

        if(bulletSR != null) {
            bulletSR.color = fireLightBuffedBulletDmg;
        }

        if(trailRenderer != null) {
            Gradient gradient = new Gradient();

            // Définition des couleurs
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0].color = fireLightBuffedBulletDmgStartColor;
            colorKeys[0].time = 0f;
            colorKeys[1].color = fireLightBuffedBulletDmg;
            colorKeys[1].time = 1f;

            // Définition de l'alpha
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = fireLightBuffedBulletDmgStartColor.a;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = fireLightBuffedBulletDmg.a;
            alphaKeys[1].time = 1f;

            gradient.SetKeys(colorKeys, alphaKeys);
            trailRenderer.colorGradient = gradient;

            trailRenderer.material.SetColor("_GlowColor", fireLightBuffedBulletDmg);
        }

    }

    [Button]
    private void ActivateBuffedDamageFeedbacks() {
        if(trailPS != null) {
            ParticleSystem.MainModule trailMainModule = trailPS.main;
            trailMainModule.startColor = Color.red;
        }

        if (bulletPS != null) {
            ParticleSystem.MainModule bulletModule = bulletPS.main;
            bulletModule.startColor = new ParticleSystem.MinMaxGradient(outFireLightBuffedBulletDmgStartColor, outFireLightBuffedBulletDmg);
        }

        if (bulletSR != null) {
            bulletSR.color = Color.red;
        }

        if (trailRenderer != null) {
            Gradient gradient = new Gradient();

            // Définition des couleurs
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0].color = outFireLightBuffedBulletDmgStartColor;
            colorKeys[0].time = 0f;
            colorKeys[1].color = outFireLightBuffedBulletDmg;
            colorKeys[1].time = 1f;

            // Définition de l'alpha
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = outFireLightBuffedBulletDmgStartColor.a;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = outFireLightBuffedBulletDmg.a;
            alphaKeys[1].time = 1f;

            gradient.SetKeys(colorKeys, alphaKeys);
            trailRenderer.colorGradient = gradient;

            trailRenderer.material.SetColor("_GlowColor", outFireLightBuffedBulletDmg);
        }


    }

    [Button]
    private void DeActivateBuffedDamageFeedbacks() {
        if (lastBulletShotDamageBuffed || outLightDamageBuffed) return;

        if(trailPS != null) {
            ParticleSystem.MainModule trailMainModule = trailPS.main;
            trailMainModule.startColor = Color.white;
        }
        if (bulletPS != null) {
            ParticleSystem.MainModule bulletModule = bulletPS.main;
            bulletModule.startColor = new ParticleSystem.MinMaxGradient(initialBulletPSColor, initialBulletPSColor);
        };


        if (bulletSR != null) {
            bulletSR.color = Color.white;
        }

        if (trailRenderer != null) {
            trailRenderer.colorGradient = initialTrailRendererGradient;
            trailRenderer.material.SetColor("_GlowColor", initialBulletPSColor);
        }
    }

    [Button]
    private void ActivateFireBulletFeedbacks() {
        if(trailPS != null) {
            ParticleSystem.MainModule trailMainModule = trailPS.main;
            trailMainModule.startColor = fireLightBuffedBulletDmg;
        }

        if (bulletPS != null) {
            ParticleSystem.MainModule bulletModule = bulletPS.main;
            bulletModule.startColor = new ParticleSystem.MinMaxGradient(fireLightBuffedBulletDmgStartColor, fireLightBuffedBulletDmg);
        }

        if (bulletSR != null) {
            bulletSR.color = fireLightBuffedBulletDmg;
        }

        if (trailRenderer != null) {
            Gradient gradient = new Gradient();

            // Définition des couleurs
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0].color = fireLightBuffedBulletDmgStartColor;
            colorKeys[0].time = 0f;
            colorKeys[1].color = fireLightBuffedBulletDmg;
            colorKeys[1].time = 1f;

            // Définition de l'alpha
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = fireLightBuffedBulletDmgStartColor.a;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = fireLightBuffedBulletDmg.a;
            alphaKeys[1].time = 1f;

            gradient.SetKeys(colorKeys, alphaKeys);
            trailRenderer.colorGradient = gradient;

            trailRenderer.material.SetColor("_GlowColor", fireLightBuffedBulletDmg);
        }
    }

    [Button]
    private void DeActivateFireBulletFeedbacks() {
        if(trailPS != null) {
            ParticleSystem.MainModule trailMainModule = trailPS.main;
            trailMainModule.startColor = Color.white;
        }

        if (bulletPS != null) {
            ParticleSystem.MainModule bulletModule = bulletPS.main;
            bulletModule.startColor = new ParticleSystem.MinMaxGradient(initialBulletPSColor, initialBulletPSColor);
        };


        if (bulletSR != null) {
            bulletSR.color = Color.white;
        }

        if (trailRenderer != null) {
            trailRenderer.colorGradient = initialTrailRendererGradient;
            trailRenderer.material.SetColor("_GlowColor", initialBulletPSColor);
        }

    }

    private void OnDestroy() {
        PlayerSkills.Instance.OnPlayerOutFireLightBuffedDmg -= PlayerSkills_OnPlayerOutFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerOutFireLightDebuffedDmg -= PlayerSkills_OnPlayerOutFireLightDebuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightBuffedDmg -= PlayerSkills_OnPlayerInFireLightBuffedDmg;
        PlayerSkills.Instance.OnPlayerInFireLightDebuffedDmg -= PlayerSkills_OnPlayerInFireLightDebuffedDmg;
        PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated -= PlayerSkills_OnActiveSkillDeactivated;
    }
}
