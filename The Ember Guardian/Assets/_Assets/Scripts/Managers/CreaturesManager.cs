using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    public static CreaturesManager Instance;

    private List<Mob> mobsAggroingPlayer = new List<Mob>();

    private List<Creature> creaturePoolList = new List<Creature>();
    private List<Creature> creaturesSpawnedList = new List<Creature>();
    private List<Creature> creaturesSpawnedAtNightList = new List<Creature>();

    public event EventHandler<OnCreatureAtNightKilledEventArgs> OnCreatureAtNightSpawned;
    public event EventHandler<OnCreatureAtNightKilledEventArgs> OnAdditionalCreatureAtNightSpawned;
    public event EventHandler<OnCreatureAtNightKilledEventArgs> OnCreatureAtNightKilled;
    public event EventHandler OnAllCreaturesAtNightKilled;

    public class OnCreatureAtNightKilledEventArgs : EventArgs {
        public Creature creature;
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        CreatureAI.OnAnyCreatureUntargetPlayer += CreatureAI_OnAnyCreatureUntargetPlayer;
    }

    private void CreatureAI_OnAnyCreatureUntargetPlayer(object sender, EventArgs e) {
        CreatureAI creatureAI = (CreatureAI)sender;
        Mob mob = creatureAI.GetComponent<Mob>();
        TryRemoveCreatureAggroingPlayer(mob);
    }
     
    private void TryRemoveCreatureAggroingPlayer(Mob mob) {
        if(mobsAggroingPlayer.Contains(mob)) {
            mobsAggroingPlayer.Remove(mob);
        }
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, EventArgs e) {
        CreatureAI creatureAI = (CreatureAI)sender;
        Mob mob = creatureAI.GetComponent<Mob>();
        mobsAggroingPlayer.Add(mob);
    }

    public Creature GetClosestCreatureInRadiusSmart(Vector2 position, float radius, int damage, bool canAttackFlying) {
        float closestXDistance = Mathf.Infinity;
        Creature closestCreatureInRadius = null;

        foreach (Creature creature in creaturesSpawnedList) {
            if(creature == null) {
                RemoveCreatureSpawned(creature);
                continue;
            }

            float distanceToCreature = Mathf.Abs(creature.transform.position.x - position.x);
            bool creatureIsOneHitAwayFromDeathAndAlreadyTargeted = creature.GetCreatureTargeted() && creature.GetCreatureHealth() <= damage;
            
            if (!creature.GetCreatureCanBeTargeted()) continue;
            if (!canAttackFlying && creature.GetCreatureSO().flying) continue;
            if ((distanceToCreature) < radius) {
                // Creature is within attack range


                if (distanceToCreature < closestXDistance && !creatureIsOneHitAwayFromDeathAndAlreadyTargeted) {
                    // Creature is the closest one

                    closestXDistance = Mathf.Abs(creature.transform.position.x - position.x);
                    closestCreatureInRadius = creature;
                }

            } 

        }

        return closestCreatureInRadius;
    }
    public Creature GetFurthestCreatureInRadiusSmart(Vector2 position, float radius, int damage, bool canAttackFlying) {

        float furthestXDistance = 0f;
        Creature closestCreatureInRadius = null;

        foreach (Creature creature in creaturesSpawnedList) {
            float distanceToCreature = Mathf.Abs(creature.transform.position.x - position.x);
            bool creatureIsOneHitAwayFromDeathAndAlreadyTargeted = creature.GetCreatureTargeted() && creature.GetCreatureHealth() <= damage;

            if (!creature.GetCreatureCanBeTargeted()) continue;
            if (!canAttackFlying && creature.GetCreatureSO().flying) continue;

            if ((distanceToCreature) < radius) {
                // Creature is within attack range

                if (distanceToCreature > furthestXDistance && !creatureIsOneHitAwayFromDeathAndAlreadyTargeted) {
                    // Creature is the furthest one

                    furthestXDistance = Mathf.Abs(creature.transform.position.x - position.x);
                    closestCreatureInRadius = creature;
                }

            }

        }

        return closestCreatureInRadius;
    }

    public void AddCreatureSpawned(Creature creature) {
        creaturesSpawnedList.Add(creature);
    }

    public void RemoveCreatureSpawned(Creature creature) {
        creaturesSpawnedList.Remove(creature);
        RemoveCreatureFromNightWave(creature);
        TryRemoveCreatureAggroingPlayer(creature);
    }

    public int GetSpawnedCreatureCount() {
        return creaturesSpawnedList.Count;
    }

    public bool CreatureIsBetweenPositions(float initialPositionX, float destinationPositionX) {
        bool creatureIsBetweenPositions = false;

        float minPositionX = destinationPositionX;
        float maxPositionX = initialPositionX;

        if(initialPositionX < destinationPositionX) {
            minPositionX = initialPositionX;
            maxPositionX = destinationPositionX;
        }

        foreach(Creature creature in creaturesSpawnedList) {
            if(creature.transform.position.x >= minPositionX && creature.transform.position.x <= maxPositionX) {
                creatureIsBetweenPositions = true;
            }
        }

        return creatureIsBetweenPositions;
    }


    public void AddCreatureToNightWave(Creature creature) {
        if (creaturesSpawnedAtNightList.Contains(creature)) return;

        creaturesSpawnedAtNightList.Add(creature);
        OnCreatureAtNightSpawned?.Invoke(this, new OnCreatureAtNightKilledEventArgs {
            creature = creature,
        });
    }

    public void AddAdditionalCreatureToNightWave(Creature creature) {
        if (creaturesSpawnedAtNightList.Contains(creature)) return;

        creaturesSpawnedAtNightList.Add(creature);
        OnAdditionalCreatureAtNightSpawned?.Invoke(this, new OnCreatureAtNightKilledEventArgs {
            creature = creature,
        });
    }

    public void RemoveCreatureFromNightWave(Creature creature) {
        if (!creaturesSpawnedAtNightList.Contains(creature)) return;

        creaturesSpawnedAtNightList.Remove(creature);
        OnCreatureAtNightKilled?.Invoke(this, new OnCreatureAtNightKilledEventArgs {
            creature = creature,
        });

        if (creaturesSpawnedAtNightList.Count == 0 && CreaturesSpawnManager.Instance.GetAllNightCreaturesKilled()) {
            Debug.Log("OnAllCreaturesAtNightKilled");
            OnAllCreaturesAtNightKilled?.Invoke(this, EventArgs.Empty);
        }

        TryRemoveCreatureAggroingPlayer(creature);
    }

    public int GetNightCreaturesCloseToPlayerCamp(float distanceToCamp) {
        int creaturesCloseToCamp = 0;

        foreach(Creature creature in creaturesSpawnedAtNightList) {
            if(creature.transform.position.x > 0 && creature.transform.position.x < CampZoneManager.Instance.GetCampCenterMaxLimit() + distanceToCamp) {
                creaturesCloseToCamp++;
            }

            if(creature.transform.position.x < 0 && creature.transform.position.x > CampZoneManager.Instance.GetCampCenterMinLimit() - distanceToCamp) {
                creaturesCloseToCamp++;
            }
        }

        return creaturesCloseToCamp;
    }

    public bool GetCreatureAggroingPlayer() {
        return mobsAggroingPlayer.Count != 0;
    }

    private void OnDestroy() {
        CreatureAI.OnAnyCreatureAggro -= CreatureAI_OnAnyCreatureAggro;
        CreatureAI.OnAnyCreatureUntargetPlayer -= CreatureAI_OnAnyCreatureUntargetPlayer;
    }

}
