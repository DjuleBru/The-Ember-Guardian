using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_AA : GunProjectile
{
    [SerializeField] private bool spawnChildBullets = true;
    [SerializeField] private int damageToFlyingMultiplier;
    [SerializeField] private Transform childGunAirProjectilePrefab;
    [SerializeField] private Transform childGunProjectilePrefab;

    [SerializeField] private float childBulletLifetime = 10f;
    [SerializeField] private float childBulletAirLifetime = .22f;
    [SerializeField] private int childBulletKnockback = 0;

    private int childDamagePerBullet;
    private int childGunProjectilesInstantiated;
    private bool instantiateChildGunGroundProjectiles;

    protected void Start() {
        instantiateChildGunGroundProjectiles = PlayerShoot.Instance.GetAAGunSpawnsChildBullets();

        childGunProjectilesInstantiated = PlayerShoot.Instance.GetHeldGun().GetSubExplosivesAmount();
        childDamagePerBullet = PlayerShoot.Instance.GetHeldGun().GetSubExplosivesDamage();
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

                float childBulletLifetimeRandomized = UnityEngine.Random.Range(childBulletLifetime - childBulletLifetime / 10, childBulletLifetime + childBulletLifetime / 10);
                gunProjectile.InitializeProjectile(parentGun, childBulletLifetimeRandomized, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier);
            }
        } else {
            for (int i = 0; i < childGunProjectilesInstantiated; ++i) {

                GunProjectile gunProjectile = Instantiate(childGunAirProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile>();
                gunProjectile.gameObject.SetActive(true);
                Vector2 initialForce = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));

                float lifeTimeRandomized = childBulletAirLifetime + UnityEngine.Random.Range(-childBulletAirLifetime/1.5f, childBulletAirLifetime / 1.5f);
                Debug.Log("lifeTimeRandomized " + lifeTimeRandomized);
                gunProjectile.InitializeProjectile(parentGun, lifeTimeRandomized, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier);
            }
        }
    }
}
