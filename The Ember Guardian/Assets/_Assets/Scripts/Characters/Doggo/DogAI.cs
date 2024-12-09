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
        followingPlayer,
        walkWithPlayer,
        runWithPlayer,
        growling,
        barking,
    }

    private State state;
    private State currentBehaviorIdleState;
    private MobMovement dogMovement;
    [SerializeField] private WorkerDetectionCollider creatureDetectionCollider;
    private Creature closestCreature;

    private Vector3 stickWithPlayerMoveTarget;
    private Vector3 stayPointToRoamAround;
    private bool hasSetSpeed;
    private bool readyToMove;

    private float roamRadius = 5f;
    private float roamTimer;
    private float roamChangeDestionationRate = 10f;

    private float walkMoveSpeed = 1.5f;
    private float runMoveSpeed = 5f;

    private float distanceToPlayer;
    private float distanceToCamp;
    private float distanceToStickWithPlayerTarget;
    private float distanceInFrontOfPlayer = 4f;
    private float minDistanceToPlayer = 1f;
    private float maxDistanceToPlayer = 10f;
    private float distanceToRunToCamp = 5f;

    private float distanceToPlayerToRoamWhenStickingAround = 1f;
    private float distanceToRunToPlayerWhenStickingAround = 8f;
    private float distanceToWalkToPlayerWhenStickingAround = 5f;

    private float creatureBarkDistance = 7f;

    private bool playerRunning;
    private float lastPlayerDirection;
    private float changeDirectionTimer;
    private float requiredDirectionChangeDuration = .5f;

    public event EventHandler OnStateChanged;

    private void Awake() {
        dogMovement = GetComponent<MobMovement>();
    }

    private void Start() {
        PlayerMovement.Instance.OnPlayerRunStarted += PlayerMovement_OnPlayerRunStarted;
        PlayerMovement.Instance.OnPlayerRunStopped += PlayerMovement_OnPlayerRunStopped;

        currentBehaviorIdleState = State.stay;
        stayPointToRoamAround = transform.position;
        roamTimer = roamChangeDestionationRate;
    }

    private void Update() {

        CheckCreaturesInGrowlRange();

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

                CheckCreaturesInBarkRange();

                break;

            case State.barking:

                CheckCreaturesInBarkRange();

            break;

        }
    }

    public void SetBaseState(State state) {
        ChangeState(state);
        currentBehaviorIdleState = state;
    }

    private void ChangeState(State newState) {
        if (state == newState) return;

        Debug.Log(newState);

        dogMovement.SetMoveTarget(transform.position);
        hasSetSpeed = false;

        if (newState == State.runWithPlayer || newState == State.runToCamp) {
            dogMovement.SetMoveSpeed(runMoveSpeed);
            hasSetSpeed = true;
        }

        if (newState == State.walkWithPlayer || newState == State.stayAtCamp) {
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

        if (!readyToMove) return;
        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestionationRate;
            RoamBehavior.RoamAroundPoint(dogMovement, roamRadius, pointToRoamAround);
        }
    }

    private void RoamInCamp() {

        if (!hasSetSpeed) {
            dogMovement.SetMoveSpeed(walkMoveSpeed);
            hasSetSpeed = true;
        }

        if (!readyToMove) return;
        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestionationRate;

            RoamBehavior.RoamInCampCenter(dogMovement);
        }
    }

    private void CatchUpWithPlayer() {
        if (!readyToMove) return;

        dogMovement.SetMoveTarget(Player.Instance.transform.position);
    }

    private void StickWithPlayer() {
        if (!readyToMove) return;

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

    public void SetReadyToMove(bool readyToMove) {
        this.readyToMove = readyToMove;
    }

    private void CheckCreaturesInGrowlRange() {
        closestCreature = creatureDetectionCollider.GetClosestCreature();

        if (closestCreature != null && state != State.growling && state != State.barking) {

            ChangeState(State.growling);

        } else {
            if(state == State.growling && closestCreature == null) {
                ChangeState(currentBehaviorIdleState);
            }
        }
    }

    private void CheckCreaturesInBarkRange() {
        if (creatureDetectionCollider.GetClosestCreatureDistance() < creatureBarkDistance) {
            ChangeState(State.barking);
        } else {
            ChangeState(State.growling);
        }
    }

    public Creature GetClosestCreature() {
        return closestCreature;
    }

    public State GetState() {
        return state;
    }
}
