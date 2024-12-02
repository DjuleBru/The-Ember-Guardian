using System;
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
    private Creature targetCreature;
    private WorkerDetectionCollider workerDetectionCollider;

    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float headToCampMoveSpeed = 2.5f;
    [SerializeField] private float trackAnimalMoveSpeed = 2f;
    [SerializeField] private float roamChangeDestinationRate = 5f;

    private float initialFiringRange = 10f;
    private float firingRange;

    private float roamTimer;
    private float checkClosestTargetTimer;
    private float checkBlockedByCreatureTimer;
    private float checkClosestTargetCooldown = .25f;

    private bool hasSetSpeed;
    private bool hasHitAnimal;

    private List<Collectible> orbsToCollect = new List<Collectible>();

    public enum HunterState {
        idle,
        blockedByCreatures,
        headingToHunt,
        pickingUpOrbs,
        droppingOrbs,
        hunting,
        headingToGuard,
        guarding,
    }

    private HunterState previousState;
    private HunterState state;

    private List<HunterState> dayHunterStates;
    private List<HunterState> dayAndNightHunterStates;
    private List<HunterState> duskAndNightHunterStates;

    private Tower destinationTower;

    public event EventHandler OnHunterChangedState;
    public event EventHandler OnHunterFindsNoAnimal;
    public event EventHandler OnHunterFoundAnimal;

    private void Awake() {
        workerDetectionCollider = GetComponentInChildren<WorkerDetectionCollider>();

        dayHunterStates = new List<HunterState>();
        dayAndNightHunterStates = new List<HunterState>();
        duskAndNightHunterStates = new List<HunterState>();

        dayHunterStates.Add(HunterState.idle);
        dayHunterStates.Add(HunterState.headingToHunt);
        dayHunterStates.Add(HunterState.hunting);

        duskAndNightHunterStates.Add(HunterState.headingToGuard);
        duskAndNightHunterStates.Add(HunterState.guarding);

        dayAndNightHunterStates.Add(HunterState.pickingUpOrbs);
        dayAndNightHunterStates.Add(HunterState.droppingOrbs);

        firingRange = initialFiringRange;
    }

    private void Update() {

        if (targetAnimal != null) {
            Debug.DrawLine(mobMovement.transform.position, targetAnimal.transform.position, Color.red);
        }
        if (targetCreature != null) {
            Debug.DrawLine(mobMovement.transform.position, targetCreature.transform.position, Color.red);
        }

        DebugExtention.DrawCircle(mobMovement.transform.position, firingRange, 20, Color.white);

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            CheckDropCurrenciesToPlayer();
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
                if(targetAnimal != null && !CheckBlockedByCreature()) {
                    ChangeState(HunterState.headingToHunt);
                };

                break;

            case HunterState.blockedByCreatures:

                StayOutOfCreatureRange();

                if(!CheckBlockedByCreature()) {
                    CheckClosestAnimal();
                    if (targetAnimal != null) {
                        ChangeState(HunterState.headingToHunt);
                    } else {
                        ChangeState(HunterState.idle);
                    }
                }

                break;

            case HunterState.headingToHunt:
                CheckClosestAnimal();

                if (targetAnimal == null) return;

                if (CheckBlockedByCreature()) {
                    ChangeState(HunterState.blockedByCreatures);
                    return;
                };

                HeadToTargetAnimal();
                if(TargetIsInHuntingRange(targetAnimal)) {
                    ChangeState(HunterState.hunting);
                };

                break;


            case HunterState.pickingUpOrbs:

                if (CheckBlockedByCreature()) {
                    ChangeState(HunterState.blockedByCreatures);
                    return;
                };

                HeadToPickUpClosestOrb();
                break;


            case HunterState.hunting:
                if (CheckBlockedByCreature()) {
                    ChangeState(HunterState.blockedByCreatures);
                    return;
                };

                if (!TargetIsStillInHuntingRange(targetAnimal) || !TargetAnimalIsWithinHuntingLimits(targetAnimal)) {
                    hunterAttack.RemoveAttackTarget();
                    ChangeState(HunterState.headingToHunt);
                    return;
                };

                hunterAttack.SetAttackTarget(targetAnimal);

                break;


            case HunterState.droppingOrbs:

                if(worker.GetTotalCurrencyAmount() == 0) {
                    ChangeState(previousState);
                    return;
                }

                if (worker.PlayerIsCloseAndStayedAround()) {
                    worker.DropCurrencies();
                    return;
                }

                if(!worker.GetPlayerIsClose()) { 
                    ChangeState(previousState);
                    return;
                }

                break;


            case HunterState.headingToGuard:

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
                        ChangeState(HunterState.headingToGuard);
                    }

                } else {

                    if(targetCreature == null) {

                        CheckClosestCreature();

                    } else {

                        if(!TargetIsInHuntingRange(targetCreature)) {
                            hunterAttack.RemoveAttackTarget();
                        } else {
                            hunterAttack.SetAttackTarget(targetCreature);
                        }

                    }
                }

                break;
        }

    }

    private bool TargetIsInHuntingRange(IDamageable iDamageable) {
        if(Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < (firingRange - firingRange/5)) {
            return true;
        } else {
            return false;
        }
    }

    private bool TargetIsStillInHuntingRange(IDamageable iDamageable) {
        if (Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < firingRange) {
            return true;
        }
        else {
            return false;
        }
    }

    private bool TargetAnimalIsWithinHuntingLimits(Animal animal) {
        return CampZoneManager.Instance.IsWithinHuntingLimits(animal.transform.position);
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

    public void StayOutOfCreatureRange() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        Creature closestCreature = workerDetectionCollider.GetClosestCreature();

        if(closestCreature != null) {
            float direction = closestCreature.transform.position.x - transform.position.x;
            if (direction > 0) {
                direction = -1;
            }
            else {
                direction = 1;
            }
            Vector3 safePosition = new Vector3(closestCreature.transform.position.x + direction * 20, 0, 0);

            mobMovement.SetMoveTarget(safePosition);
        }

    }

    private bool CheckClosestAnimal() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0 ) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Animal newTargetAnimal = AnimalManager.Instance.GetClosestAnimalInRadius(mobMovement.transform.position, worker.GetCampSideAddigned());
            if (newTargetAnimal == null && targetAnimal != null) {
                targetAnimal = null;
                OnHunterFindsNoAnimal?.Invoke(this, EventArgs.Empty);
            }

            if (newTargetAnimal != null && targetAnimal == null) {
                OnHunterFoundAnimal?.Invoke(this, EventArgs.Empty);
                TargetAnimal(newTargetAnimal);
            }
        }

        return false;
    }

    private bool CheckBlockedByCreature() {
        if(workerDetectionCollider.CreaturesInDetectionCollider()) {
            return true;
        } else {
            return false;
        }

    }

    private bool CheckClosestCreature() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Creature newTargetCreature = CreaturesManager.Instance.GetClosestCreatureInRadius(mobMovement.transform.position, firingRange);

            if (newTargetCreature == null) {
                targetCreature = null;
                return false;

            }

            else {
                TargetCreature(newTargetCreature);
                return true;
            }
        }

        return false;
    }

    private void CheckDusk() {
        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk) {
            ChangeState(HunterState.headingToGuard);
        }
    }

    private void CheckDropCurrenciesToPlayer() {
        if (worker.GetPlayerIsClose() && worker.GetTotalCurrencyAmount() > 0) {
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
            targetAnimal.OnMobDroppedCollectibles -= TargetAnimal_OnAnimalDroppedCollectibles;
            targetAnimal.OnMobDamageTaken -= TargetAnimal_OnMobDamageTaken;
        }
        targetAnimal = newTargetAnimal;
        newTargetAnimal.OnMobDroppedCollectibles += TargetAnimal_OnAnimalDroppedCollectibles;
        newTargetAnimal.OnMobDamageTaken += TargetAnimal_OnMobDamageTaken;
    }

    private void TargetCreature(Creature newTargetCreature) {
        if (targetCreature == newTargetCreature) return;

        if (targetCreature != null) {
            targetCreature.OnMobDied -= TargetCreature_OnMobDied;
        }

        targetCreature = newTargetCreature;
        targetCreature.OnMobDied += TargetCreature_OnMobDied;
    }

    private void TargetCreature_OnMobDied(object sender, System.EventArgs e) {
        targetCreature = null;
        hunterAttack.RemoveAttackTarget();
    }

    private void TargetAnimal_OnAnimalDroppedCollectibles(object sender, Animal.OnMobDroppedCollectibleEventArgs e) {
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
            mobMovement.SetMoveSpeed(trackAnimalMoveSpeed);
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
        Vector3 targetDestinationRandomized = new Vector3(targetDestination.x + UnityEngine.Random.Range(-1f, 1f), 0, 0);

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
        mobMovement.SetMoveTarget(position);

        worker.AssignStructure(tower);
        workerAnimatorManager.SetWatchDir(position.x);
    }

    public void BuffRange(float buff) {
        firingRange *= buff;
    }

    public void ResetRangeBuff() {
        firingRange = initialFiringRange;
    }

    private void ChangeState(HunterState newState) {
        if (newState == state) return;

        previousState = state;

        mobMovement.SetMoveTarget(mobMovement.transform.position);

        state = newState;

        OnHunterChangedState?.Invoke(this, EventArgs.Empty);
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
        ChangeState(HunterState.headingToGuard);
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        hunterAttack.SetHomingProjectile(true);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        checkClosestTargetTimer = 0;

        targetAnimal = null;
        hasHitAnimal = false;

        hunterAttack.SetHomingProjectile(false);

        ChangeState(HunterState.idle);
    }

    private void WorkerMovement_OnDestinationReached(object sender, System.EventArgs e) {

    }

    public HunterState GetState() {
        return state;
    }

    private void OnEnable() {
        mobMovement = GetComponent<MobMovement>();
        workerAnimatorManager = GetComponentInChildren<WorkerAnimatorManager>();
        hunterAttack = GetComponent<MobAttack>();
        worker = GetComponent<Worker>();

        mobMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;


        mobMovement.SetMoveTarget(mobMovement.transform.position);

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(HunterState.idle);
        } else {
            ChangeState(HunterState.headingToGuard);
        }

    }


    private void OnDisable() {
        hasSetSpeed = false;
        mobMovement.OnDestinationReached -= WorkerMovement_OnDestinationReached;

        if (targetAnimal != null) {
            targetAnimal.OnMobDroppedCollectibles -= TargetAnimal_OnAnimalDroppedCollectibles;
            targetAnimal.OnMobDamageTaken -= TargetAnimal_OnMobDamageTaken;
        }

        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
    }

    private void OnDestroy() {
        mobMovement.OnDestinationReached -= WorkerMovement_OnDestinationReached;
        if (targetAnimal != null) {
            targetAnimal.OnMobDroppedCollectibles -= TargetAnimal_OnAnimalDroppedCollectibles;
            targetAnimal.OnMobDamageTaken -= TargetAnimal_OnMobDamageTaken;
        }

        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
    }

}
