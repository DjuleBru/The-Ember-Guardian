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

    private Structure structureAssigned;

    private Dictionary<PlayerCurrencies.CurrencyType, int> collectedCurrencies = new Dictionary<PlayerCurrencies.CurrencyType, int>();

    private bool playerIsClose;
    private bool droppingCurrencies;
    private bool recruited;
    private float playerIsCloseTimer;

    private float dropDelay = 0.125f; // Délai entre chaque drop
    private float dropTimer = 0f; // Compteur pour suivre le temps écoulé

    private int initialHealth;
    private float refillHealthTime = 10f;
    private float refillHealthTimer;

    public static event EventHandler OnAnyOrbDroppedByWorker;
    public static event EventHandler OnAnyWorkerRecruited;
    public static event EventHandler OnAnyWorkerAssignedHunter;
    public static event EventHandler OnAnyWorkerDied;
    public event EventHandler OnWorkerHovered;
    public event EventHandler OnWorkerUnhovered;
    public event EventHandler OnWorkerCollectedCurrency;
    public event EventHandler OnWorkerDroppedAllCurrencied;

    private void Awake() {
        workerAI = GetComponent<WorkerAI>();    
    }

    private void Start() {
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        health = 1;
        initialHealth = health;
    }

    private void Update() {
        if(droppingCurrencies) {
            HandleDroppingCurrencies();
        }
        CheckPlayerIsClose();

        if(health != initialHealth) {
            refillHealthTimer += Time.deltaTime;
            if(refillHealthTimer >= refillHealthTime) {
                health = initialHealth;
            }
        }
    }

    public void RecruitWorker(bool playSound = true) {
        WorkerManager.Instance.AddRecruitedWorker(this);
        mobSpawner.RemoveMobFromMobSpawnedList(this);
        workerAI.SetJob(WorkerAI.JobTypes.jobless);
        recruited = true;

        if (!playSound) return;
        OnAnyWorkerRecruited?.Invoke(this, EventArgs.Empty);
    }

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

    public int GetTotalCurrencyAmount() {
        int totalAmount = 0;
        foreach (var amount in collectedCurrencies.Values) {
            totalAmount += amount;
        }
        return totalAmount;
    }

    public override void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false) {
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
        float timeToStayClose = 1.2f;

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

                        // Si la valeur atteint 0, retirer l'élément du dictionnaire
                        if (collectedCurrencies[currency.Key] <= 0) {
                            collectedCurrencies.Remove(currency.Key);
                        }
                        break; // Quitter la boucle dès qu'on a fait un drop
                    }
                }

                // Si plus rien à dropper, arrêter le processus
                if (collectedCurrencies.Count == 0) {
                    droppingCurrencies = false;
                    OnWorkerDroppedAllCurrencied?.Invoke(this, EventArgs.Empty);
                }
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

    [Button]
    public override void Die() {
        base.Die();

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        WorkerManager.Instance.RemoveWorker(this);
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
        if(workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            health = 5;
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            health = 2;
            OnAnyWorkerAssignedHunter?.Invoke(this, EventArgs.Empty);
        }

        initialHealth = health;
        if (workerAI.GetDebugSpawn()) return;
        if(workerAI.GetJob() != WorkerAI.JobTypes.wild && workerAI.GetJob() != WorkerAI.JobTypes.jobless) {
            WorkerManager.Instance.AutoAssignSideToWorker(this);
        }
    }

}
