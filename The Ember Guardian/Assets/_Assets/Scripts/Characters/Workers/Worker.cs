using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Worker : Mob {

    [SerializeField] private Transform blueOrbPrefab;
    [SerializeField] private LayerMask collectibleLayerMask;

    private CampZoneManager.CampSide sideAssigned;

    private WorkerAI workerAI;
    private WorkerAI.JobTypes wildJobType = WorkerAI.JobTypes.jobless;

    private Structure defensiveStructureAssigned;

    private Dictionary<PlayerCurrencies.CurrencyType, int> collectedCurrencies = new Dictionary<PlayerCurrencies.CurrencyType, int>();

    private bool playerIsClose;
    private bool droppingCurrencies;
    private bool recruited;
    private float playerIsCloseTimer;
    private float timeToStayClose = .75f;

    private float dropDelay = 0.125f; // Délai entre chaque drop
    private float dropTimer = 0f; // Compteur pour suivre le temps écoulé

    private int initialHealth;
    private float refillHealthTime = 3f;
    private float refillHealthTimer;

    public static event EventHandler OnAnyOrbDroppedByWorker;
    public static event EventHandler OnAnyWorkerRecruited;
    public static event EventHandler OnAnyWorkerAssignedHunter;
    public static event EventHandler OnAnyWorkerDied;
    public event EventHandler OnWorkerDied;
    public event EventHandler OnWorkerHovered;
    public event EventHandler OnWorkerUnhovered;
    public event EventHandler OnWorkerCollectedCurrency;
    public event EventHandler OnWorkerDroppedCurrency;
    public event EventHandler OnWorkerDroppedAllCurrencies;
    public event EventHandler OnWorkerPaused;
    public event EventHandler OnWorkerUnpaused;
    public static event EventHandler OnAnyWorkerDroppedAllCurrencies;
    public event EventHandler OnWildJobTypeSet;

    private void Awake() {
        workerAI = GetComponent<WorkerAI>();
        rb = GetComponent<Rigidbody2D>();    
    }

    private void Start() {
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        health = 1;
        initialHealth = health;

        WorkerStats.Instance.OnMaxHealthChanged += WorkerStats_OnMaxHealthChanged;

        DayNightManager.Instance.OnCyclePausedByMerchantTalk += DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused += DayNightManager_OnCycleUnpaused;
    }

    private void DayNightManager_OnCycleUnpaused(object sender, EventArgs e) {
        isPaused = false;
        rb.simulated = true;
        OnWorkerUnpaused?.Invoke(this, EventArgs.Empty);
    }

    private void DayNightManager_OnCyclePausedByMerchantTalk(object sender, EventArgs e) {
        isPaused = true;
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        OnWorkerPaused?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        if (isPaused) return;

        if(droppingCurrencies) {
            HandleDroppingCurrencies();
        }
        CheckPlayerIsClose();

        if(health != initialHealth) {
            if (!CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position) && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return;

            refillHealthTimer += Time.deltaTime;
            if(refillHealthTimer >= refillHealthTime) {
                health += 1;
                refillHealthTimer = 0;

                Debug.Log("refill health ! " + health);
            }
        }
    }
    private void WorkerStats_OnMaxHealthChanged(object sender, EventArgs e) {
        RefreshHealth();
    }

    public void RecruitWorker(bool playSound = true, bool triggerUITextLines = true) {
        WorkerManager.Instance.AddRecruitedWorker(this);

        if(mobSpawner != null) {
            mobSpawner.RemoveMobFromMobSpawnedList(this);
        }

        workerAI.SetJob(wildJobType, triggerUITextLines);
        recruited = true;

        if (!playSound) return;
        OnAnyWorkerRecruited?.Invoke(this, EventArgs.Empty);
    }

    [Button]
    public void CollectCurrency(PlayerCurrencies.CurrencyType currencyType) {
        if (!collectedCurrencies.ContainsKey(currencyType)) {
            collectedCurrencies[currencyType] = 0;
        }
        collectedCurrencies[currencyType]++;
        OnWorkerCollectedCurrency?.Invoke(this, EventArgs.Empty);
    }

    public void DropCurrencies() {
        if (droppingCurrencies) return;

        droppingCurrencies = true; // On commence le processus de drop
        dropTimer = 0f; // Réinitialiser le timer
    }

    public void RemoveCurrency(PlayerCurrencies.CurrencyType currencyType) {
        collectedCurrencies[currencyType]--;

        if (!collectedCurrencies.ContainsKey(currencyType)) {
            collectedCurrencies[currencyType] = 0;
        }

        OnWorkerDroppedCurrency?.Invoke(this, EventArgs.Empty);
    }

    public int GetTotalCurrencyAmount() {
        int totalAmount = 0;
        foreach (var amount in collectedCurrencies.Values) {
            totalAmount += amount;
        }
        return totalAmount;
    }

    [Button]
    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        if (defensiveStructureAssigned != null) return;

        base.TakeDamage(damage, damageSource, critHit, ignoreTemporaryInvincibility);

        refillHealthTimer = 0;
    }

    public int GetCurrencyAmount(PlayerCurrencies.CurrencyType currencyType) {
        if (collectedCurrencies.ContainsKey(currencyType)) {
            return collectedCurrencies[currencyType];
        }
        return 0; // Retourne 0 si le type n'est pas collecté
    }

    public void CheckPlayerIsClose() {
        float distance = 2f;

        float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        if (distanceToPlayer < distance && !Player.Instance.GetInTeleporter()) {
            playerIsClose = true;
        }
        else {
            playerIsCloseTimer = 0;
            playerIsClose = false;
        }
    }

    public bool PlayerIsCloseAndStayedAround() {
        float distance = 2f;

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

    private void HandleDroppingCurrencies() {
        // Si on est en train de dropper des currencies
        if (droppingCurrencies) {
            dropTimer += Time.deltaTime; // Ajouter le temps écoulé

            // Si le délai entre deux drops est écoulé
            if (dropTimer >= dropDelay) {
                dropTimer = 0f; // Réinitialiser le timer

                // Essayer de dropper toutes les currencies collectées
                foreach (var currency in collectedCurrencies.ToList()) {
                    if (currency.Value > 0) {
                        Transform prefabToDrop = CurrenciesManager.Instance.GetCurrencyPrefab(currency.Key);
                        Collectible droppedCurrency = Instantiate(prefabToDrop, dropSpawnPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
                        droppedCurrency.ApplyRandomFrontForce(2f, 3f);
                        droppedCurrency.SetCollectibleUnInteractable(1f);
                        OnAnyOrbDroppedByWorker?.Invoke(this, EventArgs.Empty);

                        // Réduire la valeur après chaque drop
                        collectedCurrencies[currency.Key]--;
                        OnWorkerDroppedCurrency?.Invoke(this, EventArgs.Empty);

                        // Si la valeur atteint 0, retirer l'élément du dictionnaire
                        if (collectedCurrencies[currency.Key] <= 0) {
                            collectedCurrencies.Remove(currency.Key);
                        }
                        break; // Quitter la boucle dès qu'on a fait un drop
                    }
                }

                foreach (var currency in collectedCurrencies.ToList()) {
                    // Si la valeur atteint 0, retirer l'élément du dictionnaire
                    if (collectedCurrencies[currency.Key] <= 0) {
                        collectedCurrencies.Remove(currency.Key);
                    }
                }

                // Si plus rien à dropper, arrêter le processus
                if (collectedCurrencies.Count == 0) {
                    droppingCurrencies = false;
                    OnWorkerDroppedAllCurrencies?.Invoke(this, EventArgs.Empty);
                    OnAnyWorkerDroppedAllCurrencies?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public Structure GetDefensiveStructureAssigned() {
        return defensiveStructureAssigned;
    }

    public void AssignDefensiveStructure(Structure structure) {
        defensiveStructureAssigned = structure;
    }

    public bool GetRecruited() {
        return recruited;
    }

    [Button]
    public override void Die(Transform damageSource = null) {
        base.Die();

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        WorkerManager.Instance.RemoveWorker(this);
        OnWorkerDied?.Invoke(this, EventArgs.Empty);
        OnAnyWorkerDied?.Invoke(this, EventArgs.Empty);

        StartCoroutine(DestroyGameObjectAfterDelay(1f));
    }

    public void HoverWorker(bool hover) {
        if(hover) {
            OnWorkerHovered?.Invoke(this, EventArgs.Empty);
        } else {
            OnWorkerUnhovered?.Invoke(this, EventArgs.Empty);
        }
    }

    private void WorkerAI_OnJobChanged(object sender, EventArgs e) {
        RefreshHealth();

        if (workerAI.GetJob() != WorkerAI.JobTypes.wild && workerAI.GetJob() != WorkerAI.JobTypes.jobless) {
            WorkerManager.Instance.AutoAssignSideToWorker(this);
        }
    }

    private void RefreshHealth() {
        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            health = (int)WorkerStats.Instance.GetGuardHealth();
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            health = (int)WorkerStats.Instance.GetHunterHealth();
            OnAnyWorkerAssignedHunter?.Invoke(this, EventArgs.Empty);
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            health = (int)WorkerStats.Instance.GetMinerHealth();
        }

        if (workerAI.GetJob() == WorkerAI.JobTypes.engineer) {
            timeToStayClose = 2f;
        }

        initialHealth = health;
    }

    public void SetPosition(Vector3 position) {
        transform.position = position;
    }

    public void SetWildJobType(WorkerAI.JobTypes jobType) {
        wildJobType = jobType;
        OnWildJobTypeSet?.Invoke(this, EventArgs.Empty);
    }

    public WorkerAI.JobTypes GetWildJobType() {
        return wildJobType; 
    }

    public Dictionary<PlayerCurrencies.CurrencyType, int> GetCollectedCurrencies() {
        return collectedCurrencies;
    }

    private void OnDestroy() {
        DayNightManager.Instance.OnCyclePausedByMerchantTalk -= DayNightManager_OnCyclePausedByMerchantTalk;
        DayNightManager.Instance.OnCycleUnpaused -= DayNightManager_OnCycleUnpaused;
    }
}
