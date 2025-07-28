using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Flying : CreatureAI
{
    private float minAltitude = 2f;
    private float maxAltitude = 7f;

    [SerializeField] private float repositionCooldown; // Temps entre le repositionnement après une attaque
    [SerializeField] private float distanceToDropOnTarget;
    [SerializeField] private float distanceToDropOnTargetRandomizer;
    private float distanceToDropOnTargetRandomized;
    private float repositionTimer; // Temps entre le repositionnement après une attaque
    private bool isRepositioning;

    private float playerYTargetAltitude = 1.5f;

    private float yRandomizerTimer;
    private float yRandomizerRate = 2f;
    private float yOffset;

    private Vector3 lastPlayerHitPosition;

    protected override void Start() {
        base.Start();
        creatureAttack.OnMobAttackHit += MobAttack_OnMobAttackHit;
        repositionCooldown = creatureAttack.GetCurrentCreatureAttackSO().attackCooldown - .1f;

        minAltitude = creature.GetCreatureSO().flightMinAltitude;
        maxAltitude = creature.GetCreatureSO().flightMaxAltitude;
    }

    private void MobAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        isRepositioning = true;
        repositionTimer = repositionCooldown;
        roamTimer = 0;
        creatureAttack.RemoveAttackTarget();
        lastPlayerHitPosition = transform.position;
    }

    protected override void Update() {
        if (died) return;

        HandleAggroRecently();
        if (hasRangedAndMeleeAttack) {
            CheckAttackChange();
        }

        if (isRepositioning) {
            repositionTimer -= Time.deltaTime;
            RoamAroundLastPlayerHitPosition();
            if (repositionTimer <= 0) {
                ChangeState(State.moveToTarget);
                isRepositioning = false; // Reprise du comportement normal
            }
            return;
        }

        StateSwitch();
    }

    protected override void MoveTowardsFire() {

        CheckDistanceToPlayerOrCampForMoveSpeed();
        Vector3 targetDestination = new Vector3(0, 0, 0);

        float distanceToFireX = Mathf.Abs(transform.position.x - targetDestination.x);

        yRandomizerTimer -= Time.deltaTime;

        if (yRandomizerTimer <= 0) {
            yRandomizerTimer = yRandomizerRate;

            // Déterminer une altitude différente pour la mouche
            distanceToDropOnTargetRandomized = distanceToDropOnTarget + UnityEngine.Random.Range(-distanceToDropOnTargetRandomizer, distanceToDropOnTargetRandomizer);
            yOffset = Mathf.Sin(Time.time * 2f) * 0.5f + UnityEngine.Random.Range(minAltitude, maxAltitude);
        }

        if (distanceToFireX > distanceToDropOnTargetRandomized) {
            // Si la mouche est encore loin, reste à une altitude variable
            targetDestination.y += yOffset;

        }
        else {

            // Add y position randomized
            targetDestination.y += playerYTargetAltitude;
        }

        creatureMovement.SetMoveTarget(targetDestination);
    }

    protected override void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetDestination = (attackTarget as MonoBehaviour).transform.position;

        float distanceToPlayerX = Mathf.Abs(transform.position.x - targetDestination.x);

        yRandomizerTimer -= Time.deltaTime;

        if(yRandomizerTimer <= 0) {
            yRandomizerTimer = yRandomizerRate;

            // Déterminer une altitude différente pour la mouche
            distanceToDropOnTargetRandomized = distanceToDropOnTarget + UnityEngine.Random.Range(-distanceToDropOnTargetRandomizer, distanceToDropOnTargetRandomizer);
            yOffset = Mathf.Sin(Time.time * 2f) * 0.5f + UnityEngine.Random.Range(minAltitude, maxAltitude);
        }

        if (distanceToPlayerX > distanceToDropOnTargetRandomized) {
            // Si la mouche est encore loin, reste à une altitude variable
            targetDestination.y += yOffset;

        } else {

            // Add y position randomized
            targetDestination.y += playerYTargetAltitude;
        }
        if (Vector3.Distance(transform.position, targetDestination) < minAttackRange) {
            ChangeState(State.attacking);
            return;
        }

        creatureMovement.SetMoveTarget(targetDestination);
    }

    protected void RoamAroundLastPlayerHitPosition() {

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;

            // Add y position randomized
            float yRandomized = UnityEngine.Random.Range(minAltitude, minAltitude*2);
            lastPlayerHitPosition.y += yRandomized;

            RoamBehavior.RoamAroundPoint(creatureMovement, roamRadius, lastPlayerHitPosition, true);
        }
    }
}
