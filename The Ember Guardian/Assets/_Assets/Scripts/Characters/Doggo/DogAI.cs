using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI : MonoBehaviour
{
    public enum State {
        stay,
        stayAtCamp,
        runToCamp,
        idle,
        nightInCampIdle,
        nightInCampRunToClosestCreature,
        nightInCampGrowlAtIncomingCreature,
        walkWithPlayer,
        runWithPlayer,
        growling,
        barking,
        attacking,
        pickingUpOrbs,
        droppingOrbs,
        runToPlayer,
    }

    protected State state;
    protected State previousState;
    protected State currentBehaviorIdleState;
    protected MobMovement dogMovement;
    [SerializeField] protected Dog.DogType dogAIType;
    [SerializeField] protected DogCreatureDetectionCollider creatureDetectionCollider;
    protected Creature closestCreature;
    protected Creature closestIncomingCreature;

    protected Vector3 stickWithPlayerMoveTarget;
    protected Vector3 stayPointToRoamAround;
    protected bool hasSetSpeed;

    protected float roamRadius = 5f;
    protected float roamTimer;
    protected float roamChangeDestionationRate = 10f;
    protected float biteRange = 2.5f;

    protected float walkMoveSpeed = 1.5f;
    protected float runMoveSpeed = 7f;

    protected float distanceToPlayer;
    protected float distanceToCamp;
    protected float distanceToStickWithPlayerTarget;
    protected float distanceInFrontOfPlayer = 6f;
    protected float minDistanceToPlayer = 1f;
    protected float maxDistanceToPlayer = 10f;
    protected float distanceToRunToCamp = 5f;
    protected float playerStickedAroundTimer;

    protected float distanceToPlayerToRoamWhenStickingAround = 1f;
    protected float distanceToRunToPlayerWhenStickingAround = 12f;
    protected float distanceToWalkToPlayerWhenStickingAroundReference = 10f;
    protected float distanceToWalkToPlayerWhenStickingAround = 6f;

    protected float creatureBarkDistanceToDog = 7f;
    protected float creatureBarkDistanceToPlayer = 10f;
    protected float creatureBarkDistanceToDog_DarkCompanion = 12f;
    protected float creatureBarkDistanceToPlayer_DarkCompanion = 12f;

    protected bool playerRunning;
    protected float lastPlayerDirection;
    protected float changeDirectionTimer;
    protected float requiredDirectionChangeDuration = .5f;

    protected bool dogJustStoppedGrowling;
    protected float dogJustStoppedGrowlingTimer;
    protected float dogJustStoppedGrowlingTime = 3f;

    protected float barkingTimer;
    protected float barkTimeToAttack = 1.5f;

    protected bool hasBiteUnlocked;
    protected bool biteStarted;
    protected bool biteReady;
    protected bool isHubScene;
    protected bool running;
    protected float biteTimer;
    protected float biteAnimationDelay = .5f;
    protected float maxBiteDistance_GermanShepherd = 2f;

    public event EventHandler OnStateChanged;
    public event EventHandler OnDogBite;

    protected virtual void Awake() {
        dogMovement = GetComponent<MobMovement>();
    }

    protected virtual void Start() {
        PlayerMovement.Instance.OnPlayerRunStarted += PlayerMovement_OnPlayerRunStarted;
        PlayerMovement.Instance.OnPlayerRunStopped += PlayerMovement_OnPlayerRunStopped;
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;

        currentBehaviorIdleState = Dog.Instance.GetIdleState();
        ChangeState(Dog.Instance.GetInitialState());
        stickWithPlayerMoveTarget = transform.position;
        stayPointToRoamAround = transform.position;
        roamTimer = roamChangeDestionationRate;

        hasBiteUnlocked = DogStats.Instance.GetGermanShepherdBiteAbilityUnlocked();

        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            creatureBarkDistanceToDog = creatureBarkDistanceToDog_DarkCompanion;
            creatureBarkDistanceToPlayer = creatureBarkDistanceToPlayer_DarkCompanion;
        }

        isHubScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;

        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        }
    }

    private int GetBiteDamage() {
        int biteDamage = 0; 
        if (Dog.Instance.GetDogType() == Dog.DogType.GermanShepherd) {
            biteDamage = DogStats.Instance.GetGermanShepherdBiteDamage();
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
            biteDamage = DogStats.Instance.GetRetreiverBiteDamage();
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            biteDamage = DogStats.Instance.GetDarkCompanionBiteDamage();
        }
        return biteDamage;
    }
    private float GetBiteCooldown() {
        float biteCooldown = 0;

        if (Dog.Instance.GetDogType() == Dog.DogType.GermanShepherd) {
            biteCooldown = DogStats.Instance.GetGermanShepherdBiteCooldown();
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
            biteCooldown = DogStats.Instance.GetRetreiverBiteCooldown();
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            biteCooldown = DogStats.Instance.GetDarkCompanionBiteCooldown();
        }
        return biteCooldown;
    }

    private void Dog_OnDogTypeChanged(object sender, EventArgs e) {
        ChangeState(Dog.Instance.GetInitialState());
    }

    protected void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        SetIdleBehaviorState(State.idle);
    }

    protected virtual void Update() {
        if(state != State.stay) {
            HandleGrowling();
        }

        HandleBiteTimer();

        if(!IsNightState() && state != State.attacking && state != State.growling && state != State.barking) {
            CheckNightInCamp();
        }

        if(IsNightState()) {
            closestIncomingCreature = CheckClosestIncomingCreature();
        }

        distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        distanceToStickWithPlayerTarget = Mathf.Abs(stickWithPlayerMoveTarget.x - transform.position.x);

        switch (state) {
            case State.stay:

                Roam(stayPointToRoamAround);

                break;

            case State.stayAtCamp:

                distanceToCamp = Mathf.Abs(transform.position.x) - Mathf.Abs(CampZoneManager.Instance.GetClosestExteriorZoneLimit(transform.position).x);

                if(distanceToCamp > distanceToRunToCamp && CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
                    ChangeState(State.runToCamp);
                    return;
                }

                RoamInCamp();

            break;

            case State.runToCamp:

                distanceToCamp = Mathf.Abs(transform.position.x) - Mathf.Abs(CampZoneManager.Instance.GetClosestExteriorZoneLimit(transform.position).x);

                if (distanceToCamp < distanceToRunToCamp && !CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
                    ChangeState(State.stayAtCamp);
                    return;
                }

            break;

            case State.idle:

                Roam(Player.Instance.transform.position);
                
                if (distanceToPlayer > distanceToWalkToPlayerWhenStickingAround) {
                    ChangeState(State.walkWithPlayer);
                }

                break;

            case State.nightInCampIdle:

                if(closestIncomingCreature != null) {
                    ChangeState(State.nightInCampRunToClosestCreature);
                }

                break;

            case State.nightInCampRunToClosestCreature:

                if (closestIncomingCreature == null) {
                    ChangeState(State.nightInCampIdle);
                    return;
                }

                HeadToBarkAtCampZoneLimit();

                break;

            case State.walkWithPlayer:
                StickWithPlayer();

                if (distanceToStickWithPlayerTarget < distanceToPlayerToRoamWhenStickingAround) {
                    ChangeState(State.idle);
                }

                if (distanceToStickWithPlayerTarget > distanceToRunToPlayerWhenStickingAround) {
                    ChangeState(State.runWithPlayer);
                }

                break;

            case State.runWithPlayer:
                StickWithPlayer();

                if (distanceToStickWithPlayerTarget < distanceToWalkToPlayerWhenStickingAround && !playerRunning) {
                    ChangeState(State.walkWithPlayer);
                }

                if (distanceToStickWithPlayerTarget < distanceToPlayerToRoamWhenStickingAround) {
                    ChangeState(State.idle);
                }

                break;

            case State.growling:

                if(PlayerIsTooFar() && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
                    ChangeState(State.runWithPlayer);
                    dogJustStoppedGrowling = true;
                    return;
                }

                CheckCreaturesInBarkRange();


                break;

            case State.barking:

                CheckCreaturesInBarkRange();

                if (hasBiteUnlocked) {
                    HandleBarkToAttack();
                }

                if (PlayerIsTooFar() && DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
                    ChangeState(State.runWithPlayer);
                    dogJustStoppedGrowling = true;
                    return;
                }

            break;

            case State.nightInCampGrowlAtIncomingCreature:

                if (closestIncomingCreature == null) {
                    ChangeState(State.nightInCampIdle);
                    return;
                }

                Creature newClosestIncomingCreature = CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(transform.position, 50f, 0, true); ;
                if(newClosestIncomingCreature != closestIncomingCreature) {
                    ChangeState(State.nightInCampIdle);
                    return;
                }

                if (hasBiteUnlocked) {
                    HandleBarkToAttack();
                }

                break;

            case State.attacking:
                AttackingStateUpdate();
                break;

        }
    }

    public bool IsNightState() {
        return state == State.nightInCampIdle || state == State.nightInCampGrowlAtIncomingCreature || state == State.nightInCampRunToClosestCreature || state == State.attacking || state == State.growling || state == State.barking;
    }

    public void SetIdleBehaviorState(State state) {
        currentBehaviorIdleState = state;
        ChangeState(state);
    }

    public void SetState(State state) {
        ChangeState(state);
    }

    protected virtual void ChangeState(State newState) {
        if (state == newState) return;
        RandomizeDistanceVariables();
        //Debug.Log("ChangeState " + newState);
        dogMovement.SetMoveTarget(transform.position);
        hasSetSpeed = false;

        if (newState == State.runWithPlayer || newState == State.runToCamp || newState == State.attacking || newState == State.nightInCampRunToClosestCreature) {
            dogMovement.SetMoveSpeed(runMoveSpeed);
            running = true;
            hasSetSpeed = true;
        } else {
            running = false;
        }

        if (newState == State.walkWithPlayer || newState == State.stayAtCamp || newState == State.nightInCampIdle) {
            // Ajustez la cible de mouvement du chien uniquement après confirmation
            float moveTargetX = Player.Instance.transform.position.x + distanceInFrontOfPlayer * lastPlayerDirection;
            stickWithPlayerMoveTarget = new Vector3(moveTargetX, 0, 0);

            dogMovement.SetMoveSpeed(walkMoveSpeed);
            hasSetSpeed = true;
        }

        if (newState == State.stay) {
            stayPointToRoamAround = transform.position;
        }

        roamTimer = roamChangeDestionationRate;

        previousState = state;
        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void AttackingStateUpdate() {
        if (biteStarted) return;
        if (closestCreature == null) {
            ChangeState(State.runWithPlayer);
            return;
        }

        HeadToAttackClosestCreature();
    }

    protected void HeadToBarkAtCampZoneLimit() {
        float destinationPositionX = CampZoneManager.Instance.GetMaxZoneLimit() - 4f;

        if(closestIncomingCreature.transform.position.x < 0) {
            destinationPositionX = CampZoneManager.Instance.GetMinZoneLimit() + 4f;
        }

        Vector3 destinationPosition = new Vector3(destinationPositionX, 0, 0);
        dogMovement.SetMoveTarget(destinationPosition);

        if((Mathf.Abs(transform.position.x - destinationPosition.x)) < .2f) {
            ChangeState(State.nightInCampGrowlAtIncomingCreature);
            playerStickedAroundTimer = 0;
        }
    }

    protected Creature CheckClosestIncomingCreature() {
        float xPositionToCheckFrom = transform.position.x;
        float minCampCenterLimit = CampZoneManager.Instance.GetCampCenterMinLimit();
        float maxCampCenterLimit = CampZoneManager.Instance.GetCampCenterMaxLimit();

        if (Mathf.Abs(Player.Instance.transform.position.x - maxCampCenterLimit) < 4f) {
            playerStickedAroundTimer += Time.deltaTime;
            if(playerStickedAroundTimer > 3f) {
                xPositionToCheckFrom = minCampCenterLimit;
            }
        }
        if (Mathf.Abs(Player.Instance.transform.position.x - minCampCenterLimit) < 4f) {
            playerStickedAroundTimer += Time.deltaTime;
            if (playerStickedAroundTimer > 3f) {
                xPositionToCheckFrom = maxCampCenterLimit;
            }
        }

        Vector3 positionToCheckFrom = new Vector3(xPositionToCheckFrom, 0, 0);
        return CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(positionToCheckFrom, 100f, 0, true);
    }

    protected void CheckNightInCamp() {
        if (isHubScene) return;
        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) return;
        if (currentBehaviorIdleState == State.stay) return;

        if(CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            SetIdleBehaviorState(State.nightInCampIdle);
        }
    }

    protected void RandomizeDistanceVariables() {
        distanceInFrontOfPlayer = UnityEngine.Random.Range(0, distanceToWalkToPlayerWhenStickingAroundReference);
        distanceToWalkToPlayerWhenStickingAround = UnityEngine.Random.Range(0, distanceInFrontOfPlayer);
    }

    protected void HandleGrowling() {
        if (state == State.attacking) return;

        if (dogJustStoppedGrowling) {
            dogJustStoppedGrowlingTimer -= Time.deltaTime;

            if (dogJustStoppedGrowlingTimer < 0) {
                dogJustStoppedGrowling = false;
                dogJustStoppedGrowlingTimer = dogJustStoppedGrowlingTime;
            }
        }
        else {
            CheckCreaturesInGrowlRange();
        }

    }

    protected virtual void HandleBarkToAttack() {
        barkingTimer += Time.deltaTime;
        if (closestCreature == null || closestCreature.transform.position.y > maxBiteDistance_GermanShepherd) return;

        if (barkingTimer > barkTimeToAttack && biteReady) {
            barkingTimer = 0;
            biteStarted = false;
            ChangeState(State.attacking);
        }
    }

    protected virtual void HandleBiteTimer() {
        if (!hasBiteUnlocked) return;

        if (!biteReady) {
            biteTimer -= Time.deltaTime;
            if(biteTimer < 0) {
                biteTimer = GetBiteCooldown();
                biteReady = true;
            }
        }
    }
    protected void PlayerMovement_OnPlayerRunStarted(object sender, EventArgs e) {
        playerRunning = true;
        if (state == State.walkWithPlayer) {
            float randomTimer = UnityEngine.Random.Range(0, 2f);
            StartCoroutine(StartRunningWithPlayerAfterDelay(randomTimer));
        }
    }
    protected void PlayerMovement_OnPlayerRunStopped(object sender, EventArgs e) {
        playerRunning = false;
    }
    protected IEnumerator StartRunningWithPlayerAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        ChangeState(State.runWithPlayer);
    }

    protected void Roam(Vector3 pointToRoamAround) {
        if (!hasSetSpeed) {
            dogMovement.SetMoveSpeed(walkMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestionationRate;
            RoamBehavior.RoamAroundPoint(dogMovement, roamRadius, pointToRoamAround, false);
        }
    }

    protected void RoamInCamp() {

        if (!hasSetSpeed) {
            dogMovement.SetMoveSpeed(walkMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestionationRate;

            RoamBehavior.RoamInCampCenter(dogMovement);
        }
    }

    protected bool PlayerIsTooFar() {
        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);

        if(distanceToPlayer > maxDistanceToPlayer) {
            return true;
        } else {
            return false;
        }

    }

    protected void StickWithPlayer() {
        float currentPlayerDirection = PlayerMovement.Instance.GetLastMoveDir();

        // Vérifiez si la direction a changé
        if (currentPlayerDirection != lastPlayerDirection) {
            changeDirectionTimer += Time.deltaTime;
            if (changeDirectionTimer >= requiredDirectionChangeDuration) {
                lastPlayerDirection = currentPlayerDirection;
                changeDirectionTimer = 0; // Réinitialisez le timer
                requiredDirectionChangeDuration = UnityEngine.Random.Range(.3f, 1f);
            }
        }
        else {
            changeDirectionTimer = 0; // Pas de changement, réinitialisation
        }


        // Ajustez la cible de mouvement du chien uniquement après confirmation
        float moveTargetX = Player.Instance.transform.position.x + distanceInFrontOfPlayer * lastPlayerDirection;
        stickWithPlayerMoveTarget = new Vector3(moveTargetX, 0, 0);

        dogMovement.SetMoveTarget(stickWithPlayerMoveTarget);
    }

    protected virtual void HeadToAttackClosestCreature() {
        float distanceToCreature = Mathf.Abs(transform.position.x - closestCreature.transform.position.x);

        if(dogAIType == Dog.DogType.GermanShepherd) {
            // Flying creature went back up ?
            if(closestCreature.transform.position.y > maxBiteDistance_GermanShepherd) {
                dogMovement.SetMoveTarget(transform.position);
                ChangeState(State.barking);
                return;
            }
        }

        if(distanceToCreature < biteRange && biteReady) {
            dogMovement.SetMoveTarget(transform.position);
            OnDogBite?.Invoke(this, EventArgs.Empty);

            StartCoroutine(ShootLaserOnClosestCreature(closestCreature));
            biteReady = false;
            biteStarted = true;
            return;

        } else {

            if(!biteStarted) {
                dogMovement.SetMoveTarget(closestCreature.transform.position);
            }

        }
    }

    protected virtual IEnumerator ShootLaserOnClosestCreature(Creature creature) {
        float standStillDelay = .2f;
        yield return new WaitForSeconds(standStillDelay);

        dogMovement.SetMoveTarget(closestCreature.transform.position);

        yield return new WaitForSeconds(biteAnimationDelay - standStillDelay);

        if (creature != null && !creature.GetDead()) {

            if (dogAIType == Dog.DogType.GermanShepherd) {
                // Flying creature went back up ?
                if (closestCreature.transform.position.y > maxBiteDistance_GermanShepherd) {
                    dogMovement.SetMoveTarget(transform.position);
                    ChangeState(State.barking);
                    biteStarted = false;
                    biteReady = true;
                    yield break;
                }
            }

            closestCreature.TakeDamage(GetBiteDamage(), transform);

        } else {

            biteReady = true;
        }

        dogMovement.SetMoveTarget(transform.position);
        float endBiteAnimationDelay = .4f;
        yield return new WaitForSeconds(endBiteAnimationDelay);

        biteStarted = false;

        ChangeState(State.walkWithPlayer);
    }

    protected void CheckCreaturesInGrowlRange() {
        closestCreature = creatureDetectionCollider.GetClosestCreature();
        bool ambushClose = creatureDetectionCollider.AmbushSpawnersInDetectionCollider();

        bool ambushOrCreatureClose = closestCreature || ambushClose;

        if (ambushOrCreatureClose && state != State.growling && state != State.barking) {


            ChangeState(State.growling);


        } else {
            if(state == State.growling && !ambushOrCreatureClose) {
                ChangeState(currentBehaviorIdleState);
            }
        }
    }

    protected void CheckCreaturesInBarkRange() {
        bool ambushClose = creatureDetectionCollider.AmbushSpawnersInDetectionCollider();
        if (closestCreature == null && !ambushClose) {
            ChangeState(currentBehaviorIdleState);
            return;
        }

        float closestCreatureDistanceToPlayer = 0;
        float closestCreatureDistanceToDog = 0;
        if (closestCreature != null) {
            closestCreatureDistanceToPlayer = (creatureDetectionCollider.GetClosestCreature().transform.position.x - Player.Instance.transform.position.x);
            closestCreatureDistanceToDog = creatureDetectionCollider.GetClosestCreatureDistance();
        }

        if ((closestCreatureDistanceToDog < creatureBarkDistanceToDog) || closestCreatureDistanceToPlayer < creatureBarkDistanceToPlayer) {
            ChangeState(State.barking);
        }
        else {
            ChangeState(State.growling);
        }
       
    }

    public void InvokeOnDogBite() {
        OnDogBite?.Invoke(this, EventArgs.Empty);
    }

    public void SetInitialPosition(Vector3 position) {
        stickWithPlayerMoveTarget = position;

    }

    public Creature GetClosestCreature() {
        return closestCreature;
    }
    public Creature GetClosestIncomingCreature() {
        return closestIncomingCreature;
    }
    public State GetState() {
        return state;
    }

    public Dog.DogType GetDogAIType() {
        return dogAIType;
    }

    public bool GetRunning() {
        return running;
    }
}
