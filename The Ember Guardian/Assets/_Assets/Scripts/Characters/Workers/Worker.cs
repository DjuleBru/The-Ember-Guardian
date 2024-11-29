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

    private float dropDelay = 0.15f; // Délai entre chaque drop
    private float dropTimer = 0f; // Compteur pour suivre le temps écoulé

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
        if(droppingCurrencies) {
            HandleDroppingCurrencies();
        }
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
