using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI_Robodog : DogAI
{
    [SerializeField] private ProjectileSO missileProjectileSO;
    [SerializeField] private LineRenderer speedupLineRenderer;

    [SerializeField] private Transform spawnPosition;

    private bool ammoCraftAbilityUnlocked;
    private bool speedUpAbilityUnlocked;

    private bool missilesAbilityReady;
    private bool ammoCraftAbilityReady;
    private bool speedUpAbilityReady;

    private float ammoCraftTimer;
    private float missilesTimer;
    private float speedUpTimer;

    private float ammoCraftMaxDistanceToPlayer = 15f;
    private float missilesAbilityRange = 10f;
    private float speedUpAbilityRange = 6f;

    private float delayToOpenHatch = .6f;
    private float delayToCloseHatch = .4f;
    private float delayBetweenMissiles = .4f;
    private float delayToSpawnLaser = .8f;

    public event EventHandler OnHatchOpened;
    public event EventHandler OnHatchClosed;

    public event EventHandler OnAmmoCrafted;
    public event EventHandler OnAmmoAbilityStarted;
    public event EventHandler OnSpeedupAbilityStarted;
    public event EventHandler OnMissileGenerated;
    public event EventHandler OnLaserAnimationStarted;


    protected override void Start() {
        base.Start();

        DogStats.Instance.OnNewAbilityUnlocked += DogStats_OnNewAbilityUnlocked;

        ammoCraftAbilityUnlocked = DogStats.Instance.GetRobodogAmmoCraftAbilityUnlocked();
        speedUpAbilityUnlocked = DogStats.Instance.GetRobodogSpeedUpAbilityUnlocked();
    }

    private void DogStats_OnNewAbilityUnlocked(object sender, EventArgs e) {
        ammoCraftAbilityUnlocked = DogStats.Instance.GetRobodogAmmoCraftAbilityUnlocked();
        speedUpAbilityUnlocked = DogStats.Instance.GetRobodogSpeedUpAbilityUnlocked();
    }

    protected override void Update() {
        base.Update();

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;
        HandleSpeedupAbility();
        HandleAmmoCraftAbility();
    }

    private void HandleSpeedupAbility() {

    }

    private void HandleAmmoCraftAbility() {
        if (!ammoCraftAbilityUnlocked) return;
        if (state == State.attacking || state == State.growling || state == State.barking) return;

        ammoCraftTimer += Time.deltaTime;

        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);
        if (distanceToPlayer > ammoCraftMaxDistanceToPlayer) return;

        if (ammoCraftTimer >= DogStats.Instance.GetAmmoFactoryCooldown()) {
            OnHatchOpened?.Invoke(this, EventArgs.Empty);

            StartCoroutine(CraftAmmo());
            ammoCraftTimer = 0;
        }

    }

    protected IEnumerator CraftAmmo() {
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
    }

    protected override void HandleBarkToAttack() {
        ChangeState(State.attacking);
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
