using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    public static Dog Instance;

    public enum DogType {
        GermanShepherd,
        GoldenRetreiver,
        DarkCompanion,
    }
    public DogType dogType;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;

    private List<DogAI> dogAIList = new List<DogAI>();
    private DogAI currentDogAI;
    [SerializeField] private bool useDebugDogType;
    [SerializeField] private Dog.DogType debugDogType;
    [SerializeField] private DogAI.State initialIdleState;
    [SerializeField] private DogAI.State initialState;
    private DogAI.State currentIdleState;

    public event EventHandler OnIdleStateChanged;
    public event EventHandler OnPlayerCalledDog;
    public event EventHandler OnDogTypeChanged;

    private void Awake() {
        Instance = this; 
        
        DogAI[] dogAIArray = GetComponents<DogAI>();
        foreach (DogAI dogAI in dogAIArray) {
            dogAIList.Add(dogAI);
        }

        dogType = ES3.Load("dogType", DogType.GermanShepherd);

        if (useDebugDogType) {
            dogType = debugDogType;
        }
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        currentIdleState = initialIdleState;



        SetCurrentDogAI();

        OnDogTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetCurrentDogAI() {
        foreach (DogAI dogAI in dogAIList) {
            dogAI.enabled = false;
            if (dogAI.GetDogAIType() == dogType) {
                dogAI.enabled = true;
                currentDogAI = dogAI;
            }
        }
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammed()) return;

        if (currentIdleState == DogAI.State.walkWithPlayer) {

            currentIdleState = DogAI.State.stay;

        } else if (currentIdleState == DogAI.State.stay || currentIdleState == DogAI.State.idle) {

            currentIdleState = DogAI.State.walkWithPlayer;
            OnPlayerCalledDog?.Invoke(this, EventArgs.Empty);

        }

        currentDogAI.SetIdleBehaviorState(currentIdleState);
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
        SetPosition(teleporterPlayerPosition.position);

        // To make the dog sit down
        currentDogAI.SetState(DogAI.State.idle);
        currentDogAI.SetState(DogAI.State.stay);
    }
    public void MoveOutTeleporter() {
        currentDogAI.SetState(GetIdleState());
    }

    public DogAI.State GetIdleState() {
        return currentIdleState;
    }

    public DogAI.State GetInitialState() {
        return initialState;
    }

    public void SetPosition(Vector3 position) {
        transform.position = position;
        GetComponent<MobMovement>().SetMoveTarget(position);
        currentDogAI.SetInitialPosition(position);
    }

    public DogType GetDogType() {
        return dogType;
    }

    public void SetDogType(DogType dogType) {
        this.dogType = dogType;
        SetCurrentDogAI();
        OnDogTypeChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log("SetDogType " + dogType);
    }

    public List<DogAI> GetDogAIList() {
        return dogAIList;
    }

    public DogAI GetCurrentDogAI() {
        return currentDogAI;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}
