using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_RocketLauncher : GunProjectile
{
    [SerializeField] private float accelerationForce = 2f;
    [SerializeField] private float accelerationDuration = 1f;

    [SerializeField] private bool spawnChildBullets = true;
    [SerializeField] private int damageToFlyingMultiplier = 1;
    [SerializeField] private Transform childGunProjectilePrefab;

    [SerializeField] private int childGunProjectilesInstantiated = 3;
    [SerializeField] private float childGunProjectilesForceMultiplier = 5f;
    [SerializeField] private float childBulletLifetime = .2f;
    [SerializeField] private int childInitialForceMultiplier;
    [SerializeField] private int childBulletKnockback = 0;

    private Vector2 accelerationDirection;
    private float accelerationTimer;

    private bool instantiateChildGunGroundProjectile;
    private bool nukeProjectile;

    protected void Start() {
        instantiateChildGunGroundProjectile = PlayerShoot.Instance.GetRocketLauncherMiniRockets();
        nukeProjectile = PlayerShoot.Instance.GetRocketLauncherNukeMode();

        if (instantiateChildGunGroundProjectile && childGunProjectilesInstantiated != 0) {
            projectileLifetime = .5f;
            projectileExplosionDamage /= (childGunProjectilesInstantiated + 1);
        }

        if(nukeProjectile) {
            accelerationForce = 0;
            accelerationDuration = 0;
            rb.gravityScale = 4f;
        }
    }
    protected override void Update() {
        base.Update();

        if (projectileExploded) return;

        if (accelerationTimer > 0f) {
            accelerationTimer -= Time.deltaTime;
            if (rb != null) {
                rb.AddForce(accelerationDirection * accelerationForce * Time.deltaTime, ForceMode2D.Force);
            }
        }
    }

    protected override void DamageCreatureHit(Creature creatureHit, Collider2D collision) {
        int damage = projectileExplosionDamage;
        if (creatureHit.GetCreatureSO().flying) {
            damage *= damageToFlyingMultiplier;
        }

        creatureHit.TakeDamage(damage, transform, false);

        Vector2 knockbackDirNormalized = (collision.transform.position - transform.position).normalized;
        knockbackDirNormalized.y = 0;
        creatureHit.TakeKnockback(knockBackForce, knockbackDirNormalized);
    }

    protected override void Explode() {

        if (instantiateChildGunGroundProjectile) {;

            for(int i = 0; i < childGunProjectilesInstantiated; i++) {

                GunProjectile_RocketLauncher gunProjectile = Instantiate(childGunProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile_RocketLauncher>();
                gunProjectile.SetExplodeOnContact(false);
                gunProjectile.gameObject.SetActive(true);

                float currentChildBulletLifetime = (i+1) * childBulletLifetime;

                gunProjectile.InitializeProjectile(parentGun, currentChildBulletLifetime, projectileExplosionDamage, childBulletKnockback, initialForce* childGunProjectilesForceMultiplier, explosionRadiusMultiplier, 1, 1);
            }

        }

      
        base.Explode();

        if (nukeProjectile) {

            transform.localScale = Vector3.one * 1.5f * explosionRadiusMultiplier;
        }

    }

    public override void InitializeProjectile(Gun parentGun, float projectileLifetime, int projectileDamage, float knockbackForce, Vector2 initialForce, float explosionRadiusMultiplier, int pierceAmount, float bulletSizeMultiplier) {
        base.InitializeProjectile(parentGun, projectileLifetime, projectileDamage, knockbackForce, initialForce, explosionRadiusMultiplier, pierceAmount, 1);

        accelerationDirection = initialForce.normalized;
        accelerationTimer = accelerationDuration;

    }

    public void SetExplodeOnContact(bool explodeOnContact) {
        this.explodeOnContact = explodeOnContact;
    }

}
