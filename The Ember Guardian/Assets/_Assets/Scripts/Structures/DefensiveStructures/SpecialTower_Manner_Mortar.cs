using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_Manner_Mortar : SpecialTower_Manner
{

    [SerializeField] private ProjectileSO mortarProjectileSO_lvl1;
    [SerializeField] private ProjectileSO mortarProjectileSO_lvl2;
    [SerializeField] private Transform projectileSpawnPosition;
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
            targetCreature = GetClosestCreature(engineersManning[0].GetDetectionCollider().GetCreaturesInDetectionCollider(), mannerMinimumShootDistance);
        }
        // No more targets : cancel shot
        if (targetCreature == null) yield break;

        projectile.ActivateAndInitialize(targetCreature.transform, mortarProjectileSO, engineersManning[0].GetComponent<Worker>(), bulletDamage, randomizer, true);
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
