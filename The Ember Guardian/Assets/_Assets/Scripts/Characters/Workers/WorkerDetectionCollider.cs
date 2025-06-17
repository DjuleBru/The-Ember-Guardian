using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerDetectionCollider : MonoBehaviour
{
    private List<Creature> creaturesInDetectionColliderRange = new List<Creature>();
    private bool creaturesInDetectionCollider;
    private CircleCollider2D detectionCollider;
    private float initialDetectionColliderRadius;

    public event EventHandler OnCreaturesInColliderChanged;

    private void Awake() {
        detectionCollider = GetComponent<CircleCollider2D>();
        initialDetectionColliderRadius = detectionCollider.radius;
        RandomizeDetectionColliderRadius();
    }

    private void RandomizeDetectionColliderRadius() {
        float radius = detectionCollider.radius;
        float radiusRandomized = radius + UnityEngine.Random.Range(-radius/10,radius/10);
        detectionCollider.radius = radiusRandomized;
    }

    public void SetDetectionColliderRadius(float radius) {
        detectionCollider.radius = radius;
    }
    public void ResetDetectionColliderRadius() {
        detectionCollider.radius = initialDetectionColliderRadius;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        CreatureSpawner_Ambush ambushSpawner = collision.gameObject.GetComponent<CreatureSpawner_Ambush>();

        if(creature != null) {
            if(!creaturesInDetectionColliderRange.Contains(creature)) {
                creaturesInDetectionColliderRange.Add(creature);
                RefreshCreaturesInCollider();
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

    }

    private void RefreshCreaturesInCollider() {
        if(creaturesInDetectionColliderRange.Count > 0) {
            creaturesInDetectionCollider = true;
        } else {
            creaturesInDetectionCollider = false;
        }

        OnCreaturesInColliderChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CreaturesInDetectionCollider() {
        return creaturesInDetectionCollider;
    }

    public Creature GetClosestCreature() {
        Creature closestCreature = null;
        float distanceToClosestCreature = Mathf.Infinity;

        foreach(Creature creature in creaturesInDetectionColliderRange) {
            float distanceToCreature = Mathf.Abs(creature.transform.position.x - transform.position.x);
            if (distanceToCreature < distanceToClosestCreature) {
                distanceToClosestCreature = distanceToCreature;
                closestCreature = creature;
            }
        }

        return closestCreature;
    }

    public List<Creature> GetCreaturesInDetectionCollider() {
        return creaturesInDetectionColliderRange;
    }
}
