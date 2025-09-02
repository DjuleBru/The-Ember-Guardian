using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_Manner_Mortar : SpecialTower_Manner
{

    [SerializeField] private ProjectileSO mortarProjectileSO_lvl1;
    [SerializeField] private ProjectileSO mortarProjectileSO_lvl2;
    [SerializeField] private Transform testMortarProjectileForces;

    [SerializeField] private Transform projectileSpawnPosition;

    [SerializeField] private float level2Range;
    [SerializeField] private float level2Damage;

    protected Queue<Projectile> availableProjectiles = new Queue<Projectile>();
    private ProjectileSO mortarProjectileSO;

    public event EventHandler OnProjectileShot;
    private float delayBeforeShooting = .4f;
    private bool level2Mortar;

    protected override void Awake() {
        base.Awake();
        mortarProjectileSO = mortarProjectileSO_lvl1;

    }

    protected override void SpecialTower_OnStructureUpgraded(object sender, System.EventArgs e) {
        level2Mortar = true;
        mortarProjectileSO = mortarProjectileSO_lvl2;
        mannerRange = level2Range;
        
        foreach(EngineerJob engineer in engineersManning) {
            engineer.GetDetectionCollider().SetDetectionColliderRadius(mannerRange);
        }
    }

    protected override void Update() {
        
        if (engineersManning.Count == 0) return;
        if (currentShotIndex == 0) return;
        if (reloading) return;

        if (engineersManning.Count == 2) {
            HandleCooldown();
        };

        targetCreatureTimer -= Time.deltaTime;
        if (targetCreatureTimer < 0) {
            targetCreatureTimer = targetCreatureRate;
            HandleTargetingCreatures();
        }

        if (targetCreature == null) return;

        HandleAim();

        if (readyToShoot) {
            Shoot();
        }
    }

    protected override void Shoot() {
        StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine() {
        readyToShoot = false;
        cooldownTriggered = false;

        InvokeOnMannerShot();

        yield return new WaitForSeconds(delayBeforeShooting);

        Projectile projectile = engineersManning[0].GetComponent<WorkerAttack>().GetNextProjectileInPool(mortarProjectileSO);

        projectile.gameObject.SetActive(true);
        projectile.transform.position = projectileSpawnPosition.position;
        Vector3 randomizer = new Vector3(UnityEngine.Random.Range(1, -1), 0, 0);

        if(targetCreature == null) {
            // Target may have died in the delay
            targetCreature = GetClosestCreature(engineersManning[0].GetDetectionCollider().GetCreaturesInDetectionCollider(), mannerMinimumShootDistance, false);
        }
        // No more targets : cancel shot
        if (targetCreature == null) yield break;

        ProjectileForces projectileForces = projectile.GetComponent<ProjectileForces>();
        projectileForces.ActivateAndInitializeWithForces(targetCreature, mortarProjectileSO_lvl1, engineersManning[0].transform, bulletDamage, 0 ,true);
        OnProjectileShot?.Invoke(this, EventArgs.Empty);

        if (specialTower.GetCurrentAmmoClip() == 0) {
            towerOutOfAmmo = true;
        }

        currentShotIndex--;
        if (currentShotIndex == 0 && !towerOutOfAmmo) {
            InvokeOnMannerReloadingHandsEnded();
            currentShotIndex = shotsPerAmmoClip;
        }
    }

    protected override void HandleAim() {
    }
}
