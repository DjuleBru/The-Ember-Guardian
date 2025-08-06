using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_AA : GunProjectile
{
    [SerializeField] private bool spawnChildBullets = true;
    [SerializeField] private int damageToFlyingMultiplier;
    [SerializeField] private Transform childGunAirProjectilePrefab;
    [SerializeField] private Transform childGunProjectilePrefab;

    [SerializeField] private int childGunProjectilesInstantiated = 5;
    [SerializeField] private float childBulletLifetime = 10f;
    [SerializeField] private float childBulletAirLifetime = .22f;
    [SerializeField] private int childDamagePerBullet = 5;
    [SerializeField] private int childBulletKnockback = 0;

    private bool instantiateChildGunGroundProjectiles;

    protected void Start() {
        instantiateChildGunGroundProjectiles = PlayerShoot.Instance.GetAAGunSpawnsChildBullets();
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
        base.Explode();

        if (!spawnChildBullets) return;

        if(instantiateChildGunGroundProjectiles) {
            for(int i = 0; i < childGunProjectilesInstantiated; ++i) {

                GunProjectile gunProjectile = Instantiate(childGunProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile>();
                gunProjectile.gameObject.SetActive(true);
                Vector2 initialForce = new Vector2(UnityEngine.Random.Range(-10f,10f),UnityEngine.Random.Range(-10f, 10f));

                gunProjectile.InitializeProjectile(parentGun, childBulletLifetime, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier);
            }
        } else {
            for (int i = 0; i < childGunProjectilesInstantiated; ++i) {

                GunProjectile gunProjectile = Instantiate(childGunAirProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile>();
                gunProjectile.gameObject.SetActive(true);
                Vector2 initialForce = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));

                float lifeTimeRandomized = childBulletAirLifetime + UnityEngine.Random.Range(0, childBulletAirLifetime / 2);
                gunProjectile.InitializeProjectile(parentGun, lifeTimeRandomized, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier);
            }
        }
    }
}
