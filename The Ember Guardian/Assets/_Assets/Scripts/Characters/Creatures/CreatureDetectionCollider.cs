using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureDetectionCollider : MonoBehaviour {
    private Creature creature;
    private CreatureAI creatureAI;
    private CreatureMovement creatureMovement;
    private CreatureAttack creatureAttack;
    private CircleCollider2D circleCollider;
    private List<IDamageable> iDamageablesInDetectionRange = new List<IDamageable>();
    private List<IDamageable> iDamageablesExcludedFromDetection = new List<IDamageable>();

    private Collider2D[] overlapBuffer = new Collider2D[64];
    private List<IDamageable> currentFrame = new List<IDamageable>(64);
    private List<IDamageable> toRemove = new List<IDamageable>(64);

    private bool playerShotCreature;
    private float playerShotCreatureTimer;
    private float playerShotCreatureAggroTime = 5f;
    private float unaggroTimer;
    private float unaggroTime = 5f;

    private bool guardHitCreature;
    private float guardHitCreatureAggroProbability = .25f;
    private float guardHitCreatureTimer;
    private float guardHitCreatureAggroTime = 5f;

    private float refreshTargetTimer;
    private float refreshTargetcooldown = .61f;

    // Targeting priorities : higher value = higher priority  
    private int workerTargetingPriority;
    private int playerTargetingPriority;
    private int barricadeTargetingPriority;
    private int fireTargetingPriority;

    private void Awake() {
        creature = GetComponentInParent<Creature>();
        creatureAI = GetComponentInParent<CreatureAI>();
        creatureMovement = GetComponentInParent<CreatureMovement>();
        creatureAttack = GetComponentInParent<CreatureAttack>();
        circleCollider = GetComponent<CircleCollider2D>();

        refreshTargetTimer = UnityEngine.Random.Range(0, refreshTargetcooldown);
    }

    private void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;

        workerTargetingPriority = creature.GetCreatureSO().workerTargetingPriority;
        playerTargetingPriority = creature.GetCreatureSO().playerTargetingPriority;
        barricadeTargetingPriority = creature.GetCreatureSO().barricadeTargetingPriority;
        fireTargetingPriority = creature.GetCreatureSO().fireTargetingPriority;

        if (creature.IsDayCreature()) {
            guardHitCreatureAggroProbability = .75f;
        }
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        if (iDamageablesInDetectionRange.Contains(Player.Instance)) {
            RemoveIDamageableInDetectionRange(Player.Instance);
        }
    }

    private void Update() {
        if (creature.GetIsPaused()) return;
        if (DebugManager.Instance.GetDisableCreatureDetection()) return;
        if (!creature.GetCreatureActive()) return;

        refreshTargetTimer -= Time.deltaTime;
        if (refreshTargetTimer < 0) {
            refreshTargetTimer = refreshTargetcooldown;
            ScanForTargets();
            RefreshHighestPriorityTarget();
        }

        HandleUnAggro();
        HandleGuardHitCreature();
    }

    // --- Remplacement des triggers ---
    private void ScanForTargets() {
        currentFrame.Clear();

        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            circleCollider.radius,
            overlapBuffer
        );

        for (int i = 0; i < hitCount; i++) {
            Collider2D hit = overlapBuffer[i];
            if (hit == null) continue;

            if (hit.TryGetComponent<Player>(out var player)) {
                currentFrame.Add(player);
                continue;
            }

            if (hit.TryGetComponent<Barricade>(out var barricade)) {
                currentFrame.Add(barricade);
                continue;
            }

            if (hit.TryGetComponent<Fire>(out var fire)) {
                if (!fire.GetIsEndLevelAreaFire()) {
                    currentFrame.Add(fire);
                }
                continue;
            }

            if (hit.TryGetComponent<Worker>(out var worker)) {
                if (worker.GetRecruited()) {
                    if (!(worker.GetDefensiveStructureAssigned() is Tower)) {
                        currentFrame.Add(worker);
                    }
                }
                continue;
            }
        }

        // --- ajout entrants ---
        for (int i = 0; i < currentFrame.Count; i++) {
            IDamageable dmg = currentFrame[i];

            if (iDamageablesInDetectionRange.Contains(dmg)) continue;

            AddIDamageableInDetectionRange(dmg);

            if (dmg is Worker newWorker) {
                newWorker.OnMobDied += Worker_OnMobDied;
            }

            if (dmg is Barricade newBarricade) {
                newBarricade.OnBarricadeDestroyed += Barricade_OnBarricadeDestroyed;
            }

            if (dmg is Player || dmg is Worker) {
                unaggroTimer = unaggroTime;
            }
        }

        // --- removals ---
        toRemove.Clear();

        for (int i = 0; i < iDamageablesInDetectionRange.Count; i++) {
            IDamageable dmg = iDamageablesInDetectionRange[i];

            if (!currentFrame.Contains(dmg)) {
                toRemove.Add(dmg);
            }
        }

        for (int i = 0; i < toRemove.Count; i++) {
            IDamageable dmg = toRemove[i];

            RemoveIDamageableInDetectionRange(dmg);

            if (dmg is Worker oldWorker) {
                oldWorker.OnMobDied -= Worker_OnMobDied;
            }

            if (dmg is Barricade oldBarricade) {
                oldBarricade.OnBarricadeDestroyed -= Barricade_OnBarricadeDestroyed;
            }
        }
    }


    // --- Le reste du script reste inchangé ---
    private void HandleUnAggro() {

        if (playerShotCreature) {
            playerShotCreatureTimer -= Time.deltaTime;
            if (playerShotCreatureTimer <= 0) {
                playerShotCreature = false;
            }
        }

        var target = creatureAI.GetAttackTarget();
        if (creatureAI == null || creatureMovement == null) return;
        if (target == null) return;

        if (target == Player.Instance as IDamageable) {

            bool playerIsFacingCreature = PlayerAim.Instance.GetAimDirFloat() * creatureMovement.GetLastMoveDirFloat() <= 0;
            bool creatureIsFleeingRight = creatureMovement.GetLastMoveDirFloat() > 0 && transform.position.x > 0;
            bool creatureIsFleeingLeft = creatureMovement.GetLastMoveDirFloat() < 0 && transform.position.x < 0;

            if (playerIsFacingCreature || creatureIsFleeingLeft || creatureIsFleeingRight) {
                unaggroTimer = unaggroTime;
                return;
            };

            unaggroTimer -= Time.deltaTime;
            if (unaggroTimer < 0) {
                RemoveIDamageableInDetectionRange(Player.Instance);
            }
        }

        if (target is Worker) {
            Worker worker = target as Worker;
            bool playerIsFacingWorker = worker.GetComponent<MobMovement>().GetMoveDirFloat() * creatureMovement.GetLastMoveDirFloat() <= 0;
            if (playerIsFacingWorker) {
                unaggroTimer = unaggroTime;
                return;
            };

            unaggroTimer -= Time.deltaTime;
            if (unaggroTimer < 0) {
                RemoveIDamageableInDetectionRange(worker);
            }
        }
    }

    private void HandleGuardHitCreature() {
        if (guardHitCreature) {
            guardHitCreatureTimer -= Time.deltaTime;
            if (guardHitCreatureTimer <= 0) {
                guardHitCreature = false;
                workerTargetingPriority = creature.GetCreatureSO().workerTargetingPriority;
                if (iDamageablesInDetectionRange.Count == 0) {
                    creatureAI.ResetAttackTargetInProximity();
                }
            }
        }
    }
    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        if (e.damageOriginTransform.GetComponent<Player>() != null) {
            playerShotCreature = true;
            playerShotCreatureTimer = playerShotCreatureAggroTime;
        }

        Worker worker = e.damageOriginTransform.GetComponent<Worker>();
        if (worker != null) {

            if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.guard) {
                if ((workerTargetingPriority == 0)) return;

                guardHitCreatureTimer = guardHitCreatureAggroTime;
                float randomNumber = UnityEngine.Random.Range(0f, 1f);

                if (randomNumber < guardHitCreatureAggroProbability) {
                    guardHitCreature = true;
                    workerTargetingPriority = 10;
                }
            }
        }
    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        RemoveIDamageableInDetectionRange(sender as Worker);
    }

    private void Barricade_OnBarricadeDestroyed(object sender, System.EventArgs e) {
        RemoveIDamageableInDetectionRange(sender as Barricade);
    }

    private void AddIDamageableInDetectionRange(IDamageable iDamageableAdded) {
        if (iDamageablesInDetectionRange.Contains(iDamageableAdded)) return;

        iDamageablesInDetectionRange.Add(iDamageableAdded);
    }

    private void RemoveIDamageableInDetectionRange(IDamageable iDamageable) {
        iDamageablesInDetectionRange.Remove(iDamageable);

        if (iDamageablesInDetectionRange.Count == 0 && !playerShotCreature) {
            creatureAI.ResetAttackTargetInProximity();
        }
    }

    public void ExcludeIDamageableFromDetectableTargets(IDamageable iDamageable) {
        Debug.Log("ExcludeIDamageableFromDetectableTargets " + (iDamageable as MonoBehaviour).gameObject);
        iDamageablesExcludedFromDetection.Add(iDamageable);
        RemoveIDamageableInDetectionRange(iDamageable);
    }

    private void RefreshHighestPriorityTarget() {
        if (iDamageablesInDetectionRange.Count == 0 && !playerShotCreature) {
            creatureAI.ResetAttackTargetInProximity();
            return;
        }

        IDamageable best = null;
        int bestPriority = 0;

        for (int i = 0; i < iDamageablesInDetectionRange.Count; i++) {
            IDamageable dmg = iDamageablesInDetectionRange[i];

            if (iDamageablesExcludedFromDetection.Contains(dmg)) continue;

            int priority = 0;

            if (dmg is Worker worker) {
                if (!CanAddWorkerToTargets(worker)) continue;
                priority = workerTargetingPriority;
            }
            else if (dmg is Barricade barricade) {
                if (!CanAddBarricadeToTargets(barricade)) continue;
                priority = barricadeTargetingPriority;
            }
            else if (dmg is Fire fire) {
                if (!CanAddFireToTargets(fire)) continue;
                priority = fireTargetingPriority;
            }
            else if (dmg is Player) {
                if (!CanAddPlayerToTargets()) continue;
                priority = playerTargetingPriority;
            }

            if (priority > bestPriority) {
                bestPriority = priority;
                best = dmg;
            }
        }

        creatureAI.SetAttackTarget(best, iDamageablesInDetectionRange);
    }

    private bool CanAddFireToTargets(IDamageable fire) {
        float distanceToFire = Mathf.Abs(transform.position.x - (fire as MonoBehaviour).transform.position.x);

        if (distanceToFire > 10) return false;
        if (IsTargetBehindBarricade((fire as MonoBehaviour).transform)) return false;

        Fire fireClass = fire as Fire;
        if (fireClass.GetIsSecondaryFire() && fireClass.GetCurrentFuelLevel() < 0) return false;

        return true;
    }

    private bool CanAddPlayerToTargets() {
        if (playerTargetingPriority == 0) return false;

        if (Player.Instance.transform.position.y > 0.1f) {
            CreatureAttackSO attackSO = creatureAttack.GetCurrentCreatureAttackSO();
            if (attackSO == null) return false;
            if (!attackSO.canAttackPlayerOnTower) return false;
            if (Player.Instance.transform.position.y > attackSO.minAttackRange) return false;
        }

        if (!IsTargetBehindBarricade(Player.Instance.transform)) {
            if (CampZoneManager.Instance.IsWithinCampCenterZoneLimits(Player.Instance.transform.position)) return false;
            return true;
        }
        else {
            CreatureAttackSO attackSO = creatureAttack.GetCurrentCreatureAttackSO();
            if (attackSO != null && attackSO.canAttackPlayerBehindBarricades) return true;
        }

        return false;
    }

    private bool IsTargetBehindBarricade(Transform targetTransform) {
        Vector2 origin = transform.position;
        Vector2 target = targetTransform.position;
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Barricades"));
        return hit.collider != null;
    }

    private bool CanAddBarricadeToTargets(Barricade barricade) {
        return barricadeTargetingPriority != 0 && barricade.GetBarricadeHealthNormalized() > 0;
    }

    private bool CanAddWorkerToTargets(Worker worker) {
        if (workerTargetingPriority == 0) return false;
        if (worker.GetDefensiveStructureAssigned() != null) return false;
        if (creature.GetCreatureSO().flying && DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) return false;

        if (!IsTargetBehindBarricade(worker.transform)) {
            if (CampZoneManager.Instance.IsWithinCampZoneLimits(worker.transform.position)) return false;
            return true;
        }
        return false;
    }

    public void SetRadius(float radius) {
        circleCollider.radius = radius;
    }
    public void BuffRadius(float radiusBuff) {
        circleCollider.radius *= radiusBuff;
    }
    public void DebuffRadius(float radiusBuff) {
        circleCollider.radius /= radiusBuff;
    }

    public float GetRadius() {
        return circleCollider.radius;
    }
}
