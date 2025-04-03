using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_TarnishedWidow : CreatureAI
{
    [SerializeField] private CreatureDetectionCollider detectionCollider;

    private bool isFirstAppearance;

    public event EventHandler OnWidowJumpStarted;
    public event EventHandler OnWidowLanded;
    private float jumpTimer;
    private float jumpCooldown = 75f;
    private float jumpDuration = 3.8f;
    private bool canJump;
    private bool jumping;
    private bool exitedWave;

    private float initialMoveSpeed;
    private float jumpMoveSpeed = 0f;

    private float jumpAnimationDuration = 1.7f;
    private float landAnimationDuration = 2f;

    private float distanceToBarricadeToJump = 4.5f;
    private float distanceToLandBehindBarricade = 5f;
    private Vector3 barricadeJumpedOverPosition;
    private Vector3 behindBarricadeJumpPosition;

    protected override void Start() {
        base.Start();
        GetComponent<CreatureAttack>().SetAttackIgnoresTemporaryInvincibility();
        initialMoveSpeed = creature.GetCreatureSO().nightMoveSpeed;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;

        isFirstAppearance = LevelManager.Instance.GetLevelSO().bossNightSpawns[0] == CreaturesSpawnManager.Instance.GetCurrentWaveNumber();
        Debug.Log("isFirstAppearance " + isFirstAppearance);

        if(DemoMainLevelManager.Instance != null && DemoMainLevelManager.Instance.GetDemoLevelLostAmount() == 0) {
            Debug.Log("DemoMainLevelManager.Instance.GetDemoLevelLostAmount() " + DemoMainLevelManager.Instance.GetDemoLevelLostAmount());
            Debug.Log("demo Lost amount = 0, doubling widow health ");
            creature.SetCreatureHealth(Mathf.RoundToInt(creature.GetCreatureSO().maxHealth*1.5f));
        }

        BossUI.Instance.LinkBoss(creature);
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        float mobHPNormalized = (float)creature.GetCreatureHealth() / (float)creature.GetCreatureSO().maxHealth;
        Debug.Log("Widow - OnMobDamageTaken remainingHealth" + creature.GetCreatureHealth());
        if(isFirstAppearance && mobHPNormalized < .5f && !exitedWave) {
            exitedWave = true;
            StartCoroutine(ExitWave());
        }
    }

    protected override void Update() {
        if (died) return;
        if (!spawned) return;

        HandleJumpTimers();
        HandleAggroRecently();

        if(jumping) {
            JumpingStateUpdate();
            return;
        }

        switch (state) {

            case State.idle:

                IdleStateUpdate();

                break;

            case State.walkingToFire:
                WalkingToFireStateUpdate();

                break;

            case State.walkingToSpawner:
                WalkingToSpawnerStateUpdate();

                break;

            case State.moveToTarget:
                MoveToTargetStateUpdate();

                break;

            case State.attacking:
                AttackingStateUpdate();

                break;
        }
    }

    private void JumpingStateUpdate() {
    }

    private void HandleJumpTimers() {

        if (jumping) {

            jumpTimer -= Time.deltaTime;
            if(jumpTimer < 0) {
                StartCoroutine(LandCoroutine());
                jumpTimer = jumpCooldown;
            }

        } else {

            if (canJump) return;

            jumpTimer -= Time.deltaTime;
            if (jumpTimer < 0) {
                canJump = true;
            }

        };
    }

    protected override void AttackingStateUpdate() {
        if (!detectedAttackTarget) {
            if (creature.IsDayCreature()) {
                ChangeState(State.walkingToSpawner);
            }
            else {
                ChangeState(State.walkingToFire);
            }
            return;
        }

        if (attackTarget == null || (!CheckAttackTargetInRange() && !mobAttack.GetAttackStarted())) {
            ChangeState(State.moveToTarget);
            return;
        }

        if(attackTarget is Barricade) {
            float distanceToBarricade = Mathf.Abs((attackTarget as MonoBehaviour).transform.position.x - transform.position.x);
            if (canJump && distanceToBarricade < distanceToBarricadeToJump) {
                StartCoroutine(JumpCoroutine());
            }
        }
    }

    private IEnumerator JumpCoroutine() {

        canJump = false;
        jumping = true;
        jumpTimer = jumpDuration;
        barricadeJumpedOverPosition = (attackTarget as MonoBehaviour).transform.position;
        behindBarricadeJumpPosition = barricadeJumpedOverPosition;
        mobAttack.RemoveAttackTarget();
        detectionCollider.ExcludeIDamageableFromDetectableTargets(attackTarget);
        creature.SetCreatureCanBeTargeted(false);

        if (barricadeJumpedOverPosition.x > 0) {
            behindBarricadeJumpPosition.x -= distanceToLandBehindBarricade;
        }
        else {
            behindBarricadeJumpPosition.x += distanceToLandBehindBarricade;
        }

        creatureMovement.SetMoveSpeed(jumpMoveSpeed);
        OnWidowJumpStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(jumpAnimationDuration);

        transform.position = behindBarricadeJumpPosition;
        creatureMovement.SetMoveTarget(behindBarricadeJumpPosition);

    }

    private IEnumerator LandCoroutine() {
        OnWidowLanded?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(landAnimationDuration);

        creature.SetCreatureCanBeTargeted(true);
        jumping = false;
        jumpTimer = jumpCooldown;
        creatureMovement.SetMoveSpeed(initialMoveSpeed);

    }

    private IEnumerator ExitWave() {
        canJump = false;
        jumping = true;
        jumpTimer = jumpDuration;
        creature.SetCreatureCanBeTargeted(false);
        mobAttack.RemoveAttackTarget();

        if (barricadeJumpedOverPosition.x > 0) {
            behindBarricadeJumpPosition.x -= distanceToLandBehindBarricade;
        }
        else {
            behindBarricadeJumpPosition.x += distanceToLandBehindBarricade;
        }

        creatureMovement.SetMoveSpeed(jumpMoveSpeed);
        OnWidowJumpStarted?.Invoke(this, EventArgs.Empty);

        BossUI.Instance.Hide();
        yield return new WaitForSeconds(3f);

        CreaturesManager.Instance.RemoveCreatureFromNightWave(creature);
        gameObject.SetActive(false);
    }

}
