using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Flying : CreatureAI
{
    private float minAltitude = 2f;
    private float maxAltitude = 7f;

    [SerializeField] private float distanceToDropOnTarget;
    [SerializeField] private float distanceToDropOnTargetRandomizer;
    [SerializeField] private float playerYTargetAltitude = 1.5f;
    [SerializeField] private float moveSpeedBuffWhenDropping = 1.5f;
    [SerializeField] private bool diveAttack;
    [SerializeField] private float diveAttackFollowTargetStrength = 2f;
    private float repositionCooldown; // Temps entre le repositionnement après une attaque
    protected float minRepositionDistanceToPlayer;
    protected float maxRepositionDistanceToPlayer;
    private bool diveAttackTargetSet;

    private float distanceToDropOnTargetRandomized;
    private float repositionTimer; // Temps entre le repositionnement après une attaque
    private bool isRepositioning;
    private bool droppingOnTarget;

    private Vector3 fireTargetDestination;

    private float yRandomizerTimer;
    private float yRandomizerRate = 2f;
    private float yOffset;

    private Vector3 lastPlayerHitPosition;
    private Vector3 diveAttackTargetDestination;

    public event EventHandler OnCreatureDropOnTarget;
    public event EventHandler OnCreatureDropOnTargetEnded;

    protected override void Start() {
        base.Start();
        creatureAttack.OnMobAttackHit += MobAttack_OnMobAttackHit;
        creatureAttack.OnMobAttack += CreatureAttack_OnMobAttack;
        repositionCooldown = creatureAttack.GetCurrentCreatureAttackSO().attackCooldown - .1f;
        roamChangeDestinationRate = repositionCooldown;

        float altitudeRandomizer = creature.GetCreatureSO().flightAltitudeRandomizer;
        float randomY = UnityEngine.Random.Range(0, altitudeRandomizer);
        minAltitude = creature.GetCreatureSO().flightMinAltitude + randomY;
        maxAltitude = creature.GetCreatureSO().flightMaxAltitude;

        minRepositionDistanceToPlayer = creature.GetCreatureSO().minRepositionDistanceToPlayer;
        maxRepositionDistanceToPlayer = creature.GetCreatureSO().maxRepositionDistanceToPlayer;
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
    protected virtual void CreatureAttack_OnMobAttack(object sender, EventArgs e) {
        if (diveAttack) {
            StartCoroutine(RepositionAfterAttackAfterDelay(.2f));
        }
    }

    private void MobAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        RepositionAfterAttack();
    }

    private void RepositionAfterAttack() {
        isRepositioning = true;
        repositionTimer = repositionCooldown;
        roamTimer = 0;
        creatureAttack.RemoveAttackTarget();
        lastPlayerHitPosition = Player.Instance.transform.position;

        if (diveAttack && droppingOnTarget) {
            droppingOnTarget = false;
            creatureMovement.ResetTempMoveSpeedBuffs();
        }

        diveAttackTargetSet = false;
    }

    private IEnumerator RepositionAfterAttackAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        RepositionAfterAttack();
    }

    protected override void MoveTowardsFire() {

        CheckDistanceToPlayerOrCampForMoveSpeed();

        float distanceToFireX = Mathf.Abs(transform.position.x - Fire.Instance.transform.position.x);

        yRandomizerTimer -= Time.deltaTime;

        if (yRandomizerTimer <= 0) {
            yRandomizerTimer = yRandomizerRate;

            // Recalcule le random pour la distance de drop
            distanceToDropOnTargetRandomized = distanceToDropOnTarget + UnityEngine.Random.Range(-distanceToDropOnTargetRandomizer, distanceToDropOnTargetRandomizer);

            // Calcule yOffset oscillant dans [minAltitude, maxAltitude]
            yOffset = Mathf.Sin(Time.time * 2f) * 0.5f + UnityEngine.Random.Range(minAltitude, maxAltitude);
        }

        // Gère l’altitude minimale pour éviter que la créature tombe trop bas
        if (transform.position.y < minAltitude) {
            // Force la créature à remonter au moins à minAltitude + un offset aléatoire
            fireTargetDestination.y += 1;
        }
        else {
            fireTargetDestination = Fire.Instance.transform.position;
            if (distanceToFireX > distanceToDropOnTargetRandomized) {
                // La cible est loin : vole à une altitude variable entre min et max altitude avec oscillation
                droppingOnTarget = false;
                fireTargetDestination.y += yOffset;
            }
            else {
                // La cible est proche : vole à hauteur fixe autour de playerYTargetAltitude
                fireTargetDestination.y += playerYTargetAltitude;

                if (diveAttack && !droppingOnTarget) {
                    droppingOnTarget = true;
                    OnCreatureDropOnTarget?.Invoke(this, EventArgs.Empty);
                    creatureMovement.BuffMoveSpeed(moveSpeedBuffWhenDropping);
                }
            }
        }

        creatureMovement.SetMoveTarget(fireTargetDestination);
    }

    protected override void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetDestination = (attackTarget as MonoBehaviour).transform.position;
        Vector3 playerPos = (attackTarget as MonoBehaviour).transform.position;

        float distanceToPlayerX = Mathf.Abs(transform.position.x - targetDestination.x);

        yRandomizerTimer -= Time.deltaTime;

        if(yRandomizerTimer <= 0) {
            yRandomizerTimer = yRandomizerRate;

            // Déterminer une altitude différente pour la mouche
            distanceToDropOnTargetRandomized = distanceToDropOnTarget + UnityEngine.Random.Range(-distanceToDropOnTargetRandomizer, distanceToDropOnTargetRandomizer);
            yOffset = Mathf.Sin(Time.time * 2f) * 0.5f + UnityEngine.Random.Range(minAltitude, maxAltitude);
        }

        // --- AVANT LA DIVE ---
        if (distanceToPlayerX > distanceToDropOnTargetRandomized) {
            // Si la mouche est encore loin, reste à une altitude variable
            droppingOnTarget = false;
            targetDestination.y += yOffset;

        } else {

            // Basse altitude proche du joueur
            // Add y position randomized
            targetDestination.y += playerYTargetAltitude;

            // On initialise le dive si pas encore lancé
            if (!diveAttackTargetSet) {
                diveAttackTargetSet = true;
                diveAttackTargetDestination = targetDestination;
            }

            if(diveAttack && !droppingOnTarget) {
                droppingOnTarget = true;
                OnCreatureDropOnTarget?.Invoke(this, EventArgs.Empty);
                creatureMovement.BuffMoveSpeed(moveSpeedBuffWhenDropping);
            }
        }

        // --- PENDANT LE DIVE ---
        if (diveAttack && diveAttackTargetSet) {

            // Suivi inertiel du joueur (seulement en X)
            Vector3 targetForLerp = new Vector3(
                playerPos.x,
                diveAttackTargetDestination.y,
                playerPos.z
            );

            diveAttackTargetDestination = Vector3.Lerp(
                diveAttackTargetDestination,
                targetForLerp,
                diveAttackFollowTargetStrength * Time.deltaTime
            );
        }


        Vector3 attackTargetPosition = targetDestination;
        if(diveAttack && diveAttackTargetSet) {
            attackTargetPosition = diveAttackTargetDestination;
        }

        if (Vector3.Distance(transform.position, targetDestination) < minAttackRange) {
            ChangeState(State.attacking);
            return;
        }

        creatureMovement.SetMoveTarget(attackTargetPosition);

        if (diveAttack && diveAttackTargetSet) {

            if (Vector3.Distance(transform.position, diveAttackTargetDestination) < minAttackRange) {
                RepositionAfterAttack();
                OnCreatureDropOnTargetEnded?.Invoke(this, EventArgs.Empty);
                return;
            }

        }

    }

    protected void RoamAroundLastPlayerHitPosition() {
      
        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;

            // Add y position randomized
            float yRandomized = UnityEngine.Random.Range(minAltitude, maxAltitude);
            Vector3 repositionDestination = lastPlayerHitPosition;
            repositionDestination.y += yRandomized;
            repositionDestination.y += playerYTargetAltitude;

            float xRandomized = UnityEngine.Random.Range(minRepositionDistanceToPlayer, maxRepositionDistanceToPlayer);
            if(UnityEngine.Random.value < 0.5) {
                xRandomized *= -1;
            }
            repositionDestination.x += xRandomized;
            creatureMovement.SetMoveTarget(repositionDestination);
            //RoamBehavior.RoamAroundPoint(creatureMovement, roamRadius, lastPlayerHitPosition, true);
        }
    }
}
