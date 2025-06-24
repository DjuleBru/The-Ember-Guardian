using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HunterJob : WorkerJob {

    private float trackAnimalMoveSpeed = 2f;
    private float initialFiringRange = 12f;
    private float initialFiringRangeRandomizer = 1f;

    private float attackRange;
    private float distanceToPlayerWhenCreatureIsAround = 4f;

    private Vector3 targetDefensiveDestinationRandomized;
    private Vector3 targetDefensiveDestination;
    private float randomizeDestinationTimer;
    private float randomizeDestinationRate = 2f;

    private float checkBlockedByCreatureTimer;
    private float distanceToHuntingLimit = 3f;

    private bool hasHitAnimal;

    protected Animal targetAnimal;

    public enum HunterState {
        idle, 
        followPlayerIdle,
        followPlayerAttackCreature,
        blockedByCreatures,
        workingWithPlayerToShootCreatures,
        headingToHunt,
        headingBackToHuntingLimit,
        pickingUpOrbs,
        droppingOrbs,
        hunting,
        headingToGuard,
        guarding,
        attackingDay,
    }

    private HunterState previousState;
    private HunterState state;

    private List<HunterState> dayHunterStates;
    private List<HunterState> dayAndNightHunterStates;
    private List<HunterState> duskAndNightHunterStates;

    private Tower assignedTower;

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
        dayHunterStates.Add(HunterState.attackingDay);
        dayHunterStates.Add(HunterState.pickingUpOrbs);
        dayHunterStates.Add(HunterState.workingWithPlayerToShootCreatures);

        duskAndNightHunterStates.Add(HunterState.headingToGuard);
        duskAndNightHunterStates.Add(HunterState.guarding);

        dayAndNightHunterStates.Add(HunterState.pickingUpOrbs);
        dayAndNightHunterStates.Add(HunterState.droppingOrbs);

        attackRange = initialFiringRange + UnityEngine.Random.Range(-initialFiringRangeRandomizer, initialFiringRangeRandomizer);
        distanceToHuntingLimit = UnityEngine.Random.Range(distanceToHuntingLimit - distanceToHuntingLimit / 2, distanceToHuntingLimit + distanceToHuntingLimit / 2);

        float workerDetectionColliderRadius = workerDetectionCollider.GetComponent<CircleCollider2D>().radius;
        distanceToFleeFromCreature = UnityEngine.Random.Range(workerDetectionColliderRadius - workerDetectionColliderRadius / 3, workerDetectionColliderRadius - workerDetectionColliderRadius / 4);
        distanceToPlayerWhenCreatureIsAround = UnityEngine.Random.Range(distanceToPlayerWhenCreatureIsAround - distanceToPlayerWhenCreatureIsAround / 3, distanceToPlayerWhenCreatureIsAround + distanceToPlayerWhenCreatureIsAround / 3);

    }

    private void Update() {

        if (targetAnimal != null) {
            Debug.DrawLine(mobMovement.transform.position, targetAnimal.transform.position, Color.red);
        }

        closestCreature = workerDetectionCollider.GetClosestCreature();
        if (targetCreature != null) {
            Debug.DrawLine(mobMovement.transform.position, targetCreature.transform.position, Color.red);
        }

        DebugExtention.DrawCircle(mobMovement.transform.position, attackRange, 20, Color.white);

        if (CheckDropCurrenciesToPlayer()) {

            ChangeState(HunterState.droppingOrbs);

        } else {
            if(!followingPlayer) {
                if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Dusk) {
                    if (CheckOrbsToCollect() && state != HunterState.hunting && state != HunterState.blockedByCreatures) {
                        ChangeState(HunterState.pickingUpOrbs);
                    };
                }
            }

        }


        if(dayHunterStates.Contains(state)) {
            // The current state is a "Day" state
            CheckDusk();
        }

        if (followingPlayer) {
            switch (state) {

                case HunterState.followPlayerIdle:
                    FollowPlayer();

                    if (closestCreature != null) {

                        CheckClosestCreatureSmart();

                        if (targetCreature == null) {

                            targetCreature = null;
                            workerAttack.RemoveAttackTarget();

                        }
                        else {
                            if (!CreatureIsTooClose(closestCreature, minimumDistanceToStaySafeFromCreature)) {
                                ChangeState(HunterState.followPlayerAttackCreature);
                            };

                        }
                    }

                    break;

                case HunterState.followPlayerAttackCreature:
                    if (targetCreature == null) {
                        workerAttack.RemoveAttackTarget();
                        ChangeState(HunterState.followPlayerIdle);
                        return;
                    }

                    if (CreatureIsTooClose(closestCreature, minimumDistanceToStaySafeFromCreature)) {
                        workerAttack.RemoveAttackTarget();
                        StayAwayFromCreature(closestCreature);
                        return;
                    }

                    if (PlayerIsTooFar()) {
                        workerAttack.RemoveAttackTarget();
                        ChangeState(HunterState.followPlayerIdle);
                        return;
                    }

                    if (!TargetIsInHuntingRange(targetCreature)) {
                        workerAttack.RemoveAttackTarget();
                        ChangeState(HunterState.followPlayerIdle);
                    }
                    else {
                        mobMovement.SetMoveTarget(transform.position);
                        workerAttack.SetAttackTarget(targetCreature);
                    }

                    CheckClosestCreatureSmart();

                    break;

                case HunterState.droppingOrbs:
                    DroppingOrbsUpdate();

                    break;
            }
        } else {

            switch (state) {

                case HunterState.idle:

                    CheckClosestAnimal();
                    CheckClosestCreatureSmart();

                    if (CheckBlockedByCreature()) {
                        ChangeState(HunterState.blockedByCreatures);
                        return;
                    };

                    if (IsInSafeZone() && targetCreature != null) {
                        ChangeState(HunterState.attackingDay);
                        return;
                    }

                    if (targetAnimal != null && !CheckBlockedByCreature()) {
                        ChangeState(HunterState.headingToHunt);
                        return;
                    };

                    Roam(3f, CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned()));
                    break;

                case HunterState.headingBackToHuntingLimit:
                    CheckClosestAnimal();

                    HeadBackToHuntingLimits();
                    if (HunterIsBackInHuntingLimits()) {
                        ChangeState(HunterState.idle);
                        return;
                    }

                    if (targetAnimal != null) {
                        if (TargetIsInHuntingRange(targetAnimal)) {
                            ChangeState(HunterState.hunting);
                        };
                    };

                    break;

                case HunterState.blockedByCreatures:

                    StayOutOfCreatureRange();

                    blockedByCreaturesTimer -= Time.deltaTime;
                    if(blockedByCreaturesTimer < 0) {
                        if (!CheckBlockedByCreature()) {
                            ChangeState(HunterState.idle);
                        }
                    }

                    break;

                case HunterState.workingWithPlayerToShootCreatures:

                    if (!CheckBlockedByCreature()) {
                        CheckClosestAnimal();
                        if (targetAnimal != null) {
                            ChangeState(HunterState.headingToHunt);
                        }
                        else {
                            ChangeState(HunterState.idle);
                        }
                    }

                    HeadToCreatureShootingPosition(closestCreature);

                    break;

                case HunterState.headingToHunt:

                    CheckClosestAnimal();

                    if (!HunterIsWithinHuntingLimits()) {
                        ChangeState(HunterState.headingBackToHuntingLimit);
                        return;
                    }

                    if (targetAnimal == null) {
                        Roam(5f, transform.position);
                        return;
                    }

                    if (CheckBlockedByCreature()) {
                        ChangeState(HunterState.blockedByCreatures);
                        return;
                    };

                    if (TargetIsInHuntingRange(targetAnimal)) {
                        ChangeState(HunterState.hunting);
                        return;
                    };

                    HeadToTargetAnimal();

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

                    if (!TargetIsStillInHuntingRange(targetAnimal)) {
                        workerAttack.RemoveAttackTarget();
                        ChangeState(HunterState.headingToHunt);
                        return;
                    };

                    workerAttack.SetAttackTarget(targetAnimal);

                    break;

                case HunterState.droppingOrbs:
                    DroppingOrbsUpdate();
                    break;

                case HunterState.headingToGuard:

                    if (worker.GetDefensiveStructureAssigned() != null) {
                        assignedTower = worker.GetDefensiveStructureAssigned() as Tower;
                    }
                    else {
                        TryAssignTower();
                    }

                    if (assignedTower != null) {
                        HeadToAssignedTower();
                    }
                    else {
                        HeadToMostExteriorBarricade();
                    }

                    break;

                case HunterState.guarding:

                    // Keep checking if camp limits have changed for ungarrisoned hunters
                    if (worker.GetDefensiveStructureAssigned() == null) {

                        Vector3 targetDestination = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), 2f);

                        if (Mathf.Abs(transform.position.x - targetDestination.x) > 2f) {
                            ChangeState(HunterState.headingToGuard);
                            return;
                        }
                    }


                    // DUSK : Keep checking if tower spots have been opened
                    if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {

                        if (worker.GetDefensiveStructureAssigned() == null) {
                            TryAssignTower();

                            if (assignedTower != null) {
                                ChangeState(HunterState.headingToGuard);
                            }
                        };

                    }

                    if (targetCreature == null) {

                        if(assignedTower == null) {
                            CheckClosestCreatureSmart();
                        } else {
                            CheckFurthestCreatureSmart();
                        } 

                    }
                    else {

                        if (!TargetIsInGuardingRange(targetCreature)) {
                            workerAttack.RemoveAttackTarget();
                        }
                        else {
                            workerAttack.SetAttackTarget(targetCreature);
                        }

                    }
                    

                    break;

                case HunterState.attackingDay:

                    if (!HunterIsWithinHuntingLimits()) {
                        ChangeState(HunterState.headingBackToHuntingLimit);
                        return;
                    }

                    CheckClosestCreatureSmart();

                    bool playerIsInFrontOfHunter = Mathf.Abs(transform.position.x) - Mathf.Abs((Player.Instance.transform.position.x)) < 0;

                    if (CreatureIsTooClose(closestCreature, minimumDistanceToStaySafeFromCreature) || !playerIsInFrontOfHunter) {
                        ChangeState(HunterState.blockedByCreatures);
                        return;
                    }

                    if (targetCreature == null) {
                        ChangeState(HunterState.idle);
                    }

                    else {

                        if (!TargetIsInHuntingRange(targetCreature)) {
                            workerAttack.RemoveAttackTarget();
                            ChangeState(HunterState.blockedByCreatures);
                        }
                        else {
                            workerAttack.SetAttackTarget(targetCreature);
                        }

                    }

                    break;

            }
        }

    }

    private bool TargetIsInHuntingRange(IDamageable iDamageable) {
        if(Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < (attackRange - attackRange/5)) {
            return true;
        } else {
            return false;
        }
    }
    private bool TargetIsInGuardingRange(IDamageable iDamageable) {
        if (Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < (attackRange)) {
            return true;
        }
        else {
            return false;
        }
    }
    private bool TargetIsStillInHuntingRange(IDamageable iDamageable) {
        if (Mathf.Abs((iDamageable as MonoBehaviour).transform.position.x - mobMovement.transform.position.x) < attackRange) {
            return true;
        }
        else {
            return false;
        }
    }
    private bool PlayerIsTooFar() {
        float distanceFromPlayerToAggroedCreature = Mathf.Abs(Mathf.Abs(Player.Instance.transform.position.x) - Mathf.Abs(transform.position.x));
        return distanceFromPlayerToAggroedCreature > maxDistanceToPlayerWhenFollowing;
    }

    private bool HunterIsWithinHuntingLimits() {
        return CampZoneManager.Instance.IsWithinHuntingLimits(transform.position);
    }

    private bool HunterIsBackInHuntingLimits() {
        Vector3 closestHuntingLimitInterior = Vector3.zero;

        if (transform.position.x < 0) {
            closestHuntingLimitInterior.x = CampZoneManager.Instance.GetClosestHuntingLimit(transform.position, distanceToHuntingLimit).x;
        }
        else {
            closestHuntingLimitInterior.x = CampZoneManager.Instance.GetClosestHuntingLimit(transform.position, distanceToHuntingLimit).x;
        }

        if(Mathf.Abs(transform.position.x - closestHuntingLimitInterior.x) < .1f) {
            return true;
        } else {
            return false;
        }
    }

    private void FollowPlayer() {
        Vector3 destination = WorkerFollowPlayerHandler.Instance.GetWorkerFollowPosition(worker);

        if (Mathf.Abs(destination.x - transform.position.x) > .5f) {
            mobMovement.SetMoveTarget(destination);
        }
    }

    public void StayOutOfCreatureRange() {
        mobMovement.SetMoveSpeed(fleeMoveSpeed);

        Creature closestCreature = workerDetectionCollider.GetClosestCreature();

        if (closestCreature != null) {
            bool playerIsInFrontOfHunter = Mathf.Abs(transform.position.x) - Mathf.Abs((Player.Instance.transform.position.x)) < 0;
            bool playerIsCloseToCreature = Mathf.Abs(Player.Instance.transform.position.x - closestCreature.transform.position.x) < 15f;

            if(!playerIsInFrontOfHunter) {

                if (CreatureIsTooClose(closestCreature, distanceToStartFleeingFromCreature)) {
                    StayAwayFromCreature(closestCreature);
                    return;
                }

            } else {
                if (CreatureIsTooClose(closestCreature, minimumDistanceToStaySafeFromCreature)) {
                    StayAwayFromCreature(closestCreature);
                    return;
                }
                else {
                    if(playerIsCloseToCreature) {
                        ChangeState(HunterState.workingWithPlayerToShootCreatures);
                    }
                }
            };

            
        }
    }

    private void HeadToCreatureShootingPosition(Creature closestCreature) {
        if (closestCreature == null) return;
        float direction = transform.position.x - closestCreature.transform.position.x;
        if (direction < 0) {
            direction = -1;
        }
        else {
            direction = 1;
        }

        Vector3 safePosition = Vector3.zero;
        bool playerIsInFrontOfCreature = Mathf.Abs(Player.Instance.transform.position.x) - Mathf.Abs((closestCreature.transform.position.x)) < 0;

        if (playerIsInFrontOfCreature) {
            safePosition = new Vector3(Player.Instance.transform.position.x + direction * distanceToPlayerWhenCreatureIsAround, 0, 0);
        }
        else {
            safePosition = new Vector3(closestCreature.transform.position.x + direction * minimumDistanceToStaySafeFromCreature, 0, 0);
        }

        bool workerIsCloseToSafePosition = (Mathf.Abs(transform.position.x - safePosition.x)) < 1f;

        if (workerIsCloseToSafePosition) {
            // Worker is close to safe position
            CheckClosestCreatureSmart();
            if (targetCreature != null && TargetIsInHuntingRange(targetCreature)) {
                ChangeState(HunterState.attackingDay);
            }
        }
        else {
            mobMovement.SetMoveTarget(safePosition);
        }
    }

    private bool CheckClosestAnimal() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0 ) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Animal newTargetAnimal = AnimalManager.Instance.GetClosestAvailableAnimalInRadius(worker);

            if (newTargetAnimal == null) {
                // Hunter had a target animal but finds non anymore

                if(targetAnimal != null) {
                    RemoveCurrentTargetAnimal();
                }

                OnHunterFindsNoAnimal?.Invoke(this, EventArgs.Empty);

            }

            if (newTargetAnimal != null && targetAnimal != newTargetAnimal) {
                
                if(newTargetAnimal.GetMaxHuntersAssigned()) {
                    // New animal has max hunters but furthest hunter is further than this worker
                    Worker furthestWorker = newTargetAnimal.GetFurthestWorkerAssigned();
                    furthestWorker.GetComponent<HunterJob>().RemoveCurrentTargetAnimal();
                    newTargetAnimal.UnAssignHunter(furthestWorker);
                }

                OnHunterFoundAnimal?.Invoke(this, EventArgs.Empty);
                TargetAnimal(newTargetAnimal);
            }
        }

        return false;
    }

    private bool CheckClosestCreatureSmart() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Creature newTargetCreature = CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(mobMovement.transform.position, attackRange, workerAttack.GetAttackDamage(), true);

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
    private bool CheckFurthestCreatureSmart() {
        checkClosestTargetTimer -= Time.deltaTime;

        if (checkClosestTargetTimer < 0) {
            checkClosestTargetTimer = checkClosestTargetCooldown;

            Creature newTargetCreature = CreaturesManager.Instance.GetFurthestCreatureInRadiusSmart(mobMovement.transform.position, attackRange, workerAttack.GetAttackDamage(), true);

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
        if (followingPlayer) return;
        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk) {
            ChangeState(HunterState.headingToGuard);
        }
    }

    private void TargetAnimal(Animal newTargetAnimal) {
        if (targetAnimal != null) {
            // Hunter already had an animal assigned
            RemoveCurrentTargetAnimal();
        }

        targetAnimal = newTargetAnimal;
        newTargetAnimal.OnMobDroppedCollectibles += TargetAnimal_OnAnimalDroppedCollectibles;
        newTargetAnimal.OnMobDamageTaken += TargetAnimal_OnMobDamageTaken;
        targetAnimal.AssignHunter(worker);
    }

    public void RemoveCurrentTargetAnimal() {
        if (targetAnimal == null) return;
        targetAnimal.OnMobDroppedCollectibles -= TargetAnimal_OnAnimalDroppedCollectibles;
        targetAnimal.OnMobDamageTaken -= TargetAnimal_OnMobDamageTaken;
        targetAnimal.UnAssignHunter(worker);
        hasHitAnimal = false;
        targetAnimal = null;

        ChangeState(HunterState.idle);
    }

    protected override void TargetCreature(Creature newTargetCreature) {
        if (targetCreature == newTargetCreature) return;

        if (targetCreature != null) {
            targetCreature.OnMobDied -= TargetCreature_OnMobDied;
            targetCreature.OnCreatureUntargetable -= TargetCreature_OnCreatureUntargetable;
        }

        targetCreature = newTargetCreature;
        targetCreature.OnMobDied += TargetCreature_OnMobDied;
        targetCreature.OnCreatureUntargetable += TargetCreature_OnCreatureUntargetable;
    }

    private void TargetAnimal_OnAnimalDroppedCollectibles(object sender, Animal.OnMobDroppedCollectibleEventArgs e) {
        foreach(Collectible collectible in e.collectibleDroppedList) {
            AssignCollectible(collectible);
        }

        RemoveCurrentTargetAnimal();
        CheckClosestAnimal();

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

    public void HeadBackToHuntingLimits() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }
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

        randomizeDestinationTimer -= Time.deltaTime;

        if(randomizeDestinationTimer <= 0) {
            randomizeDestinationTimer = randomizeDestinationRate;
            targetDefensiveDestination = CampZoneManager.Instance.GetClosestExteriorZoneLimit(worker.GetCampSideAddigned(), 3.5f);
            targetDefensiveDestinationRandomized = new Vector3(targetDefensiveDestination.x + UnityEngine.Random.Range(-2f, 2f), 0, 0);
        }

        mobMovement.SetMoveTarget(targetDefensiveDestinationRandomized);

        if(Mathf.Abs(transform.position.x - targetDefensiveDestinationRandomized.x) < 0.1f) {
            ChangeState(HunterState.guarding);
        }

        hasSetSpeed = false;
    }

    public void TryAssignTower() {
        if (assignedTower != null) return;

        if(PlayerCamp.Instance.GetAvailableTowers(worker.GetCampSideAddigned()) != 0) {
            assignedTower = PlayerCamp.Instance.GetClosestAvailableTower(worker.GetCampSideAddigned(), transform.position);
            assignedTower.AssignWorker(worker);
        }
        
    }

    public void HeadToAssignedTower() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(headToCampMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = assignedTower.transform.position;
        mobMovement.SetMoveTarget(targetDestination);

        if (Mathf.Abs(transform.position.x - targetDestination.x) < 0.1f) {
            
            assignedTower.GarrisonWorker(worker);

            ChangeState(HunterState.guarding);
        }

        hasSetSpeed = false;
    }

    public void SetGarrisoned(Vector3 position, Tower tower) {
        mobMovement.SetMoveTarget(position);

        worker.AssignDefensiveStructure(tower);
        workerAnimatorManager.SetWatchDir(position.x);
    }

    public void BuffRange(float buff) {
        attackRange *= buff;
    }

    public void ResetRangeBuff() {
        attackRange = initialFiringRange;
    }
    public void BuffDamage(float buff) {
        workerAttack.BuffDamage(buff);
    }

    public void ResetDamageBuff() {
        workerAttack.ResetDamageBuff();
    }

    private void ChangeState(HunterState newState) {
        if (newState == state) return;
        previousState = state;

        Vector3 targetDestination = mobMovement.transform.position;
        randomizeDestinationTimer = 0;

        if (newState == HunterState.headingBackToHuntingLimit) {
            if (transform.position.x < 0) {
                targetDestination.x = CampZoneManager.Instance.GetClosestHuntingLimit(transform.position, distanceToHuntingLimit).x;
            }
            else {
                targetDestination.x = CampZoneManager.Instance.GetClosestHuntingLimit(transform.position, distanceToHuntingLimit).x;
            }
        }

        if(newState != HunterState.hunting && newState != HunterState.guarding) {
            workerAttack.RemoveAttackTarget();
        }

        if(newState == HunterState.blockedByCreatures) {
            blockedByCreaturesTimer = blockedByCreaturesCooldown;
        }

        mobMovement.SetMoveTarget(targetDestination);
        state = newState;
        OnHunterChangedState?.Invoke(this, EventArgs.Empty);
    }
    public override void ReturnToPreviousState() {
        ChangeState(previousState);
    }


    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        CheckNewDayCycleParameters();
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        CheckNewDayCycleParameters();
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        CheckNewDayCycleParameters();
        targetCreature = null;
    }

    private void CheckNewDayCycleParameters() {
        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dawn) {
            SetDawnStartParameters();
        }
        if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            SetNightStartParameters();
        }
        if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk) {
            SetDuskStartParameters();
        }
    }

    private void SetDawnStartParameters() {
        if (followingPlayer) return;

        checkClosestTargetTimer = 0;

        RemoveCurrentTargetAnimal();
        assignedTower = null;

        ChangeState(HunterState.idle);
    }

    private void SetNightStartParameters() {
        if (followingPlayer) return;
    }

    private void SetDuskStartParameters() {
        if (followingPlayer) return;

        ChangeState(HunterState.headingToGuard);
    }

    private void WorkerMovement_OnDestinationReached(object sender, System.EventArgs e) {

    }

    public HunterState GetState() {
        return state;
    }

    public bool IsInSafeZone() {
        return (transform.position.x > CampZoneManager.Instance.GetCampCenterMinLimit() && transform.position.x < CampZoneManager.Instance.GetCampCenterMaxLimit());
    }

    public override void InitializeJob() {
        base.InitializeJob();

        mobMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;


        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            ChangeState(HunterState.idle);
        }
        else {
            ChangeState(HunterState.headingToGuard);
        }
    }

    protected override void WorkerAI_OnWorkerFollowPlayerChanged(object sender, EventArgs e) {
        base.WorkerAI_OnWorkerFollowPlayerChanged(sender, e);
        workerAttack.RemoveAttackTarget();

        if (followingPlayer) {
            state = HunterState.followPlayerIdle;
        } else {
            state = HunterState.idle;
            CheckNewDayCycleParameters();
        }

        roamTimer = 0f;
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
}
