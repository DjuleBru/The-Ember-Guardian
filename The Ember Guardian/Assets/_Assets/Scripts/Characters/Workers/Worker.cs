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

    private int orbAmount;

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

    public void CollectOrb() {
        orbAmount++;
    }

    public void DropOrbs() {
        if (droppingOrbs) return;

        droppingOrbs = true;
        StartCoroutine(DropOrbsCoroutine(.15f));
    }

    public int GetOrbAmount() {
        return orbAmount;
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

    private void CheckOrbsNearby() {
        float range = .15f;

        // Check right

        Vector2 directionLeft = new Vector2(1, 0);
        Vector2 directionRight = new Vector2(1, 0);

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, directionLeft, range, collectibleLayerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, directionRight, range, collectibleLayerMask);

        Collectible orbCollected = null;

        if(hitLeft.collider != null) {
            Collectible collectibleHit = hitLeft.collider.gameObject.GetComponent<Collectible>();

            if(collectibleHit != null) {
                if(collectibleHit.GetCurrencyType() == PlayerCurrencies.CurrencyType.blueOrb && collectibleHit.GetCanBePickedUpByWorker() && !collectibleHit.GetCollected()) {
                    orbCollected = collectibleHit;
                    orbCollected.SetCollected();
                }
            }
        }

        if (hitRight.collider != null) {
            Collectible collectibleHit = hitRight.collider.gameObject.GetComponent<Collectible>();

            if (collectibleHit != null) {
                if (collectibleHit.GetCurrencyType() == PlayerCurrencies.CurrencyType.blueOrb && collectibleHit.GetCanBePickedUpByWorker() && !collectibleHit.GetCollected()) {
                    orbCollected = collectibleHit;
                    orbCollected.SetCollected();
                }
            }
        }


        if(orbCollected != null) {

            orbCollected.SetCollected();
            Destroy(orbCollected);

            if (workerAI.GetJob() == WorkerAI.JobTypes.wild) {
                RecruitWorker();
            }
            else {
                CollectOrb();
            }
        }
    }

    private IEnumerator DropOrbsCoroutine(float delayBetweenOrbs) {
        for (int i = 0; i < orbAmount; i++) {

            Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(blueOrbPrefab, transform.position, Quaternion.identity).GetComponent<Collectible>();
            lastBlueOrbDroppedOnTheFloor.ApplyRandomFrontForce(2f, 3f);
            lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(1f);
            OnAnyOrbDroppedByWorker?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(delayBetweenOrbs);
        }

        droppingOrbs = false;
        orbAmount = 0;
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
