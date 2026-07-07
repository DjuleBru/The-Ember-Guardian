using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI_Robodog : DogAI
{
    [SerializeField] private Transform missileProjectilePrefab;
    [SerializeField] private LineRenderer speedupLineRenderer;

    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Transform laserSpeedupSpawnPosition;

    private bool ammoCraftAbilityUnlocked;
    private bool speedUpAbilityUnlocked;

    private bool missilesAbilityReady;
    private bool ammoCraftAbilityReady;
    private bool speedUpAbilityReady;

    private float speedUpAmountActive;
    private bool speedUpActive;


    private bool isHandlingAmmoHatch;
    private bool isHandlingSpeedupHatch;
    private bool isHandlingMissileHatch;

    private float ammoCraftTimer;
    private float missilesTimer;
    private float speedUpTimer;
    private float speedUpTimeTimer;

    private float ammoCraftMaxDistanceToPlayer = 15f;
    private float speedUpAbilityRange = 10f;

    private float delayToOpenHatch = .6f;
    private float delayToCloseHatch = .4f;
    private float delayBetweenMissiles = .4f;
    private float delayToSpawnLaser = .8f;
    private float delayAfterLaserSpawnedToCloseHatch = .4f;

    private float missilesLaunchSpeed = 6f;

    public event EventHandler OnHatchOpened;
    public event EventHandler OnHatchClosed;
    public event EventHandler OnHatchClosedFinished;

    public event EventHandler OnAmmoCrafted;
    public event EventHandler OnAmmoAbilityStarted;
    public event EventHandler OnSpeedupAbilityStarted;
    public event EventHandler OnSpeedupAnimationStarted;
    public event EventHandler OnMissileAbilityStarted;
    public event EventHandler OnMissileAbilityEnded;
    public event EventHandler OnMissileGenerated;


    protected override void Start() {
        base.Start();

        DogStats.Instance.OnNewAbilityUnlocked += DogStats_OnNewAbilityUnlocked;

        ammoCraftAbilityUnlocked = DogStats.Instance.GetRobodogAmmoCraftAbilityUnlocked();
        speedUpAbilityUnlocked = DogStats.Instance.GetRobodogSpeedUpAbilityUnlocked();

        speedupLineRenderer.enabled = false;
    }

    private void DogStats_OnNewAbilityUnlocked(object sender, EventArgs e) {
        ammoCraftAbilityUnlocked = DogStats.Instance.GetRobodogAmmoCraftAbilityUnlocked();
        speedUpAbilityUnlocked = DogStats.Instance.GetRobodogSpeedUpAbilityUnlocked();
    }

    protected override void Update() {
        HandleSpeedUpDuration();

        base.Update();

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;
        HandleSpeedupAbility();
        HandleAmmoCraftAbility();
    }

    protected override void HeadToAttackClosestCreature() {
        float distanceToCreature = Mathf.Abs(transform.position.x - closestCreature.transform.position.x);
        if (distanceToCreature < creatureBarkDistanceToDog && biteReady) {
            dogMovement.SetMoveTarget(transform.position);

            StartCoroutine(MissilesAbilityCoroutine());
            biteReady = false;
            biteStarted = true;
            biteTimer = GetBiteCooldown();
            return;

        }
        else {

            if (!biteStarted) {
                dogMovement.SetMoveTarget(closestCreature.transform.position);
            }

        }
    }

    private void HandleSpeedupAbility() {
        if (!speedUpAbilityUnlocked) return;

        speedUpTimer += Time.deltaTime;

        if (isHandlingAmmoHatch) return;
        if (isHandlingMissileHatch) return;
        if (state == State.attacking) return;


        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);
        if (distanceToPlayer > speedUpAbilityRange) return;


        if (speedUpTimer >= DogStats.Instance.GetRobodogSpeedUpCooldown()) {
            OnHatchOpened?.Invoke(this, EventArgs.Empty);
            StartCoroutine(SpeedUp());
            speedUpTimer = 0;
        }

    }

    private void HandleAmmoCraftAbility() {
        if (!ammoCraftAbilityUnlocked) return;

        ammoCraftTimer += Time.deltaTime;

        if (isHandlingSpeedupHatch) return;
        if (isHandlingMissileHatch) return;
        if (state == State.attacking) return;


        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);
        if (distanceToPlayer > ammoCraftMaxDistanceToPlayer) return;

        if (ammoCraftTimer >= DogStats.Instance.GetAmmoFactoryCooldown()) {
            OnHatchOpened?.Invoke(this, EventArgs.Empty);

            StartCoroutine(CraftAmmo());
            ammoCraftTimer = 0;
        }

    }

    private IEnumerator MissilesAbilityCoroutine() {
        OnHatchOpened?.Invoke(this, EventArgs.Empty);
        OnMissileAbilityStarted?.Invoke(this, EventArgs.Empty);
        isHandlingMissileHatch = true;

        yield return new WaitForSeconds(delayToOpenHatch);

        for(int i = 0; i < DogStats.Instance.GetMissilesRocketAmount(); i++) {
            GunProjectile projectile = Instantiate(missileProjectilePrefab, spawnPosition.position, Quaternion.identity).GetComponent<GunProjectile>();
            projectile.InitializeProjectile(null, 3f, DogStats.Instance.GetRobodogMissilesDamage(), 0, new Vector2(0, missilesLaunchSpeed), 1, 10000, 1);

            OnMissileGenerated?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(delayBetweenMissiles);
        }

        OnMissileAbilityEnded?.Invoke(this, EventArgs.Empty);

        biteStarted = false;
        biteReady = false;

        yield return new WaitForSeconds(delayToCloseHatch);

        OnHatchClosed?.Invoke(this, EventArgs.Empty);
        isHandlingMissileHatch = false;
        ChangeState(State.walkWithPlayer);
    }

    protected IEnumerator CraftAmmo() {
        isHandlingAmmoHatch = true;
        OnAmmoAbilityStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToOpenHatch);
        OnAmmoCrafted?.Invoke(this, EventArgs.Empty);

        bool hasOnlySpecialAmmo = PlayerShoot.Instance.GetHasOnlySpecialAmmo();
        bool hasBothAmmoTypes = PlayerShoot.Instance.GetHasBothAmmoTypes();

        PlayerCurrencies.CurrencyType ammoTypeToSpawn = PlayerCurrencies.CurrencyType.ammo;

        if(hasOnlySpecialAmmo) {
            ammoTypeToSpawn = PlayerCurrencies.CurrencyType.ammo_special;
        }
        if(hasBothAmmoTypes) {
            if(UnityEngine.Random.value < .5f) {
                ammoTypeToSpawn = PlayerCurrencies.CurrencyType.ammo_special;
            }
        }

        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(ammoTypeToSpawn), spawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomForce(-4, 4, 10, 20);
        collectible.ApplyRandomTorque(-20, 20);
        collectible.SetCollectibleUnInteractable(.3f);

        yield return new WaitForSeconds(delayToCloseHatch);
        OnHatchClosed?.Invoke(this, EventArgs.Empty);

        isHandlingAmmoHatch = false;
    }

    protected IEnumerator SpeedUp() {
        isHandlingSpeedupHatch = true;
        OnSpeedupAbilityStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToOpenHatch);
        OnSpeedupAnimationStarted?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(delayToSpawnLaser);


        speedUpActive = true;
        speedUpTimeTimer = 0;
        speedUpAmountActive = 1 + DogStats.Instance.GetSpeedUpAmount() / 100f;
        PlayerMovement.Instance.BuffMoveSpeed("robodog", speedUpAmountActive);
        PlayerMovement.Instance.StartRobodogBoost();

        // Activer le line renderer vers le joueur pendant une durée limitée (.2s)

        speedupLineRenderer.enabled = true;
        speedupLineRenderer.positionCount = 2;

        float timer = .3f;

        while (timer > 0) {
            speedupLineRenderer.SetPosition(0, laserSpeedupSpawnPosition.position);
            speedupLineRenderer.SetPosition(1, new Vector3(Player.Instance.transform.position.x, 1f, 0));

            timer -= Time.deltaTime;
            yield return null;
        }

        speedupLineRenderer.enabled = false;

        yield return new WaitForSeconds(delayAfterLaserSpawnedToCloseHatch);
        yield return new WaitForSeconds(delayToCloseHatch);
        OnHatchClosedFinished?.Invoke(this, EventArgs.Empty);
        OnHatchClosed?.Invoke(this, EventArgs.Empty);

        isHandlingSpeedupHatch = false;
    }

    private void HandleSpeedUpDuration() {
        if (!speedUpActive) return;

        speedUpTimeTimer += Time.deltaTime;
        if(speedUpTimeTimer > DogStats.Instance.GetSpeedUpDuration()) {
            speedUpActive = false;
            PlayerMovement.Instance.DebuffMoveSpeed("robodog", speedUpAmountActive);
            PlayerMovement.Instance.EndRobodogBoost();
        }

    }

    protected override void HandleBarkToAttack() {
        barkingTimer += Time.deltaTime;
        if (closestCreature == null) return;

        foreach (Creature creature in creatureDetectionCollider.GetCreaturesInRange()) {
            if (!creature.GetCreatureSO().flying) return;
        }

        if (barkingTimer > barkTimeToAttack && biteReady) {
            barkingTimer = 0;
            biteStarted = false;
            ChangeState(State.attacking);
        }
    }
    protected override void AttackingStateUpdate() {
        if (biteStarted) return;
        if (closestCreature == null) {
            ChangeState(State.runWithPlayer);
            return;
        }

        HeadToAttackClosestCreature();
    }
}
