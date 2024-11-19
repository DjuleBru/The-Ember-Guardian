using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurrencyManager : MonoBehaviour
{

    public static UICurrencyManager Instance;

    [SerializeField] private Transform blueOrbsSpawnPosition;
    [SerializeField] private Transform redOrbsSpawnPosition;
    [SerializeField] private Transform ammoSpawnPosition;

    [SerializeField] private Transform currencyContainer;

    [SerializeField] private Transform blueOrbUIPrefab;
    [SerializeField] private Transform smallBlueOrbUIPrefab;
    [SerializeField] private Transform redOrbUIPrefab;
    [SerializeField] private Transform smallRedOrbUIPrefab;
    [SerializeField] private Transform ammoUIPrefab;

    [SerializeField] private int smallOrbValue = 5;
    [SerializeField] private float smallOrbSmoothTime = 5f;

    List<Currency_UI> currenciesInBag = new List<Currency_UI>();

    private PayCurrencyUI currentPayCurrencyUI;

    private float timeBetweenSmallOrbsPickup = .2f;
    private float smallOrbsPickupTimer;
    int smallOrbIndex;
    int bigOrbIndex;
    int smallOrbsNecessaryToFormBigOrb = 5;

    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyDropped;
    public event EventHandler<OnCurrencyTryPayEventArgs> OnCurrencyTryPay;
    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyCollected;
    public event EventHandler OnCurrencyFailedToDrop;
    public class OnCurrencyTryPayEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
        public PayCurrencyTemplateWorldUI destionationOrbTemplate;
    }
    public class OnCurrencyDroppedEventArgs : EventArgs {
        public Currency_UI currencyUIDropped;
    }

    private bool payingCurrencyJustCanceled;
    private bool formingBigOrb;
    private bool formingBigOrbCanceled;
    private bool tryingToDropOrb;
    private bool structureJustBuilt;

    private float tryingToDropOrbTimer;
    private float tryingToDropOrbHoldTime = .2f;

    int debugInitialBigOrbs = 5;
    int debugInitialSmallOrbs = 0;
    int debugInitialBigRedOrbs = 0;
    int debugInitialSmallRedOrbs = 0;
    int debugInitialAmmo = 3;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;

        StartCoroutine(DebugAddCurrency(PlayerCurrencies.CurrencyType.bigBlueOrb, debugInitialBigOrbs));
        StartCoroutine(DebugAddCurrency(PlayerCurrencies.CurrencyType.smallBlueOrb, debugInitialSmallOrbs));
        StartCoroutine(DebugAddCurrency(PlayerCurrencies.CurrencyType.bigRedOrb, debugInitialBigRedOrbs));
        StartCoroutine(DebugAddCurrency(PlayerCurrencies.CurrencyType.smallRedOrb, debugInitialSmallRedOrbs));
        StartCoroutine(DebugAddCurrency(PlayerCurrencies.CurrencyType.ammo, debugInitialAmmo));
        
    }

    private void Update() {

        if (tryingToDropOrb) {
            tryingToDropOrbTimer += Time.deltaTime;

            if (tryingToDropOrbTimer > tryingToDropOrbHoldTime && !formingBigOrb && !formingBigOrbCanceled) {
                TryFormBigOrb(true);
            }

        }

        //if (formingBigOrb) {
        //    FormBigOrbWithSmallOrb();
        //}


        //if(Input.GetKeyDown(KeyCode.J)) {
        //    AddCurrencyInBag(PlayerCurrencies.CurrencyType.ammo);
        //}
        //if (Input.GetKeyDown(KeyCode.K)) {
        //    AddCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
        //}
        //if (Input.GetKeyDown(KeyCode.L)) {
        //    AddCurrencyInBag(PlayerCurrencies.CurrencyType.smallBlueOrb);
        //}
    }

    public void AddCurrencyInBag(PlayerCurrencies.CurrencyType currencyType) {
        Vector2 force = new Vector2(UnityEngine.Random.Range(0, 0), 0);
        Transform currencyTransform = null;

        if (currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            currencyTransform = Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if(currencyType == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            currencyTransform = Instantiate(smallBlueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.bigRedOrb) {
            currencyTransform = Instantiate(redOrbUIPrefab, redOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.smallRedOrb) {
            currencyTransform = Instantiate(smallRedOrbUIPrefab, redOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
            currencyTransform = Instantiate(ammoUIPrefab, ammoSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        Currency_UI currencyUICollected = currencyTransform.GetComponent<Currency_UI>();
        OnCurrencyCollected?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUICollected
        });

        currenciesInBag.Add(currencyUICollected);
    }

    private IEnumerator DebugAddCurrency(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
        for (int i = 0; i < currencyAmount; i++) {
            AddCurrencyInBag(currencyType);
            yield return new WaitForSeconds(.2f);
        }
    }

    public List<Currency_UI> GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType currencyType) {
        List<Currency_UI> currencyList = new List<Currency_UI>();

        foreach(Currency_UI currency in currenciesInBag) {
            if(currency.GetCurrencyType() == currencyType) {
                currencyList.Add(currency);
            }
        }

        return currencyList;
    }

    public void TryFormBigOrb(bool droppingOrb) {
        //if (formingBigOrb) return;

        //formingBigOrb = true;

        //smallOrbsPickupTimer = 0;
        //smallOrbsInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.smallBlueOrb);
        //smallOrbIndex = smallOrbsInBag.Count -1;
    }

    public void DropNextCurrencyInBag(PlayerCurrencies.CurrencyType currencyType) {
        List<Currency_UI> currenciesOfType = GetCurrenciesInBagOfType(currencyType);
        DropCurrencyFromBag(currenciesOfType[currenciesOfType.Count-1]);
    }

    private void DropCurrencyFromBag(Currency_UI currencyUI) {
        currencyUI.RemoveFromBag();

        OnCurrencyDropped?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUI
        });
    }

    public void RemoveCurrencyUIFromInventoryList(Currency_UI currencyUI) {
        currenciesInBag.Remove(currencyUI);
    }

    public void CurrencyFellFromBag(Currency_UI currencyUI) {
        currenciesInBag.Remove(currencyUI);
    }

    public void SetPayingCurrency(PayCurrencyUI payOrbsUI, PlayerCurrencies.CurrencyType currencyTypeToPay, bool payingCurrency) {
       
        if (currentPayCurrencyUI != null) {
            currentPayCurrencyUI.OnSingleCurrencyPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
            currentPayCurrencyUI.OnCurrencyPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
        }

        if (payingCurrency) {
            currentPayCurrencyUI = payOrbsUI;
            currentPayCurrencyUI.OnSingleCurrencyPaid += CurrentPayOrbsUI_OnSingleOrbPaid;
            currentPayCurrencyUI.OnCurrencyPaymentSuccess += CurrentPayOrbsUI_OnOrbPaymentSuccess;
            PayNextCurrency(currencyTypeToPay);
        }

        payingCurrencyJustCanceled = !payingCurrency;
    }

    private void CurrentPayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        currentPayCurrencyUI.OnSingleCurrencyPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
        currentPayCurrencyUI.OnCurrencyPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
    }

    private void CurrentPayOrbsUI_OnSingleOrbPaid(object sender, PayCurrencyUI.OnSingleOrbFilledEventArgs e) {

        PlayerCurrencies.CurrencyType nextCurrencyTypeToPay = currentPayCurrencyUI.GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay();

        PayNextCurrency(nextCurrencyTypeToPay);
    }

    private void PayNextCurrency(PlayerCurrencies.CurrencyType currencyTypeToPay) {
        PayCurrencyTemplateWorldUI currencyTemplateUI = currentPayCurrencyUI.GetCurrentCurrencyTemplateWorldUI();

        List<Currency_UI> currenciesInBagOfType = GetCurrenciesInBagOfType(currencyTypeToPay);

        if (currenciesInBagOfType.Count > 0 ) {
            currenciesInBagOfType[currenciesInBagOfType.Count-1].RemoveFromBag();

            OnCurrencyTryPay?.Invoke(this, new OnCurrencyTryPayEventArgs {
                currencyType = currencyTypeToPay,
                destionationOrbTemplate = currencyTemplateUI
            });

        } else {
            OnCurrencyFailedToDrop?.Invoke(this, EventArgs.Empty);
            currentPayCurrencyUI.ResetCurrencyPayment();
            PlayerCurrencies.Instance.CancelCurrencyPayment();
        }
    }

    private void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (Player.Instance.GetCanDropOrbOnTheFloor()) {
            tryingToDropOrb = true;
            tryingToDropOrbTimer = 0;
            formingBigOrbCanceled = false;
        }
    }

    private void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {

    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (structureJustBuilt) {
            structureJustBuilt = false;
            return;
        };

        if(payingCurrencyJustCanceled) {
            payingCurrencyJustCanceled = false;
            return;
        }

        if (Player.Instance.GetCanDropOrbOnTheFloor()) {
            if (GetHasBigOrb()) {
                DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
            } else {
                OnCurrencyFailedToDrop?.Invoke(this, EventArgs.Empty);
            }
        }

        tryingToDropOrb = false;
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        structureJustBuilt = true;
    }

    public int GetSmallOrbValue() {
        return smallOrbValue;
    }

    public bool GetHasBigOrb() {
        return GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.bigBlueOrb).Count > 0;
    }

}
