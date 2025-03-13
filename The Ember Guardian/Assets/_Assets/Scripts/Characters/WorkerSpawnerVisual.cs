using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerSpawnerVisual : MonoBehaviour
{
    private MobSpawner mobSpawner;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
        mobSpawner = GetComponentInParent<MobSpawner>();
        mobSpawner.OnAllMobRemoved += MobSpawner_OnAllMobRemoved;
        animator.enabled = false;
    }

    private void MobSpawner_OnAllMobRemoved(object sender, System.EventArgs e) {
        if(mobSpawner.GetMaxMobAmountSpawnedAtDawn() == 0) {
            animator.enabled = true;
        }
    }
}
