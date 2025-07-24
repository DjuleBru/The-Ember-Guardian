using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorkerSpawnerVisual : MonoBehaviour
{
    private MobSpawner mobSpawner;
    private Animator animator;
    [SerializeField] private List<Light2D> lightList;
    [SerializeField] private Animator lightAnimator;

    private void Awake() {
        animator = GetComponent<Animator>();
        mobSpawner = GetComponentInParent<MobSpawner>();
        mobSpawner.OnAllMobRemoved += MobSpawner_OnAllMobRemoved;
        mobSpawner.OnMobSpawned += MobSpawner_OnMobSpawned;
        animator.enabled = false;
    }

    private void MobSpawner_OnMobSpawned(object sender, MobSpawner.OnMobSpawnedEventArgs e) {
        if (lightAnimator != null) {
            lightAnimator.ResetTrigger("Off");
            lightAnimator.SetTrigger("On");
        }

        if(lightList.Count != 0) {
            foreach(Light2D light in lightList) {
                light.enabled = true;
            }
        }
    }

    private void MobSpawner_OnAllMobRemoved(object sender, System.EventArgs e) {
        if(mobSpawner.GetMaxMobAmountSpawnedAtDawn() == 0) {
            animator.enabled = true;
        }

        if(lightAnimator != null) {
            lightAnimator.ResetTrigger("On");
            lightAnimator.SetTrigger("Off");
        }

        if (lightList.Count != 0) {
            foreach (Light2D light in lightList) {
                light.enabled = false;
            }
        }
    }
}
