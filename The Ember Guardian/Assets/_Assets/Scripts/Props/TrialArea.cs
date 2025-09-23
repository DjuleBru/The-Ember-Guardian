using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialArea : MonoBehaviour
{
    [SerializeField] private Chest trialChest;
    [SerializeField] private List<MobSpawner> creatureSpawners;
    [SerializeField] private int waveAmount;
    [SerializeField] private int spawnersPerWave;
    private int currentWave;
    private int currentSpawner;

    [SerializeField] protected Transform orbTemplateWorldUIParent;
    protected List<PayCurrencyTemplateWorldUI> buildStructureOrbTemplates = new List<PayCurrencyTemplateWorldUI>();
    private PayCurrencyUI payCurrencyUI;

    private bool playerInTriggerArea;
    private bool trialCompleted;
    private bool trialStarted;

    public event EventHandler OnTrialPaid;
    public event EventHandler OnTrialWallsLifted;
    public event EventHandler OnTrialStarted;
    public event EventHandler OnTrialCompleted;
    public event EventHandler OnTrialChestUnlocked;
    public event EventHandler OnTrialFailed;
    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public static event EventHandler OnAnyTrialPaid;

    private List<Creature> creatureSpawnedList = new List<Creature>();

    protected void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();
    }

    protected void Start() {
        trialChest.SetChestLocked(true);
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        payCurrencyUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payCurrencyUI.SetOrbTemplateUIList(buildStructureOrbTemplates);

        foreach(MobSpawner spawner in creatureSpawners) {
            spawner.OnMobRemoved += Spawner_OnMobRemoved;
            spawner.OnMobSpawned += Spawner_OnMobSpawned;
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (trialCompleted) return;
        if(trialStarted) {
            FailTrial();
        }
    }

    private void Spawner_OnMobSpawned(object sender, MobSpawner.OnMobSpawnedEventArgs e) {
        Creature creature = e.mob as Creature;
        creatureSpawnedList.Add(creature);
    }

    private void Spawner_OnMobRemoved(object sender, MobSpawner.OnMobSpawnedEventArgs e) {
        Creature creature = e.mob as Creature;
        creatureSpawnedList.Remove(creature);

        if (creatureSpawnedList.Count == 0 && trialStarted ) {

            currentWave++;

            if (currentWave == waveAmount) {
                CompleteTrial();
            } else {
                StartCoroutine(NextWave());
            }
        }

    }

    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        Player.Instance.SetInPayCurrencyArea(false);
        StartCoroutine(StartTrialCoroutine());
        trialChest.SetTrialChestPaid();
    }

    private IEnumerator StartTrialCoroutine() {
        trialStarted = true;
        OnTrialPaid?.Invoke(this, EventArgs.Empty);
        OnAnyTrialPaid?.Invoke(this, EventArgs.Empty);
        DayNightManager.Instance.SetCyclePaused(true, true);

        yield return new WaitForSeconds(.5f);
        OnTrialWallsLifted?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(1.5f);

        OnTrialStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        StartCoroutine(NextWave());
    }

    private IEnumerator NextWave() {
        yield return new WaitForSeconds(1f);
        for(int i =  0; i <= spawnersPerWave-1; i++) {
            MobSpawner spawner = creatureSpawners[currentSpawner];
            yield return StartCoroutine(spawner.SpawnMobsCoroutine(.25f));
            currentSpawner++;
        }

    }

    private void CompleteTrial() {
        StartCoroutine(CompleteTrialCoroutine());
    }

    private IEnumerator CompleteTrialCoroutine() {
        trialCompleted = true;
        OnTrialCompleted?.Invoke(this, EventArgs.Empty);
        DayNightManager.Instance.SetCyclePaused(false, true);

        yield return new WaitForSeconds(2f);

        trialChest.SetChestLocked(false);
        OnTrialChestUnlocked?.Invoke(this, EventArgs.Empty);

    }

    private void FailTrial() {
        trialStarted = false;
        OnTrialFailed?.Invoke(this, EventArgs.Empty);
        DayNightManager.Instance.SetCyclePaused(false, true);
        StartCoroutine(KillAllCreaturesRemaining());
    }

    private IEnumerator KillAllCreaturesRemaining() {
        List<Creature> creatureSpawnedListCopy = new List<Creature>();

        foreach(Creature creature in creatureSpawnedList) {
            creatureSpawnedListCopy.Add(creature);
            
        }

        foreach (Creature creature in creatureSpawnedListCopy) {
            creature.Die();
            yield return new WaitForSeconds(.5f);
        }
    }

    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (trialStarted || trialCompleted) return;

        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }

    protected void InitializeOrbTemplateList() {
        PayCurrencyTemplateWorldUI[] orbTemplates = orbTemplateWorldUIParent.GetComponentsInChildren<PayCurrencyTemplateWorldUI>();

        foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in orbTemplates) {
            buildStructureOrbTemplates.Add(orbTemplateWorldUI);
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (trialCompleted) return;
        if (trialStarted) return;

        Player.Instance.SetInPayCurrencyArea(true);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (trialCompleted) return;
        if (trialStarted) return;

        Player.Instance.SetInPayCurrencyArea(false);
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        payCurrencyUI.SetPlayerInteracting(false);
        playerInTriggerArea = false;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractStarted;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
    }
}
