using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_MannerVisual_Mortar : SpecialTower_MannerVisual
{
    [SerializeField] private SpecialTower_Manner_Mortar mortar;
    [SerializeField] private RuntimeAnimatorController level2AnimatorController;
    [SerializeField] protected SpriteRenderer reloaderSpriteRenderer;
    [SerializeField] protected SpriteRenderer shooterSpriteRenderer_Hands;

    [SerializeField] private Sprite aimingFarLeftSprite_lvl1;
    [SerializeField] private Sprite aimingCloseLeftSprite_lvl1;
    [SerializeField] private Sprite aimingCloseSprite_lvl1;
    [SerializeField] private Sprite aimingCloseRightSprite_lvl1;
    [SerializeField] private Sprite aimingFarRightSprite_lvl1;

    [SerializeField] private Sprite aimingFarLeftSprite_lvl2;
    [SerializeField] private Sprite aimingCloseLeftSprite_lvl2;
    [SerializeField] private Sprite aimingCloseSprite_lvl2;
    [SerializeField] private Sprite aimingCloseRightSprite_lvl2;
    [SerializeField] private Sprite aimingFarRightSprite_lvl2;

    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private Transform aimingFarLeftSpawnPosition;
    [SerializeField] private Transform aimingCloseLeftSpawnPosition;
    [SerializeField] private Transform aimingCloseSpawnPosition;
    [SerializeField] private Transform aimingCloseRightSpawnPosition;
    [SerializeField] private Transform aimingFarRightSpawnPosition;

    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private MMF_Player shootFeedbacks;


    private Sprite aimingFarLeftSprite;
    private Sprite aimingCloseLeftSprite;
    private Sprite aimingCloseSprite;
    private Sprite aimingCloseRightSprite;
    private Sprite aimingFarRightSprite;

    private float distanceToCreatureFar = 15;
    private float distanceToCreatureClose = 3;

    private float refreshAimingVisualsTimer;
    private float refreshAimingVisualsRate = 1f;

    protected override void Awake() {
        base.Awake();
        reloaderSpriteRenderer.enabled = false;
        shooterSpriteRenderer_Hands.enabled = false;
        specialTower.OnStructureUpgraded += SpecialTower_OnStructureUpgraded;

        aimingFarLeftSprite = aimingFarLeftSprite_lvl1;
        aimingCloseLeftSprite = aimingCloseLeftSprite_lvl1;
        aimingCloseSprite = aimingCloseSprite_lvl1;
        aimingCloseRightSprite = aimingCloseRightSprite_lvl1;
        aimingFarRightSprite = aimingFarRightSprite_lvl1;

        mortar.OnProjectileShot += Mortar_OnProjectileShot;

        weaponSpriteRenderer.sprite = aimingCloseRightSprite;
    }

    private void Mortar_OnProjectileShot(object sender, System.EventArgs e) {
        shootFeedbacks.PlayFeedbacks();
        shootPS.Play();
    }

    private void Update() {
        if (!aiming) {
            shooterSpriteRenderer_Hands.enabled = false;
        } else {
            shooterSpriteRenderer_Hands.enabled = true;

            refreshAimingVisualsTimer -= Time.deltaTime;
            if (refreshAimingVisualsTimer < 0) {
                refreshAimingVisualsTimer = refreshAimingVisualsRate;
                HandleAimingMortarVisuals();
            }
        };


    }
    protected override void Manner_OnMannerShot(object sender, System.EventArgs e) {
        mannerAnimator.SetTrigger("Shoot");
        cooldownAnimationDone = false;

        float bulletsAmountNormalized = (float)manner.GetCurrentShotIndex() / (float)manner.GetShotsPerAmmoClip();

        if (bulletsAmountNormalized == .5f) {
            bulletsAmountNormalized = .49f;
        }

        //lightSpriteIndex = Mathf.FloorToInt(bulletsAmountNormalized * (lightsSpriteList.Count));
        //lightSpriteIndex = Mathf.Clamp(lightSpriteIndex, 0, lightsSpriteList.Count - 1);

        //weaponLightsSpriteRenderer.sprite = lightsSpriteList[lightSpriteIndex];

        if (outOfAmmo && manner.GetCurrentShotIndex() == 0) {
            mannerAnimator.SetBool("Aiming", false);
            //weaponLightsSpriteRenderer.color = outOfAmmoLightColor;
            //weaponLightsSpriteRenderer.sprite = lightsSpriteList[lightsSpriteList.Count - 1];
        }

        if (lastClipStartedEmptying) {
            outOfAmmo = true;
        }
    }

    protected override void Manner_OnMannerCooldownEventTriggered(object sender, System.EventArgs e) {

        weaponSpriteRenderer.sprite = aimingCloseRightSprite;
        if (hasCooldown) {
            mannerAnimator.SetTrigger("Cooldown");
        }

        cooldownAnimationDone = true;
        StartCoroutine(TriggerShellOutAfterDelay());
    }

    private void SpecialTower_OnStructureUpgraded(object sender, System.EventArgs e) {
        int engineersManning = mannerAnimator.GetInteger("EngineersManning");

        mannerAnimator.runtimeAnimatorController = level2AnimatorController; 
        aimingFarLeftSprite = aimingFarLeftSprite_lvl2;
        aimingCloseLeftSprite = aimingCloseLeftSprite_lvl2;
        aimingCloseSprite = aimingCloseSprite_lvl2;
        aimingCloseRightSprite = aimingCloseRightSprite_lvl2;
        aimingFarRightSprite = aimingFarRightSprite_lvl2;


        weaponSpriteRenderer.sprite = aimingCloseRightSprite;
        mannerAnimator.SetInteger("EngineersManning", engineersManning);
    }

    private void HandleAimingMortarVisuals() {
        float distanceToCreature = manner.GetTargetCreature().transform.position.x - transform.position.x;

        if(transform.position.x > 0) {
            if (distanceToCreature < -distanceToCreatureFar) {
                weaponSpriteRenderer.sprite = aimingFarLeftSprite;
                projectileSpawnPosition.position = aimingFarLeftSpawnPosition.position;
                shootPS.transform.position = aimingFarLeftSpawnPosition.position;
                return;
            }
            if (distanceToCreature > -distanceToCreatureFar && distanceToCreature < -distanceToCreatureClose) {
                weaponSpriteRenderer.sprite = aimingCloseLeftSprite;
                projectileSpawnPosition.position = aimingCloseLeftSpawnPosition.position;
                shootPS.transform.position = aimingCloseLeftSpawnPosition.position;
                return;
            }
            if (distanceToCreature > -distanceToCreatureClose && distanceToCreature < distanceToCreatureClose) {
                weaponSpriteRenderer.sprite = aimingCloseSprite;
                projectileSpawnPosition.position = aimingCloseSpawnPosition.position;
                shootPS.transform.position = aimingCloseSpawnPosition.position;
                return;
            }
            if (distanceToCreature > distanceToCreatureClose && distanceToCreature < distanceToCreatureFar) {
                weaponSpriteRenderer.sprite = aimingCloseRightSprite;
                projectileSpawnPosition.position = aimingCloseRightSpawnPosition.position;
                shootPS.transform.position = aimingCloseRightSpawnPosition.position;
                return;
            }
            if (distanceToCreature > distanceToCreatureFar) {
                weaponSpriteRenderer.sprite = aimingFarRightSprite;
                projectileSpawnPosition.position = aimingFarRightSpawnPosition.position;
                shootPS.transform.position = aimingFarRightSpawnPosition.position;
                return;
            }
        } else {

            if (distanceToCreature < -distanceToCreatureFar) {
                weaponSpriteRenderer.sprite = aimingFarRightSprite;
                projectileSpawnPosition.position = aimingFarRightSpawnPosition.position;
                shootPS.transform.position = aimingFarRightSpawnPosition.position;
                return;
            }
            if (distanceToCreature > -distanceToCreatureFar && distanceToCreature < -distanceToCreatureClose) {
                weaponSpriteRenderer.sprite = aimingCloseRightSprite;
                projectileSpawnPosition.position = aimingCloseRightSpawnPosition.position;
                shootPS.transform.position = aimingCloseRightSpawnPosition.position;
                return;
            }
            if (distanceToCreature > -distanceToCreatureClose && distanceToCreature < distanceToCreatureClose) {
                weaponSpriteRenderer.sprite = aimingCloseSprite;
                projectileSpawnPosition.position = aimingCloseSpawnPosition.position;
                shootPS.transform.position = aimingCloseSpawnPosition.position;
                return;
            }
            if (distanceToCreature > distanceToCreatureClose && distanceToCreature < distanceToCreatureFar) {
                weaponSpriteRenderer.sprite = aimingCloseLeftSprite;
                projectileSpawnPosition.position = aimingCloseLeftSpawnPosition.position;
                shootPS.transform.position = aimingCloseLeftSpawnPosition.position;
                return;
            }
            if (distanceToCreature > distanceToCreatureFar) {
                weaponSpriteRenderer.sprite = aimingFarLeftSprite;
                projectileSpawnPosition.position = aimingFarLeftSpawnPosition.position;
                shootPS.transform.position = aimingFarLeftSpawnPosition.position;
                return;
            }
        }
    }

    protected override void Manner_OnEngineerStoppedManning(object sender, System.EventArgs e) {
        base.Manner_OnEngineerStoppedManning(sender, e);
        if (manner.GetEngineersManning() < manner.GetMaxEngineersManning()) {
            reloaderSpriteRenderer.enabled = false;
        }
        else {
            reloaderSpriteRenderer.enabled = true;
        }

        if(manner.GetEngineersManning() == 0) {
            shooterSpriteRenderer_Hands.enabled = false;
        }
    }

    protected override void Manner_OnEngineerStartedManning(object sender, System.EventArgs e) {
        base.Manner_OnEngineerStartedManning(sender, e);

        if (manner.GetEngineersManning() < manner.GetMaxEngineersManning()) {
            reloaderSpriteRenderer.enabled = false;
        }
        else {
            reloaderSpriteRenderer.enabled = true;
        }
    }
}
