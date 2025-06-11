using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureDetectionCollider : MonoBehaviour
{
    private Creature creature;
    private CreatureAI creatureAI;
    private CreatureMovement creatureMovement;
    private CircleCollider2D circleCollider;
    private List<IDamageable> iDamageablesInDetectionRange = new List<IDamageable>();
    private List<IDamageable> iDamageablesExcludedFromDetection = new List<IDamageable>();

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
    private float refreshTargetcooldown = .25f;

    // Targeting priorities : higher value = higher priority  
    private int workerTargetingPriority;
    private int playerTargetingPriority;
    private int barricadeTargetingPriority;

    private void Awake() {
        creature = GetComponentInParent<Creature>();
        creatureAI = GetComponentInParent<CreatureAI>();
        creatureMovement = GetComponentInParent<CreatureMovement>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;

        workerTargetingPriority = creature.GetCreatureSO().workerTargetingPriority;
        playerTargetingPriority = creature.GetCreatureSO().playerTargetingPriority;
        barricadeTargetingPriority = creature.GetCreatureSO().barricadeTargetingPriority;

        if(creature.IsDayCreature()) {
            guardHitCreatureAggroProbability = .75f;
        }
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        if(iDamageablesInDetectionRange.Contains(Player.Instance)) {
            RemoveIDamageableInDetectionRange(Player.Instance);
        }
    }

    private void Update() {
        if (DebugManager.Instance.GetDisableCreatureDetection()) return;
        refreshTargetTimer -= Time.deltaTime;
        if(refreshTargetTimer < 0) {
            refreshTargetTimer = refreshTargetcooldown;
            RefreshHighestPriorityTarget();
        }

        HandleUnAggro();
        HandleGuardHitCreature();
    }

    void OnTriggerEnter2D(Collider2D other) {
        // Player
        Player player = other.GetComponent<Player>(); 

        if (player != null) {
            AddIDamageableInDetectionRange(player);
            unaggroTimer = unaggroTime;
        }

        // Barricade
        Barricade barricade = other.GetComponent<Barricade>();
        if (barricade != null && !iDamageablesInDetectionRange.Contains(barricade)) {
            barricade.OnBarricadeDestroyed += Barricade_OnBarricadeDestroyed;
            AddIDamageableInDetectionRange(barricade);
        }

        // Fire
        Fire fire = other.GetComponent<Fire>();
        if (fire != null) {
            AddIDamageableInDetectionRange(fire);
        }

        // Worker
        Worker worker = other.GetComponent<Worker>();
        if (worker != null && !iDamageablesInDetectionRange.Contains(worker)) {
            if(worker.GetRecruited() && !(worker.GetStructureAssigned() is Tower)) {
                worker.OnMobDied += Worker_OnMobDied;
                AddIDamageableInDetectionRange(worker);
                unaggroTimer = unaggroTime;
            }
        }

    }

    void OnTriggerExit2D(Collider2D other) {

        // Player
        Player player = other.GetComponent<Player>();
        if (player != null) {
            RemoveIDamageableInDetectionRange(player);
        }

        // Barricade
        Barricade barricade = other.GetComponent<Barricade>();
        if (barricade != null) {
            barricade.OnBarricadeDestroyed -= Barricade_OnBarricadeDestroyed;
            RemoveIDamageableInDetectionRange(barricade);
        }

        // Fire
        Fire fire = other.GetComponent<Fire>();
        if (fire != null) {
            RemoveIDamageableInDetectionRange(fire);
        }

        // Worker
        Worker worker = other.GetComponent<Worker>();
        if (worker != null) {
            worker.OnMobDied -= Worker_OnMobDied;
            RemoveIDamageableInDetectionRange(worker);
        }

    }

    private void HandleUnAggro() {
        if (playerShotCreature) {
            playerShotCreatureTimer -= Time.deltaTime;
            if (playerShotCreatureTimer <= 0) {
                playerShotCreature = false;
            }
        }

        if(creatureAI.GetAttackTarget() == Player.Instance as IDamageable) {

            bool playerIsFacingCreature = PlayerAim.Instance.GetAimDirFloat() * creatureMovement.GetLastMoveDirFloat() <= 0;
            bool creatureIsFleeingRight = creatureMovement.GetLastMoveDirFloat() > 0 && transform.position.x > 0;
            bool creatureIsFleeingLeft = creatureMovement.GetLastMoveDirFloat() < 0 && transform.position.x < 0;

            if (playerIsFacingCreature || creatureIsFleeingLeft || creatureIsFleeingRight) {
                unaggroTimer = unaggroTime;
                return;
            };

            unaggroTimer -= Time.deltaTime;
            if(unaggroTimer < 0) {
                RemoveIDamageableInDetectionRange(Player.Instance);
            }
        }
        if (creatureAI.GetAttackTarget() is Worker) {
            Worker worker = creatureAI.GetAttackTarget() as Worker;
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

                if(randomNumber < guardHitCreatureAggroProbability) {
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
        List<IDamageable> iDamageablesDetected = new List<IDamageable>();

        foreach(IDamageable iDamageable in iDamageablesInDetectionRange) {
            if (iDamageablesExcludedFromDetection.Contains(iDamageable)) continue;
            iDamageablesDetected.Add(iDamageable);
        }

        if (playerShotCreature && !iDamageablesInDetectionRange.Contains(Player.Instance) && !CampZoneManager.Instance.IsWithinCampZoneLimits(Player.Instance.transform.position)) {
            iDamageablesDetected.Add(Player.Instance);
        }
;
        if (iDamageablesDetected.Count == 0) {
            creatureAI.ResetAttackTargetInProximity();
            return;
        }

        IDamageable highestPriorityTarget = null;
        int highestPriority = 0; // Initialise à une valeur élevée

        foreach (IDamageable iDamageable in iDamageablesDetected) {
            int currentPriority = 0;

            if (iDamageable is Worker) {

                Worker worker = (Worker)iDamageable;
                if (CanAddWorkerToTargets(worker)) {
                    currentPriority = workerTargetingPriority;
                }
                else continue;

            }

            if (iDamageable is Barricade) {
                Barricade barricade = (Barricade)iDamageable;

                if (CanAddBarricadeToTargets(barricade)) {
                    currentPriority = barricadeTargetingPriority;
                }
                else continue;
            }

            if (iDamageable is Fire) {
                if (CanAddFireToTargets(iDamageable)) {
                    currentPriority = int.MaxValue;
                }
                else continue;
            }

            if (iDamageable is Player) {
                if (CanAddPlayerToTargets()) {
                    currentPriority = playerTargetingPriority;
                }
                else continue;
                
            }

            // Si la priorité actuelle est plus haute, on la met à jour
            if (currentPriority > highestPriority) {
                highestPriorityTarget = iDamageable;
                highestPriority = currentPriority;
            }

        }

        creatureAI.SetAttackTarget(highestPriorityTarget, iDamageablesDetected);
    }

    private bool CanAddFireToTargets(IDamageable fire) {
        float distanceToFire = Mathf.Abs(transform.position.x - (fire as MonoBehaviour).transform.position.x);

        if (distanceToFire > 10) {
            return false;
        } else {
            return true;
        }
    }

    private bool CanAddPlayerToTargets() {
        if (playerTargetingPriority == 0) return false;

        // Check if player is in range in the y axis (tower) !
        if(Player.Instance.transform.position.y > 0.1f) {

            if (!creature.GetCreatureSO().canAttackPlayerOnTower) {
                return false;
            }

            else {
                // Creature can attack player on tower : is he in range ? (ex. ghouls on lvl 1 towers)
                if (Player.Instance.transform.position.y > creature.GetCreatureSO().minAttackRange) {
                    return false;
                }
            }

        }

        // Check if player is out of camp
        if (!CampZoneManager.Instance.IsWithinCampZoneLimits(Player.Instance.transform.position)) {

            // Player is out of camp
            return true;

        } else {

            // Player is within camp zone

            if (creature.GetCreatureSO().canAttackPlayerBehindBarricades) {
                return true;
            }

        }

        return false;
    }
    private bool CanAddBarricadeToTargets(Barricade barricade) {
        if (barricadeTargetingPriority == 0) return false;
        if (barricade.GetBarricadeHealthNormalized() > 0) {
            return true;
        }
        return false;
    }

    private bool CanAddWorkerToTargets(Worker worker) {

        // Check if worker is out of camp AND player is around too
        if (workerTargetingPriority == 0) return false;
        if (!CampZoneManager.Instance.IsWithinCampZoneLimits(worker.transform.position)) {
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
