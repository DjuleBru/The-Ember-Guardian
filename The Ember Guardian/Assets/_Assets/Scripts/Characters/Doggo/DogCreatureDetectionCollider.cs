using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogCreatureDetectionCollider : MonoBehaviour
{
    private List<Creature> creaturesInDetectionColliderRange = new List<Creature>();
    private bool creaturesInDetectionCollider;

    private void OnTriggerEnter2D(Collider2D collision) {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null) {
            if (!creaturesInDetectionColliderRange.Contains(creature)) {
                creaturesInDetectionColliderRange.Add(creature);
            }
            RefreshCreaturesInCollider();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null) {
            if (creaturesInDetectionColliderRange.Contains(creature)) {
                creaturesInDetectionColliderRange.Remove(creature);
            }
            RefreshCreaturesInCollider();
        }
    }


    private void RefreshCreaturesInCollider() {
        if (creaturesInDetectionColliderRange.Count > 0) {
            creaturesInDetectionCollider = true;
        }
        else {
            creaturesInDetectionCollider = false;
        }
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
