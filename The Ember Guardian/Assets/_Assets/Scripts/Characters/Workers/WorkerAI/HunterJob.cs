using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HunterJob : MonoBehaviour, IJobBehavior {

    private Worker worker;
    private MobMovement mobMovement;
    private WorkerAnimatorManager workerAnimatorManager;
    private MobAttack hunterAttack;
    private Animal targetAnimal;

    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float headToCampMoveSpeed = 2.5f;
    [SerializeField] private float trackAnimalMoveSpeed = 2f;
    [SerializeField] private float roamChangeDestinationRate = 5f;

    [SerializeField] private float huntingRange = 4f;

    private float maxAnimalTargetingDistanceToCampOuterPoint = 60f;

    private float roamTimer;
    private float checkClosestAnimalTimer;
    private float checkClosestAnimalRate = 1f;

    private bool hasSetSpeed;
    private bool hasHitAnimal;

    private List<Collectible> orbsToCollect = new List<Collectible>();

    public enum HunterState {
        idle,
        headingToHunt,
        pickingUpOrbs,
        droppingOrbs,
        hunting,
        headingToSafety,
        guarding,
    }

    private HunterState previousState;
    private HunterState state;

    private List<HunterState> dayHunterStates;
    private List<HunterState> dayAndNightHunterStates;
    private List<HunterState> duskAndNightHunterStates;

    private Tower destinationTower;

    private void Awake() {
        dayHunterStates = new List<HunterState>();
        dayAndNightHunterStates = new List<HunterState>();
        duskAndNightHunterStates = new List<HunterState>();

        dayHunterStates.Add(HunterState.idle);
        dayHunterStates.Add(HunterState.headingToHunt);
        dayHunterStates.Add(HunterState.hunting);

        duskAndNightHunterStates.Add(HunterState.headingToSafety);
        duskAndNightHunterStates.Add(HunterState.guarding);

        dayAndNightHunterStates.Add(HunterState.pickingUpOrbs);
        dayAndNightHunterStates.Add(HunterState.droppingOrbs);
    }

    private void Update() {

        if (targetAnimal != null) {
            Debug.DrawLine(mobMovement.transform.position, targetAnimal.transform.position, Color.red);
        }

        if(DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            CheckDroporbsToPlayer();
            CheckOrbsToCollect();
        }

        if(dayHunterStates.Contains(state)) {
            // The current state is a "Day" state
            CheckDusk();
        }

        switch (state) {

            case HunterState.idle:

                Roam();
                CheckClosestAnimal();

                break;


            case HunterState.headingToHunt:

                CheckClosestAnimal();

                if (targetAnimal == null) return;

                HeadToTargetAnimal();
                if(TargetAnimalIsInHuntingRange()) {
                    ChangeState(HunterState.hunting);
                };

                break;


            case HunterState.pickingUpOrbs:
                HeadToPickUpClosestOrb();
                break;


            case HunterState.hunting:

                if (!TargetAnimalIsInHuntingRange()) {
                    hunterAttack.RemoveAttackTarget();
                    ChangeState(HunterState.headingToHunt);
                    return;
                };

                hunterAttack.SetAttackTarget(targetAnimal);

                break;


            case HunterState.droppingOrbs:

                if(worker.GetOrbAmount() == 0) {
                    ChangeState(previousState);
                    return;
                }

                if (worker.PlayerIsCloseAndStayedAround()) {
                    worker.DropOrbs();
                    return;
                }

                if(!worker.GetPlayerIsClose()) { 
                    ChangeState(previousState);
                    return;
                }

                break;


            case HunterState.headingToSafety:

                destinationTower = StructuresManager.Instance.GetClosestTower(worker.GetCampSideAddigned(), transform.position);

                if (destinationTower != null) {
                    HeadToClosestTower();
                } else {
                    HeadToMostExteriorBarricade();
                }

                break;


            case HunterState.guarding:

                // Keep checking if tower spots have been opened
                if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
                    if (worker.GetStructureAssigned() != null) return;

                    destinationTower = StructuresManager.Instance.GetClosestTower(worker.GetCampSideAddigned(), transform.position);

                    if (destinationTower != null) {
                        ChangeState(HunterState.headingToSafety);
                    }

                }

                break;
        }

    }

    private bool TargetAnimalIsInHuntingRange() {
        if(Mathf.Abs(targetAnimal.transform.position.x - mobMovement.transform.position.x) < huntingRange) {
            return true;
        } else {
            return false;
        }
    }

    public void Roam() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(mobMovement, 3f , CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned()));
        }
    }

    private bool CheckClosestAnimal() {

        if (hasHitAnimal) return false;

        checkClosestAnimalTimer -= Time.deltaTime;

        if(checkClosestAnimalTimer < 0 ) {
            checkClosestAnimalTimer = checkClosestAnimalRate;

            Animal newTargetAnimal = AnimalManager.Instance.GetClosestAnimalInRadius(mobMovement.transform.position, worker.GetCampSideAddigned(), maxAnimalTargetingDistanceToCampOuterPoint);

            if(newTargetAnimal == null) {
                targetAnimal = null;
                ChangeState(HunterState.idle);
                return false;

            } else {
                TargetAnimal(newTargetAnimal);
                ChangeState(HunterState.headingToHunt);
                return true;
            }
        }

        return false;
    }

    private void CheckDusk() {
        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk) {
            ChangeState(HunterState.headingToSafety);
        }
    }

    private void CheckDroporbsToPlayer() {
        if (worker.GetOrbAmount() > 0 && worker.GetPlayerIsClose()) {
            ChangeState(HunterState.droppingOrbs);
        }
    }

    private void CheckOrbsToCollect() {
        if(orbsToCollect.Count > 0 && state != HunterState.hunting) {
            ChangeState(HunterState.pickingUpOrbs);
        }
    }

    private void TargetAnimal(Animal newTargetAnimal) {
        if (targetAnimal == newTargetAnimal) return;

        if (targetAnimal != null) {
            newTargetAnimal.OnAnimalDroppedCollectibles -= TargetAnimal_OnAnimalDroppedCollectibles;
        }

        targetAnimal = newTargetAnimal;
        targetAnimal.OnAnimalDroppedCollectibles += TargetAnimal_OnAnimalDroppedCollectibles;
        targetAnimal.OnMobDamageTaken += TargetAnimal_OnMobDamageTaken;
    }

    private void TargetAnimal_OnAnimalDroppedCollectibles(object sender, Animal.OnAnimalDroppedCollectibleEventArgs e) {
        foreach(Collectible collectible1 in e.collectibleDroppedList) {
            orbsToCollect.Add(collectible1);
        }

        foreach(Collectible collectible in orbsToCollect) {
            collectible.OnCollectibleDestroyed += Collectible_OnCollectibleDestroyed;
        }

        targetAnimal = null;
        hasHitAnimal = false;

        ChangeState(HunterState.idle);
    }

    private void TargetAnimal_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        hasHitAnimal = true;
    }

    public void HeadToTargetAnimal() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = new Vector3(targetAnimal.transform.position.x, 0, 0);
            
        mobMovement.SetMoveTarget(targetDestination);
        hasSetSpeed = false;
    }

    public void HeadToPickUpClosestOrb() {

        if (orbsToCollect.Count == 0) {
            ChangeState(HunterState.headingToHunt);
            return;
        }

        Collectible orbToCollect = orbsToCollect[0];
        Vector3 targetDestination = new Vector3(orbToCollect.transform.position.x, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);

    }

    public void HeadToMostExteriorBarricade() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), 2f);
        Vector3 targetDestinationRandomized = new Vector3(targetDestination.x + Random.Range(-1f, 1f), 0, 0);

        mobMovement.SetMoveTarget(targetDestinationRandomized);

        if(Mathf.Abs(transform.position.x - targetDestinationRandomized.x) < 0.1f) {
            ChangeState(HunterState.guarding);
        }

        hasSetSpeed = false;
    }

    public void HeadToClosestTower() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = destinationTower.transform.position;
        mobMovement.SetMoveTarget(targetDestination);

        if (Mathf.Abs(transform.position.x - targetDestination.x) < 0.1f) {
            
            if(!destinationTower.GetTowerFull() && !destinationTower.GetWorkerAssigned(worker)) {
                destinationTower.AssignWorker(worker);
            }

            ChangeState(HunterState.guarding);
        }

        hasSetSpeed = false;
    }

    public void SetGarrisoned(Vector3 position, Tower tower) {
        Debug.Log("SetGarrisoned " + position);
        mobMovement.SetMoveTarget(position);

        worker.AssignStructure(tower);
        workerAnimatorManager.SetWatchDir(position.x);
    }


    private void ChangeState(HunterState newState) {
        if (newState == state) return;

        previousState = state;
        Debug.Log("previousState " + previousState);

        mobMovement.SetMoveTarget(mobMovement.transform.position);

        Debug.Log("newState " + newState);
        state = newState;
    }

    private void Collectible_OnCollectibleDestroyed(object sender, System.EventArgs e) {
        Collectible collectible = sender as Collectible;
        RemoveOrbToCollect(collectible);
    }

    public void RemoveOrbToCollect(Collectible collectible) {
        if(orbsToCollect.Contains(collectible)) {
            orbsToCollect.Remove(collectible);
        }
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        ChangeState(HunterState.headingToSafety);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        checkClosestAnimalTimer = 0;

        targetAnimal = null;
        hasHitAnimal = false;
        
        ChangeState(HunterState.idle);
    }

    private void WorkerMovement_OnDestinationReached(object sender, System.EventArgs e) {

    }

    private void OnEnable() {
        mobMovement = GetComponent<MobMovement>();
        workerAnimatorManager = GetComponentInChildren<WorkerAnimatorManager>();
        hunterAttack = GetComponent<MobAttack>();
        worker = GetComponent<Worker>();

        mobMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;


        mobMovement.SetMoveTarget(mobMovement.transform.position);

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(HunterState.idle);
        } else {
            ChangeState(HunterState.headingToSafety);
        }

    }

    private void OnDisable() {
        hasSetSpeed = false;
        mobMovement.OnDestinationReached -= WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
    }

}
