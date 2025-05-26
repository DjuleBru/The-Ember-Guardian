using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCreatureDetectionCollider : MonoBehaviour
{
    public static PlayerCreatureDetectionCollider Instance;

    private List<Creature> creaturesInDetectionCollider = new List<Creature>();
    public event EventHandler OnCreatureDetected;
    public event EventHandler OnCloseCreaturesCleared;

    private void Awake() {
        Instance = this;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Creature creature = collision.GetComponent<Creature>();
        if(creature != null && collision.GetComponent<CreatureDetectionCollider>()  == null) {
            if (creaturesInDetectionCollider.Contains(creature)) return;

            if (creaturesInDetectionCollider.Count == 0) {
                OnCreatureDetected?.Invoke(this, EventArgs.Empty);
            }

            creaturesInDetectionCollider.Add(creature);

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Creature creature = collision.GetComponent<Creature>();
        if (creature != null && collision.GetComponent<CreatureDetectionCollider>() == null) {
            if (!creaturesInDetectionCollider.Contains(creature)) return;
            creaturesInDetectionCollider.Remove(creature);

            if(creaturesInDetectionCollider.Count == 0) {
                OnCloseCreaturesCleared?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
