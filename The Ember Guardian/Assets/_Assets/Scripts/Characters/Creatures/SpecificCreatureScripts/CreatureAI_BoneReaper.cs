using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_BoneReaper : CreatureAI {

    public event EventHandler OnBoneReaperEnraged;
    public event EventHandler OnBoneReaperSpawned;

    [SerializeField] private CreatureAttackSO standardProjectileAttackSO;
    [SerializeField] private CreatureAttackSO laserAttackSO;
    [SerializeField] private CreatureAttackSO handsAttackSO;
    [SerializeField] private CreatureAttackSO handsAttackSO_NoSlide;
    [SerializeField] private CreatureAttackSO specialAttackSO;

    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject critHitZones;
    private bool handsActive;

    private float meleeAttackMoveSpeedBuff = 1.5f;

    private float laserAttackTimer;
    private float laserAttackRate = 15f;
    private float specialAttackTimer;
    private float specialAttackRate = 30f;
    private float handsAttackTimer;
    private float handsAttackRate = 10f;
    private bool attacking;

    private bool isFirstAppearance;
    private bool enraged;
    private bool enraging;
    private float enragingTimer;
    private float enragingTime = 2f;
    private float enragedHealthTreshold = .5f;

    protected override void Start() {
        base.Start();
        creatureAttack.OnMobAttack += CreatureAttack_OnMobAttack;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;

        isFirstAppearance = LevelManager.Instance.GetLevelSO().bossNightSpawns[0] == CreaturesSpawnManager.Instance.GetCurrentWaveNumber();
        BossUI.Instance.LinkBoss(creature, !isFirstAppearance);
        BossUI.Instance.SetBossName(LocalizationManager.Instance.GetLocalizedText("BoneReaper"));

        if(isFirstAppearance) {
            handsActive = false;
            leftHand.SetActive(false);
            rightHand.SetActive(false);
            critHitZones.SetActive(false);
        } else {
            handsActive = true;
            creature.SetCreatureHealth((int)(creature.GetCreatureSO().maxHealth * 1.5f));
        }
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        if (enraged) return;
        if (isFirstAppearance) return;
        if((float)creature.GetHealth()/ (float)creature.GetCreatureMaxHealth() < enragedHealthTreshold) {
            enraged = true;
            enraging = true;
            OnBoneReaperEnraged?.Invoke(this, EventArgs.Empty);
            meleeAttackMoveSpeedBuff = 2.5f;
        }
    }

    protected override void Update() {
        if (died) return;
        if (!spawned) return;
        if (enraging) {
            enragingTimer += Time.deltaTime;
            if(enragingTimer >= enragingTime) {
                enraging = false;
            }
            return;
        }


        if (!attacking) {

            laserAttackTimer += Time.deltaTime;
            specialAttackTimer += Time.deltaTime;
            handsAttackTimer += Time.deltaTime;

            if (specialAttackTimer > specialAttackRate) {
                creatureAttack.SetAttackSO(specialAttackSO);
                creatureMovement.BuffMoveSpeed(meleeAttackMoveSpeedBuff);
                specialAttackTimer = 0;
                attacking = true;
                return;
            }

            if (handsActive) {
                if (handsAttackTimer > handsAttackRate) {
                    if(enraged) {
                        creatureAttack.SetAttackSO(handsAttackSO_NoSlide);
                    } else {
                        creatureAttack.SetAttackSO(handsAttackSO);
                    }

                    creatureMovement.BuffMoveSpeed(meleeAttackMoveSpeedBuff);
                    handsAttackTimer = 0;
                    attacking = true;
                    return;
                }

                if (laserAttackTimer > laserAttackRate) {
                    creatureAttack.SetAttackSO(laserAttackSO);
                    creatureMovement.BuffMoveSpeed(meleeAttackMoveSpeedBuff);
                    laserAttackTimer = 0;
                    attacking = true;
                    return;
                }
            }
        };


        HandleAggroRecently();
        StateSwitch();
    }

    protected override void HeadToTarget() {
        if ((attackTarget as MonoBehaviour) == null) return;

        Vector3 targetPosition = attackTarget.GetMeleeAttackPosition().position;
        Vector3 destination;

        destination = targetPosition;
        // Distance jusqu’à la position derrière la cible
        float distanceToDestination = Mathf.Abs(transform.position.x - destination.x);

        // Si on y est: attaque
        if (distanceToDestination < creatureAttack.GetCurrentCreatureAttackSO().minAttackRange) {
            ChangeState(State.attacking);
            return;
        }

        // Dans les deux cas, on continue à se déplacer
        creatureMovement.SetMoveTarget(destination);
    }

    private void CreatureAttack_OnMobAttack(object sender, EventArgs e) {
        float delay = creatureAttack.GetCurrentCreatureAttackSO().attackAnimationDelay;
        if (creatureAttack.GetCurrentCreatureAttackSO() != standardProjectileAttackSO) {
            creatureMovement.ResetTempMoveSpeedBuffs();
            StartCoroutine(ResetAttackSOAfterDelay(delay));
        }
    }

    private IEnumerator ResetAttackSOAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        creatureAttack.SetAttackSO(standardProjectileAttackSO);
        attacking = false;
    }

    public bool GetEnraged() {
        return enraged;
    }
    public bool GetHandsActive() {
        return handsActive;
    }

    public bool GetIsProjectileAttack() {
        return creatureAttack.GetCurrentCreatureAttackSO() == standardProjectileAttackSO;
    }
    public bool GetIsSpecialAttack() {
        return creatureAttack.GetCurrentCreatureAttackSO() == specialAttackSO;
    }
    public bool GetIsLaserAttack() {
        return creatureAttack.GetCurrentCreatureAttackSO() == laserAttackSO;
    }
    public bool GetIsHandsAttack() {
        return creatureAttack.GetCurrentCreatureAttackSO() == handsAttackSO || creatureAttack.GetCurrentCreatureAttackSO() == handsAttackSO_NoSlide;
    }

    public override void TriggerBossSpawnAnimation() {
        OnBoneReaperSpawned?.Invoke(this, EventArgs.Empty);
    }

}
