using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCurrencies : MonoBehaviour
{
    public static PlayerCurrencies Instance;

    [SerializeField] private int initialOrbAmountDebug;
    [SerializeField] private Transform blueOrbPrefab;
    [SerializeField] private Transform blueOrbDropPoint;

    private List<Collectible> collectiblesBeingPaid = new List<Collectible>();

    public enum CurrencyType {
        bigBlueOrb,
        smallBlueOrb,
        bigRedOrb,
        smallRedOrb,
        greenGem,
        redGem,
        ember,
        ammo,
    }

    public event EventHandler<OnCurrencyChangedEventArgs> OnBlueOrbChanged;
    public event EventHandler<OnBlueOrbDroppedOnTheFloorEventArgs> OnBlueOrbDroppedOnTheFloor;
    public class OnCurrencyChangedEventArgs : EventArgs {
        public int previousAmount;
        public int newAmount;
    }
    public class OnBlueOrbDroppedOnTheFloorEventArgs : EventArgs {
        public Collectible blueOrbDropped;
    }

    private Collectible lastBlueOrbDroppedOnTheFloor;
    private Collectible lastBlueOrbPaying;


    private void Awake() {
        Instance = this;
    }


    private void Start() {
        UICurrencyManager.Instance.OnCurrencyDropped += UIOrbManager_OnCurrencyDropped;
        UICurrencyManager.Instance.OnBigBlueOrbTryPay += UIOrbManager_OnBigBlueOrbTryPay;
    }

    private void UIOrbManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if(e.currencyUIDropped.GetCurrencyType() == CurrencyType.bigBlueOrb) {
            DropBigOrbOnFloor();
        }
    }

    private void UIOrbManager_OnBigBlueOrbTryPay(object sender, UICurrencyManager.OnBigOrbTryPayEventArgs e) {
        DropBigOrbToPay(e.destionationOrbTemplate);
    }


    private void DropBigOrbOnFloor() {
        Debug.Log("DropBigOrbOnFloor");
        lastBlueOrbDroppedOnTheFloor = Instantiate(blueOrbPrefab, blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();

        float aimDirX = PlayerAim.Instance.GetAimDir().x;
        lastBlueOrbDroppedOnTheFloor.ApplyRandomForce(1f * aimDirX, 2f * aimDirX, 6f, 8f);
        lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);
        lastBlueOrbDroppedOnTheFloor.SetDroppedByPlayer();

        OnBlueOrbDroppedOnTheFloor?.Invoke(this, new OnBlueOrbDroppedOnTheFloorEventArgs {
            blueOrbDropped = lastBlueOrbDroppedOnTheFloor
        });
    }

    private void DropBigOrbToPay(OrbTemplateWorldUI destination) {
        Debug.Log("DropBigOrbToPay");
        lastBlueOrbPaying = Instantiate(blueOrbPrefab, blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
        lastBlueOrbPaying.SetMoving(true, destination.transform);
        collectiblesBeingPaid.Add(lastBlueOrbPaying);

    }

    public void FinalizeCurrencyPayment() {
        Debug.Log("FinalizeCurrencyPayment");
        foreach (Collectible collectible in collectiblesBeingPaid) {
            Destroy(collectible.gameObject);
        }
        collectiblesBeingPaid.Clear();
    }

    public void CancelCurrencyPayment() {
        Debug.Log("CancelCurrencyPayment");
        foreach(Collectible collectible in collectiblesBeingPaid) {
            collectible.SetMoving(false);
            collectible.ApplyRandomUpwardsForce(1, 5);
        }
        collectiblesBeingPaid.Clear();
    }

}
