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
        triggerSoundTimer -= Time.deltaTime;

        if(triggerSoundTimer < 0) {
            OnCreatureIdleSoundTriggered?.Invoke(this, EventArgs.Empty);
            triggerSoundTimer = triggerSoundTime;
        }

        if(detectionRangeIncreased) {

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

    private void OnDestroy() {
        PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
        PlayerMovement.Instance.OnPlayerCrouched -= PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded -= PlayerMovement_OnPlayerCrouchedEnded;
    }


}
