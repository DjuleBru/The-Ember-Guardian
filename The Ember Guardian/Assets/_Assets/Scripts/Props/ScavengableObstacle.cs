using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableObstacle : Obstacle, IScavengable
{

    public event EventHandler OnPlayerTriggerIn;
    public event EventHandler OnPlayerTriggerOut;
    public event EventHandler OnScavengableDepleted;
    public event EventHandler OnScavengableMarkedToScavenge;
    public static event EventHandler OnAnyScavengableMarkedToScavenge;
    public event EventHandler OnMinerExtractedResourceFromMine;
    public event EventHandler OnMinerStartsMining;
    public event EventHandler OnMinerStopsMining;
    public event EventHandler OnActivatedMining;
    public static event EventHandler OnAnyScavengableObstacleActivatedMining;
    public event EventHandler OnDeactivatedMining;
    public event EventHandler OnDamageTaken;
    public event EventHandler OnCreatureSpawned;

    [SerializeField] protected int miningPriority;
    [SerializeField] protected bool requiresMiners;
    [SerializeField] private List<Transform> minePoints;
    [SerializeField] private int maxMinersAssigned;
    [SerializeField] private int hitsToRemoveObstacle;
    [SerializeField] private bool playerHasToBeCloseToScavenge = true;
    private float maxPlayerDistanceToScavenge = 40f;

    [SerializeField] private List<SpawnOnDamageThreshold> spawnOnDamageThresholds;

    private int health;
    private int hitsTaken;
    private bool depleted;
    private bool markedToScavenge;
    private bool scavengingActive = true;

    private List<MinerJob> minerAssignedList = new List<MinerJob>();
    private List<MinerJob> minersMiningList = new List<MinerJob>();

    protected override void Awake() {
        base.Awake();
        health = hitsToRemoveObstacle;

    }

    protected override void Start() {
        base.Start();
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
    }

    protected void Update() {
        if (!scavengingActive) return;

        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);
        if(distanceToPlayer > maxPlayerDistanceToScavenge) {
            ToggleScavengingActive();
        }
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        if(scavengingActive) {
            ToggleScavengingActive();
        }
    }

    protected override void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (depleted) return;

        if (markedToScavenge) {
            ToggleScavengingActive();
        } else {
            payCurrencyUI.SetPlayerInteracting(true);
        };

    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (hubScene) return;
        if (!playerInTriggerArea) return;
        if (depleted) return;
        if (markedToScavenge) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }
    private void ToggleScavengingActive() {
        scavengingActive = !scavengingActive;

        if (scavengingActive) {
            OnActivatedMining?.Invoke(this, EventArgs.Empty);
            OnAnyScavengableObstacleActivatedMining?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnDeactivatedMining?.Invoke(this, EventArgs.Empty);
            UnassignAllMiners();
        }
    }

    protected override void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        markedToScavenge = true;
        OnScavengableMarkedToScavenge?.Invoke(this, EventArgs.Empty);
        OnAnyScavengableMarkedToScavenge?.Invoke(this, EventArgs.Empty);
        ToggleScavengingActive();
    }

    public void TakeDamage(int damage, Transform damageSource, bool crit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        health -= damage;
        hitsTaken += damage;

        OnDamageTaken?.Invoke(this, EventArgs.Empty);

        CheckSpawnCreatures();

        if (health <= 0) {
            Die();
        }
    }

    private void CheckSpawnCreatures() {
        float healthNormalized = (float)health / hitsToRemoveObstacle;

        foreach (var spawnThreshold in spawnOnDamageThresholds) {
            if (spawnThreshold.hasTriggered) continue;


            if (healthNormalized <= spawnThreshold.healthThresholdNormalized) {
                OnCreatureSpawned?.Invoke(this, EventArgs.Empty);

                StartCoroutine(SpawnCreatures(spawnThreshold));
                spawnThreshold.hasTriggered = true;
            }
        }
    }

    private IEnumerator SpawnCreatures(SpawnOnDamageThreshold spawnTreshold) {
        foreach (var spawnData in spawnTreshold.creaturesToSpawn) {

            for(int  i = 0; i < spawnData.amount; i++) {
                spawnData.mobSpawner.SpawnCreatures(spawnData.creatureType, 1, true);
                yield return new WaitForSeconds(1f);
            }

        }
    }

    public void Die() {
        OnScavengableDepleted?.Invoke(this, EventArgs.Empty);
        BuildObstacle();
        UnassignAllMiners();
    }

    private void UnassignAllMiners() {

        List<MinerJob> minerAssignedListCopy = new List<MinerJob>();
        foreach (MinerJob minerJob in minerAssignedList) {
            if (minerJob == null) continue;
            minerAssignedListCopy.Add(minerJob);
        }

        foreach (MinerJob miner in minerAssignedListCopy) {
            miner.UnAssignScavengable();
        }

        minerAssignedList.Clear();
    }

    public override void BuildObstacle() {
        foreach (Collider2D collider in blockingColliders) {
            collider.enabled = false;
        }

        depleted = true;
        obstacleBuilt = true;
        InvokeObstacleBuiltEvents();
        SetTriggerExit();
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    public Transform GetMeleeAttackPosition() {
        return minePoints[UnityEngine.Random.Range(0, minePoints.Count)];
    }

    public bool GetDepleted() {
        return depleted;
    }

    public bool GetMaxMinersAssigned() {
        return minerAssignedList.Count >= maxMinersAssigned;
    }

    public bool GetMarkedToScavenge() {
        return markedToScavenge;
    }

    public bool GetIsMine() {
        return false;
    }

    public bool GetScavengingActive() {
        return scavengingActive;
    }

    public float GetTimeToExtractOneResourceNormalized() {
        return 0f;
    }

    public void AssignMiner(MinerJob minerJob) {
        if (minerAssignedList.Contains(minerJob)) return;
        minerAssignedList.Add(minerJob);
    }

    public void UnassignMiner(MinerJob minerJob) {
        if (!minerAssignedList.Contains(minerJob)) return;
        minerAssignedList.Remove(minerJob);

        if (!minersMiningList.Contains(minerJob)) return;
        MinerStopsMining(minerJob);
    }

    public int GetMaxMinerAmount() {
        return maxMinersAssigned;
    }

    public int GetMinerAmountMining() {
        return minersMiningList.Count;
    }

    public void MinerStartsMining(MinerJob minerJob) {
        minersMiningList.Add(minerJob);
        OnMinerStartsMining?.Invoke(this, EventArgs.Empty);
    }

    public void MinerStopsMining(MinerJob minerJob) {
        minersMiningList.Remove(minerJob);
        OnMinerStopsMining?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized() {
        return (float)hitsTaken / (float)hitsToRemoveObstacle;
    }

    public int GetMiningPriority() {
        return miningPriority;
    }

    public void SetScavengableUnlocked(bool unlocked) {
        return;
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (obstacleBuilt) return;

        OnPlayerTriggerIn?.Invoke(this, EventArgs.Empty);
        Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
        SetTriggerEnter();
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        OnPlayerTriggerOut?.Invoke(this, EventArgs.Empty);
        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        SetTriggerExit();
    }
}
