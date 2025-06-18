using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] private CreatureSO creatureSO;
    [SerializeField] private CreatureDetectionCollider detectionCollider;
    [SerializeField] private CreatureMovement creatureMovement;
    [SerializeField] private List<Collider2D> critZoneColliders;
    [SerializeField] private Transform autoAimPosition;

    private bool creatureUnlocked;
    private bool dropRedOrbsUnlocked;
    private bool dayCreature;
    private int inFireLightAmount;

    private bool creatureTargeted;
    private bool creatureCanBeTargeted = true;

    private bool eliteCreature;
    private bool eliteHPCreature;
    private bool eliteSpeedCreature;
    private bool eliteDamageCreature;

    public event EventHandler OnCreatureEnteredLight;
    public event EventHandler OnCreatureExitedLight;

    public event EventHandler OnCreatureDied;
    public event EventHandler OnCreatureUntargetable;
    public event EventHandler OnCreatureTargetable;
    public event EventHandler OnCreatureIdleSoundTriggered;

    private float triggerSoundTimer;
    private float triggerSoundTime = 5f;

    private float detectionRangeIncreasedTimer;
    private float detectionRangeIncreasedTime = 5f;
    private bool detectionRangeIncreased;

    private bool playerCrouchRangeDecreased;
    private float playerShootDetectionRangeMultiplier;

    public event EventHandler OnCreatureImmobilizedStarted;
    public event EventHandler OnCreatureImmobilizedStopped;
    private bool immobilizeImmune;
    private bool immobilized;
    private float immobilizedDuration;
    private float immobilizedTimer;

    public event EventHandler OnCreaturePoisonedStarted;
    public event EventHandler OnCreaturePoisoneStopped;
    private bool poisoned;
    private bool poisonImmune;
    private int poisonAmount;
    private float poisonedTimer;
    private float poisonRate = 1.5f;
    private float poisonRateTimer;
    private float poisonedDuration = 10f;

    public event EventHandler OnCreatureBurningStarted;
    public event EventHandler OnCreatureBurningStopped;
    private bool burning;
    private bool burnImmune;
    private int burnAmount = 2;
    private float burningTimer;
    private float burningRate = .5f;
    private float burningRateTimer;
    private float burningDuration = 2f;

    public event EventHandler OnCreatureShockedStarted;
    public event EventHandler OnCreatureShockedStopped;
    private bool shocked;
    private bool shockedImmune;
    private float shockedDuration = 10f;
    private float shockedTimer;
    private float shockedSlowAmount;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = creatureSO.mass;
        triggerSoundTimer = UnityEngine.Random.Range(0, triggerSoundTime);

        shockedImmune = creatureSO.immuneToShock;
        poisonImmune = creatureSO.immuneToPoison;
        immobilizeImmune = creatureSO.immuneToImmobilize;
    }

    private void Start() {
        creatureUnlocked = MetaProgressionManager.Instance.GetCreatureUnlocked(creatureSO);

        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;
        MetaProgressionManager.Instance.OnCreatureSOUnlocked += MetaProgressionMaanger_OnCreatureSOUnlocked;
    }

    private void OnEnable() {
        CreaturesManager.Instance.AddCreatureSpawned(this);
        health = creatureSO.maxHealth;
        maxHealth = creatureSO.maxHealth;
    }

    protected override void Update() {
        base.Update();
        if (dead) return;

        triggerSoundTimer -= Time.deltaTime;

        if(triggerSoundTimer < 0) {
            OnCreatureIdleSoundTriggered?.Invoke(this, EventArgs.Empty);
            triggerSoundTimer = triggerSoundTime;
        }

        HandleStatusEffects();

        if (detectionRangeIncreased) {

            if(creatureMovement == null) {
                Debug.Log("creatureMovement is null");
                return;
            }
            if (PlayerAim.Instance == null) {
                Debug.Log("PlayerAim.Instance is null");
                return;
            }
            bool playerIsFacingCreature = PlayerAim.Instance.GetAimDirFloat() * creatureMovement.GetLastMoveDirFloat() <= 0;
            if (playerIsFacingCreature) {
                detectionRangeIncreasedTimer = detectionRangeIncreasedTime;
                return;
            };

            detectionRangeIncreasedTimer -= Time.deltaTime;
            if(detectionRangeIncreasedTimer < 0) {
                CreatureHeardPlayerShoot(false);
                detectionRangeIncreased = false;
            }
        }

    }

    public override void Die() {
        dead = true;

        CreaturesManager.Instance.RemoveCreatureSpawned(this);

        if(mobSpawner != null) {
            mobSpawner.RemoveMobFromMobSpawnedList(this);
        }

        base.Die();

        if(!creatureUnlocked) {
            creatureUnlocked = true;
            MetaProgressionManager.Instance.SetCreatureUnlocked(creatureSO);
        }

        if (dropRedOrbsUnlocked) {
            SpawnDroppedCurrencies(creatureSO.currencyTypeDroppedList, creatureSO.currencyDropAmountList);
            InvokeOnMobDroppedCollectibles(collectiblesDropped);
        }

        if(DemoMainLevelManager.Instance != null && DemoMainLevelManager.Instance.GetIsMainDemoLevel()) {
            DemoDropGems();
        }

        if(eliteCreature) {
            if(DemoMainLevelManager.Instance != null && DemoMainLevelManager.Instance.GetIsMainDemoLevel()) {
                DemoDropGems();
            } else {
                EliteDropGems();
            }
        }

        if(creatureSO.isBoss) {
            BossUI.Instance.Hide();
        }

        OnCreatureDied?.Invoke(this, EventArgs.Empty);
        StartCoroutine(DestroyGameObjectAfterDelay());
        GetComponent<Collider2D>().enabled = false;

        foreach(Collider2D cd in critZoneColliders) {
            cd.enabled = false;
        }

        GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    private void DemoDropGems() {
        List<PlayerCurrencies.CurrencyType> gemTypeDrop = new List<PlayerCurrencies.CurrencyType>();
        List<int> gemTypeAmountDrop = new List<int>();

        int gemTypeDropped = UnityEngine.Random.Range(0, 10);

        if (gemTypeDropped <= 3) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.greenGem);
        }

        if (gemTypeDropped > 3) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.redGem);
        }


        int gemAmountDropped = GetDroppedGems(.1f,5);
        gemTypeAmountDrop.Add(gemAmountDropped);

        SpawnDroppedCurrencies(gemTypeDrop, gemTypeAmountDrop);
    }

    private void EliteDropGems() {
        List<PlayerCurrencies.CurrencyType> gemTypeDrop = new List<PlayerCurrencies.CurrencyType>();
        List<int> gemTypeAmountDrop = new List<int>();

        int gemTypeDropped = UnityEngine.Random.Range(0, 5);
        if(gemTypeDropped == 0) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.greenGem);
        }
        if (gemTypeDropped == 1) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.redGem);
        }
        if (gemTypeDropped == 2) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.yellowGem);
        }
        if (gemTypeDropped == 3) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.blueGem);
        }
        if (gemTypeDropped == 4) {
            gemTypeDrop.Add(PlayerCurrencies.CurrencyType.purpleGem);
        }

        int gemAmountDropped = GetDroppedGems(.5f, 5);
        gemTypeAmountDrop.Add(gemAmountDropped);

        SpawnDroppedCurrencies(gemTypeDrop, gemTypeAmountDrop);
    }

    public int GetDroppedGems(float baseProbability, int maxGems) {
        int gemsDropped = 0;
        float probability = baseProbability;

        // Tant que la probabilité permet de dropper plus de gemmes et que le nombre de gemmes n'a pas atteint le max
        float randomValue = UnityEngine.Random.value;
        while (gemsDropped < maxGems && randomValue < probability) {
            gemsDropped++;
            // La probabilité de dropper une gemme supplémentaire est divisée par 2 à chaque fois
            probability *= 0.5f;
        }

        return gemsDropped;
    }

    public void SetAsEliteCreature() {
        eliteCreature = true;

        int randomStatBuffed = UnityEngine.Random.Range(0, 3);
        if(randomStatBuffed == 0) {
            eliteHPCreature = true;
            health *= 2;
        }
        if (randomStatBuffed == 1) {
            eliteSpeedCreature = true;

        }
        if (randomStatBuffed == 2) {
            eliteDamageCreature = true;
        }

    }

    private IEnumerator DisableGameObjectAfterDelay() {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    private IEnumerator DestroyGameObjectAfterDelay() {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public CreatureSO GetCreatureSO() {
        return creatureSO;
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if(collision.gameObject.GetComponentInParent<Fire>() != null) {
            if (inFireLightAmount != 0) return;
            if (creatureSO.isBoss) return;

            inFireLightAmount++;
            OnCreatureEnteredLight?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        if (collision.gameObject.GetComponentInParent<Fire>() != null) {
            inFireLightAmount--;

            if (inFireLightAmount != 0) return;
            if (creatureSO.isBoss) return;

            OnCreatureExitedLight?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetAsDayCreature(bool dayCreature) {
        this.dayCreature = dayCreature;

        if (dayCreature) {
            float radiusRandomizer = UnityEngine.Random.Range(-creatureSO.detectionRange_Day / 5, creatureSO.detectionRange_Day / 5);
            detectionCollider.SetRadius(creatureSO.detectionRange_Day + radiusRandomizer);
        }
        else {
            detectionCollider.SetRadius(creatureSO.detectionRange_Night);
        }
    }

    private void MetaProgressionMaanger_OnCreatureSOUnlocked(object sender, MetaProgressionManager.OnCreatureSOUnlockedEventArgs e) {
        if(e.creatureSOUnlocked == creatureSO) {
            creatureUnlocked = true;
        }
    }

    private void PlayerMovement_OnPlayerCrouchedEnded(object sender, EventArgs e) {
        float playerCrouchDetectionRangeDivider = PlayerStats.Instance.GetCrouchDetectionRangeReductionPercentBuff_Meta()/100f;
        if (playerCrouchDetectionRangeDivider == 0) return;
        detectionCollider.DebuffRadius(playerCrouchDetectionRangeDivider);
    }

    private void PlayerMovement_OnPlayerCrouched(object sender, EventArgs e) {
        float playerCrouchDetectionRangeDivider = PlayerStats.Instance.GetCrouchDetectionRangeReductionPercentBuff_Meta()/100f;
        if (playerCrouchDetectionRangeDivider == 0) return;
        detectionCollider.BuffRadius(playerCrouchDetectionRangeDivider);

        playerCrouchRangeDecreased = true;
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, EventArgs e) {
        if (detectionRangeIncreased) return;

        playerShootDetectionRangeMultiplier = PlayerShoot.Instance.GetHeldGunSO().shootCreatureHearMultiplier;
        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) > detectionCollider.GetRadius() * playerShootDetectionRangeMultiplier) return;
        // Player is too far

        detectionRangeIncreased = true;
        detectionRangeIncreasedTimer = detectionRangeIncreasedTime;
        CreatureHeardPlayerShoot(true);
    }

    private void CreatureHeardPlayerShoot(bool heard) {
        if (heard) {
            detectionCollider.BuffRadius(playerShootDetectionRangeMultiplier);
        } else {
            detectionCollider.DebuffRadius(playerShootDetectionRangeMultiplier);
        }
    }

    public bool IsDayCreature() { 
        return dayCreature;
    }

    public void SetCreatureTargeted(bool creatureTargeted) {
        this.creatureTargeted = creatureTargeted;
    }

    public void SetCreatureCanBeTargeted(bool canBeTargeted) {
        this.creatureCanBeTargeted = canBeTargeted;

        if(canBeTargeted) {
            OnCreatureTargetable?.Invoke(this, EventArgs.Empty);
        } else {
            OnCreatureUntargetable?.Invoke(this, EventArgs.Empty);
        }
    }
    public void SetCreatureHealth(int health) {
        this.health = health;
        this.maxHealth = health;
    }

    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {

        if (weakSpotHit) {
            float scaledDamage = damage * 1.2f;
            int baseDamage = Mathf.FloorToInt(scaledDamage);
            float fractional = scaledDamage - baseDamage;

            if (UnityEngine.Random.value < fractional)
                baseDamage += 1;

            damage = baseDamage;
        }

        base.TakeDamage(damage, damageSource, critHit, ignoreTemporaryInvincibility, weakSpotHit);

        if (!SettingsManager.Instance.GetShowDamageNumbers()) return;

        int damageToWriteAsNumber = damage;
        if(critHit) {
            damageToWriteAsNumber *= 2;
        }

        DamageNumber damageNumber = DamageNumberPool.Instance.Get(projectileTarget.position);
        damageNumber.Initialize(damageToWriteAsNumber, weakSpotHit, critHit);
    }

    #region STATUS EFFECTS
    private void HandleStatusEffects() {
        if(immobilized) {
            immobilizedTimer -= Time.deltaTime;
            if(immobilizedTimer < 0) {
                immobilized = false;
                OnCreatureImmobilizedStopped?.Invoke(this, EventArgs.Empty);
            }
        }
        if(poisoned) {
            poisonedTimer -= Time.deltaTime;
            poisonRateTimer += Time.deltaTime;
            if(poisonRateTimer >= poisonRate) {
                TakeDamage(poisonAmount, transform);
                poisonRateTimer = 0;
            }

            if (poisonedTimer < 0) {
                poisoned = false;
                OnCreaturePoisoneStopped?.Invoke(this, EventArgs.Empty);
            }
        }
        if (burning) {
            burningTimer -= Time.deltaTime;
            burningRateTimer += Time.deltaTime;
            if (burningRateTimer >= burningRate) {
                TakeDamage(burnAmount, transform);
                burningRateTimer = 0;
            }

            if (burningTimer < 0) {
                burning = false;
                OnCreatureBurningStopped?.Invoke(this, EventArgs.Empty);
            }
        }
        if (shocked) {
            shockedTimer -= Time.deltaTime;
            if (shockedTimer < 0) {
                shocked = false;
                OnCreatureShockedStopped?.Invoke(this, EventArgs.Empty);
            }
        }

    }

    public void ApplyImmobilizeEffect(float immobilizeDuration, Vector3 immobilizePosition) {
        if (dead) return;
        if (immobilizeImmune) return;
        immobilized = true;
        immobilizedDuration = immobilizeDuration;
        immobilizedTimer = immobilizedDuration;

        Vector3 position = new Vector3(immobilizePosition.x, transform.position.y, 0);
        transform.position = position;

        OnCreatureImmobilizedStarted?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyPoisonEffect(int poisonAmount) {
        if (dead) return;
        if (poisonImmune) return;
        poisoned = true;
        this.poisonAmount = poisonAmount;
        poisonedTimer = poisonedDuration;

        OnCreaturePoisonedStarted?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyShockedEffect(float slowAmount) {
        if (dead) return;
        if (shockedImmune) return;
        if (shocked) return;
        shocked = true;
        this.shockedSlowAmount = slowAmount;
        shockedTimer = shockedDuration;

        OnCreatureShockedStarted?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyBurning(int burnDuration) {
        if (dead) return;
        if (burnImmune) return;
        burning = true;
        this.burningDuration = burnDuration;
        burningTimer = burningDuration;

        OnCreatureBurningStarted?.Invoke(this, EventArgs.Empty);
    }

    #endregion
    public bool GetCreatureTargeted() {
        return creatureTargeted;
    }

    public int GetCreatureHealth() {
        return health;
    }
    public int GetCreatureMaxHealth() {
        return maxHealth;
    }
    public bool GetIsEliteCreature() {
        return eliteCreature;
    }

    public bool GetIsEliteSpeedCreature() {
        return eliteSpeedCreature;
    }

    public bool GetIsEliteDamageCreature() {
        return eliteDamageCreature;
    }

    public float GetShockSlowAmount() {
        return shockedSlowAmount;
    }

    public bool GetCreatureCanBeTargeted() {
        return creatureCanBeTargeted;
    }

    public Transform GetAutoAimPosition() {
        return autoAimPosition;
    }

    private void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched -= PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded -= PlayerMovement_OnPlayerCrouchedEnded;
    }


}
