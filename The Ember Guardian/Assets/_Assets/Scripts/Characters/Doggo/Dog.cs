using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    public static Dog Instance;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;

    private DogAI dogAI;
    public DogAI.State currentIdleState;

    public event EventHandler OnIdleStateChanged;
    public event EventHandler OnPlayerCalledDog;

    private void Awake() {
        Instance = this;
        dogAI = GetComponent<DogAI>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        currentIdleState = DogAI.State.idle;
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (Player.Instance.GetInteractingWithMerchant()) return;

        if (currentIdleState == DogAI.State.walkWithPlayer) {

            currentIdleState = DogAI.State.stay;

        } else if (currentIdleState == DogAI.State.stay || currentIdleState == DogAI.State.idle) {

            currentIdleState = DogAI.State.walkWithPlayer;
            OnPlayerCalledDog?.Invoke(this, EventArgs.Empty);

        }

        dogAI.SetBaseState(currentIdleState);
        OnIdleStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Player>() != null) {
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        }
    }

    public void MoveOnTeleporter(Transform teleporterPlayerPosition) {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = teleporterPlayerPosition.position;

        // To make the dog sit down
        dogAI.SetState(DogAI.State.idle);
        dogAI.SetState(DogAI.State.stay);
    }

    public DogAI.State GetIdleState() {
        return currentIdleState;
    }

    public void SetPosition(Vector3 position) {
        transform.position = position;
        GetComponent<MobMovement>().SetMoveTarget(position);
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}
