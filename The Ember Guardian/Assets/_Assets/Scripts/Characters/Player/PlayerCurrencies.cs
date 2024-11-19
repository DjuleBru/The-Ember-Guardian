using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCurrencies : MonoBehaviour
{
    public static PlayerCurrencies Instance;

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

    public event EventHandler<OnBlueOrbDroppedOnTheFloorEventArgs> OnBlueOrbDroppedOnTheFloor;
    public class OnCurrencyChangedEventArgs : EventArgs {
        public int previousAmount;
        public int newAmount;
    }
    public class OnBlueOrbDroppedOnTheFloorEventArgs : EventArgs {
        public Collectible blueOrbDropped;
    }

    private Collectible lastBlueOrbDroppedOnTheFloor;
    private Collectible lastCurrencyPaying;


    private void Awake() {
        Instance = this;
    }


    private void Start() {
        UICurrencyManager.Instance.OnCurrencyDropped += UIOrbManager_OnCurrencyDropped;
        UICurrencyManager.Instance.OnCurrencyTryPay += UICurrencyManager_OnCurrencyTryPay;
    }

    private void UIOrbManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if(e.currencyUIDropped.GetCurrencyType() == CurrencyType.bigBlueOrb) {
            DropBigOrbOnFloor();
        }
    }

    private void UICurrencyManager_OnCurrencyTryPay(object sender, UICurrencyManager.OnCurrencyTryPayEventArgs e) {
        StartPayingCurrency(e.currencyType, e.destionationOrbTemplate);
    }


    private void DropBigOrbOnFloor() {
        Debug.Log("DropBigOrbOnFloor");
        lastBlueOrbDroppedOnTheFloor = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(CurrencyType.bigBlueOrb), blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();

        float aimDirX = PlayerAim.Instance.GetAimDir().x;
        lastBlueOrbDroppedOnTheFloor.ApplyRandomForce(1f * aimDirX, 2f * aimDirX, 6f, 8f);
        lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);
        lastBlueOrbDroppedOnTheFloor.SetDroppedByPlayer();

        OnBlueOrbDroppedOnTheFloor?.Invoke(this, new OnBlueOrbDroppedOnTheFloorEventArgs {
            blueOrbDropped = lastBlueOrbDroppedOnTheFloor
        });
    }

    private void StartPayingCurrency(CurrencyType currencyType, PayCurrencyTemplateWorldUI destination) {
        Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(currencyType);

        lastCurrencyPaying = Instantiate(currencyPrefab, blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
        lastCurrencyPaying.SetMovingForPayment(true, destination.transform);
        collectiblesBeingPaid.Add(lastCurrencyPaying);

    }

    public void FinalizeCurrencyPayment() {
        foreach (Collectible collectible in collectiblesBeingPaid) {
            Destroy(collectible.gameObject);
        }
        collectiblesBeingPaid.Clear();
    }

    public void CancelCurrencyPayment() {
        foreach(Collectible collectible in collectiblesBeingPaid) {
            collectible.SetMovingForPayment(false);
            collectible.ApplyRandomUpwardsForce(1, 5);
        }
        collectiblesBeingPaid.Clear();
    }

}
