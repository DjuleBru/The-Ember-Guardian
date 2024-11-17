using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : Mob {

    [SerializeField] private Transform blueOrbPrefab;
    [SerializeField] private LayerMask collectibleLayerMask;

    private CampZoneManager.CampSide sideAssigned;

    private WorkerAI workerAI;

    private Structure structureAssigned;

    private Dictionary<PlayerCurrencies.CurrencyType, int> collectedCurrencies = new Dictionary<PlayerCurrencies.CurrencyType, int>();

    private bool playerIsClose;
    private bool droppingOrbs;
    private bool recruited;
    private float playerIsCloseTimer;

    public static event EventHandler OnAnyOrbDroppedByWorker;
    public static event EventHandler OnAnyWorkerRecruited;
    public static event EventHandler OnAnyWorkerAssignedHunter;
    public static event EventHandler OnAnyWorkerDied;

    private void Awake() {
        workerAI = GetComponent<WorkerAI>();    
    }

    private void Start() {
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }


    private void Update() {
        //CheckOrbsNearby();
        CheckPlayerIsClose();
    }

    public void RecruitWorker() {
        WorkerManager.Instance.AddRecruitedWorker(this);
        mobSpawner.RemoveMobFromMobSpawnedList(this);
        workerAI.SetJob(WorkerAI.JobTypes.jobless);
        recruited = true;

        OnAnyWorkerRecruited?.Invoke(this, EventArgs.Empty);
    }

    public void CollectCurrency(PlayerCurrencies.CurrencyType currencyType) {
        if (!collectedCurrencies.ContainsKey(currencyType)) {
            collectedCurrencies[currencyType] = 0;
        }
        collectedCurrencies[currencyType]++;
    }

    public void DropCurrencies() {
        if (droppingOrbs) return;

        droppingOrbs = true;
        StartCoroutine(DropCurrenciesCoroutine(0.15f));
    }
    public int GetTotalCurrencyAmount() {
        int totalAmount = 0;
        foreach (var amount in collectedCurrencies.Values) {
            totalAmount += amount;
        }
        return totalAmount;
    }

    public int GetCurrencyAmount(PlayerCurrencies.CurrencyType currencyType) {
        if (collectedCurrencies.ContainsKey(currencyType)) {
            return collectedCurrencies[currencyType];
        }
        return 0; // Retourne 0 si le type n'est pas collecté
    }

    public void CheckPlayerIsClose() {
        float distance = 2f;

        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) < distance) {
            playerIsClose = true;
        }
        else {
            playerIsCloseTimer = 0;
            playerIsClose = false;
        }
    }

    public bool PlayerIsCloseAndStayedAround() {
        float distance = 2f;
        float timeToStayClose = 1.5f;

        playerIsCloseTimer += Time.deltaTime;

        if (Mathf.Abs(Player.Instance.transform.position.x - transform.position.x) < distance && playerIsCloseTimer > timeToStayClose) {
            return true;
        }
        else {
            return false;
        }
    }

    public bool GetPlayerIsClose() {
        return playerIsClose;
    }

    public void AssignSide(CampZoneManager.CampSide side) {
        sideAssigned = side;
    }

    public CampZoneManager.CampSide GetCampSideAddigned() {
        return sideAssigned;
    }

    private IEnumerator DropCurrenciesCoroutine(float delayBetweenDrops) {
        foreach (var currency in collectedCurrencies) {

            for (int i = 0; i < currency.Value; i++) {
                Transform prefabToDrop = CurrenciesManager.Instance.GetCurrencyPrefab(currency.Key);
                Collectible droppedCurrency = Instantiate(prefabToDrop, transform.position, Quaternion.identity).GetComponent<Collectible>();
                droppedCurrency.ApplyRandomFrontForce(2f, 3f);
                droppedCurrency.SetCollectibleUnInteractable(1f);
                OnAnyOrbDroppedByWorker?.Invoke(this, EventArgs.Empty);

                yield return new WaitForSeconds(delayBetweenDrops);
            }

        }
    }

    public Structure GetStructureAssigned() {
        return structureAssigned;
    }

    public void AssignStructure(Structure structure) {
        structureAssigned = structure;
    }

    public bool GetRecruited() {
        return recruited;
    }

    public override void Die() {
        base.Die();

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        WorkerManager.Instance.RemoveWorker(this);
        OnAnyWorkerDied?.Invoke(this, EventArgs.Empty);

        StartCoroutine(DestroyGameObjectAfterDelay(1f));
    }

    private void WorkerAI_OnJobChanged(object sender, EventArgs e) {
        if(workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            health = 5;
        } else {
            health = 1;
        }

        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            OnAnyWorkerAssignedHunter?.Invoke(this, EventArgs.Empty);
        }
    }

}
