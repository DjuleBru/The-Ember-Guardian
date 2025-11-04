using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_AncientGuardian : CreatureAI
{
    public event EventHandler OnGuardianStartedTP;
    public event EventHandler OnGuardianEndedTP;

    [SerializeField] private CreatureAttackSO standardAttackSO;
    [SerializeField] private CreatureAttackSO laserAttackSO;
    [SerializeField] private CreatureAttackSO AOEAttackSO;

    private float laserAttackTimer;
    private float laserAttackRate = 15f;
    private float AOEAttackTimer;
    private float AOEAttackRate = 25f;

    private Vector3 teleportStartPosition;
    private bool isFirstAppearance;
    private bool teleporting;
    private bool teleported;
    private bool canTeleport;
    private float teleportTimer;
    private float teleportRate = 20f;
    private float teleportDistanceToPlayer = 4f;

    
    protected override void Start() {
       base.Start();
        creatureAttack.OnMobAttack += CreatureAttack_OnMobAttack;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;

        isFirstAppearance = LevelManager.Instance.GetLevelSO().bossNightSpawns[0] == CreaturesSpawnManager.Instance.GetCurrentWaveNumber();
        BossUI.Instance.LinkBossMultiple(creature, isFirstAppearance);

        if (isFirstAppearance) {
            BossUI.Instance.SetBossName(LocalizationManager.Instance.GetLocalizedText("AncientGuardian_Solo"));
        }
        else {
            BossUI.Instance.SetBossName(LocalizationManager.Instance.GetLocalizedText("AncientGuardian_Duo"));
        }
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        canTeleport = true;
    }

    protected override void Update() {
        if (died) return;
        if (!spawned) return;

        if(canTeleport && !teleported && !creatureAttack.GetAttacking()) {
            teleportTimer += Time.deltaTime;
            if(teleportTimer > teleportRate && PlayerIsInSuitableTPPosition()) {
                StartCoroutine(Teleport(true));
                teleported = true;
                teleportTimer = 0;
                teleportStartPosition = transform.position;
            }
        }

        laserAttackTimer += Time.deltaTime;
        AOEAttackTimer += Time.deltaTime;

        if (laserAttackTimer > laserAttackRate) {
            creatureAttack.SetAttackSO(laserAttackSO);
            laserAttackTimer = 0;
            return;
        }

        if (AOEAttackTimer > AOEAttackRate) {
            creatureAttack.SetAttackSO(AOEAttackSO);
            AOEAttackTimer = 0;
            return;
        }

        if (teleporting) return;
        HandleAggroRecently();
        StateSwitch();
    }

    private void CreatureAttack_OnMobAttack(object sender, EventArgs e) {
        float delay = creatureAttack.GetCurrentCreatureAttackSO().attackAnimationDelay;
        if(creatureAttack.GetCurrentCreatureAttackSO() != standardAttackSO) {
            StartCoroutine(ResetAttackSOAfterDelay(delay));
        }

        if (teleported) {
            StartCoroutine(Teleport(false, 2f));
            teleported = false;
            Debug.Log("EndTP");
        }
    }

    private IEnumerator ResetAttackSOAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        creatureAttack.SetAttackSO(standardAttackSO);
    }

    private bool PlayerIsInSuitableTPPosition() {
        // Is Player on Tower
        if (Player.Instance.transform.position.y > 1) return false;

        // Is Player too close to fire ?
        if (Mathf.Abs(Player.Instance.transform.position.x) < 10) return false;

        if (!isFirstAppearance) {
            // Is Boss on same side as player ?
            if (transform.position.x * Player.Instance.transform.position.x < 0) return false;
        };

        // Is Player too close?
        float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        if (distanceToPlayer < 10) return false;

        return true;
    }

    public override void TriggerBossSpawnAnimation() {
        creatureAttack.SetAttackSO(AOEAttackSO);
        creatureAttack.Attack();
        creatureAttack.SetAttackSO(standardAttackSO);
    }

    private IEnumerator Teleport(bool startTeleport, float delayToTeleport = 0) {
        teleporting = true;
        creatureAttack.RemoveAttackTarget();
        creatureMovement.SetCanMove(false);
        ChangeState(State.moveToTarget);

        yield return new WaitForSeconds(delayToTeleport);

        GetComponent<Collider2D>().enabled = false;
        OnGuardianStartedTP?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        float newPositionX = Player.Instance.transform.position.x;

        if (startTeleport) {
            if (Player.Instance.transform.position.x > 0) {
                newPositionX -= teleportDistanceToPlayer;
            }
            else {
                newPositionX += teleportDistanceToPlayer;
            }
        } else {
            newPositionX = teleportStartPosition.x;
        }

        transform.position = new Vector3(newPositionX, 0, 0);
        OnGuardianEndedTP?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(1f);
        GetComponent<Collider2D>().enabled = true;

        creatureAttack.RemoveAttackTarget();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        teleporting = false;

        creatureMovement.SetCanMove(true);

        if (startTeleport) {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            creatureAttack.SetAttackTarget(Player.Instance);
        }
    }
}
