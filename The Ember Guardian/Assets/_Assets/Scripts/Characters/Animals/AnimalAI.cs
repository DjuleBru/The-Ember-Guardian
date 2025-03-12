using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAI : MonoBehaviour
{
    private Animal animal;
    private MobMovement animalMovement;

    private Vector3 positionToRoamAmound;

    private float roamMoveSpeed;
    private float fleeMoveSpeed;
    private float fleeDistance;

    private float roamChangeDestinationRate;
    private float roamRadius;

    private float roamTimer;

    private bool hasSetSpeed;
    private bool hasSetFleeDestination;

    private bool isSafe = true;

    public event EventHandler OnAnimalReachedSafeZone;

    private void Awake() {
        animal = GetComponent<Animal>();
        animal.OnMobDamageTaken += Animal_OnMobDamageTaken;
        animal.OnAnimalHitObstacle += Animal_OnAnimalHitObstacle;

    }

    private void Start() {

        roamMoveSpeed = animal.GetAnimalSO().roamMoveSpeed;
        fleeMoveSpeed = animal.GetAnimalSO().fleeMoveSpeed;
        fleeDistance = animal.GetAnimalSO().fleeDistance;
        roamChangeDestinationRate = animal.GetAnimalSO().roamChangeDestinationRate;
        roamRadius = animal.GetAnimalSO().roamRadius;

        positionToRoamAmound = animal.GetMobSpawner().transform.position;

        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
    }

    private void Update() {

        if (isSafe) {
            Roam();
        }

    }

    private void Animal_OnAnimalHitObstacle(object sender, EventArgs e) {
        Debug.Log("animal hit obstacle");

        isSafe = true;
        animalMovement.SetMoveSpeed(roamMoveSpeed);
        OnAnimalReachedSafeZone?.Invoke(this, EventArgs.Empty);
        positionToRoamAmound = animal.GetMobSpawner().transform.position;

        animalMovement.SetMoveTarget(positionToRoamAmound);
    }

    private void AnimalMovement_OnDestinationReached(object sender, System.EventArgs e) {
        isSafe = true;
        animalMovement.SetMoveSpeed(roamMoveSpeed);
        OnAnimalReachedSafeZone?.Invoke(this, EventArgs.Empty);
    }

    private void Animal_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        isSafe = false;

        positionToRoamAmound = FleeBehavior.GetFleeFromTargetDestination(animalMovement, e.damageOriginTransform.position, fleeDistance);
        animalMovement.SetMoveTarget(positionToRoamAmound);
        animalMovement.SetMoveSpeed(fleeMoveSpeed);
    }


    public void Roam() {
        if (!hasSetSpeed) {
            animalMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(animal.GetComponent<MobMovement>(), roamRadius, positionToRoamAmound, false);
        }
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {
        positionToRoamAmound = animal.GetMobSpawner().transform.position;
    }

    private void OnEnable() {
        animalMovement = GetComponent<MobMovement>();
        animal = GetComponent<Animal>();
        animalMovement.OnDestinationReached += AnimalMovement_OnDestinationReached;
    }

    private void OnDisable() {
        hasSetSpeed = false;
        animalMovement.OnDestinationReached -= AnimalMovement_OnDestinationReached;
    }
}
