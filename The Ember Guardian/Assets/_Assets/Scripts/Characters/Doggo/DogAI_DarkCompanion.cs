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
    private float laserAbilityCooldown;
    private float stompAbilityCooldown;
    private float laserAbilityTimer;
    private float stompAbilityTimer;
    private float biteLaserShotRange = 15f;
    private int laserAbilityTickDamage;
    private int stompAbilityDamage;
    private float laserAbilityTickCooldown;
    private float laserAbilityRange = 6f;
    private float stompAbilityRange = 1f;
    private float stompStunDuration;

    private float minAttackDistance;

    private Creature targetCreature;

    public event EventHandler OnLaserAbilityStarted;
    public event EventHandler OnStompAbilityStarted;
    public event EventHandler OnLaserAbilityEnded;
    public event EventHandler OnStompAbilityEnded;

    protected override void Start() {
        base.Start();
        biteRange = biteLaserShotRange;

        laserAbilityUnlocked = DogStats.Instance.GetDarkCompanionLaserAbilityUnlocked();
        stompAbilityUnlocked = DogStats.Instance.GetDarkCompanionStompAbilityUnlocked();
        hasBiteUnlocked = hasBiteUnlocked || laserAbilityUnlocked || stompAbilityUnlocked;

        laserAbilityCooldown = DogStats.Instance.GetDarkCompanionLaserCooldown();
        stompAbilityCooldown = DogStats.Instance.GetDarkCompanionStompCooldown();

        laserAbilityTickDamage = DogStats.Instance.GetDarkCompanionLaserDamage();
        stompAbilityDamage = DogStats.Instance.GetDarkCompanionStompDamage();
        stompStunDuration = DogStats.Instance.GetDarkCompanionStompStunDuration();

        laserAbilityTickCooldown = DogStats.Instance.GetDarkCompanionLaserTickCooldown();

        currentAttackAbility = AttackAbility.none;
    }

    protected override void HandleBiteTimer() {

        if (stompAbilityUnlocked) {
            if (!stompAbilityReady) {
                stompAbilityTimer -= Time.deltaTime;
                if (stompAbilityTimer < 0 && currentAttackAbility == AttackAbility.none) {
                    stompAbilityReady = true;
                    stompAbilityTimer = stompAbilityCooldown;
                    biteRange = stompAbilityRange;
                    minAttackDistance = 0f;
                    currentAttackAbility = AttackAbility.stomp;
                }
            }
        }

        if (laserAbilityUnlocked) {
            if (!laserAbilityReady) {
                laserAbilityTimer -= Time.deltaTime;

                if (laserAbilityTimer < 0 && currentAttackAbility == AttackAbility.none) {
                    laserAbilityReady = true;
                    laserAbilityTimer = laserAbilityCooldown;
                    biteRange = laserAbilityRange;
                    minAttackDistance = 3f;
                    currentAttackAbility = AttackAbility.laserContinuous;
                }

            }

        }

        if (hasBiteUnlocked) {
            if (!biteReady) {
                biteTimer -= Time.deltaTime;
                if (biteTimer < 0 && currentAttackAbility == AttackAbility.none) {
                    biteTimer = biteCooldown;
                    biteReady = true;
                    minAttackDistance = 5f;
                    currentAttackAbility = AttackAbility.laserShot;
                }
            }
        }

    }

    protected override void HandleBarkToAttack() {
        if (closestCreature == null) return;

        if (stompAbilityReady && closestCreature.transform.position.y < 2f) {
            currentAttackAbility = AttackAbility.stomp;
            ChangeState(State.attacking);
            return;
        }

        if (laserAbilityReady) {
            currentAttackAbility = AttackAbility.laserContinuous;
            ChangeState(State.attacking);
            return;
        }

        if (biteReady && closestCreature.transform.position.y < 2f) {
            currentAttackAbility = AttackAbility.laserShot;
            ChangeState(State.attacking);
            return;
        }
    }

    protected override void HeadToAttackClosestCreature() {
        targetCreature = closestCreature;
        //Debug.Log("targetCreature " + targetCreature);
        if (targetCreature == null) return;

        if(currentAttackAbility == AttackAbility.stomp || currentAttackAbility == AttackAbility.laserContinuous) {
            targetCreature = creatureDetectionCollider.GetCreatureWithHighestLocalDensity();
        }
        float distanceToCreature = Mathf.Abs(transform.position.x - targetCreature.transform.position.x);

        //Debug.Log("distanceToCreature " + distanceToCreature);
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
            projectile.ActivateAndInitialize(creature.GetProjectileTarget(), laserProjectileSO, Dog.Instance.transform, biteDamage, Vector3.zero, true);
        }

        if(currentAttackAbility == AttackAbility.laserContinuous) {
            OnLaserAbilityStarted?.Invoke(this, EventArgs.Empty);
            yield return new WaitForEndOfFrame();
            StaticProjectile_ContinuousDamage projectile = Instantiate(laserContinuousStaticProjectilePrefab, transform.position, Quaternion.identity).GetComponent<StaticProjectile_ContinuousDamage>();
            float watchDir = creature.transform.position.x - transform.position.x;
            projectile.InitializeContinuous(watchDir, null, laserAbilityTickDamage, laserAbilityTickCooldown, true, true);
        }

        if (currentAttackAbility == AttackAbility.stomp) {
            OnStompAbilityStarted?.Invoke(this, EventArgs.Empty);
            yield return new WaitForEndOfFrame();
            StaticProjectile projectile = Instantiate(shockWaveStaticProjectile, transform.position, Quaternion.identity).GetComponent<StaticProjectile>();
            float watchDir = creature.transform.position.x - transform.position.x;
            projectile.Initialize(watchDir, null, stompAbilityDamage, true, false);
            projectile.InitializeStun(stompStunDuration);
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

    public float GetWatchDir() {
        if (targetCreature == null) return 0;
        return targetCreature.transform.position.x - transform.position.x;
    }

}
