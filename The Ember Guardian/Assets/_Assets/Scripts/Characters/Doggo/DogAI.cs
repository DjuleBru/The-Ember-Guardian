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
        followingPlayer,
        walkWithPlayer,
        runWithPlayer,
        growling,
        barking,
        attacking,
    }

    private State state;
    private State currentBehaviorIdleState;
    private MobMovement dogMovement;
    [SerializeField] private DogCreatureDetectionCollider creatureDetectionCollider;
    private Creature closestCreature;
    private Creature closestIncomingCreature;

    private Vector3 stickWithPlayerMoveTarget;
    private Vector3 stayPointToRoamAround;
    private bool hasSetSpeed;

    private float roamRadius = 5f;
    private float roamTimer;
    private float roamChangeDestionationRate = 10f;

    private float walkMoveSpeed = 1.5f;
    private float runMoveSpeed = 6f;

    private float distanceToPlayer;
    private float distanceToCamp;
    private float distanceToStickWithPlayerTarget;
    private float distanceInFrontOfPlayer = 6f;
    private float minDistanceToPlayer = 1f;
    private float maxDistanceToPlayer = 10f;
    private float distanceToRunToCamp = 5f;

    private float distanceToPlayerToRoamWhenStickingAround = 1f;
    private float distanceToRunToPlayerWhenStickingAround = 12f;
    private float distanceToWalkToPlayerWhenStickingAroundReference = 10f;
    private float distanceToWalkToPlayerWhenStickingAround = 6f;

    private float creatureBarkDistanceToDog = 7f;
    private float creatureBarkDistanceToPlayer = 10f;

    private bool playerRunning;
    private float lastPlayerDirection;
    private float changeDirectionTimer;
    private float requiredDirectionChangeDuration = .5f;

    private bool dogJustStoppedGrowling;
    private float dogJustStoppedGrowlingTimer;
    private float dogJustStoppedGrowlingTime = 3f;

    private float barkingTimer;
    private float barkTimeToAttack = 1.5f;

    private bool hasBiteUnlocked;
    private bool biteStarted;
    private bool biteReady;
    private bool isHubScene;
    private float biteTimer;
    private float biteAnimationDelay = .5f;
    private float biteCooldown;
    private int biteDamage;

    public event EventHandler OnStateChanged;
    public event EventHandler OnDogBite;

    private void Awake() {
        dogMovement = GetComponent<MobMovement>();
    }

    private void Start() {
        PlayerMovement.Instance.OnPlayerRunStarted += PlayerMovement_OnPlayerRunStarted;
        PlayerMovement.Instance.OnPlayerRunStopped += PlayerMovement_OnPlayerRunStopped;

        currentBehaviorIdleState = Dog.Instance.GetIdleState();
        ChangeState(Dog.Instance.GetInitialState());
        stickWithPlayerMoveTarget = transform.position;
        stayPointToRoamAround = transform.position;
        roamTimer = roamChangeDestionationRate;

        hasBiteUnlocked = DogStats.Instance.GetBiteAbilityUnlocked();
        biteCooldown = DogStats.Instance.GetBiteCooldown();
        biteDamage = DogStats.Instance.GetBiteDamage();

        isHubScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;

        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        }
    }

    private void DayNightManager_OnDuskStart(object sender, EventArgs e) {
        SetIdleBehaviorState(State.idle);
    }

    private void Update() {
        HandleGrowling();

        if (hasBiteUnlocked) {
            HandleBiteTimer();
        }

        if(!IsNightState() && state != State.attacking && state != State.growling && state != State.barking) {
            CheckNightInCamp();
        }

        if(IsNightState()) {
            CheckClosestIncomingCreature();
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

                if(PlayerIsTooFar()) {
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

                if (PlayerIsTooFar()) {
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

                if (PlayerIsTooFar() || closestCreature == null) {
                    ChangeState(State.runWithPlayer);
                    return;
                }

                HeadToAttackClosestCreature();
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

    private void ChangeState(State newState) {
        if (state == newState) return;
        RandomizeDistanceVariables();

        dogMovement.SetMoveTarget(transform.position);
        hasSetSpeed = false;

        if (newState == State.runWithPlayer || newState == State.runToCamp || newState == State.attacking || newState == State.nightInCampRunToClosestCreature) {
            dogMovement.SetMoveSpeed(runMoveSpeed);
            hasSetSpeed = true;
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

        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HeadToBarkAtCampZoneLimit() {
        float destinationPositionX = CampZoneManager.Instance.GetMaxZoneLimit();

        if(closestIncomingCreature.transform.position.x < 0) {
            destinationPositionX = CampZoneManager.Instance.GetMinZoneLimit();
        }

        Vector3 destinationPosition = new Vector3(destinationPositionX, 0, 0);
        dogMovement.SetMoveTarget(destinationPosition);

        if((Mathf.Abs(transform.position.x - destinationPosition.x)) < 1f) {
            ChangeState(State.nightInCampGrowlAtIncomingCreature);
        }
    }

    private void CheckClosestIncomingCreature() {
        closestIncomingCreature = CreaturesManager.Instance.GetClosestCreatureInRadiusSmart(transform.position, 100f, 0, true);
    }

    private void CheckNightInCamp() {
        if (isHubScene) return;
        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) return;

        if(CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            SetIdleBehaviorState(State.nightInCampIdle);
        }
    }

    private void RandomizeDistanceVariables() {
        distanceInFrontOfPlayer = UnityEngine.Random.Range(0, distanceToWalkToPlayerWhenStickingAroundReference);
        distanceToWalkToPlayerWhenStickingAround = UnityEngine.Random.Range(0, distanceInFrontOfPlayer);
    }

    private void HandleGrowling() {
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

    private void HandleBarkToAttack() {
        barkingTimer += Time.deltaTime;
        if (closestCreature == null || closestCreature.transform.position.y > 2f) return;

        if (barkingTimer > barkTimeToAttack && biteReady) {
            barkingTimer = 0;
            ChangeState(State.attacking);
        }
    }

    private void HandleBiteTimer() {
        if(!biteReady) {
            biteTimer -= Time.deltaTime;
            if(biteTimer < 0) {
                biteTimer = biteCooldown;
                biteReady = true;
            }
        }
    }
    private void PlayerMovement_OnPlayerRunStarted(object sender, EventArgs e) {
        playerRunning = true;
        if (state == State.walkWithPlayer) {
            float randomTimer = UnityEngine.Random.Range(0, 2f);
            StartCoroutine(StartRunningWithPlayerAfterDelay(randomTimer));
        }
    }
    private void PlayerMovement_OnPlayerRunStopped(object sender, EventArgs e) {
        playerRunning = false;
    }
    private IEnumerator StartRunningWithPlayerAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        ChangeState(State.runWithPlayer);
    }

    private void Roam(Vector3 pointToRoamAround) {
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

    private void RoamInCamp() {

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

    private bool PlayerIsTooFar() {
        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);

        if(distanceToPlayer > maxDistanceToPlayer) {
            return true;
        } else {
            return false;
        }

    }

    private void StickWithPlayer() {
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

    private void HeadToAttackClosestCreature() {
        float distanceToCreature = Mathf.Abs(transform.position.x - closestCreature.transform.position.x);
        float biteRange = 2.5f;

        if(distanceToCreature < biteRange && biteReady) {
            dogMovement.SetMoveTarget(transform.position);
            OnDogBite?.Invoke(this, EventArgs.Empty);

            StartCoroutine(AttackClosestCreature(closestCreature));
            biteReady = false;
            biteStarted = true;
            return;

        } else {

            if(!biteStarted) {
                dogMovement.SetMoveTarget(closestCreature.transform.position);
            }

        }
    }

    private IEnumerator AttackClosestCreature(Creature creature) {
        float standStillDelay = .2f;
        yield return new WaitForSeconds(standStillDelay);

        dogMovement.SetMoveTarget(closestCreature.transform.position);

        yield return new WaitForSeconds(biteAnimationDelay - standStillDelay);

        if (creature != null) {
            closestCreature.TakeDamage(biteDamage, transform);
        }

        dogMovement.SetMoveTarget(transform.position);
        float endBiteAnimationDelay = .4f;
        yield return new WaitForSeconds(endBiteAnimationDelay);

        biteStarted = false;

        ChangeState(State.walkWithPlayer);
    }

    private void CheckCreaturesInGrowlRange() {
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

    private void CheckCreaturesInBarkRange() {
        if(closestCreature == null) {
            ChangeState(currentBehaviorIdleState);
            return;
        }

        float closestCreatureDistanceToPlayer = (creatureDetectionCollider.GetClosestCreature().transform.position.x - Player.Instance.transform.position.x);
        float closestCreatureDistanceToDog = creatureDetectionCollider.GetClosestCreatureDistance();

        if ((closestCreatureDistanceToDog < creatureBarkDistanceToDog) || closestCreatureDistanceToPlayer < creatureBarkDistanceToPlayer) {
            ChangeState(State.barking);
        } else {
            ChangeState(State.growling);
        }
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
}
