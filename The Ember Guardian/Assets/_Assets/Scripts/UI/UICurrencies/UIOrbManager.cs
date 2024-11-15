using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOrbManager : MonoBehaviour
{

    public static UIOrbManager Instance;

    [SerializeField] private Transform blueOrbsSpawnPosition;
    [SerializeField] private Transform smallBlueOrbsContainer;
    [SerializeField] private Transform bigBlueOrbsContainer;
    [SerializeField] private Transform blueOrbUIPrefab;
    [SerializeField] private Transform smallBlueOrbUIPrefab;

    [SerializeField] private int smallOrbValue = 5;

    [SerializeField] private float smallOrbSmoothTime = 5f;
    private float bigOrbSmoothTime = 1f;

    SmallOrb_UI[] smallOrbsInCauldron;
    BigOrb_UI[] bigOrbsInCauldron;
    List<SmallOrb_UI> smallOrbsFormingBigOrb = new List<SmallOrb_UI>();

    private bool payingOrb;
    private bool formingBigOrb;
    private bool formingBigOrbCanceled;

    private float timeBetweenSmallOrbsPickup = .2f;
    private float smallOrbsPickupTimer;
    int smallOrbIndex;
    int smallOrbsNecessaryToFormBigOrb = 5;

    private bool droppingBigOrb;
    public event EventHandler OnBigBlueOrbDropped;

    private bool tryingToDropOrb;
    private bool tryingToFormOrb;
    private float tryingToDropOrbTimer;
    private float tryingToDropOrbHoldTime = .2f;

    int debugInitialBigOrbs = 2;
    int debugInitialSmallOrbs = 8;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;

        //for (int i = 0; i< debugInitialBigOrbs; i++) {
        //    AddBigBlueOrb();
        //}
        for (int i = 0; i < debugInitialSmallOrbs; i++) {
            AddSmallBlueOrb();
        }
    }

    private void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        tryingToDropOrb = true;
        tryingToDropOrbTimer = 0;
        formingBigOrbCanceled = false;
    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {

        if (tryingToDropOrbTimer < tryingToDropOrbHoldTime) {
            // Player pressed Interact once

            if (Player.Instance.GetCanDropOrbOnTheFloor()) {
                if (GetCanDropBigOrb()) {
                    DropBigOrb();
                }
            }
        } else {
            // Player was holding Interact
            CancelOrbFormation();
        }

        tryingToDropOrb = false;
    }

    private void Update() {

        if (tryingToDropOrb) {
            tryingToDropOrbTimer += Time.deltaTime;

            if (tryingToDropOrbTimer > tryingToDropOrbHoldTime && !formingBigOrb && !formingBigOrbCanceled) {
                TryFormBigOrb(true);
            }

        }

        //if(Input.GetKeyDown(KeyCode.O)) {
        //    TryPayOrb();
        //}

        //if (Input.GetKeyDown(KeyCode.P)) {
        //    TryDropOrb();
        //}

        //if (Input.GetKeyDown(KeyCode.S)) {
        //    CancelOrbFormation();
        //}

        if (formingBigOrb) {
            FormBigOrbWithSmallOrb();
        }
    }

    private void FormBigOrbWithSmallOrb() {

        if (smallOrbIndex < 0) {
            CancelOrbFormation();
        }

        smallOrbsPickupTimer -= Time.deltaTime;

        if (smallOrbsPickupTimer <= 0) {

            smallOrbsInCauldron = smallBlueOrbsContainer.GetComponentsInChildren<SmallOrb_UI>();
            smallOrbsInCauldron[smallOrbIndex].SetFormingBigOrb(true);
            smallOrbsInCauldron[smallOrbIndex].SetDestination(blueOrbsSpawnPosition.position, smallOrbSmoothTime);
            smallOrbsFormingBigOrb.Add(smallOrbsInCauldron[smallOrbIndex]);

            if (smallOrbsFormingBigOrb.Count < smallOrbsNecessaryToFormBigOrb) {

                // Next small orb
                smallOrbsPickupTimer = timeBetweenSmallOrbsPickup;
                smallOrbIndex--;

            }
            else {
                // Enough small orbs extracted : FORM BIG ORB
            }
        }
    }

    public void CancelOrbFormation() {
        formingBigOrbCanceled = true;
        formingBigOrb = false;

        foreach (SmallOrb_UI smallOrb in smallOrbsFormingBigOrb) {
            // Not enough small orbs extracted : CANCEL
            smallOrb.SetFormingBigOrb(false);
        }
        smallOrbsFormingBigOrb.Clear();
    }

    public void MergeSmallOrbs() {
        formingBigOrb = false;
        formingBigOrbCanceled = false;

        foreach (SmallOrb_UI smallOrb in smallOrbsFormingBigOrb) {
            Destroy(smallOrb.gameObject);
        }

        BigOrb_UI bigOrb = Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.transform.position, Quaternion.identity, bigBlueOrbsContainer).GetComponent<BigOrb_UI>();
        
        if(droppingBigOrb) {
            // Player is dropping orb
            DropBigOrb(bigOrb);
        } else {
            // Player is paying orb
            PayBigOrb(bigOrb);
        }

        smallOrbsFormingBigOrb.Clear();
    }

    public void AddBigBlueOrb() {
        Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, bigBlueOrbsContainer);
    }

    public void AddSmallBlueOrb() {
        Instantiate(smallBlueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, smallBlueOrbsContainer);
    }

    public void TryFormBigOrb(bool droppingOrb) {
        if (formingBigOrb) return;

        droppingBigOrb = droppingOrb;
        formingBigOrb = true;

        smallOrbsPickupTimer = 0;
        smallOrbsInCauldron = smallBlueOrbsContainer.GetComponentsInChildren<SmallOrb_UI>();
        smallOrbIndex = smallOrbsInCauldron.Length -1;
    }

    private void DropBigOrb() {
        bigOrbsInCauldron = bigBlueOrbsContainer.GetComponentsInChildren<BigOrb_UI>();
        DropBigOrb(bigOrbsInCauldron[0]);
    }

    private void PayBigOrb(BigOrb_UI bigOrb) {
        bigOrb.SetMovingBigOrb(true);
        bigOrb.SetDestination(blueOrbsSpawnPosition.position, smallOrbSmoothTime);
    }

    private void DropBigOrb(BigOrb_UI bigOrb) {
        bigOrb.DropOrb();
        OnBigBlueOrbDropped?.Invoke(this, EventArgs.Empty);
        droppingBigOrb = false;
    }

    public void CrackleBigOrb(Vector3 originPosition) {

        for (int i = 0; i < smallOrbValue; i++) {

            Vector3 spawnPosition = new Vector3(originPosition.x + UnityEngine.Random.Range(-3f, 3f), originPosition.y + UnityEngine.Random.Range(-3f, 3f), 0);

            Rigidbody2D smallOrbRigidBody = Instantiate(smallBlueOrbUIPrefab, spawnPosition, Quaternion.identity, smallBlueOrbsContainer).GetComponent<Rigidbody2D>();

            Vector2 forceDirectionNormalized = (smallOrbRigidBody.transform.position - originPosition).normalized;
            float force = 75f;
            smallOrbRigidBody.AddForce(forceDirectionNormalized * force, ForceMode2D.Impulse);
        }
    }

    public int GetSmallOrbValue() {
        return smallOrbValue;
    }

    public bool GetCanDropBigOrb() {
        bigOrbsInCauldron = bigBlueOrbsContainer.GetComponentsInChildren<BigOrb_UI>();
        return bigOrbsInCauldron.Length > 0;
    }

}
