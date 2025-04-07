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
    private bool playerCanRefreshPettingDog;
    private bool playerPettingDog;
    private bool playerPettingDogOnCooldown;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnPlayerStartedPettingDog;
    public event EventHandler OnPlayerRefreshedPettingDog;
    public event EventHandler OnPlayerStoppedPettingDog;
    public event EventHandler OnPlayerEndedPettingDog;

    private float petStartAnimationDuration = .3f;
    private float petLoopAnimationDuration = .5f;
    private float petEndAnimationDuration = .5f;
    private float playerPettingDogCooldown = 1f;

    private Coroutine currentCoroutine;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        dogMovement = dogAI.GetComponent<MobMovement>();
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        if (playerPettingDogOnCooldown) return;

        if(!playerPettingDog) {
            StartPetDog();
        } else {
            if(playerCanRefreshPettingDog) {
                StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(RefreshPetDogCoroutine());
            }
        }
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

        float playerWatchDir = Player.Instance.transform.position.x - transform.position.x;
        PlayerAim.Instance.SetXScale(playerWatchDir);
        Player.Instance.transform.position = playerPetDogPosition.position;
        OnPlayerStartedPettingDog?.Invoke(this, EventArgs.Empty);


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
        if (!Player.Instance.GetCanDropOrbOnTheFloor()) return;

        if (dogAI.GetState() == DogAI.State.stay || dogAI.GetState() == DogAI.State.stayAtCamp || dogAI.GetState() == DogAI.State.idle || dogAI.GetState() == DogAI.State.stayAtCamp || dogAI.GetState() == DogAI.State.stayAtCamp || dogAI.GetState() == DogAI.State.walkWithPlayer) {
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
            playerInTriggerArea = true;
            Player.Instance.SetInPetDogTriggerArea(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        Player.Instance.SetInPetDogTriggerArea(false);

        if (!Player.Instance.GetCanDropOrbOnTheFloor()) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = false;
    }

}
