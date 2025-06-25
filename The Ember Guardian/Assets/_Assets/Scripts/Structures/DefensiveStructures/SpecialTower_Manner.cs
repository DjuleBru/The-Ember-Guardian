using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_Manner : MonoBehaviour {

    public enum MannerType {
        Sniper,
        Mortar,
        MachineGun,
    }

    [SerializeField] protected MannerType mannerType;
    [SerializeField] protected SpecialTower specialTower;
    [SerializeField] protected GameObject visualGameObject;
    [SerializeField] protected int mannerIndex;
    [SerializeField] protected Transform weaponTransform;
    [SerializeField] protected ParticleSystem shootPS;
    [SerializeField] protected ParticleCollision shootPS_Collision;

    [SerializeField] protected bool bulletIsParticle;
    [SerializeField] protected bool bulletIsProjectile;
    [SerializeField] protected bool hasCooldownAnimation;
    [SerializeField] protected bool hasCooldownSound;
    [SerializeField] protected float cooldownDelay;
    [SerializeField] protected float mannerMinimumShootDistance;
    [SerializeField] protected float mannerRange;
    [SerializeField] protected float mannerReloadTime;
    [SerializeField] protected float mannerReloadHandsTime;
    [SerializeField] protected float mannerCooldownTime;
    [SerializeField] protected int bulletDamage;
    [SerializeField] protected float bulletKnockback;
    [SerializeField] protected float collisionDistanceTreshold;
    [SerializeField] protected int shotsPerAmmoClip;
    [SerializeField] protected int pelletsPerBullet;
    [SerializeField] protected float maxAngleToAim = 360f;
    protected int currentShotIndex;

    [SerializeField] protected int maxEngineersManning;
    [SerializeField] protected bool hasReloadingEngineer;
    protected List<EngineerJob> engineersManning = new List<EngineerJob>();
    protected float reloadTimer;
    protected float cooldownTimer;
    protected bool reloading;
    protected bool reloadingStarted;
    protected bool readyToShoot;
    protected bool cooldownTriggered;

    public event EventHandler OnEngineerStartedManning;
    public event EventHandler OnEngineerStoppedManning;
    public event EventHandler OnCreatureTargeted;
    public event EventHandler OnNoCreatureFound;
    public event EventHandler OnMannerShot;
    public event EventHandler OnMannerReloadingStarted;
    public event EventHandler OnMannerReloadingHandsEnded;
    public event EventHandler OnMannerCooldownEventTriggered;

    protected bool towerOutOfAmmo;
    protected Creature targetCreature;

    protected float targetCreatureTimer;
    protected float targetCreatureRate = .5f;

    protected virtual void Awake() {
        if (mannerIndex != 0) {
            visualGameObject.SetActive(false);
        }
        if(bulletIsParticle) {
            shootPS_Collision.InitializeBulletPS(transform, bulletDamage, bulletKnockback, collisionDistanceTreshold);
        }

        currentShotIndex = shotsPerAmmoClip;
        readyToShoot = true;
    }

    protected void Start() {
        specialTower.OnEngineerExitedTower += SpecialTower_OnEngineerExitedTower;
        specialTower.OnStructureUpgraded += SpecialTower_OnStructureUpgraded;
        specialTower.OnAmmoClipAdded += SpecialTower_OnAmmoClipAdded;
    }

    protected void SpecialTower_OnAmmoClipAdded(object sender, EventArgs e) {
        if(towerOutOfAmmo && currentShotIndex == 0) {
            StartCoroutine(HandleReloading());
        }

        towerOutOfAmmo = false;
    }

    protected void Update() {
        if (engineersManning.Count == 0) return;
        if (currentShotIndex == 0) return;
        if (reloading) return;

        HandleCooldown();

        targetCreatureTimer -= Time.deltaTime;
        if(targetCreatureTimer < 0) {
            targetCreatureTimer = targetCreatureRate;
            HandleTargetingCreatures();
        }

        if (targetCreature == null) return;

        HandleAim();

        if(readyToShoot) {
            Shoot();
        }
    }

    protected virtual void Shoot() {
        if (bulletIsProjectile) {
            //GunProjectile gunProjectile = Instantiate(projectilePrefab, projectileSpawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
            //gunProjectile.gameObject.SetActive(true);
            //Vector2 initialForce = PlayerAim.Instance.GetAimDir().normalized * bulletSpeed;
            //gunProjectile.InitializeProjectile(this, bulletLifetime, damagePerBullet, bulletKnockback, initialForce, explosionRadiusMultiplier);
        }

        if (specialTower.GetCurrentAmmoClip() == 0) {
            towerOutOfAmmo = true;
        }

        currentShotIndex--;
        if(currentShotIndex == 0 && !towerOutOfAmmo) {

            StartCoroutine(HandleReloading());
            reloading = true;
        }


        if (bulletIsParticle) {
            shootPS.Emit(pelletsPerBullet);
        }

        OnMannerShot?.Invoke(this, EventArgs.Empty);
        readyToShoot = false;
        cooldownTriggered = false;
    }

    protected IEnumerator HandleReloading() {
        if (hasReloadingEngineer && engineersManning.Count < maxEngineersManning) yield break;

        yield return new WaitForSeconds(.5f);

        OnMannerReloadingStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(mannerReloadHandsTime);

        OnMannerReloadingHandsEnded?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(mannerReloadTime - mannerReloadHandsTime);

        currentShotIndex = shotsPerAmmoClip;

        reloading = false;
        readyToShoot = true;
        reloadTimer = 0;
    }

    public void InvokeOnMannerReloadingHandsEnded() {
        OnMannerReloadingHandsEnded?.Invoke(this, EventArgs.Empty);
    }

    protected void HandleCooldown() {
        if (readyToShoot) return;

        cooldownTimer += Time.deltaTime;

        if(cooldownTimer > cooldownDelay) {
            if(!cooldownTriggered) {
                cooldownTriggered = true;
                OnMannerCooldownEventTriggered?.Invoke(this, EventArgs.Empty);
            }
        }

        if(cooldownTimer > mannerCooldownTime) {
            readyToShoot = true;
            cooldownTimer = UnityEngine.Random.Range(0, cooldownDelay/8);
        }
    }

    protected virtual void HandleAim() {
        Transform target = targetCreature.GetAutoAimPosition();
        Vector3 direction = target.position - weaponTransform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        Vector3 localScale = Vector3.one;

        if (direction.x < 0) {
            localScale.y = -1;
        }
        if (transform.position.x < 0) {
            localScale.x = -1;
        }

        weaponTransform.localScale = localScale;

    }

    public void AssignEngineer(EngineerJob engineer) {

        SetEngineerManning(engineer, true);
        OnEngineerStartedManning?.Invoke(this, EventArgs.Empty);
    }

    protected void SpecialTower_OnEngineerExitedTower(object sender, SpecialTower.OnEngineerEnteredTowerEventArgs e) {
        if (engineersManning.Contains(e.engineerJob)) {
            SetEngineerManning(e.engineerJob, false);
            OnEngineerStoppedManning?.Invoke(this, EventArgs.Empty);
            weaponTransform.rotation = Quaternion.identity;
            weaponTransform.localScale = Vector3.one;
        }
    }

    protected virtual void SpecialTower_OnStructureUpgraded(object sender, System.EventArgs e) {
        if (mannerIndex == 1) {
            visualGameObject.SetActive(true);
        }
    }

    protected void SetEngineerManning(EngineerJob engineerJob, bool manning) {
        if (manning) {
            engineersManning.Add(engineerJob);
            engineerJob.GetDetectionCollider().SetDetectionColliderRadius(mannerRange);
            engineerJob.GetDetectionCollider().OnCreaturesInColliderChanged += SpecialTower_Manner_OnCreaturesInColliderChanged;
        }
        else {
            engineerJob.GetDetectionCollider().ResetDetectionColliderRadius();
            engineerJob.GetDetectionCollider().OnCreaturesInColliderChanged -= SpecialTower_Manner_OnCreaturesInColliderChanged;
            engineersManning.Remove(engineerJob);
        }
    }

    protected void SpecialTower_Manner_OnCreaturesInColliderChanged(object sender, EventArgs e) {
        HandleTargetingCreatures();
    }

    protected void HandleTargetingCreatures() {
        if (mannerType == MannerType.Sniper) {
            targetCreature = GetHighestHealthCreature(engineersManning[0].GetDetectionCollider().GetCreaturesInDetectionCollider(), mannerMinimumShootDistance);
        }
        if (mannerType == MannerType.MachineGun) {
            targetCreature = GetClosestCreature(engineersManning[0].GetDetectionCollider().GetCreaturesInDetectionCollider(), mannerMinimumShootDistance);
        }
        if (mannerType == MannerType.Mortar) {
            targetCreature = GetClosestCreature(engineersManning[0].GetDetectionCollider().GetCreaturesInDetectionCollider(), mannerMinimumShootDistance);
        }

        if (targetCreature != null) {
            OnCreatureTargeted?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnNoCreatureFound?.Invoke(this, EventArgs.Empty);
        }
    }

    public int GetCurrentShotIndex() {
        return currentShotIndex;
    }
    public int GetShotsPerAmmoClip() {
        return shotsPerAmmoClip;
    }

    protected Creature GetHighestHealthCreature(List<Creature> creaturesInRange, float minDistanceToShoot) {
        int maxCreatureHP = 0;
        Creature highestHPCreature = null;

        foreach(Creature creature in creaturesInRange) {

            float distanceToCreature = Vector3.Distance(transform.position, creature.transform.position);
            if (distanceToCreature < minDistanceToShoot) continue;

            if(creature.GetCreatureHealth() > maxCreatureHP) {
                highestHPCreature = creature;
                maxCreatureHP = creature.GetCreatureHealth();
            }
        }

        return highestHPCreature;
    }

    protected Creature GetClosestCreature(List<Creature> creaturesInRange, float minDistanceToShoot) {
        float closestCreatureDistance = Mathf.Infinity;
        Creature closestCreature = null;

        foreach (Creature creature in creaturesInRange) {

            float distanceToCreature = Vector3.Distance(transform.position, creature.transform.position);
            if (distanceToCreature < minDistanceToShoot) continue;

            // Si un angle max est défini (< 360), on le prend en compte
            Vector3 directionToCreature = creature.transform.position - transform.position;
            Vector3 aimDir = new Vector3(1, 0, 0);
            if (transform.position.x < 0) {
                aimDir = new Vector3(-1, 0, 0);
            }
            if (maxAngleToAim < 360f) {
                float angle = Vector3.Angle(aimDir, directionToCreature);
                if (angle > maxAngleToAim * 0.5f)
                    continue;
            }

            if (distanceToCreature < closestCreatureDistance) {
                closestCreature = creature;
                closestCreatureDistance = distanceToCreature;
            }
        }

        return closestCreature;
    }


    public void InvokeOnMannerShot() {
        OnMannerShot?.Invoke(this, EventArgs.Empty);
    }
    public bool GetHasCooldownAnimation() {
        return hasCooldownAnimation;
    }

    public bool HasAvailableSlot() {
        return engineersManning.Count < maxEngineersManning;
    }
    public int GetMaxEngineersManning() {
        return maxEngineersManning;
    }
    public int GetEngineersManning() {
        return engineersManning.Count;
    }

    public Creature GetTargetCreature() {
        return targetCreature;
    }

    private void OnDestroy() {
        foreach (EngineerJob engineer in engineersManning) {
            engineer.GetDetectionCollider().OnCreaturesInColliderChanged -= SpecialTower_Manner_OnCreaturesInColliderChanged;
        }
    }
}
