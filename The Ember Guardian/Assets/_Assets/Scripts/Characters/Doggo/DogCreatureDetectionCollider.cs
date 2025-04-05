using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogCreatureDetectionCollider : MonoBehaviour
{
    private List<Creature> creaturesInDetectionColliderRange = new List<Creature>();
    private List<CreatureSpawner_Ambush> creaturesAmbushSpawnersInDetectionColliderRange = new List<CreatureSpawner_Ambush>();
    private bool creaturesInDetectionCollider;
    private bool ambushSpawnersInDetectionCollider;
    private CircleCollider2D detectionCollider;

    private float detectionColliderRadius_Day = 14f;
    private float detectionColliderRadius_Night = 7f;

    private void Awake() {
        detectionCollider = GetComponent<CircleCollider2D>();
    }
    private void Start() {
        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        }
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        detectionCollider.radius = detectionColliderRadius_Day;
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        detectionCollider.radius = detectionColliderRadius_Night;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        CreatureSpawner_Ambush ambushSpawner = collision.gameObject.GetComponent<CreatureSpawner_Ambush>();

        if (creature != null) {
            if (!creaturesInDetectionColliderRange.Contains(creature)) {
                creaturesInDetectionColliderRange.Add(creature);
                RefreshCreaturesInCollider();
            }
        }

        if (ambushSpawner != null) {
            if (!DogStats.Instance.GetDetectAmbushAbilityUnlocked()) return;

            float probabilityToDetectAmbush = DogStats.Instance.GetDetectAmbushProbability()/100f;
            float randomFloat = Random.Range(0f, 1f);
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
        if (creaturesInDetectionColliderRange.Count > 0) {
            creaturesInDetectionCollider = true;
        }
        else {
            creaturesInDetectionCollider = false;
        }

        if (creaturesAmbushSpawnersInDetectionColliderRange.Count > 0) {
            ambushSpawnersInDetectionCollider = true;
        }
        else {
            ambushSpawnersInDetectionCollider = false;
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
            float distanceToCreature = Mathf.Abs(creature.transform.position.x - transform.position.x);
            if (distanceToCreature < distanceToClosestCreature) {
                distanceToClosestCreature = distanceToCreature;
                closestCreature = creature;
            }
        }

        return closestCreature;
    }

    public float GetClosestCreatureDistance() {
        Creature closestCreature = null;
        float distanceToClosestCreature = Mathf.Infinity;

        foreach (Creature creature in creaturesInDetectionColliderRange) {
            float distanceToCreature = Mathf.Abs(creature.transform.position.x - transform.position.x);
            if (distanceToCreature < distanceToClosestCreature) {
                distanceToClosestCreature = distanceToCreature;
                closestCreature = creature;
            }
        }

        return distanceToClosestCreature;
    }
}
