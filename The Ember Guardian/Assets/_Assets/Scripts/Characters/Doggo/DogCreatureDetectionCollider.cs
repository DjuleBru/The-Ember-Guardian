using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogCreatureDetectionCollider : MonoBehaviour
{
    private List<Creature> creaturesInDetectionColliderRange = new List<Creature>();
    private List<CreatureSpawner_Ambush> creaturesAmbushSpawnersInDetectionColliderRange = new List<CreatureSpawner_Ambush>();
    private bool creaturesInDetectionCollider;
    private bool ambushSpawnersInDetectionCollider;
    private bool isHub;
    private CircleCollider2D detectionCollider;

    private float detectionColliderRadius_Day = 14f;
    private float detectionColliderRadius_Night = 7f;

    public event EventHandler OnAmbushDetected;
    public event EventHandler OnNoAmbushDetected;

    private void Awake() {
        detectionCollider = GetComponent<CircleCollider2D>();

    }
    private void Start() {
        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        }
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            isHub = true;
        }
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        detectionCollider.radius = detectionColliderRadius_Day;
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        detectionCollider.radius = detectionColliderRadius_Night;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (isHub) return;

        Creature creature = collision.gameObject.GetComponent<Creature>();
        CreatureSpawner_Ambush ambushSpawner = collision.gameObject.GetComponent<CreatureSpawner_Ambush>();

        if (creature != null) {
            if (!creaturesInDetectionColliderRange.Contains(creature)) {
                creaturesInDetectionColliderRange.Add(creature);
                RefreshCreaturesInCollider();
            }
        }

        if (ambushSpawner != null) {
            if (!DogStats.Instance.GetGermanShepherdDetectAmbushAbilityUnlocked()) return;
            if (Dog.Instance.GetDogType() != Dog.DogType.GermanShepherd) return;

            float probabilityToDetectAmbush = DogStats.Instance.GetGermanShepherdDetectAmbushProbability()/100f;
            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            Debug.Log("randomFloat " + randomFloat + " probabilityToDetectAmbush " + probabilityToDetectAmbush);

            if(randomFloat <= probabilityToDetectAmbush) {
                if (!creaturesAmbushSpawnersInDetectionColliderRange.Contains(ambushSpawner)) {
                    creaturesAmbushSpawnersInDetectionColliderRange.Add(ambushSpawner);
                    RefreshCreaturesInCollider();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (isHub) return;
        Creature creature = collision.gameObject.GetComponent<Creature>();
        CreatureSpawner_Ambush ambushSpawner = collision.gameObject.GetComponent<CreatureSpawner_Ambush>();

        if (creature != null) {
            if (creaturesInDetectionColliderRange.Contains(creature)) {
                creaturesInDetectionColliderRange.Remove(creature);
                RefreshCreaturesInCollider();
            }
        }

        if (ambushSpawner != null) {
            if (creaturesAmbushSpawnersInDetectionColliderRange.Contains(ambushSpawner)) {
                creaturesAmbushSpawnersInDetectionColliderRange.Remove(ambushSpawner);
                RefreshCreaturesInCollider();
            }
        }

    }

    private void RefreshCreaturesInCollider() {
        creaturesInDetectionColliderRange.RemoveAll(c => c == null);

        if (creaturesInDetectionColliderRange.Count > 0) {
            creaturesInDetectionCollider = true;
        }
        else {
            creaturesInDetectionCollider = false;
        }


        if (creaturesAmbushSpawnersInDetectionColliderRange.Count > 0) {
            ambushSpawnersInDetectionCollider = true;
            OnAmbushDetected?.Invoke(this, EventArgs.Empty);
        }
        else {
            ambushSpawnersInDetectionCollider = false;
            OnNoAmbushDetected?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CreaturesInDetectionCollider() {
        return creaturesInDetectionCollider;
    }

    public bool AmbushSpawnersInDetectionCollider() {
        return ambushSpawnersInDetectionCollider;
    }

    public Creature GetClosestCreature() {
        Creature closestCreature = null;
        float distanceToClosestCreature = Mathf.Infinity;

        foreach (Creature creature in creaturesInDetectionColliderRange) {
            if (creature == null) continue;

            if (creature.transform.position.x > LevelManager.Instance.GetMaxLevelLimit()) continue;
            if (creature.transform.position.x < LevelManager.Instance.GetMinLevelLimit()) continue;

            float distanceToCreature = Mathf.Abs(creature.transform.position.x - transform.position.x);
            if (distanceToCreature < distanceToClosestCreature) {
                distanceToClosestCreature = distanceToCreature;
                closestCreature = creature;
            }
        }

        return closestCreature;
    }

    public Creature GetCreatureWithHighestLocalDensity(float radius = 4f) {
        Creature densestCreature = null;
        int maxNeighborCount = -1;

        foreach (Creature candidate in creaturesInDetectionColliderRange) {
            if (candidate == null) continue;

            int neighborCount = 0;

            foreach (Creature other in creaturesInDetectionColliderRange) {
                if (other == null) continue;
                if (other == candidate) continue;

                float distance = Vector2.Distance(candidate.transform.position, other.transform.position);
                if (distance <= radius) {
                    neighborCount++;
                }
            }

            if (neighborCount > maxNeighborCount) {
                maxNeighborCount = neighborCount;
                densestCreature = candidate;
            }
        }

        return densestCreature;
    }
    public float GetClosestCreatureDistance() {
        Creature closestCreature = null;
        float distanceToClosestCreature = Mathf.Infinity;

        foreach (Creature creature in creaturesInDetectionColliderRange) {
            if (creature == null) continue;

            float distanceToCreature = Mathf.Abs(creature.transform.position.x - transform.position.x);
            if (distanceToCreature < distanceToClosestCreature) {
                distanceToClosestCreature = distanceToCreature;
                closestCreature = creature;
            }
        }

        return distanceToClosestCreature;
    }

    public List<Creature> GetCreaturesInRange() {
        return creaturesInDetectionColliderRange;
    }
}
