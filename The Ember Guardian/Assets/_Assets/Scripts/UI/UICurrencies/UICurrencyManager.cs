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

    private Canvas canvas;

    private float bigOrbSmoothTime = 1f;

    List<Currency_UI> smallOrbsInBag;
    List<Currency_UI> currenciesInBag;
    //List<Currency_UI> smallOrbsFormingBigOrb = new List<Currency_UI>();

    private PayCurrencyUI currentPayCurrencyUI;


    private float timeBetweenSmallOrbsPickup = .2f;
    private float smallOrbsPickupTimer;
    int smallOrbIndex;
    int bigOrbIndex;
    int smallOrbsNecessaryToFormBigOrb = 5;

    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyDropped;
    public event EventHandler<OnCurrencyTryPayEventArgs> OnCurrencyTryPay;
    public class OnCurrencyTryPayEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
        public PayCurrencyTemplateWorldUI destionationOrbTemplate;
    }
    public class OnCurrencyDroppedEventArgs : EventArgs {
        public Currency_UI currencyUIDropped;
    }

    private bool payingOrb;
    private bool formingBigOrb;
    private bool formingBigOrbCanceled;
    private bool droppingCurrency;
    private bool tryingToDropOrb;
    private bool structureJustBuilt;

    private float tryingToDropOrbTimer;
    private float tryingToDropOrbHoldTime = .2f;

    int debugInitialBigOrbs = 5;
    int debugInitialSmallOrbs = 2;
    int debugInitialBigRedOrbs = 0;
    int debugInitialSmallRedOrbs = 0;
    int debugInitialAmmo = 3;

    private void Awake() {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
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


        if(Input.GetKeyDown(KeyCode.J)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.ammo);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.smallBlueOrb);
        }
    }
    //private void FormBigOrbWithSmallOrb() {

    //    if (smallOrbIndex < 0) {
    //        formingBigOrbCanceled = true;
    //        formingBigOrb = false;
    //    }

    //    smallOrbsPickupTimer -= Time.deltaTime;

    //    if (smallOrbsPickupTimer <= 0) {

    //        smallOrbsInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.smallBlueOrb);
    //        smallOrbsInBag[smallOrbIndex].SetMoving(true);
    //        smallOrbsInBag[smallOrbIndex].SetDestination(blueOrbsSpawnPosition, smallOrbSmoothTime);
    //        smallOrbsFormingBigOrb.Add(smallOrbsInBag[smallOrbIndex]);

    //        if (smallOrbsFormingBigOrb.Count < smallOrbsNecessaryToFormBigOrb) {

    //            // Next small orb
    //            smallOrbsPickupTimer = timeBetweenSmallOrbsPickup;
    //            smallOrbIndex--;

    //        }
    //        else {
    //            // Enough small orbs extracted : FORM BIG ORB
    //        }
    //    }
    //}

    //public void CancelOrbFormation() {
    //    formingBigOrbCanceled = true;
    //    formingBigOrb = false;

    //    foreach (Currency_UI smallOrb in smallOrbsFormingBigOrb) {
    //        // Not enough small orbs extracted : CANCEL
    //        smallOrb.SetMoving(false);
    //    }
    //    smallOrbsFormingBigOrb.Clear();
    //}

    //public void MergeSmallOrbs() {
    //    formingBigOrb = false;
    //    formingBigOrbCanceled = false;

    //    foreach (Currency_UI smallOrb in smallOrbsFormingBigOrb) {
    //        Destroy(smallOrb.gameObject);
    //    }

    //    Currency_UI bigOrb = Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.transform.position, Quaternion.identity, currencyContainer).GetComponent<Currency_UI>();
        
    //    if(droppingCurrency) {
    //        // Player is dropping orb
    //        DropCurrencyFromBag(bigOrb);
    //    }

    //    smallOrbsFormingBigOrb.Clear();
    //}

    public void AddCurrencyInBag(PlayerCurrencies.CurrencyType currencyType) {
        Vector2 force = new Vector2(UnityEngine.Random.Range(0, 0), 0);

        if(currencyType == PlayerCurrencies.CurrencyType.bigBlueOrb) {
            Transform currencyTransform = Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if(currencyType == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            Transform currencyTransform = Instantiate(smallBlueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.bigRedOrb) {
            Transform currencyTransform = Instantiate(redOrbUIPrefab, redOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.smallRedOrb) {
            Transform currencyTransform = Instantiate(smallRedOrbUIPrefab, redOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
            Transform currencyTransform = Instantiate(ammoUIPrefab, ammoSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

    }

    private IEnumerator DebugAddCurrency(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
        for (int i = 0; i < currencyAmount; i++) {
            AddCurrencyInBag(currencyType);
            yield return new WaitForSeconds(.2f);
        }
    }

    public List<Currency_UI> GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType currencyType) {
        List<Currency_UI> currencyList = new List<Currency_UI>();

        foreach(Currency_UI currency in currencyContainer.GetComponentsInChildren<Currency_UI>()) {
            if(currency.GetCurrencyType() == currencyType) {
                currencyList.Add(currency);
            }
        }

        return currencyList;
    }

    public void TryFormBigOrb(bool droppingOrb) {
        if (formingBigOrb) return;

        droppingCurrency = droppingOrb;
        formingBigOrb = true;

        smallOrbsPickupTimer = 0;
        smallOrbsInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.smallBlueOrb);
        smallOrbIndex = smallOrbsInBag.Count -1;
    }

    public void DropNextCurrencyInBag(PlayerCurrencies.CurrencyType currencyType) {
        currenciesInBag = GetCurrenciesInBagOfType(currencyType);
        DropCurrencyFromBag(currenciesInBag[0]);
    }

    private void DropCurrencyFromBag(Currency_UI currencyUI) {
        currencyUI.RemoveFromBag();
        OnCurrencyDropped?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUI
        });
        droppingCurrency = false;
    }

    public void SetPayingCurrency(PayCurrencyUI payOrbsUI, PlayerCurrencies.CurrencyType currencyTypeToPay, bool payingCurrency) {
        Debug.Log("SetPayingOrbs " + payingCurrency);
       
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
    }

    private void CurrentPayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        Debug.Log("CurrentPayOrbsUI_OnOrbPaymentSuccess");
        currentPayCurrencyUI.OnSingleCurrencyPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
        currentPayCurrencyUI.OnCurrencyPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
    }

    private void CurrentPayOrbsUI_OnSingleOrbPaid(object sender, PayCurrencyUI.OnSingleOrbFilledEventArgs e) {
        Debug.Log("CurrentPayOrbsUI_OnSingleOrbPaid");

        PlayerCurrencies.CurrencyType nextCurrencyTypeToPay = currentPayCurrencyUI.GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay();

        PayNextCurrency(nextCurrencyTypeToPay);
    }

    private void PayNextCurrency(PlayerCurrencies.CurrencyType currencyTypeToPay) {
        Debug.Log("PayNextBigOrb");
        PayCurrencyTemplateWorldUI currencyTemplateUI = currentPayCurrencyUI.GetCurrentCurrencyTemplateWorldUI();

        currenciesInBag = GetCurrenciesInBagOfType(currencyTypeToPay);

        if (currenciesInBag.Count > 0 ) {
            currenciesInBag[0].RemoveFromBag();

            OnCurrencyTryPay?.Invoke(this, new OnCurrencyTryPayEventArgs {
                currencyType = currencyTypeToPay,
                destionationOrbTemplate = currencyTemplateUI
            });

        } else {
            currentPayCurrencyUI.CancelCurrencyPayment();
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
        if (structureJustBuilt) return;

        if (Player.Instance.GetCanDropOrbOnTheFloor()) {
            if (GetHasBigOrb()) {
                DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
            }
        }

        tryingToDropOrb = false;
        structureJustBuilt = false;
    }


    private void StructureLocation_OnAnyStructureBuilt(object sender, EventArgs e) {
        structureJustBuilt = true;
    }

    public int GetSmallOrbValue() {
        return smallOrbValue;
    }

    public bool GetHasBigOrb() {
        currenciesInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.bigBlueOrb);
        return currenciesInBag.Count > 0;
    }

}
