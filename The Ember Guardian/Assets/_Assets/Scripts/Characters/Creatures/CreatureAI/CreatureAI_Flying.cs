using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Flying : CreatureAI
{
    private float minAltitude = 2f;
    private float maxAltitude = 7f;

    [SerializeField] private float repositionCooldown; // Temps entre le repositionnement après une attaque
    private float repositionTimer; // Temps entre le repositionnement après une attaque
    private bool isRepositioning;

    private float playerYTargetMin = 1.3f;
    private float playerYTargetMax = 1.3f;

    private float yRandomizerTimer;
    private float yRandomizerRate = 2f;
    private float distanceToDropOnTarget;
    private float yOffset;

    protected override void Start() {
        base.Start();
        mobAttack.OnMobAttackHit += MobAttack_OnMobAttackHit;
        repositionCooldown = creature.GetCreatureSO().attackRate - .1f;

        minAltitude = creature.GetCreatureSO().flightMinAltitude;
        maxAltitude = creature.GetCreatureSO().flightMaxAltitude;
    }

    private void MobAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        isRepositioning = true;
        repositionTimer = repositionCooldown;
        roamTimer = 0;
        mobAttack.RemoveAttackTarget();
    }

    protected override void Update() {
        if (died) return;

        HandleAggroRecently();


        if (isRepositioning) {
            repositionTimer -= Time.deltaTime;
            RoamAroundPlayer();
            if (repositionTimer <= 0) {
                ChangeState(State.moveToTarget);
                isRepositioning = false; // Reprise du comportement normal
            }
            return;
        }

        switch (state) {

            case State.idle:

                if (detectedAttackTarget && !aggroedRecently) {
                    ChangeState(State.moveToTarget);
                }

                Roam();

                break;

            case State.walkingToFire:

                MoveTowardsFire();
                if (detectedAttackTarget && !aggroedRecently) {
                    ChangeState(State.moveToTarget);
                }

                break;

            case State.walkingToSpawner:

                MoveTowardsSpawner();
                if (detectedAttackTarget && !aggroedRecently) {
                    ChangeState(State.moveToTarget);
                }

                break;

            case State.moveToTarget:
                if (!detectedAttackTarget) {
                    if (creature.IsDayCreature()) {
                        ChangeState(State.walkingToSpawner);
                        return;
                    }
                    else {
                        ChangeState(State.walkingToFire);
                        return;
                    }
                }

                HeadToTarget();

                break;

            case State.attacking:

                if (!detectedAttackTarget) {
                    if (creature.IsDayCreature()) {
                        ChangeState(State.walkingToSpawner);
                    }
                    else {
                        ChangeState(State.walkingToFire);
                    }
                    return;
                }

                if (!CheckAttackTargetInRange() && !mobAttack.GetAttackStarted()) {
                    ChangeState(State.moveToTarget);
                    return;
                }

                break;
        }
    }

    protected override void MoveTowardsFire() {
        Vector3 targetDestination = new Vector3(0, 0, 0);

        float distanceToFireX = Mathf.Abs(transform.position.x - targetDestination.x);

        yRandomizerTimer -= Time.deltaTime;

        if (yRandomizerTimer <= 0) {
            yRandomizerTimer = yRandomizerRate;

            // Déterminer une altitude différente pour la mouche
            distanceToDropOnTarget = UnityEngine.Random.Range(1f, 4f);
            yOffset = Mathf.Sin(Time.time * 2f) * 0.5f + UnityEngine.Random.Range(minAltitude, maxAltitude);
        }

        if (distanceToFireX > distanceToDropOnTarget) {
            // Si la mouche est encore loin, reste à une altitude variable
            targetDestination.y += yOffset;

        }
        else {

            // Add y position randomized
            float yRandomized = UnityEngine.Random.Range(playerYTargetMin, playerYTargetMax);
            targetDestination.y += yRandomized;
        }

        creatureMovement.SetMoveTarget(targetDestination);
    }

    protected override void HeadToTarget() {
        if (attackTarget == null) return;

        Vector3 targetDestination = (attackTarget as MonoBehaviour).transform.position;

        float distanceToPlayerX = Mathf.Abs(transform.position.x - targetDestination.x);

        yRandomizerTimer -= Time.deltaTime;

        if(yRandomizerTimer <= 0) {
            yRandomizerTimer = yRandomizerRate;

            // Déterminer une altitude différente pour la mouche
            distanceToDropOnTarget = UnityEngine.Random.Range(1f, 3f);
            yOffset = Mathf.Sin(Time.time * 2f) * 0.5f + UnityEngine.Random.Range(minAltitude, maxAltitude);
        }

        if (distanceToPlayerX > distanceToDropOnTarget) {
            // Si la mouche est encore loin, reste à une altitude variable
            targetDestination.y += yOffset;

        } else {

            // Add y position randomized
            float yRandomized = UnityEngine.Random.Range(playerYTargetMin, playerYTargetMax);
            targetDestination.y += yRandomized;
        }


        if (Vector3.Distance(transform.position, targetDestination) < minAttackRange) {
            ChangeState(State.attacking);
            return;
        }

        if (Vector3.Distance(transform.position, targetDestination) < minAttackRange) {
            ChangeState(State.attacking);
        }

        creatureMovement.SetMoveTarget(targetDestination);
    }

    protected void RoamAroundPlayer() {

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;

            Vector3 positionToRoamAround = Player.Instance.transform.position;
            // Add y position randomized
            float yRandomized = UnityEngine.Random.Range(minAltitude, minAltitude*2);
            positionToRoamAround.y += yRandomized;

            RoamBehavior.RoamAroundPoint(creatureMovement, roamRadius, positionToRoamAround, true);
        }
    }
}
