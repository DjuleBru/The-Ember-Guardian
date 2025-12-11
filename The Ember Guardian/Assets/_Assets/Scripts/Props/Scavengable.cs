using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScavengableObstacle;

public class Scavengable : MonoBehaviour, IDamageable, IScavengable {


    [SerializeField] private string scavengableID;
    public string GetScavengableID() => scavengableID;

#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrEmpty(scavengableID) && gameObject.scene.IsValid()) {
            scavengableID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    public void ForceNewID() {
        scavengableID = System.Guid.NewGuid().ToString();
    }
#endif

    private PayCurrencyUI payOrbsUI;
    protected List<PayCurrencyTemplateWorldUI> scavengeStructureCurrencyTemplates = new List<PayCurrencyTemplateWorldUI>();

    [SerializeField] protected Transform orbTemplateWorldUIParent;
    [SerializeField] private Transform currencySpawnPoint;
    [SerializeField] private List<Transform> minePoints;
    [SerializeField] private PlayerCurrencies.CurrencyType currencyTypeCollected;
    [SerializeField] private PlayerCurrencies.CurrencyType luckyPickaxeCurrencyTypeCollected;
    [SerializeField] private int currencyAmountCollected;

    [SerializeField] private int hitsToCollectOneCurrency;
    [SerializeField] private bool isMine;
    [SerializeField] private bool infiniteSource;
    [SerializeField] private bool doNotYieldResources;

    [SerializeField] private int miningPriority;
    [SerializeField] private int maxMinersAssigned;
    [SerializeField] private float initialTimeToMineOneResource = 25;
    [SerializeField] private float maxTimeToMineOneResource;
    [SerializeField] private float miningTimeIncreasePerResourceExtracted = 1;
    private float timeToMineOneResource;
    private float miningTimer;

    private List<MinerJob> minerAssignedList = new List<MinerJob>();
    private List<MinerJob> minersMiningList = new List<MinerJob>();

    private int health;
    private int hitsTaken;

    private bool scavengedUnlocked = true;
    private bool markedToScavenge;
    private bool depleted;
    private bool playerInTriggerArea;
    private bool scavengingActive = true;


    public event EventHandler<OnScavengableDeactivatedMiningEventArgs> OnActivatedMining;
    public event EventHandler<OnScavengableDeactivatedMiningEventArgs> OnDeactivatedMining;
    public event EventHandler OnPlayerTriggerIn;
    public event EventHandler OnPlayerTriggerOut;
    public event EventHandler OnDamageTaken;
    public event EventHandler OnScavengableMarkedToScavenge;
    public event EventHandler OnMinerStartsMining;
    public event EventHandler OnMinerStopsMining;
    public event EventHandler OnMinerExtractedResourceFromMine;
    public event EventHandler OnMineEffortLoaded;
    public static event EventHandler<OnAnyScavengableMarkedToScavengeEventArgs> OnAnyScavengableMarkedToScavenge;
    public event EventHandler OnScavengableDepleted;
    public event EventHandler<OnStavengableSpawnedCurrencyEventArgs> OnScavengableSpawnedCurrency;
    public class OnAnyScavengableMarkedToScavengeEventArgs : EventArgs {
        public bool triggerSFX;
    }
    public class OnScavengableDeactivatedMiningEventArgs : EventArgs {
        public bool triggerSFX;
    }
    public class OnStavengableSpawnedCurrencyEventArgs : EventArgs {
        public Collectible collectibleSpawned;
    }

    private void Awake() {
        payOrbsUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();
        health = hitsToCollectOneCurrency * currencyAmountCollected +1;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

        payOrbsUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payOrbsUI.SetOrbTemplateUIList(scavengeStructureCurrencyTemplates);

        if(currencyTypeCollected == PlayerCurrencies.CurrencyType.ammo) {
            StartCoroutine(SetAmmoTypeCollected());
        }

        if(isMine) {
            timeToMineOneResource = initialTimeToMineOneResource;
            miningTimer = timeToMineOneResource;
            OnDeactivatedMining?.Invoke(this, new OnScavengableDeactivatedMiningEventArgs {
                triggerSFX = false
            });
        }
    }

    private void Update() {
        if (!isMine) return;
        if (minersMiningList.Count == 0) return;

        miningTimer -= Time.deltaTime * minersMiningList.Count;

        if(miningTimer <= 0) {
            timeToMineOneResource += miningTimeIncreasePerResourceExtracted;
            miningTimer = timeToMineOneResource;
            OnMinerExtractedResourceFromMine?.Invoke(this, EventArgs.Empty);
            AddCurrencyToMiner();
        }
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        if(isMine) {
            StartCoroutine(RemoveMinersFromMine());
        }
    }

    private IEnumerator RemoveMinersFromMine() {
        List<MinerJob> minersAssignedCopy = new List<MinerJob>();
        foreach (MinerJob miner in minerAssignedList) {
            minersAssignedCopy.Add(miner);
        }

        foreach (MinerJob miner in minersAssignedCopy) {
            UnassignMiner(miner);
            miner.UnAssignScavengable();

            yield return new WaitForSeconds(.3f);
        }
    }

