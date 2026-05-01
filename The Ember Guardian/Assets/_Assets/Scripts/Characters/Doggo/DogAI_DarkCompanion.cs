using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI_DarkCompanion : DogAI
{
    [SerializeField] private ProjectileSO laserProjectileSO;
    [SerializeField] private Transform laserContinuousStaticProjectilePrefab;
    [SerializeField] private Transform shockWaveStaticProjectile;
    [SerializeField] private Transform laserProjectileSpawnPosition;

    public enum AttackAbility {
        none,
        laserShot,
        laserContinuous,
        stomp,
    }
    private AttackAbility currentAttackAbility;

    private bool laserAbilityUnlocked;
    private bool stompAbilityUnlocked;
    private bool laserAbilityReady;
    private bool stompAbilityReady;
    private float laserAbilityTimer;
    private float stompAbilityTimer;
    private float biteLaserShotRange = 15f;
    private float laserAbilityRange = 6f;
    private float stompAbilityRange = 1f;

    private float minAttackDistance;

    private float decisionTimer;
    private float decisionInterval = 0.2f;

    private Creature targetCreature;

    public event EventHandler OnLaserAbilityStarted;
    public event EventHandler OnStompAbilityStarted;
    public event EventHandler OnLaserAbilityEnded;
    public event EventHandler OnStompAbilityEnded;

    protected override void Start() {
        base.Start();
        biteRange = biteLaserShotRange;

        DogStats.Instance.OnNewAbilityUnlocked += DogStats_OnNewAbilityUnlocked;

        laserAbilityUnlocked = DogStats.Instance.GetDarkCompanionLaserAbilityUnlocked();
        stompAbilityUnlocked = DogStats.Instance.GetDarkCompanionStompAbilityUnlocked();
        hasBiteUnlocked = hasBiteUnlocked || laserAbilityUnlocked || stompAbilityUnlocked;

        currentAttackAbility = AttackAbility.none;
    }

    private void DogStats_OnNewAbilityUnlocked(object sender, EventArgs e) {
        laserAbilityUnlocked = DogStats.Instance.GetDarkCompanionLaserAbilityUnlocked();
        stompAbilityUnlocked = DogStats.Instance.GetDarkCompanionStompAbilityUnlocked();
    }

    protected override void HandleBiteTimer() {
        if (currentAttackAbility != AttackAbility.none) return;

        if (stompAbilityUnlocked) {
            if (!stompAbilityReady) {
                stompAbilityTimer -= Time.deltaTime;

                if (stompAbilityTimer < 0) {
                    stompAbilityReady = true;
                    stompAbilityTimer = DogStats.Instance.GetDarkCompanionStompCooldown();
                }
            }
        }

        if (laserAbilityUnlocked) {
            if (!laserAbilityReady) {
                laserAbilityTimer -= Time.deltaTime;

                if (laserAbilityTimer < 0) {
                    laserAbilityReady = true;
                    laserAbilityTimer = DogStats.Instance.GetDarkCompanionLaserCooldown();
                }
            }
        }

        if (hasBiteUnlocked) {
            if (!biteReady) {
                biteTimer -= Time.deltaTime;

                if (biteTimer < 0) {
                    biteReady = true;
                    biteTimer = DogStats.Instance.GetDarkCompanionBiteCooldown();
                }
            }
        }
    }

    protected override void HandleBarkToAttack() {

        decisionTimer -= Time.deltaTime;
        if (decisionTimer > 0) return;
        decisionTimer = decisionInterval;

        var result = DecideBestTargetAndAbility();

        Creature bestTarget = result.Item1;
        AttackAbility ability = result.Item2;

        if (bestTarget == null) return;
        if (ability == AttackAbility.none) return;

        targetCreature = bestTarget;
        currentAttackAbility = ability;

        ApplyAbilitySettings(ability);

        ChangeState(State.attacking);
    }

    protected override void HeadToAttackClosestCreature() {
        targetCreature = closestCreature;

        if (currentAttackAbility == AttackAbility.stomp || currentAttackAbility == AttackAbility.laserContinuous) {
            targetCreature = creatureDetectionCollider.GetCreatureWithHighestLocalDensity();
        }

        if (targetCreature == null) {
            ChangeState(State.runWithPlayer);
            return;
        };

        float distanceToCreature = Mathf.Abs(transform.position.x - targetCreature.transform.position.x);

        if (distanceToCreature < biteRange && distanceToCreature > minAttackDistance) {
            dogMovement.SetMoveTarget(transform.position);

            if(currentAttackAbility == AttackAbility.laserShot) {
                InvokeOnDogBite();
                StartCoroutine(AttackAbilityCoroutine(targetCreature, .7f, .8f));
            }
            if (currentAttackAbility == AttackAbility.laserContinuous) {
                StartCoroutine(AttackAbilityCoroutine(targetCreature, 0, 2.9f));
            }

            if (currentAttackAbility == AttackAbility.stomp) {
                StartCoroutine(AttackAbilityCoroutine(targetCreature, 0, 1.8f));
            }

            biteReady = false;
            biteStarted = true;
            return;

        }
        else {
            if (!biteStarted) {

                if(currentAttackAbility == AttackAbility.stomp) {
                    dogMovement.SetMoveTarget(targetCreature.transform.position);
                }

                if (currentAttackAbility == AttackAbility.laserShot || currentAttackAbility == AttackAbility.laserContinuous) {
                    Vector3 moveTarget = targetCreature.transform.position;

                    float dirToCreature = targetCreature.transform.position.x - transform.position.x;

                    if(dirToCreature > 0) {
                        moveTarget.x -= (minAttackDistance+1);
                    } else {
                        moveTarget.x += (minAttackDistance+1);
                    }

                    dogMovement.SetMoveTarget(moveTarget);
                }

            }

        }
    }

    protected IEnumerator AttackAbilityCoroutine(Creature creature, float spawnProjectileDelay, float endAnimationDelay) {
        dogMovement.SetMoveTarget(transform.position);

        yield return new WaitForSeconds(spawnProjectileDelay);

        if(currentAttackAbility == AttackAbility.laserShot) {
            Projectile projectile = Instantiate(laserProjectileSO.projectilePrefab, laserProjectileSpawnPosition.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.ActivateAndInitialize(creature.GetProjectileTarget(), laserProjectileSO, Dog.Instance.transform, DogStats.Instance.GetDarkCompanionBiteDamage(), Vector3.zero, true);
        }

        if(currentAttackAbility == AttackAbility.laserContinuous) {
            OnLaserAbilityStarted?.Invoke(this, EventArgs.Empty);
            yield return new WaitForEndOfFrame();
            StaticProjectile_ContinuousDamage projectile = Instantiate(laserContinuousStaticProjectilePrefab, transform.position, Quaternion.identity).GetComponent<StaticProjectile_ContinuousDamage>();
            float watchDir = creature.transform.position.x - transform.position.x;
            projectile.InitializeContinuous(watchDir, null, DogStats.Instance.GetDarkCompanionLaserDamage(), DogStats.Instance.GetDarkCompanionLaserTickCooldown(), true, true);
        }

        if (currentAttackAbility == AttackAbility.stomp) {
            OnStompAbilityStarted?.Invoke(this, EventArgs.Empty);
            yield return new WaitForEndOfFrame();
            StaticProjectile projectile = Instantiate(shockWaveStaticProjectile, transform.position, Quaternion.identity).GetComponent<StaticProjectile>();
            float watchDir = creature.transform.position.x - transform.position.x;
            projectile.Initialize(watchDir, null, DogStats.Instance.GetDarkCompanionStompDamage(), true, false);
            projectile.InitializeStun(DogStats.Instance.GetDarkCompanionStompStunDuration());
        }

        yield return new WaitForSeconds(endAnimationDelay);

        biteStarted = false;
        ChangeState(State.walkWithPlayer);

        if (currentAttackAbility == AttackAbility.laserShot) {
            OnLaserAbilityEnded?.Invoke(this, EventArgs.Empty);
            biteReady = false;
        }

        if (currentAttackAbility == AttackAbility.laserContinuous) {
            OnLaserAbilityEnded?.Invoke(this, EventArgs.Empty);
            laserAbilityReady = false;
        }
        if (currentAttackAbility == AttackAbility.stomp) {
            OnStompAbilityEnded?.Invoke(this, EventArgs.Empty);
            stompAbilityReady = false;
        }

        currentAttackAbility = AttackAbility.none;
    }

    private (Creature, AttackAbility) DecideBestTargetAndAbility() {
        List<Creature> creatures = GetRelevantCreatures(5);

        Creature bestCreature = null;
        AttackAbility bestAbility = AttackAbility.none;
        float bestScore = float.MinValue;

        foreach (Creature creature in creatures) {

            float distance = Mathf.Abs(transform.position.x - creature.transform.position.x);
            bool isGrounded = creature.transform.position.y < .5f;

            Evaluate(creature, distance, isGrounded,
                AttackAbility.stomp, ref bestCreature, ref bestAbility, ref bestScore);

            Evaluate(creature, distance, isGrounded,
                AttackAbility.laserContinuous, ref bestCreature, ref bestAbility, ref bestScore);

            Evaluate(creature, distance, isGrounded,
                AttackAbility.laserShot, ref bestCreature, ref bestAbility, ref bestScore);
        }

        return (bestCreature, bestAbility);
    }

    private void Evaluate(Creature creature, float distance, bool isGrounded, AttackAbility ability, ref Creature bestCreature, ref AttackAbility bestAbility, ref float bestScore) {

        float score = -distance;

        if (ability == AttackAbility.stomp) {

            if (stompAbilityReady && isGrounded && distance < stompAbilityRange + 1f) {
                score += 50f;
            }
            else {
                return;
            }
        }

        if (ability == AttackAbility.laserContinuous) {

            if (!laserAbilityReady || !isGrounded) return;

            float s = 35f;

            if (HasNearbyGroup(creature, 4f)) {
                s += 40f;
            }

            s += Mathf.Clamp(10f - distance, 0f, 10f);

            score += s;
        }

        if (ability == AttackAbility.laserShot) {

            if (!biteReady || distance > biteLaserShotRange) return;

            score += 20f;
        }

        if (score > bestScore) {
            bestScore = score;
            bestCreature = creature;
            bestAbility = ability;
        }
    }

    private List<Creature> GetRelevantCreatures(int maxCount = 5) {

        List<Creature> all = creatureDetectionCollider.GetCreaturesInRange();

        all.RemoveAll(c => c == null);

        all.Sort((a, b) => {
            float da = Mathf.Abs(a.transform.position.x - transform.position.x);
            float db = Mathf.Abs(b.transform.position.x - transform.position.x);
            return da.CompareTo(db);
        });

        if (all.Count > maxCount) {
            all.RemoveRange(maxCount, all.Count - maxCount);
        }

        return all;
    }

    private bool HasNearbyGroup(Creature creature, float radius) {
        int count = 0;

        List<Creature> creatures = creatureDetectionCollider.GetCreaturesInRange();

        foreach (Creature other in creatures) {

            if (other == null) continue;
            if (other == creature) continue;

            float dist = Mathf.Abs(other.transform.position.x - creature.transform.position.x);

            if (dist < radius) {
                count++;

                if (count >= 2) {
                    return true; // early exit
                }
            }
        }

        return false;
    }

    private void ApplyAbilitySettings(AttackAbility ability) {

        if (ability == AttackAbility.stomp) {
            biteRange = stompAbilityRange;
            minAttackDistance = 0f;
        }
        else if (ability == AttackAbility.laserContinuous) {
            biteRange = laserAbilityRange;
            minAttackDistance = 3f;
        }
        else if (ability == AttackAbility.laserShot) {
            biteRange = biteLaserShotRange;
            minAttackDistance = 5f;
        }
    }

    public float GetWatchDir() {
        if (targetCreature == null) return 0;
        return targetCreature.transform.position.x - transform.position.x;
    }

}
