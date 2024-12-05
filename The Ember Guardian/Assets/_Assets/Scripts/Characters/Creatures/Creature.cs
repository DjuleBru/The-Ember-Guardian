using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] private CreatureSO creatureSO;
    [SerializeField] private CircleCollider2D detectionCollider;
    [SerializeField] private List<Collider2D> critZoneColliders;

    private Rigidbody2D rb;
    private bool dayCreature;
    private bool enteredLight;

    public event EventHandler OnCreatureEnteredLight;
    public event EventHandler OnCreatureExitedLight;

    public event EventHandler OnCreatureDied;
    public event EventHandler OnCreatureIdleSoundTriggered;

    private float triggerSoundTimer;
    private float triggerSoundTime = 5f;

    private float detectionRangeIncreasedTimer;
    private float detectionRangeIncreasedTime = 2f;
    private bool detectionRangeIncreased;
    private float playerShootDetectionRangeMultiplier = 1.3f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = creatureSO.mass;
        triggerSoundTimer = UnityEngine.Random.Range(0, triggerSoundTime);
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
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
            detectionRangeIncreasedTimer -= Time.deltaTime;
            if(detectionRangeIncreasedTimer < 0) {
                CreatureHeardPlayerShoot(false);
                detectionRangeIncreased = false;
            }
        }
    }

    public override void Die() {
        CreaturesManager.Instance.RemoveCreatureSpawned(this);

        if(IsDayCreature()) {
            mobSpawner.RemoveMobFromMobSpawnedList(this);
        }

        base.Die();

        SpawnDroppedCurrencies(creatureSO.currencyTypeDroppedList, creatureSO.currencyDropAmountList);
        InvokeOnMobDroppedCollectibles(collectiblesDropped);

        OnCreatureDied?.Invoke(this, EventArgs.Empty);
        StartCoroutine(DisableGameObjectAfterDelay());
        GetComponent<Collider2D>().enabled = false;

        foreach(Collider2D cd in critZoneColliders) {
            cd.enabled = false;
        }

        GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    private IEnumerator DisableGameObjectAfterDelay() {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
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
            detectionCollider.radius = creatureSO.detectionRange_Day + radiusRandomizer;
        }
        else {
            detectionCollider.radius = creatureSO.detectionRange_Night;
        }
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, EventArgs e) {
        if (detectionRangeIncreased) return;
        detectionRangeIncreased = true;
        detectionRangeIncreasedTimer = detectionRangeIncreasedTime;
        CreatureHeardPlayerShoot(true);
    }

    private void CreatureHeardPlayerShoot(bool heard) {
        // Player is too far
        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) > detectionCollider.radius*2) return;

        if (heard) {
            detectionCollider.radius *= playerShootDetectionRangeMultiplier;
        } else {
            detectionCollider.radius /= playerShootDetectionRangeMultiplier;
        }
    }

    public bool IsDayCreature() { 
        return dayCreature;
    }
}
