using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDog : MonoBehaviour
{
    public static PetDog Instance;

    [SerializeField] private DogAI dogAI;
    [SerializeField] private Transform playerPetDogPosition;
    private MobMovement dogMovement;

    private bool playerInTriggerArea;
    private bool playerCanPetDog;
    private bool playerCanRefreshPettingDog;
    private bool playerPettingDog;
    private bool playerPettingDogOnCooldown;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnPlayerCantPetDog;
    public event EventHandler OnPlayerStartedPettingDog;
    public event EventHandler OnPlayerRefreshedPettingDog;
    public event EventHandler OnPlayerStoppedPettingDog;
    public event EventHandler OnPlayerEndedPettingDog;

    private float petStartAnimationDuration = .6f;
    private float petLoopAnimationDuration = 1.6f;
    private float petEndAnimationDuration = .8f;
    private float playerPettingDogCooldown = 1f;

    private Coroutine currentCoroutine;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        Player.Instance.OnPlayerEnteredAnyInteractableTriggerArea += Player_OnPlayerEnteredAnyInteractableTriggerArea;
        Player.Instance.OnPlayerExitedAnyInteractableTriggerArea += Player_OnPlayerExitedAnyInteractableTriggerArea;
        Player.Instance.OnPlayerStartedInteractingWithAnyInteractable += Player_OnPlayerStartedInteractingWithAnyInteractable;
        Player.Instance.OnPlayerStoppedInteractingWithAnyInteractable += Player_OnPlayerStoppedInteractingWithAnyInteractable;
        dogMovement = dogAI.GetComponent<MobMovement>();
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (playerPettingDogOnCooldown) return;
        if (!playerCanPetDog) return;
        if (PlayerShoot.Instance.GetActionBlockedByJammedGun(GameInput.Binding.callDoggo)) return;

        if (!playerPettingDog) {
            StartPetDog();
        }
        else {
            if (playerCanRefreshPettingDog) {
                StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(RefreshPetDogCoroutine());
            }
        }
    }

    private void Player_OnPlayerStoppedInteractingWithAnyInteractable(object sender, EventArgs e) {
        RefreshCanPetDog();
    }

    private void Player_OnPlayerStartedInteractingWithAnyInteractable(object sender, EventArgs e) {
        RefreshCanPetDog();
    }

    private void Player_OnPlayerExitedAnyInteractableTriggerArea(object sender, EventArgs e) {
        RefreshCanPetDog();
    }

    private void Player_OnPlayerEnteredAnyInteractableTriggerArea(object sender, EventArgs e) {
        RefreshCanPetDog();
    }

    private void StartPetDog() {
        playerPettingDog = true;

        dogMovement.SetMoveTarget(Player.Instance.transform.position);
        currentCoroutine = StartCoroutine(PetDogCoroutine());
    }

    private IEnumerator PetDogCoroutine() {

        dogMovement.SetCanMove(false);
        Player.Instance.SetPettingDog(true);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Player.Instance.transform.position = playerPetDogPosition.position;
        float playerWatchDir = Player.Instance.transform.position.x - transform.position.x;
        PlayerAim.Instance.SetXScale(playerWatchDir);
        OnPlayerStartedPettingDog?.Invoke(this, EventArgs.Empty);
        PlayerMovement.Instance.StopMovement();

        yield return new WaitForSeconds(petStartAnimationDuration);
        playerCanRefreshPettingDog = true;


        yield return new WaitForSeconds(petLoopAnimationDuration);
        playerCanRefreshPettingDog = false;


        OnPlayerStoppedPettingDog?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(petEndAnimationDuration);

        OnPlayerEndedPettingDog?.Invoke(this, EventArgs.Empty);
        dogMovement.SetCanMove(true);
        Player.Instance.SetPettingDog(false);
        playerPettingDog = false;
        playerPettingDogOnCooldown = true;

        yield return new WaitForSeconds(playerPettingDogCooldown);

        playerPettingDogOnCooldown = false;
    }

    private IEnumerator RefreshPetDogCoroutine() {

        OnPlayerRefreshedPettingDog?.Invoke(this, EventArgs.Empty);
        playerCanRefreshPettingDog = true;
        
        yield return new WaitForSeconds(petLoopAnimationDuration);


        playerCanRefreshPettingDog = false;

        OnPlayerStoppedPettingDog?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(petEndAnimationDuration);

        playerPettingDog = false;
        playerPettingDogOnCooldown = true;

        OnPlayerEndedPettingDog?.Invoke(this, EventArgs.Empty);
        dogMovement.SetCanMove(true);
        Player.Instance.SetPettingDog(false);


        yield return new WaitForSeconds(playerPettingDogCooldown);

        playerPettingDogOnCooldown = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        playerInTriggerArea = true;

        RefreshCanPetDog();

        if(playerCanPetDog) {
            Player.Instance.SetInPetDogTriggerArea(true);
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
            playerInTriggerArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetInPetDogTriggerArea(false);
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = false;
        playerCanPetDog = false;
    }

    private void RefreshCanPetDog() {
        if (playerPettingDogOnCooldown) {
            playerCanPetDog = false;
            return;
        }

        if (!playerInTriggerArea) {
            playerCanPetDog = false;
            return; 
        }

        if (!Player.Instance.GetCanPetDog()) {
            playerCanPetDog = false;
            OnPlayerCantPetDog?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (PlayerMovement.Instance.GetRunning()) {
            playerCanPetDog = false;
            OnPlayerCantPetDog?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (CreaturesManager.Instance != null) {
            if (CreaturesManager.Instance.GetCreatureAggroingPlayer()) {
                playerCanPetDog = false;
                OnPlayerCantPetDog?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        if (dogAI.GetState() == DogAI.State.stay || dogAI.GetState() == DogAI.State.stayAtCamp || dogAI.GetState() == DogAI.State.idle || dogAI.GetState() == DogAI.State.stayAtCamp || dogAI.GetState() == DogAI.State.stayAtCamp || dogAI.GetState() == DogAI.State.walkWithPlayer) {
            playerCanPetDog = true;
        }
    }

}
