using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI : MonoBehaviour {

    private Creature creature;
    private MobMovement mobMovement;

    private float moveSpeed;

    private void Awake() {
        mobMovement = GetComponent<MobMovement>();
        creature = GetComponent<Creature>();
    }

    private void Start() {
        moveSpeed = creature.GetCreatureSO().moveSpeed;
        mobMovement.SetMoveSpeed(moveSpeed);

        mobMovement.OnDestinationReached += MobMovement_OnDestinationReached;
    }

    private void Update() {
        mobMovement.SetMoveTarget(Vector3.zero);
    }

    private void MobMovement_OnDestinationReached(object sender, System.EventArgs e) {

    }
}
