using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_AA : GunProjectile
{
    [SerializeField] private bool spawnChildBullets = true;
    [SerializeField] private int damageToFlyingMultiplier;
    [SerializeField] private Transform childGunAirProjectilePrefab;
    [SerializeField] private Transform childGunProjectilePrefab;
    [SerializeField] private Animator projectileAnimator;
    [SerializeField] private AnimatorOverrideController mineAnimator;
    [SerializeField] private CircleCollider2D detectionCollider;
    [SerializeField] private LayerMask creatureLayerMask;

    [SerializeField] private float childBulletLifetime = 10f;
    [SerializeField] private float childBulletAirLifetime = .22f;
    [SerializeField] private int childBulletKnockback = 0;

    private float timeBeforeMineDeployment = 2f;
    private float mineDeployTimer;
    private bool mineDeployed;

    private int childDamagePerBullet;
    private int childGunProjectilesInstantiated;
    private bool instantiateChildGunGroundProjectiles;
    private bool mine;
    private bool isChildProjectile;

    private static List<GunProjectile_AA> activeMines = new List<GunProjectile_AA>();
    public static int minesDeployedAmount;
    private int maxMinesDeployed = 10;

    protected void Start() {
        instantiateChildGunGroundProjectiles = PlayerShoot.Instance.GetAAGunSpawnsChildBullets();
        mine = PlayerShoot.Instance.GetAAGunSpawnsMines();

        if(mine && !isChildProjectile) {
            projectileAnimator.runtimeAnimatorController = mineAnimator;
            detectionCollider.excludeLayers = creatureLayerMask;
            detectionCollider.radius = .5f;

            activeMines.Add(this); 
            
            if (activeMines.Count > maxMinesDeployed) {
                ExplodeOldestMine();
            }

            minesDeployedAmount++;
        }

        childGunProjectilesInstantiated = PlayerShoot.Instance.GetHeldGun().GetSubExplosivesAmount();
        childDamagePerBullet = PlayerShoot.Instance.GetHeldGun().GetSubExplosivesDamage();
    }

    private void ExplodeOldestMine() {

        if (activeMines.Count == 0) return;

        GunProjectile_AA oldestMine = activeMines[0];

        if (oldestMine != null) {
            oldestMine.ForceExplode();
        }
    }

    public void ForceExplode() {

        if (projectileExploded) return;

        Explode();
    }

    protected override void Update() {
        if (projectileExploded) return;

        if (mine && !isChildProjectile) {

            if(!mineDeployed) {
                mineDeployTimer += Time.deltaTime;
                if (mineDeployTimer > timeBeforeMineDeployment) {
                    mineDeployed = true;
                    detectionCollider.excludeLayers = 0;
                    detectionCollider.radius = 1.22f;
                }

            }
           
            return;
        } else {

            base.Update();
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

    public void SetIsChildProjectile() {
        isChildProjectile = true;
    }

    protected override void Explode() {
        base.Explode();

        if (!spawnChildBullets) return;

        if (mine && activeMines.Contains(this)) {
            activeMines.Remove(this);
        }

        if (instantiateChildGunGroundProjectiles) {
            for(int i = 0; i < childGunProjectilesInstantiated; ++i) {

                GunProjectile_AA gunProjectile = Instantiate(childGunProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile_AA>();
                gunProjectile.gameObject.SetActive(true);
                gunProjectile.SetIsChildProjectile();
                Vector2 initialForce = new Vector2(UnityEngine.Random.Range(-10f,10f),UnityEngine.Random.Range(-10f, 10f));

                float childBulletLifetimeRandomized = UnityEngine.Random.Range(childBulletLifetime - childBulletLifetime / 10, childBulletLifetime + childBulletLifetime / 10);
                gunProjectile.InitializeProjectile(parentGun, childBulletLifetimeRandomized, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier, 1, 1);
            }
        } else {
            for (int i = 0; i < childGunProjectilesInstantiated; ++i) {

                GunProjectile_AA gunProjectile = Instantiate(childGunAirProjectilePrefab, transform.position, Quaternion.identity).GetComponent<GunProjectile_AA>();
                gunProjectile.gameObject.SetActive(true);
                gunProjectile.SetIsChildProjectile();
                Vector2 initialForce = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));

                float lifeTimeRandomized = childBulletAirLifetime + UnityEngine.Random.Range(-childBulletAirLifetime/1.5f, childBulletAirLifetime / 1.5f);
                gunProjectile.InitializeProjectile(parentGun, lifeTimeRandomized, childDamagePerBullet, childBulletKnockback, initialForce, explosionRadiusMultiplier, 1, 1);
            }
        }
    }
}
