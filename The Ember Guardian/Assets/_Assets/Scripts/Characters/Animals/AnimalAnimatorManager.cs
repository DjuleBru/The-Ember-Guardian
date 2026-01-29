using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAnimatorManager : MonoBehaviour
{
    private Animal animal;
    private AnimalAI animalAI;
    private Animator animator;
    private MobMovement animalMovement;

    private float moveDir;
    private float previousMoveDir = 1f;

    [SerializeField] private float probabilityToTriggerIdleSound;
    public event EventHandler OnFootstepTriggered;
    public event EventHandler OnIdleSoundTriggered;

    private void Awake() {
        animator = GetComponent<Animator>();
        animal = GetComponentInParent<Animal>();
        animalAI = GetComponentInParent<AnimalAI>();
        animalMovement = GetComponentInParent<MobMovement>();
    }

    private void Start() {
        animal.OnMobDied += Animal_OnMobDied;
        animal.OnMobDamageTaken += Animal_OnMobDamageTaken;
        animalAI.OnAnimalReachedSafeZone += AnimalAI_OnAnimalReachedSafeZone;
        animalMovement.OnDestinationReached += AnimalMovement_OnDestinationReached;
        animalMovement.OnDestinationSet += AnimalMovement_OnDestinationSet;

        animal.OnAnimalPaused += Animal_OnAnimalPaused;
        animal.OnAnimalUnpaused += Animal_OnAnimalUnpaused;
    }

    private void Animal_OnAnimalUnpaused(object sender, EventArgs e) {
        animator.speed = 1f; // resume
    }

    private void Animal_OnAnimalPaused(object sender, EventArgs e) {
        animator.speed = 0f; // resume
    }

    private void AnimalMovement_OnDestinationSet(object sender, System.EventArgs e) {
        animator.SetBool("Walking", true);
    }

    private void AnimalMovement_OnDestinationReached(object sender, System.EventArgs e) {
        animator.SetBool("Walking", false);
    }

    private void Update() {

        moveDir = animalMovement.GetMoveDirFloat();

        HandleXScale();
        //HandleAnimatorMovementBool();
    }

    private void HandleXScale() {

        if (moveDir < 0 && previousMoveDir > 0) {
            previousMoveDir = moveDir;
            Vector3 newScale = new Vector3(-1, 1, 1);
            transform.localScale = newScale;
        }

        if (moveDir > 0 && previousMoveDir < 0) {
            previousMoveDir = moveDir;
            Vector3 newScale = new Vector3(1, 1, 1);
            transform.localScale = newScale;
        }
    }

    private void Animal_OnMobDied(object sender, System.EventArgs e) {
        animator.speed = 1f;
        animator.SetTrigger("Die");
    }

    private void Animal_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        animator.speed = 2f;
    }

    private void AnimalAI_OnAnimalReachedSafeZone(object sender, System.EventArgs e) {
        animator.speed = 1f;
    }

    public void SetReadyToMove() {
        animalMovement.SetReadyToMoveAnimator(true);
    }

    public void SetUnReadyToMove() {
        animalMovement.SetReadyToMoveAnimator(false);
    }

    public void TriggerFootStep() {
        OnFootstepTriggered?.Invoke(this, EventArgs.Empty);
    }

    public void TryTriggerIdleAudio() {
        float randomFloat = UnityEngine.Random.Range(0f, 1f);

        if (randomFloat < probabilityToTriggerIdleSound) {
            OnIdleSoundTriggered?.Invoke(this, EventArgs.Empty);
        }
    }

}
