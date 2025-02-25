using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCurrencies : MonoBehaviour
{
    public static PlayerCurrencies Instance;

    [SerializeField] private Transform blueOrbDropPoint;
    [SerializeField] private Transform emberHoldPosition;

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
        blueGem,
        purpleGem,
        yellowGem,
        bearTrap,
        bladeTrap,
        smokeEjector,
        shockerEjector,
        spikeEjector,
    }
    public enum CurrencyCategory {
        orb,
        ammo,
        gem,
        trap,
    }

    public event EventHandler<OnBlueOrbDroppedOnTheFloorEventArgs> OnBlueOrbDroppedOnTheFloor;
    public event EventHandler OnEmberDropped;

    public class OnCurrencyChangedEventArgs : EventArgs {
        public int previousAmount;
        public int newAmount;
    }
    public class OnBlueOrbDroppedOnTheFloorEventArgs : EventArgs {
        public Collectible blueOrbDropped;
    }

    private Collectible lastBlueOrbDroppedOnTheFloor;
    private Collectible lastCurrencyPaying;

    private bool carryingEmber;

    private void Awake() {
        Instance = this;
    }


    private void Start() {
        if(UICurrencyManager.PlayerInventoryUI != null) {
            UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped += UIOrbManager_OnCurrencyDropped;
            UICurrencyManager.PlayerInventoryUI.OnCurrencyTryPay += UICurrencyManager_OnCurrencyTryPay;
        }

        if (SceneLoader.Instance == null) return;

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB) {
            StartCoroutine(SetCarryingEmberAfterDelay());
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            SetCarryingEmber(false);
        }
    }

    private IEnumerator SetCarryingEmberAfterDelay() {
        yield return new WaitForSeconds(.05f);
        SetCarryingEmber(true);
    }

    public void SetCarryingEmber(bool carryingEmber) {
        this.carryingEmber = carryingEmber;
        if(carryingEmber) {

            emberHoldPosition.gameObject.SetActive(true);
            UICurrencyManager.PlayerInventoryUI.AddCurrencyInBag(CurrencyType.ember);

        } else {

            emberHoldPosition.gameObject.SetActive(false);
            OnEmberDropped?.Invoke(this, EventArgs.Empty);

        }
    }

    private void UIOrbManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if(e.currencyUIDropped.GetCurrencyType() == CurrencyType.bigBlueOrb) {
            DropBigOrbOnFloor();
        }
    }

    private void UICurrencyManager_OnCurrencyTryPay(object sender, UICurrencyManager.OnCurrencyTryPayEventArgs e) {
        StartPayingCurrency(e.currencyType, e.destionationOrbTemplate, e.currencyIndexNormalized);
    }

    private void DropBigOrbOnFloor() {
        lastBlueOrbDroppedOnTheFloor = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(CurrencyType.bigBlueOrb), blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();

        float aimDirX = PlayerAim.Instance.GetAimDir().x;
        if(aimDirX > 0) {
            aimDirX = 1;
        } else {
            aimDirX = -1;
        }

        lastBlueOrbDroppedOnTheFloor.ApplyRandomForce(aimDirX, aimDirX*2, 6f, 8f);
        lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);
        lastBlueOrbDroppedOnTheFloor.SetDroppedByPlayer();

        OnBlueOrbDroppedOnTheFloor?.Invoke(this, new OnBlueOrbDroppedOnTheFloorEventArgs {
            blueOrbDropped = lastBlueOrbDroppedOnTheFloor
        });
    }

    public void CurrencyFellFromBag(CurrencyType currencyType) {
        lastBlueOrbDroppedOnTheFloor = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();

        lastBlueOrbDroppedOnTheFloor.ApplyRandomForce(-2,2,3, 4);
        lastBlueOrbDroppedOnTheFloor.ApplyRandomTorque(-8,8);
        lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);
        lastBlueOrbDroppedOnTheFloor.SetCollectibleFellFromBag();
        lastBlueOrbDroppedOnTheFloor.SetDroppedByPlayer();
    }

    private void StartPayingCurrency(CurrencyType currencyType, PayCurrencyTemplateWorldUI destination, float currencyIndexNormalized) {

        float smoothTime = destination.GetInitialPayCurrencySmoothTime() * (currencyIndexNormalized) + destination.GetInitialPayCurrencySmoothTime();

        if(currencyType != PlayerCurrencies.CurrencyType.ember) {
            Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(currencyType);

            lastCurrencyPaying = Instantiate(currencyPrefab, blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();
            lastCurrencyPaying.SetMovingForPayment(true, smoothTime, destination.transform);
            collectiblesBeingPaid.Add(lastCurrencyPaying);

        } else {

            Collectible emberCarriedByPlayer = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), emberHoldPosition.position, Quaternion.identity).GetComponent<Collectible>();
            emberCarriedByPlayer.SetMovingForPayment(true, 3f, destination.transform);
            collectiblesBeingPaid.Add(emberCarriedByPlayer);

            SetCarryingEmber(false);
        }

    }

    public void FinalizeCurrencyPayment() {
        foreach (Collectible collectible in collectiblesBeingPaid) {
            Destroy(collectible.gameObject);
        }
        collectiblesBeingPaid.Clear();
    }

    public void CancelCurrencyPayment(bool collectiblesFallInWater) {
        foreach(Collectible collectible in collectiblesBeingPaid) {
            collectible.SetMovingForPayment(false);
            collectible.ApplyRandomUpwardsForce(1, 5);

            if(collectiblesFallInWater) {
                collectible.SetCollectibleFellFromBag();
            }

        }
        collectiblesBeingPaid.Clear();
    }

    public bool GetCarryingEmber() {
        return carryingEmber;
    }

    public Transform GetEmberHoldPosition() {
        return emberHoldPosition;
    }

}
