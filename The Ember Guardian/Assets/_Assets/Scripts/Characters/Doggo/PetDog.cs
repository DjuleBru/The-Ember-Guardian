using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDog : MonoBehaviour
{
    public static PetDog Instance;

    [SerializeField] private Transform playerPetDogPosition;
    [SerializeField] private Transform playerPetDogPosition_DarkCompanion;
    private CircleCollider2D petCollider;
    private Transform currentPlayerPetDogPosition;
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

        petCollider = GetComponent<CircleCollider2D>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        Player.Instance.OnPlayerEnteredAnyInteractableTriggerArea += Player_OnPlayerEnteredAnyInteractableTriggerArea;
        Player.Instance.OnPlayerExitedAnyInteractableTriggerArea += Player_OnPlayerExitedAnyInteractableTriggerArea;
        Player.Instance.OnPlayerStartedInteractingWithAnyInteractable += Player_OnPlayerStartedInteractingWithAnyInteractable;
        Player.Instance.OnPlayerStoppedInteractingWithAnyInteractable += Player_OnPlayerStoppedInteractingWithAnyInteractable;
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;

        dogMovement = Dog.Instance.GetComponent<MobMovement>();

        RefreshPlayerPetPosition();
    }

    private void Dog_OnDogTypeChanged(object sender, EventArgs e) {
        RefreshPlayerPetPosition();
    }

    private void RefreshPlayerPetPosition() {
        if(Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            currentPlayerPetDogPosition = playerPetDogPosition_DarkCompanion;
        } else {
            currentPlayerPetDogPosition = playerPetDogPosition;
        }

    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (playerPettingDogOnCooldown) return;
        if (!playerCanPetDog) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammedAndNextInputSequence(GameInput.Binding.callDoggo)) return;

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

    public void StartPetDog() {
        playerPettingDog = true;

        dogMovement.SetMoveTarget(Player.Instance.transform.position);
        currentCoroutine = StartCoroutine(PetDogCoroutine());
    }

    private IEnumerator PetDogCoroutine() {

        dogMovement.SetCanMove(false);
        Player.Instance.SetPettingDog(true);
        petCollider.radius = 2f;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Player.Instance.transform.position = currentPlayerPetDogPosition.position;
        float playerWatchDir = Player.Instance.transform.position.x - transform.position.x;
        PlayerAim.Instance.SetXScale(playerWatchDir);
        OnPlayerStartedPettingDog?.Invoke(this, EventArgs.Empty);
        PlayerMovement.Instance.StopMovement();

        yield return new WaitForSeconds(petStartAnimationDuration);
        playerCanRefreshPettingDog = true;


        yield return new WaitForSeconds(petLoopAnimationDuration);
        playerCanRefreshPettingDog = false;


        OnPlayerStoppedPettingDog?.Invoke(this, EventArgs.Empty);
        petCollider.radius = .57f;
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

        DogAI.State dogState = Dog.Instance.GetCurrentDogAI().GetState();
        if (dogState == DogAI.State.stay || dogState == DogAI.State.stayAtCamp || dogState == DogAI.State.idle || dogState == DogAI.State.stayAtCamp || dogState == DogAI.State.stayAtCamp || dogState == DogAI.State.walkWithPlayer) {
            playerCanPetDog = true;
        }
    }

    public bool GetPlayerCanPetDog() {
        return playerCanPetDog;
    }

}
