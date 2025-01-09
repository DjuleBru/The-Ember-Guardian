using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawnerContinuous_AnimatorManager : MonoBehaviour
{
    [SerializeField] private Animator spawnerAnimator;
    [SerializeField] private Animator bodyAnimator;
    private CreatureSpawnerContinuous spawner;

    private void Awake() {
        spawner = GetComponentInParent<CreatureSpawnerContinuous>();
    }

    private void Start() {
        spawner.OnSpawnerDamaged += Spawner_OnSpawnerDamaged;
        spawner.OnSpawnerDied += Spawner_OnSpawnerDied;
        spawner.OnSpawnerSpawnStart += OnSpawnerSpawnStart;
    }

    private void OnSpawnerSpawnStart(object sender, System.EventArgs e) {
        spawnerAnimator.SetTrigger("SpawnCreature");
    }

    private void Spawner_OnSpawnerDied(object sender, System.EventArgs e) {
        spawnerAnimator.SetTrigger("Die");

    }

    private void Spawner_OnSpawnerDamaged(object sender, System.EventArgs e) {
        bodyAnimator.SetTrigger("Hit");
    }

}
