using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_AA : GunProjectile
{
    [SerializeField] private int damageToFlyingMultiplier;
    [SerializeField] private Transform childGunProjectilePrefab;

    private bool instantiateChildGunProjectiles;
    private int childGunProjectilesInstantiated = 5;

    private float childBulletLifetime = 10f;
    private int childDamagePerBullet = 5;
    private int childBulletKnockback = 0;

    protected void Start() {
        instantiateChildGunProjectiles = PlayerShoot.Instance.GetAAGunSpawnsChildBullets();
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

        if(instantiateChildGunProjectiles) {
            for(int i = 0; i < childGunProjectilesInstantiated; ++i) {

                GunProjectile gunProjectile = Instantiate(childGunProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile>();
                gunProjectile.gameObject.SetActive(true);
                Vector2 initialForce = new Vector2(UnityEngine.Random.Range(-10f,10f),UnityEngine.Random.Range(-10f, 10f));

                gunProjectile.InitializeProjectile(parentGun, childBulletLifetime, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier);
            }
        }
    }
}