    private IEnumerator SetAmmoTypeCollected() {
        yield return new WaitForSeconds(1f);
        bool secondaryGunUnlocked = PlayerShoot.Instance.GetSecondaryGunSO() != null;

        if (PlayerShoot.Instance.GetPrimaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special || (secondaryGunUnlocked && PlayerShoot.Instance.GetSecondaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special)) {
            luckyPickaxeCurrencyTypeCollected = PlayerCurrencies.CurrencyType.ammo_special;
            
            if (PlayerShoot.Instance.GetPrimaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special && (secondaryGunUnlocked && PlayerShoot.Instance.GetSecondaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special)) {
                currencyTypeCollected = PlayerCurrencies.CurrencyType.ammo_special;
                timeToMineOneResource *= 2;
            }
        }
    }

    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        MarkToScavenge();
    }

    public void MarkToScavenge(bool markedFromSave = false) {
        Debug.Log("MarkToScavenge");
        markedToScavenge = true;

        OnScavengableMarkedToScavenge?.Invoke(this, EventArgs.Empty);
        OnAnyScavengableMarkedToScavenge?.Invoke(this, new OnAnyScavengableMarkedToScavengeEventArgs {
            triggerSFX = !markedFromSave,
        });

        if (isMine) {

            if(!markedFromSave) {
                Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
            }

            OnActivatedMining?.Invoke(this, new OnScavengableDeactivatedMiningEventArgs {
                triggerSFX = !markedFromSave,
            });
        }
    }

    public void Die(Transform damageSource = null) {
        depleted = true;
        OnScavengableDepleted?.Invoke(this, EventArgs.Empty);

        List<MinerJob> minerAssignedListCopy = new List<MinerJob>();
        foreach(MinerJob minerJob in minerAssignedList) {
            minerAssignedListCopy.Add(minerJob);
        }

        foreach(MinerJob miner in minerAssignedListCopy) {
            miner.UnAssignScavengable();
        }
    }

    public Transform GetMeleeAttackPosition() {
        return minePoints[UnityEngine.Random.Range(0, minePoints.Count)];
    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    [Button]
    public void TakeDamage(int damage, Transform damageSource, bool crit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        health -= damage;
        hitsTaken += damage;
        OnDamageTaken?.Invoke(this, EventArgs.Empty);
        if (hitsTaken >= hitsToCollectOneCurrency && !doNotYieldResources) {
            hitsTaken = 0;
            SpawnCurrency();
        }

        if (health <= 0 && !infiniteSource) {
            Die();
        }
    }

    private void AddCurrencyToMiner() {
        MinerJob minerJob = minerAssignedList[UnityEngine.Random.Range(0, minerAssignedList.Count)];
        minerJob.GetComponent<Worker>().CollectCurrency(currencyTypeCollected);

        float luckyPickaxeChance = UnityEngine.Random.Range(0f, 1f);
        if (luckyPickaxeChance < WorkerStats.Instance.GetMinerLuckyPickaxeProb()) {
            minerJob.GetComponent<Worker>().CollectCurrency(luckyPickaxeCurrencyTypeCollected);
        }
    }

    private void SpawnCurrency() {
        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeCollected), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomSidewardsForce(5, 8);
        collectible.SetCollectibleUnInteractable(.5f);
        collectible.SetCanBePickedUpByWorker();

        AssignRandomMinerToCollect(collectible);

        OnScavengableSpawnedCurrency?.Invoke(this, new OnStavengableSpawnedCurrencyEventArgs {
            collectibleSpawned = collectible,
        });

        float luckyPickaxeChance = UnityEngine.Random.Range(0f, 1f);
        if(luckyPickaxeChance < WorkerStats.Instance.GetMinerLuckyPickaxeProb()) {
            SpawnLuckyCurrency();
        }
    }
    private void SpawnLuckyCurrency() {
        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(luckyPickaxeCurrencyTypeCollected), currencySpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomSidewardsForce(5, 8);
        collectible.SetCollectibleUnInteractable(.5f);
        collectible.SetCanBePickedUpByWorker();

        AssignRandomMinerToCollect(collectible);

        OnScavengableSpawnedCurrency?.Invoke(this, new OnStavengableSpawnedCurrencyEventArgs {
            collectibleSpawned = collectible,
        });
    }

    private void AssignRandomMinerToCollect(Collectible collectible) {
        if (minerAssignedList.Count == 0) return;
        MinerJob miner = minerAssignedList[UnityEngine.Random.Range(0, minerAssignedList.Count)];
        miner.AssignCollectible(collectible);
    }

    public void AssignMiner(MinerJob miner) {
        if (minerAssignedList.Contains(miner)) return;
        minerAssignedList.Add(miner);
    }

    public void MinerStartsMining(MinerJob minerJob) {
        minersMiningList.Add(minerJob);
        OnMinerStartsMining?.Invoke(this, EventArgs.Empty);
    }

    public void MinerStopsMining(MinerJob minerJob) {
        if (!minersMiningList.Contains(minerJob)) return;
        minersMiningList.Remove(minerJob);
        OnMinerStopsMining?.Invoke(this, EventArgs.Empty);
    }

    public void UnassignMiner(MinerJob miner) {
        Debug.Log("UnassignMiner " + miner);
        MinerStopsMining(miner);

        if (minerAssignedList.Contains(miner)) {
            minerAssignedList.Remove(miner);
        };

        if (isMine) {
            miner.ExitFromMine();
        }
    }

    public bool GetMaxMinersAssigned() {
        return minerAssignedList.Count >= maxMinersAssigned;
    }

    public int GetMaxMinerAmount() {
        return maxMinersAssigned;
    }

    public int GetMinerAmountMining() {
        return minersMiningList.Count;
    }

    public bool GetDepleted() {
        return depleted;
    }

    public int GetHealth() {
        return health;
    }

    public int GetHitsTaken() {
        return hitsTaken;
    }

    public void SetHealth(int health) {
        this.health = health;

        if (health <= 0 && !infiniteSource) {
            Die();
        }
    }
    public void SetHitsTaken(int hitsTaken) {
        this.hitsTaken = hitsTaken;
    }
    public void SetTimeToMineOneResource(float timeToMine) {
        if (!isMine) return;
        this.timeToMineOneResource = timeToMine;
        OnMineEffortLoaded?.Invoke(this, EventArgs.Empty);
    }

    public float GetTimeToMineOneResource() {
        return timeToMineOneResource;
    }

    public int GetMiningPriority() {
        return miningPriority;
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

        if(!markedToScavenge) {
            payOrbsUI.SetPlayerInteracting(true);
        } else {
            if(isMine) {
                ToggleScavengingActive();
            }
        }
    }

    private void ToggleScavengingActive() {
        scavengingActive = !scavengingActive;

        if(scavengingActive) {
            OnActivatedMining?.Invoke(this, new OnScavengableDeactivatedMiningEventArgs {
                triggerSFX = true
            });

        } else {
            OnDeactivatedMining?.Invoke(this, new OnScavengableDeactivatedMiningEventArgs {
                triggerSFX = true
            });
            StartCoroutine(RemoveMinersFromMine());
        }
    }

    public void SetScavengingActive(bool scavengingActive, bool setFromLoad = true) {
        if (!isMine) return;

        this.scavengingActive = scavengingActive;

        if (scavengingActive) {
            OnActivatedMining?.Invoke(this, new OnScavengableDeactivatedMiningEventArgs {
                triggerSFX = !setFromLoad
            });

        }
        else {
            OnDeactivatedMining?.Invoke(this, new OnScavengableDeactivatedMiningEventArgs {
                triggerSFX = !setFromLoad
            });
            StartCoroutine(RemoveMinersFromMine());
        }
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        payOrbsUI.SetPlayerInteracting(false);
        payOrbsUI.ResetCurrencyPayment();
    }

    public bool GetIsMine() {
        return isMine;
    }

    public bool GetScavengingActive() {
        return scavengingActive;
    }


    public float GetTimeToExtractOneResourceNormalized() {
        return (timeToMineOneResource - initialTimeToMineOneResource) / (maxTimeToMineOneResource - initialTimeToMineOneResource);
    }

    public void SetScavengableUnlocked(bool unlocked) {
        if (LevelManager.Instance.IsHordeMode()) return;
        this.scavengedUnlocked = unlocked;
    }

    public void SetParameters(int amountToCollect, int hitsToCollectOne) {
        currencyAmountCollected = amountToCollect;
        hitsToCollectOneCurrency = hitsToCollectOne;
    }

    public int GetCurrencyAmountCollected() {
        return currencyAmountCollected;
    }
    public int GetHitsToCollect() {
        return hitsToCollectOneCurrency;
    }

    public PlayerCurrencies.CurrencyType GetCurrencyTypeCollected() {
        return currencyTypeCollected;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!scavengedUnlocked) return;
        if (depleted) return;
        if (collision.GetComponent<Player>() == null) return;

        playerInTriggerArea = true;
        OnPlayerTriggerIn?.Invoke(this, EventArgs.Empty);

        if(isMine && markedToScavenge) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
        }

        if (markedToScavenge && !isMine) return;
        Player.Instance.SetInPayCurrencyArea(true);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!scavengedUnlocked) return;
        if (collision.GetComponent<Player>() == null) return;

        Player.Instance.SetInPayCurrencyArea(false);

        if (depleted) return;

        payOrbsUI.SetPlayerInteracting(false);
        playerInTriggerArea = false;
        OnPlayerTriggerOut?.Invoke(this, EventArgs.Empty);

        if (isMine && markedToScavenge) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractStarted;
        DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
    }
}
