using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAttack : MobAttack
{
    protected Creature creature;
    protected float enteredLightAttackSpeedDebuff;
    protected float nightAttackSpeedBuff = 1.5f;
    protected CreatureAttackSO currentCreatureAttackSO;

    protected float minAttackRange;
    protected float maxAttackRange;
    protected float attackRangeRandomizer;
    protected float attackRangeMaxDistanceMiss;
    protected int damageToFire;
    protected int damageToBarricades;

    public event EventHandler OnAttackSOChanged;

    protected Transform primaryAttackProjectileSpawnPosition;
    [SerializeField] protected Transform secondaryAttackProjectileSpawnPosition;

    protected override void Awake() {
        base.Awake();
        creature = GetComponent<Creature>();

        enteredLightAttackSpeedDebuff = creature.GetCreatureSO().enteredLightattackRateDebuff;
        creature.OnCreatureStunStarted += Creature_OnCreatureStunStarted;
        creature.OnCreatureStunStopped += Creature_OnCreatureStunStopped;
        creature.OnCreatureEnabled += Creature_OnCreatureEnabled;

        primaryAttackProjectileSpawnPosition = projectileSpawnPoint;
        SetAttackSO(creature.GetCreatureSO().primaryAttackSO);
    }

    protected void Start() {
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;

    }
    private void Creature_OnCreatureEnabled(object sender, EventArgs e) {
        attacking = false;
        attackStarted = false;
        notStunned = true;
        InitializeCreatureAttack();
    }

    private void InitializeCreatureAttack() {

        if (creature.GetIsEliteDamageCreature()) {
            attackDamage *= 2;
        }
        if (!creature.IsDayCreature()) {
            attackCooldown /= nightAttackSpeedBuff;
        }
    }

    public void SetAttackSO(CreatureAttackSO attackSO) {
        if (attackSO == null) return;

        currentCreatureAttackSO = attackSO;

        isProjectileAttack = attackSO.isProjectileAttack;
        isStaticProjectileAttack = attackSO.isStaticProjectileAttack;
        isAnimatedAttack = attackSO.isAnimatedAttack;
        staticProjectileAutoTargetsPlayer = attackSO.staticProjectileAutoTargetsPlayer;

        attackDamage = attackSO.damage;
        damageToFire = attackSO.damageToFire;
        damageToBarricades = attackSO.damageToBarricades;

        attackCooldown = attackSO.attackCooldown;
        attackDamage = attackSO.damage;

        minAttackRange = attackSO.minAttackRange;
        maxAttackRange = attackSO.maxAttackRange;

        if(attackSO.hasDifferentDayAndNightRange && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            minAttackRange = attackSO.minAttackRange_Night;
            maxAttackRange = attackSO.maxAttackRange_Night;
        }

        attackRangeRandomizer = attackSO.attackRangeRandomizer;
        attackRangeMaxDistanceMiss = attackSO.attackRangeRandomizerMiss;

        attackAnimationDelay = attackSO.attackAnimationDelay;
        totalAttackAnimationTime = attackSO.totalAttackAnimationTime;

        projectileAmountInPool = attackSO.projectileAmountInPool;
        projectileAmountShotInAttack = attackSO.projectileAmountShotInAttack;
        delayBetweenProjectileSpawns = attackSO.delayBetweenProjectileSpawns;


        projectileSO = attackSO.projectileSO;
        staticProjectilePrefab = attackSO.staticProjectilePrefab;

        if(attackSO == creature.GetCreatureSO().secondaryAttackSO) {
            projectileSpawnPoint = secondaryAttackProjectileSpawnPosition;
        }
        if (attackSO == creature.GetCreatureSO().primaryAttackSO) {
            projectileSpawnPoint = primaryAttackProjectileSpawnPosition;
        }

        OnAttackSOChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void DealDamage() {
        if (attackTargetIDamageable != null) {

            if ((attackTargetIDamageable is Fire)) {
                attackTargetIDamageable.TakeDamage(damageToFire, transform, false, attackIgnoresTemporaryInvincibility);

                if (!creature.GetCreatureSO().isBoss) {
                    mob.Die();
                }

            } else {

                if(attackTargetIDamageable is Barricade) {
                    attackDamage = damageToBarricades;
                }

                attackTargetIDamageable.TakeDamage(attackDamage, transform, false, attackIgnoresTemporaryInvincibility);

                if(!GetIsRangedAttack()) {
                    // Melee attack creature

                    Barricade barricade = attackTargetIDamageable as Barricade;
                    if (barricade != null) {
                        if (!barricade.GetBarricadeSpiked()) return;
                        creature.TakeDamage(barricade.GetSpikeDamage(), barricade.transform);
                    }
                }
            }

        }
        InvokeAttackHit();
    }

    protected void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        attackCooldown /= enteredLightAttackSpeedDebuff;
    }

    protected void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        attackCooldown *= enteredLightAttackSpeedDebuff;
    }

    protected override Vector3 GetEndPointRandomOffstetValue() {
        float distanceToTargetNormalized = Mathf.Abs(attackTargetGameObject.transform.position.x - transform.position.x)/ minAttackRange;

        Vector3 endPointRandomized = new Vector3(distanceToTargetNormalized * attackRangeMaxDistanceMiss, 0, 0);
        
        return endPointRandomized;
    }

    public override void RemoveAttackTarget() {
        attacking = false;
        attackTargetIDamageable = null;
    }

    protected void Creature_OnCreatureStunStopped(object sender, EventArgs e) {
        notStunned = true;
    }

    protected void Creature_OnCreatureStunStarted(object sender, EventArgs e) {
        notStunned = false;
    }

    public bool GetIsPrimaryAttack() {
        return currentCreatureAttackSO == creature.GetCreatureSO().primaryAttackSO;
    }
    public bool GetIsSecondaryAttack() {
        return currentCreatureAttackSO == creature.GetCreatureSO().secondaryAttackSO;
    }

    public CreatureAttackSO GetCurrentCreatureAttackSO() {
        return currentCreatureAttackSO;
    }

}
