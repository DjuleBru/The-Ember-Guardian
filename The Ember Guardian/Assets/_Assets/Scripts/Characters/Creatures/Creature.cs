using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] private CreatureSO creatureSO;
    [SerializeField] private CreatureDetectionCollider detectionCollider;
    [SerializeField] private CreatureMovement creatureMovement;
    [SerializeField] private List<Collider2D> critZoneColliders;

    private bool dropRedOrbsUnlocked;
    private bool dayCreature;
    private bool enteredLight;
    private bool creatureTargeted;

    private bool eliteCreature;
    private bool eliteHPCreature;
    private bool eliteSpeedCreature;
    private bool eliteDamageCreature;

    public event EventHandler OnCreatureEnteredLight;
    public event EventHandler OnCreatureExitedLight;

    public event EventHandler OnCreatureDied;
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
    private bool immobilized;
    private float immobilizedDuration;
    private float immobilizedTimer;

    public event EventHandler OnCreaturePoisonedStarted;
    public event EventHandler OnCreaturePoisoneStopped;
    private bool poisoned;
    private int poisonAmount;
    private float poisonedTimer;
    private float poisonRate = 1.5f;
    private float poisonRateTimer;
    private float poisonedDuration = 10f;

    public event EventHandler OnCreatureShockedStarted;
    public event EventHandler OnCreatureShockedStopped;
    private bool shocked;
    private float shockedDuration = 10f;
    private float shockedTimer;
    private float shockedSlowAmount;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = creatureSO.mass;
        triggerSoundTimer = UnityEngine.Random.Range(0, triggerSoundTime);
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;
    }

    private void OnEnable() {
        CreaturesManager.Instance.AddCreatureSpawned(this);
        health = creatureSO.maxHealth;
    }

    private void Update() {
        if (dead) return;

        triggerSoundTimer -= Time.deltaTime;

        if(triggerSoundTimer < 0) {
            OnCreatureIdleSoundTriggered?.Invoke(this, EventArgs.Empty);
            triggerSoundTimer = triggerSoundTime;
        }

        if(detectionRangeIncreased) {

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

        HandleStatusEffects();
    }

    public override void Die() {
        dead = true;

        CreaturesManager.Instance.RemoveCreatureSpawned(this);

        if(mobSpawner != null) {
            mobSpawner.RemoveMobFromMobSpawnedList(this);
        }

        base.Die();

        if (dropRedOrbsUnlocked) {
            SpawnDroppedCurrencies(creatureSO.currencyTypeDroppedList, creatureSO.currencyDropAmountList);
            InvokeOnMobDroppedCollectibles(collectiblesDropped);
        }

        if(eliteCreature) {
            EliteDropGems();
        }

        OnCreatureDied?.Invoke(this, EventArgs.Empty);
        StartCoroutine(DestroyGameObjectAfterDelay());
        GetComponent<Collider2D>().enabled = false;

        foreach(Collider2D cd in critZoneColliders) {
            cd.enabled = false;
        }

        GetComponent<Rigidbody2D>().gravityScale = 0;
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

        int gemAmountDropped = UnityEngine.Random.Range(1, 3);
        gemTypeAmountDrop.Add(gemAmountDropped);

        SpawnDroppedCurrencies(gemTypeDrop, gemTypeAmountDrop);
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

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponentInParent<Fire>() != null) {
            if (enteredLight) return;
            OnCreatureEnteredLight?.Invoke(this, EventArgs.Empty);
            enteredLight = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponentInParent<Fire>() != null) {
            if (!enteredLight) return;

            OnCreatureExitedLight?.Invoke(this, EventArgs.Empty);
            enteredLight = false;
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
        if(shocked) {
            shockedTimer -= Time.deltaTime;
            if (shockedTimer < 0) {
                shocked = false;
                OnCreatureShockedStopped?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void ApplyBearTrapEffect(float immobilizeDuration, Vector3 trapPosition) {
        immobilized = true;
        immobilizedDuration = immobilizeDuration;
        immobilizedTimer = immobilizedDuration;

        Vector3 position = new Vector3(trapPosition.x, transform.position.y, 0);
        transform.position = position;

        OnCreatureImmobilizedStarted?.Invoke(this, EventArgs.Empty);
    }

    public void ApplySmokeTrapEffect(int poisonAmount) {
        poisoned = true;
        this.poisonAmount = poisonAmount;
        poisonedTimer = poisonedDuration;

        OnCreaturePoisonedStarted?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyShockTrapEffect(float slowAmount) {
        shocked = true;
        this.shockedSlowAmount = slowAmount;
        shockedTimer = shockedDuration;

        OnCreatureShockedStarted?.Invoke(this, EventArgs.Empty);
    }

    #endregion
    public bool GetCreatureTargeted() {
        return creatureTargeted;
    }

    public int GetCreatureHealth() {
        return health;
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
    private void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched -= PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded -= PlayerMovement_OnPlayerCrouchedEnded;
    }


}
