using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] protected CreatureSO creatureSO;
    [SerializeField] protected CreatureDetectionCollider detectionCollider;
    [SerializeField] protected CreatureMovement creatureMovement;
    [SerializeField] protected List<Collider2D> critZoneColliders;
    [SerializeField] protected Transform autoAimPosition;

    [SerializeField] private float stuckCheckDelay = 1f;
    [SerializeField] private float stuckKillTime = 10f;
    [SerializeField] private float minMovementThreshold = 0.1f;
    private float stuckTimer;
    private float stuckCheckTimer;
    private Vector3 lastPosition;

    protected CreatureAI creatureAI;
    protected CreatureAttack creatureAttack;

    protected bool creatureActive = true;

    protected bool creatureUnlocked;
    protected bool dropRedOrbs;
    protected bool dayCreature;
    protected bool agressiveDayCreature;
    protected int inFireLightAmount;

    protected bool creatureTargeted;
    protected bool creatureCanBeTargeted = true;

    protected bool eliteCreature;
    protected bool eliteHPCreature;
    protected bool eliteSpeedCreature;
    protected bool eliteDamageCreature;

    public event EventHandler OnCreatureDied;
    public static event EventHandler<OnCreatureDiedEventArgs> OnAnyCreatureDied;

    public class OnCreatureDiedEventArgs : EventArgs {
        public Transform damageSource;
    }

    public event EventHandler OnCreatureEnteredLight;
    public event EventHandler OnCreatureExitedLight;

    public event EventHandler OnCreatureUntargetable;
    public event EventHandler OnCreatureTargetable;
    public event EventHandler OnCreatureIdleSoundTriggered;
    public event EventHandler OnCreatureEnabled;
    public event EventHandler OnCreaturePaused;
    public event EventHandler OnCreatureUnpaused;
    public static event EventHandler OnAnyCreatureKilledByDog;

    protected float triggerSoundTimer;
    protected float triggerSoundTime = 5f;

    protected float detectionRangeIncreasedTimer;
    protected float detectionRangeIncreasedTime = 5f;
    protected bool detectionRangeIncreased;
    protected float probabilityToDropOrb;
    protected float nightCreatureDropProbabilityReductionFactor = 4f;

    protected bool playerCrouchRangeDecreased;
    protected float playerShootDetectionRangeMultiplier;

    public event EventHandler OnCreatureImmobilizedStarted;
    public event EventHandler OnCreatureImmobilizedStopped;
    protected bool immobilizeImmune;
    protected bool immobilized;
    protected float immobilizedDuration;
    protected float immobilizedTimer;

    public event EventHandler OnCreaturePoisonedStarted;
    public event EventHandler OnCreaturePoisoneStopped;
    protected bool poisoned;
    protected bool poisonImmune;
    protected int poisonAmount;
    protected float poisonedTimer;
    protected float poisonRate = 1f;
    protected float poisonRateTimer;
    protected float poisonedDuration = 10f;

    public event EventHandler OnCreatureBurningStarted;
    public event EventHandler OnCreatureBurningStopped;
    protected bool burning;
    protected bool burnImmune;
    protected int burnAmount = 2;
    protected float burningTimer;
    protected float burningRate = .2f;
    protected float burningRateTimer;
    protected float burningDuration = 2f;

    public event EventHandler OnCreatureShockedStarted;
    public event EventHandler OnCreatureShockedStopped;
    protected bool shocked;
    protected bool shockedImmune;
    protected float shockedDuration = 10f;
    protected float shockedTimer;
    protected float shockedSlowAmount;


    public event EventHandler OnCreatureStunStarted;
    public event EventHandler OnCreatureStunStopped;
    protected bool stunned;
    protected bool stunImmune;
    protected float stunDuration = 10f;
    protected float stunTimer;

    protected bool creatureWasInFireWhenKilled;
    protected float initialGravityScale;

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = creatureSO.mass;
        creatureAI = GetComponent<CreatureAI>();
        creatureAttack = GetComponent<CreatureAttack>();

        triggerSoundTimer = UnityEngine.Random.Range(0, triggerSoundTime);

        shockedImmune = creatureSO.immuneToShock;
        poisonImmune = creatureSO.immuneToPoison;
        immobilizeImmune = creatureSO.immuneToImmobilize;
        burnImmune = creatureSO.immuneToFire;
        probabilityToDropOrb = creatureSO.probabilityToDropOrb;
        initialGravityScale = rb.gravityScale;
    }

    protected virtual void Start() {
        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.OnCyclePausedByMerchantTalk += DayNightManager_OnCyclePausedByMerchantTalk;
            DayNightManager.Instance.OnCycleUnpaused += DayNightManager_OnCycleUnpaused;
        }

        creatureUnlocked = MetaProgressionManager.Instance.GetCreatureUnlocked(creatureSO);

        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;
        MetaProgressionManager.Instance.OnCreatureSOUnlocked += MetaProgressionMaanger_OnCreatureSOUnlocked;

      
    }

    private void DayNightManager_OnCycleUnpaused(object sender, EventArgs e) {
        if (!creatureActive) return;
        if (rb == null) return;

        isPaused = false;
        rb.simulated = true;
        OnCreatureUnpaused?.Invoke(this, EventArgs.Empty);
    }

    private void DayNightManager_OnCyclePausedByMerchantTalk(object sender, EventArgs e) {
        if (!creatureActive) return;

        isPaused = true;
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        OnCreaturePaused?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnEnable() {
        CreaturesManager.Instance.AddCreatureSpawned(this);
        dead = false;
        maxHealth = creatureSO.maxHealth;

        if (SettingsManager.Instance.GetDifficulty() == SettingsManager.Difficulty.Hard) {
            maxHealth += Mathf.RoundToInt(maxHealth * 0.2f);
        }

        if (SettingsManager.Instance.GetDifficulty() == SettingsManager.Difficulty.Easy) {
            maxHealth -= Mathf.RoundToInt(maxHealth * 0.33f);
        }

        health = maxHealth;

        GetComponent<Collider2D>().enabled = true;
        rb.gravityScale = initialGravityScale;
        foreach (Collider2D cd in critZoneColliders) {
            cd.enabled = true;
        }

        creatureMovement.enabled = true;
        creatureAttack.enabled = true;
        if (creatureAttack != null) {
            creatureAttack.enabled = true;
        }
        ResetStatusFX();

        stuckTimer = 0f;
        stuckCheckTimer = 0f;
        lastPosition = transform.position;

        OnCreatureEnabled?.Invoke(this, EventArgs.Empty);
    }

    protected override void Update() {
        if (isPaused) return;

        base.Update();
        if (dead) return;

        triggerSoundTimer -= Time.deltaTime;

        if(triggerSoundTimer < 0) {
            OnCreatureIdleSoundTriggered?.Invoke(this, EventArgs.Empty);
            triggerSoundTimer = triggerSoundTime;
        }

        HandleStatusEffects();
        HandleStuckCheck();

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

    private void HandleStuckCheck() {

        if (dead) return;

        if (IsDayCreature()) return;
        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) return;
        if (creatureAI.GetState() == CreatureAI.State.attacking) return;
        if (immobilized || stunned) return;

        if (creatureSO.isBoss) return;

        stuckCheckTimer -= Time.deltaTime;

        if (stuckCheckTimer > 0) return;

        stuckCheckTimer = stuckCheckDelay;

        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance < minMovementThreshold) {

            stuckTimer += stuckCheckDelay;

            if (stuckTimer >= stuckKillTime) {
                Debug.Log("Creature killed because stuck: " + gameObject.name);
                Die();
            }

        }
        else {

            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    [Button]
    public override void Die(Transform damageSource = null) {

        if(inFireLightAmount > 0) {
            creatureWasInFireWhenKilled = true;
        }

        CreatureDieFunction();
        OnCreatureDied?.Invoke(this, EventArgs.Empty);

        OnAnyCreatureDied?.Invoke(this, new OnCreatureDiedEventArgs {
            damageSource = damageSource
        });

    }

    private void CreatureDieFunction() {
        dead = true;

        CreaturesManager.Instance.RemoveCreatureSpawned(this);

        if (mobSpawner != null) {
            mobSpawner.RemoveMobFromMobSpawnedList(this);
        }

        base.Die();

        if (!creatureUnlocked) {
            creatureUnlocked = true;
            MetaProgressionManager.Instance.SetCreatureUnlocked(creatureSO);
        }

        bool isHordeMode = LevelManager.Instance != null && LevelManager.Instance.IsHordeMode();
        if (isHordeMode) {
            dropRedOrbs = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.OrbAlchemist);
        } else {
            dropRedOrbs = MetaProgressionManager.Instance.GetDropRedOrbsUnlocked() && PlayerCamp.Instance.GetHasSkillMerchantInLayoutAndUnlocked();
        }

        if (dropRedOrbs) {
            float randomValue = UnityEngine.Random.value;

            if(!IsDayCreature()) {
                // Night creatures : drop probability reduced
                probabilityToDropOrb /= nightCreatureDropProbabilityReductionFactor;
            }

            if(randomValue < probabilityToDropOrb) {
                SpawnDroppedCurrencies(creatureSO.currencyTypeDroppedList, creatureSO.currencyDropAmountList);
                InvokeOnMobDroppedCollectibles(collectiblesDropped);
            }
        }

        if (DemoMainLevelManager.Instance != null && DemoMainLevelManager.Instance.GetIsMainDemoLevel()) {
            DemoDropGems();
        }

        if (isHordeMode) {

            HordeModeDropGems();

        } else {

            if (eliteCreature) {
                if (DemoMainLevelManager.Instance != null && DemoMainLevelManager.Instance.GetIsMainDemoLevel()) {
                    DemoDropGems();
                }
                else {
                    EliteDropGems();
                }
            }
        }


        if (creatureSO.isBoss) {
            BossUI.Instance.Hide();
        }

        StartCoroutine(DestroyGameObjectAfterDelay());
        GetComponent<Collider2D>().enabled = false;

        foreach (Collider2D cd in critZoneColliders) {
            cd.enabled = false;
        }

        GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    public bool GetCreatureWasInFireWhenKilled() {
        return creatureWasInFireWhenKilled;
    }

    protected void DemoDropGems() {
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

    protected void HordeModeDropGems() {
        if (!HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.Armorer)) return;

        List<PlayerCurrencies.CurrencyType> gemTypeDrop = new List<PlayerCurrencies.CurrencyType>();
        List<int> gemTypeAmountDrop = new List<int>();
        gemTypeDrop.Add(PlayerCurrencies.CurrencyType.redGem);

        int gemAmountDropped = GetDroppedGems(.1f, 3);
        //Debug.Log("gemAmountDropped");
        gemTypeAmountDrop.Add(gemAmountDropped);

        SpawnDroppedCurrencies(gemTypeDrop, gemTypeAmountDrop);
    }

    [Button]
    protected void EliteDropGems() {
        int randomRequestedAmount = GetDroppedGems(0.5f, 2);

        if (!GemDropManager.Instance.TrySpendFromDropPool(randomRequestedAmount, out int actualDropAmount))
            return;

        PlayerCurrencies.CurrencyType selectedGemType = GemDropManager.Instance.GetBalancedGemType();

        List<PlayerCurrencies.CurrencyType> gemTypeDrop = new List<PlayerCurrencies.CurrencyType> { selectedGemType };
        List<int> gemTypeAmountDrop = new List<int> { actualDropAmount };

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

    protected IEnumerator DisableGameObjectAfterDelay() {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    protected IEnumerator DestroyGameObjectAfterDelay() {
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);

        //if (IsDayCreature()) {
        //    Destroy(gameObject);
        //} else {
        //    gameObject.SetActive(false);
        //}

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

    public void SetAsAgressiveDayCreature() {
        agressiveDayCreature = true;
    }

    protected void MetaProgressionMaanger_OnCreatureSOUnlocked(object sender, MetaProgressionManager.OnCreatureSOUnlockedEventArgs e) {
        if(e.creatureSOUnlocked == creatureSO) {
            creatureUnlocked = true;
        }
    }

    protected void PlayerMovement_OnPlayerCrouchedEnded(object sender, EventArgs e) {
        float playerCrouchDetectionRangeDivider = PlayerStats.Instance.GetCrouchDetectionRangeReductionPercentBuff_Meta()/100f;
        if (playerCrouchDetectionRangeDivider == 0) return;
        detectionCollider.DebuffRadius(playerCrouchDetectionRangeDivider);
    }

    protected void PlayerMovement_OnPlayerCrouched(object sender, EventArgs e) {
        float playerCrouchDetectionRangeDivider = PlayerStats.Instance.GetCrouchDetectionRangeReductionPercentBuff_Meta()/100f;
        if (playerCrouchDetectionRangeDivider == 0) return;
        detectionCollider.BuffRadius(playerCrouchDetectionRangeDivider);

        playerCrouchRangeDecreased = true;
    }

    protected void PlayerShoot_OnPlayerShotProjectile(object sender, EventArgs e) {
        if (detectionRangeIncreased) return;

        playerShootDetectionRangeMultiplier = PlayerShoot.Instance.GetHeldGun().GetShootCreatureHearMultiplier();
        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) > detectionCollider.GetRadius() * playerShootDetectionRangeMultiplier) return;
        // Player is too far
        CreatureHeardPlayerShoot(true);
    }

    protected void CreatureHeardPlayerShoot(bool heard) {
        if (isPaused) return;
        if (heard) {

            float randomFloat = UnityEngine.Random.value;
            if(randomFloat < creatureSO.probabilityToAggroOnGunShot) {
                detectionRangeIncreased = true;
                detectionRangeIncreasedTimer = detectionRangeIncreasedTime;

                detectionCollider.BuffRadius(playerShootDetectionRangeMultiplier);
            }

        } else {
            detectionCollider.DebuffRadius(playerShootDetectionRangeMultiplier);
        }
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

    public void EnableCollider(bool enable) {
        GetComponent<Collider2D>().enabled = enable;
    }

    public void SetGravityScale(float gravityScale) {
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityScale;
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
        bool playerIsDamageSource = (damageSource.GetComponent<Player>() != null);

        if (health <= 0) {
            OnAnyCreatureDied?.Invoke(this, new OnCreatureDiedEventArgs {
                damageSource = damageSource,
            });
        }

        if (health <= 0 && damageSource == Dog.Instance.transform) {
            OnAnyCreatureKilledByDog?.Invoke(this, EventArgs.Empty);
        }

        ShowDamageNumber(damage, critHit, weakSpotHit, playerIsDamageSource);
    }

    protected virtual void ShowDamageNumber(int damage, bool critHit, bool weakSpotHit, bool playerIsDamageSource) {
        if (!SettingsManager.Instance.GetShowDamageNumbers()) return;

        int damageToWriteAsNumber = damage;
        if (critHit) {
            damageToWriteAsNumber *= 2;
        }

        DamageNumber damageNumber = DamageNumberPool.Instance.Get(projectileTarget.position);
        damageNumber.Initialize(damageToWriteAsNumber, weakSpotHit, critHit, playerIsDamageSource);
    }

    #region STATUS EFFECTS
    protected void HandleStatusEffects() {
        if(immobilized) {
            immobilizedTimer -= Time.deltaTime;
            if(immobilizedTimer < 0) {
                immobilized = false;
                OnCreatureImmobilizedStopped?.Invoke(this, EventArgs.Empty);
            }
        }
        if (stunned) {
            stunTimer -= Time.deltaTime;
            if (stunTimer < 0) {
                stunned = false;
                OnCreatureStunStopped?.Invoke(this, EventArgs.Empty);
            }
        }
        if (poisoned) {
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

    public void ApplyStunEffect(float stunDuration, Vector3 immobilizePosition) {
        if (dead) return;
        if (stunImmune) return;
        stunned = true;
        this.stunDuration = stunDuration;
        stunTimer = stunDuration;

        Vector3 position = new Vector3(immobilizePosition.x, transform.position.y, 0);
        transform.position = position;

        OnCreatureStunStarted?.Invoke(this, EventArgs.Empty);
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

    private void ResetStatusFX() {
        immobilized = false;
        stunned = false;
        poisoned = false;
        shocked = false;
        burning = false;
    }

    #endregion
    public bool GetCreatureTargeted() {
        return creatureTargeted;
    }

    public int GetCreatureHealth() {
        return health;
    }
    public virtual int GetCreatureMaxHealth() {
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

    public bool IsDayCreature() {
        return dayCreature;
    }
    public bool IsAgressiveDayCreature() {
        return agressiveDayCreature;
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

    public void SetCreatureActive(bool active) {
        if (creatureActive == active) return;
        creatureActive = active;

        creatureAttack.enabled = active;
        creatureAI.enabled = active;
        creatureMovement.enabled = active;

        if(active) {
            rb.bodyType = RigidbodyType2D.Dynamic;
        } else {
            rb.bodyType = RigidbodyType2D.Static;
        }

    }

    public bool GetCreatureActive() {
        return creatureActive;
    }

    public int GetInFireLightAmount() {
        return inFireLightAmount;
    }

    protected virtual void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched -= PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded -= PlayerMovement_OnPlayerCrouchedEnded;

        DayNightManager.Instance.OnCyclePausedByMerchantTalk -= DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused -= DayNightManager_OnCycleUnpaused;

    }


}
