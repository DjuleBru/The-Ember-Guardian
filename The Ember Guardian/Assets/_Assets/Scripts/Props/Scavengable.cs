using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scavengable : MonoBehaviour, IDamageable {

    private PayCurrencyUI payOrbsUI;
    protected List<PayCurrencyTemplateWorldUI> scavengeStructureCurrencyTemplates = new List<PayCurrencyTemplateWorldUI>();

    [SerializeField] protected Transform orbTemplateWorldUIParent;
    [SerializeField] private Transform currencySpawnPoint;
    [SerializeField] private List<Transform> minePoints;
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeCollected;
    [SerializeField] private int currencyAmountCollected;

    [SerializeField] private int hitsToCollectOneCurrency;
    [SerializeField] private bool infiniteSource;

    [SerializeField] private int maxMinersAssigned;

    private List<MinerJob> minerAssignedList = new List<MinerJob>();

    private int health;
    private int hitsTaken;

    private bool scavengedUnlocked = true;
    private bool markedToScavenge;
    private bool depleted;
    private bool playerInTriggerArea;

    public event EventHandler OnPlayerTriggerIn;
    public event EventHandler OnPlayerTriggerOut;
    public event EventHandler OnScavengableMarkedToScavenge;
    public event EventHandler OnScavengableDepleted;
    public event EventHandler<OnStavengableSpawnedCurrencyEventArgs> OnScavengableSpawnedCurrency;

    public class OnStavengableSpawnedCurrencyEventArgs : EventArgs {
        public Collectible collectibleSpawned;
    }

    private void Awake() {
        payOrbsUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();
        health = hitsToCollectOneCurrency * currencyAmountCollected;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;

        payOrbsUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payOrbsUI.SetOrbTemplateUIList(scavengeStructureCurrencyTemplates);
    }

    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        markedToScavenge = true;
        OnScavengableMarkedToScavenge?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {
        depleted = true;
        OnScavengableDepleted?.Invoke(this, EventArgs.Empty);

        foreach(MinerJob miner in minerAssignedList) {
            miner.UnAssignScavengable();
        }
    }

    public Transform GetMeleeAttackPosition() {
        return minePoints[UnityEngine.Random.Range(0, minePoints.Count)];
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    public void TakeDamage(int damage, Transform damageSource, bool crit = false) {
        health -= damage;
        hitsTaken += damage;

        if(hitsTaken >= hitsToCollectOneCurrency) {
            hitsTaken = 0;
            SpawnCurrency();
        }

        if (health <= damage && !infiniteSource) {
            Die();
        }
    }

    private void SpawnCurrency() {
        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeCollected), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomUpwardsForce(5, 8);
        collectible.SetCollectibleUnInteractable(.75f);
        collectible.SetCanBePickedUpByWorker();

        OnScavengableSpawnedCurrency?.Invoke(this, new OnStavengableSpawnedCurrencyEventArgs {
            collectibleSpawned = collectible,
        });
    }

    public void AssignMiner(MinerJob miner) {
        if (minerAssignedList.Contains(miner)) return;
        minerAssignedList.Add(miner);
    }

    public void UnassignMiner(MinerJob miner) {
        if (!minerAssignedList.Contains(miner)) return;
        minerAssignedList.Remove(miner);
    }

    public bool GetMaxMinersAssigned() {
        return minerAssignedList.Count > maxMinersAssigned;
    }

    public bool GetDepleted() {
        return depleted;
    }

    public bool GetMarkedToScavenge() {
        return markedToScavenge;
    }

    protected void InitializeOrbTemplateList() {
        PayCurrencyTemplateWorldUI[] orbTemplates = orbTemplateWorldUIParent.GetComponentsInChildren<PayCurrencyTemplateWorldUI>();

        foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in orbTemplates) {
            scavengeStructureCurrencyTemplates.Add(orbTemplateWorldUI);
        }
    }

    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        payOrbsUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        payOrbsUI.SetPlayerInteracting(false);
        payOrbsUI.ResetCurrencyPayment();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!scavengedUnlocked) return;
        if (collision.GetComponent<Player>() == null) return;

        Player.Instance.SetCanDropOrbOnTheFloor(false);
        playerInTriggerArea = true;
        OnPlayerTriggerIn?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!scavengedUnlocked) return;
        if (collision.GetComponent<Player>() == null) return;

        Player.Instance.SetCanDropOrbOnTheFloor(true);
        payOrbsUI.SetPlayerInteracting(false);
        playerInTriggerArea = false;
        OnPlayerTriggerOut?.Invoke(this, EventArgs.Empty);
    }
}
