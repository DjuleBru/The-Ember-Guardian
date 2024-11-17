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
    List<Currency_UI> smallOrbsFormingBigOrb = new List<Currency_UI>();

    private PayOrbsUI currentPayOrbsUI;


    private float timeBetweenSmallOrbsPickup = .2f;
    private float smallOrbsPickupTimer;
    int smallOrbIndex;
    int bigOrbIndex;
    int smallOrbsNecessaryToFormBigOrb = 5;

    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyDropped;
    public event EventHandler<OnBigOrbTryPayEventArgs> OnBigBlueOrbTryPay;
    public class OnBigOrbTryPayEventArgs : EventArgs {
        public OrbTemplateWorldUI destionationOrbTemplate;
    }
    public class OnCurrencyDroppedEventArgs : EventArgs {
        public Currency_UI currencyUIDropped;
    }

    private bool payingOrb;
    private bool formingBigOrb;
    private bool formingBigOrbCanceled;
    private bool droppingCurrency;
    private bool tryingToDropOrb;

    private float tryingToDropOrbTimer;
    private float tryingToDropOrbHoldTime = .2f;

    int debugInitialBigOrbs = 5;
    int debugInitialSmallOrbs = 2;
    int debugInitialBigRedOrbs = 2;
    int debugInitialSmallRedOrbs = 7;
    int debugInitialAmmo = 8;

    private void Awake() {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;

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

        if (formingBigOrb) {
            FormBigOrbWithSmallOrb();
        }


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
    private void FormBigOrbWithSmallOrb() {

        if (smallOrbIndex < 0) {
            formingBigOrbCanceled = true;
            formingBigOrb = false;
        }

        smallOrbsPickupTimer -= Time.deltaTime;

        if (smallOrbsPickupTimer <= 0) {

            smallOrbsInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.smallBlueOrb);
            smallOrbsInBag[smallOrbIndex].SetMoving(true);
            smallOrbsInBag[smallOrbIndex].SetDestination(blueOrbsSpawnPosition, smallOrbSmoothTime);
            smallOrbsFormingBigOrb.Add(smallOrbsInBag[smallOrbIndex]);

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

        foreach (Currency_UI smallOrb in smallOrbsFormingBigOrb) {
            // Not enough small orbs extracted : CANCEL
            smallOrb.SetMoving(false);
        }
        smallOrbsFormingBigOrb.Clear();
    }

    public void MergeSmallOrbs() {
        formingBigOrb = false;
        formingBigOrbCanceled = false;

        foreach (Currency_UI smallOrb in smallOrbsFormingBigOrb) {
            Destroy(smallOrb.gameObject);
        }

        Currency_UI bigOrb = Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.transform.position, Quaternion.identity, currencyContainer).GetComponent<Currency_UI>();
        
        if(droppingCurrency) {
            // Player is dropping orb
            DropCurrencyFromBag(bigOrb);
        }

        smallOrbsFormingBigOrb.Clear();
    }

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

    public void SetPayingOrbs(PayOrbsUI payOrbsUI, bool payingOrbs) {
        Debug.Log("SetPayingOrbs " + payingOrbs);
       
        if (currentPayOrbsUI != null) {
            currentPayOrbsUI.OnSingleOrbPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
            currentPayOrbsUI.OnOrbPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
        }

        if (payingOrbs) {
            currentPayOrbsUI = payOrbsUI;
            currentPayOrbsUI.OnSingleOrbPaid += CurrentPayOrbsUI_OnSingleOrbPaid;
            currentPayOrbsUI.OnOrbPaymentSuccess += CurrentPayOrbsUI_OnOrbPaymentSuccess;
            PayNextBigOrb();
        }
    }

    private void CurrentPayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        Debug.Log("CurrentPayOrbsUI_OnOrbPaymentSuccess");
        currentPayOrbsUI.OnSingleOrbPaid -= CurrentPayOrbsUI_OnSingleOrbPaid;
        currentPayOrbsUI.OnOrbPaymentSuccess -= CurrentPayOrbsUI_OnOrbPaymentSuccess;
    }

    private void CurrentPayOrbsUI_OnSingleOrbPaid(object sender, PayOrbsUI.OnSingleOrbFilledEventArgs e) {
        Debug.Log("CurrentPayOrbsUI_OnSingleOrbPaid");

        currentPayOrbsUI.GetCurrentOrbTemplateWorldUI();

        PayNextBigOrb();
    }

    private void PayNextBigOrb() {
        Debug.Log("PayNextBigOrb");
        OrbTemplateWorldUI orbTemplate = currentPayOrbsUI.GetCurrentOrbTemplateWorldUI();

        currenciesInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.bigBlueOrb);

        if (currenciesInBag.Count > 0 ) {
            currenciesInBag[0].RemoveFromBag();

            OnBigBlueOrbTryPay?.Invoke(this, new OnBigOrbTryPayEventArgs {
                destionationOrbTemplate = orbTemplate
            });
        } else {
            currentPayOrbsUI.CancelOrbPayment();
            PlayerCurrencies.Instance.CancelCurrencyPayment();
        }
    }

    public void CrackleBigOrb(Vector3 originPosition) {

        for (int i = 0; i < smallOrbValue; i++) {

            Vector3 spawnPosition = new Vector3(originPosition.x + UnityEngine.Random.Range(-3f, 3f), originPosition.y + UnityEngine.Random.Range(-3f, 3f), 0);

            Rigidbody2D smallOrbRigidBody = Instantiate(smallBlueOrbUIPrefab, spawnPosition, Quaternion.identity, currencyContainer).GetComponent<Rigidbody2D>();

            Vector2 forceDirectionNormalized = (smallOrbRigidBody.transform.position - originPosition).normalized;
            float force = 75f;
            smallOrbRigidBody.AddForce(forceDirectionNormalized * force, ForceMode2D.Impulse);
        }
    }

    private void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (Player.Instance.GetCanDropOrbOnTheFloor()) {
            tryingToDropOrb = true;
            tryingToDropOrbTimer = 0;
            formingBigOrbCanceled = false;
        }
    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {

        if (tryingToDropOrbTimer < tryingToDropOrbHoldTime) {
            // Player pressed Interact once

            if (Player.Instance.GetCanDropOrbOnTheFloor()) {
                if (GetHasBigOrb()) {
                    DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
                }
            }
        }
        else {
            // Player was holding Interact
            CancelOrbFormation();
        }

        tryingToDropOrb = false;
    }

    public int GetSmallOrbValue() {
        return smallOrbValue;
    }

    public bool GetHasBigOrb() {
        currenciesInBag = GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.bigBlueOrb);
        return currenciesInBag.Count > 0;
    }

}
