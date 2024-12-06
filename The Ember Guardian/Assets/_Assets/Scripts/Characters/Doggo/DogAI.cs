using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI : MonoBehaviour
{
    public enum State {
        stay,
        idle,
        runToPlayer,
        walkToPlayer,
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

    private Vector3 stickWithPlayerMoveTarget;
    private bool hasSetSpeed;
    private bool readyToMove;

    private float roamRadius = 5f;
    private float roamTimer;
    private float roamChangeDestionationRate = 10f;

    private float walkMoveSpeed = 1.5f;
    private float runMoveSpeed = 5f;

    private float distanceToPlayer;
    private float distanceToStickWithPlayerTarget;
    private float distanceInFrontOfPlayer = 3f;
    private float minDistanceToPlayer = 1f;
    private float maxDistanceToPlayer = 10f;

    private float minDistanceToPlayerWhenStickingAround = 1f;
    private float maxDistanceToPlayerWhenStickingAround = 5f;

    private float creatureBarkDistance = 7f;

    private float changeDirectionTimer;
    private float changeDirectionRate = .5f;

    public event EventHandler OnStateChanged;

    private void Awake() {
        dogMovement = GetComponent<MobMovement>();
        state = State.idle;
        roamTimer = roamChangeDestionationRate;
    }

    private void Start() {
        PlayerMovement.Instance.OnPlayerRunStarted += PlayerMovement_OnPlayerRunStarted;
        currentBehaviorIdleState = State.idle;
    }

    private void Update() {

        if(Input.GetKeyDown(KeyCode.Y)) {
            SetBaseState(State.walkWithPlayer);
            currentBehaviorIdleState = State.walkWithPlayer;
        }

        CheckCreaturesInGrowlRange();

        distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);

        float moveTargetX = Player.Instance.transform.position.x + distanceInFrontOfPlayer * PlayerMovement.Instance.GetLastMoveDir();
        stickWithPlayerMoveTarget = new Vector3(moveTargetX, 0, 0);
        distanceToStickWithPlayerTarget = Mathf.Abs(stickWithPlayerMoveTarget.x - transform.position.x);

        switch (state) {
            case State.stay:

                Roam(Player.Instance.transform.position);

                break;
            case State.idle:

                if (distanceToPlayer > maxDistanceToPlayer) {
                    ChangeState(State.runToPlayer);
                }
                else {
                    Roam(Player.Instance.transform.position);
                }

                break;

            case State.runToPlayer:

                CatchUpWithPlayer();

                if (distanceToPlayer < maxDistanceToPlayerWhenStickingAround) {
                    ChangeState(State.walkToPlayer);
                }

            break;

            case State.walkToPlayer:

                CatchUpWithPlayer();

                if (distanceToPlayer > maxDistanceToPlayer) {
                    ChangeState(State.runToPlayer);
                }

                if (distanceToPlayer < minDistanceToPlayer) {
                    ChangeState(State.idle);
                }

            break;

            case State.walkWithPlayer:
                StickWithPlayer();

                if (distanceToStickWithPlayerTarget > maxDistanceToPlayerWhenStickingAround) {
                    ChangeState(State.runWithPlayer);
                }

                break;

            case State.runWithPlayer:
                StickWithPlayer();

                if (distanceToStickWithPlayerTarget < minDistanceToPlayerWhenStickingAround) {
                    ChangeState(State.walkWithPlayer);
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

        dogMovement.SetMoveTarget(transform.position);
        hasSetSpeed = false;

        if (newState == State.runToPlayer || newState == State.runWithPlayer) {
            dogMovement.SetMoveSpeed(runMoveSpeed);
            hasSetSpeed = true;
        }

        if (newState == State.walkToPlayer || newState == State.walkWithPlayer) {
            dogMovement.SetMoveSpeed(walkMoveSpeed);
            hasSetSpeed = true;
        }

        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerMovement_OnPlayerRunStarted(object sender, EventArgs e) {
        if(state == State.walkWithPlayer) {
            float randomTimer = UnityEngine.Random.Range(0, 2f);
            StartCoroutine(StartRunningWithPlayerAfterDelay(randomTimer));
        }
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

    private void CatchUpWithPlayer() {
        if (!readyToMove) return;

        dogMovement.SetMoveTarget(Player.Instance.transform.position);
    }

    private void StickWithPlayer() {
        if (!readyToMove) return;

        dogMovement.SetMoveTarget(stickWithPlayerMoveTarget);
        changeDirectionTimer -= Time.deltaTime;
        if(changeDirectionTimer < 0) {
            changeDirectionTimer = changeDirectionRate;
        }
    }

    public void SetReadyToMove(bool readyToMove) {
        this.readyToMove = readyToMove;
    }

    private void CheckCreaturesInGrowlRange() {
        if (creatureDetectionCollider.CreaturesInDetectionCollider() && state != State.growling) {
            ChangeState(State.growling);
        } else {
            if(state == State.growling && !creatureDetectionCollider.CreaturesInDetectionCollider()) {
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

    public State GetState() {
        return state;
    }
}
