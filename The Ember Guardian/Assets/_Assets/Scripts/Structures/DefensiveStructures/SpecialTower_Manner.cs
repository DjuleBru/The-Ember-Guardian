using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_Manner : MonoBehaviour {
    [SerializeField] private SpecialTower specialTower;
    [SerializeField] private GameObject visualGameObject;
    [SerializeField] private int mannerIndex;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private ParticleCollision shootPS_Collision;

    [SerializeField] private bool bulletIsParticle;
    [SerializeField] private bool bulletIsProjectile;
    [SerializeField] private float mannerRange;
    [SerializeField] private float mannerReloadTime;
    [SerializeField] private float mannerReloadHandsTime;
    [SerializeField] private float mannerCooldownTime;
    [SerializeField] private int bulletDamage;
    [SerializeField] private float bulletKnockback;
    [SerializeField] private float collisionDistanceTreshold;
    [SerializeField] private int shotsPerAmmoClip;
    [SerializeField] private int pelletsPerBullet;
    private int currentShotIndex;

    private EngineerJob engineerManning;
    private float reloadTimer;
    private float cooldownTimer;
    private bool reloading;
    private bool reloadingStarted;
    private bool readyToShoot;

    public event EventHandler OnEngineerStartedManning;
    public event EventHandler OnEngineerStoppedManning;
    public event EventHandler OnCreatureTargeted;
    public event EventHandler OnNoCreatureFound;
    public event EventHandler OnMannerShot;
    public event EventHandler OnMannerReloadingStarted;
    public event EventHandler OnMannerReloadingHandsEnded;

    private bool isManning;
    private Creature targetCreature;

    private void Awake() {
        if (mannerIndex != 0) {
            visualGameObject.SetActive(false);
        }
        shootPS_Collision.InitializeBulletPS(transform, bulletDamage, bulletKnockback, collisionDistanceTreshold);

        currentShotIndex = shotsPerAmmoClip;
    }

    private void Start() {
        specialTower.OnEngineerExitedTower += SpecialTower_OnEngineerStoppedManning;
        specialTower.OnEngineerEnteredTower += SpecialTower_OnEngineerStartedManning;
        specialTower.OnStructureUpgraded += SpecialTower_OnStructureUpgraded;
    }


    private void Update() {
        if (!isManning) return;

        HandleCooldown();

        if (reloading || specialTower.GetCurrentAmmoClip() == 0) return;
        
        if (targetCreature == null) return;

        HandleAim();

        if(readyToShoot) {
            Shoot();
        }
    }

    private void Shoot() {

        if (bulletIsParticle) {
            shootPS.Emit(pelletsPerBullet);
            OnMannerShot?.Invoke(this, EventArgs.Empty);
        }

        if (bulletIsProjectile) {
            //GunProjectile gunProjectile = Instantiate(projectilePrefab, projectileSpawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
            //gunProjectile.gameObject.SetActive(true);
            //Vector2 initialForce = PlayerAim.Instance.GetAimDir().normalized * bulletSpeed;
            //gunProjectile.InitializeProjectile(this, bulletLifetime, damagePerBullet, bulletKnockback, initialForce, explosionRadiusMultiplier);
        }

        currentShotIndex--;
        if(currentShotIndex == 0) {
            StartCoroutine(HandleReloading());
            reloading = true;
        }

        readyToShoot = false;
    }

    private IEnumerator HandleReloading() {
        OnMannerReloadingStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(mannerReloadHandsTime);

        OnMannerReloadingHandsEnded?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(mannerReloadTime - mannerReloadHandsTime);

        currentShotIndex = shotsPerAmmoClip;

        reloading = false;
        reloadTimer = 0;
    }

    private void HandleCooldown() {
        if (readyToShoot) return;

        cooldownTimer += Time.deltaTime;
        if(cooldownTimer > mannerCooldownTime) {
            readyToShoot = true;
            cooldownTimer = 0;
        }
    }

    private void HandleAim() {
        Transform target = targetCreature.GetAutoAimPosition();
        Vector3 direction = target.position - weaponTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        Vector3 localScale = Vector3.one;
        if (direction.x < 0) {
            localScale.y = -1;
        }
        weaponTransform.localScale = localScale;

    }

    private void SpecialTower_OnEngineerStoppedManning(object sender, System.EventArgs e) {
        if (specialTower.GetEngineersWorking().Count == mannerIndex) {
            isManning = false;
            OnEngineerStoppedManning?.Invoke(this, EventArgs.Empty);
            SetEngineerManning(engineerManning, false);
            weaponTransform.rotation = Quaternion.identity;
            weaponTransform.localScale = Vector3.one;
        }
    }

    private void SpecialTower_OnEngineerStartedManning(object sender, System.EventArgs e) {

        if (specialTower.GetEngineersWorking().Count == mannerIndex + 1) {
            isManning = true;
            OnEngineerStartedManning?.Invoke(this, EventArgs.Empty);
            SetEngineerManning(specialTower.GetEngineersWorking()[mannerIndex], true);
        }
    }

    private void SpecialTower_OnStructureUpgraded(object sender, System.EventArgs e) {
        if (mannerIndex == 1) {
            visualGameObject.SetActive(true);
        }
    }

    private void SetEngineerManning(EngineerJob engineerJob, bool manning) {

        if (manning) {
            engineerManning = engineerJob;
            engineerManning.GetDetectionCollider().SetDetectionColliderRadius(mannerRange);
            engineerManning.GetDetectionCollider().OnCreaturesInColliderChanged += SpecialTower_Manner_OnCreaturesInColliderChanged;
        }
        else {
            engineerManning.GetDetectionCollider().ResetDetectionColliderRadius();
            engineerManning.GetDetectionCollider().OnCreaturesInColliderChanged -= SpecialTower_Manner_OnCreaturesInColliderChanged;
        }

    }


    private void SpecialTower_Manner_OnCreaturesInColliderChanged(object sender, EventArgs e) {
        targetCreature = engineerManning.GetDetectionCollider().GetClosestCreature();

        if (targetCreature != null) {
            OnCreatureTargeted?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnNoCreatureFound?.Invoke(this, EventArgs.Empty);
        }
    }
}
