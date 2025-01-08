using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurrencyManager : MonoBehaviour
{

    public static UICurrencyManager Instance;

    [SerializeField] private Transform blueOrbsSpawnPosition;
    [SerializeField] private Transform redOrbsSpawnPosition;
    [SerializeField] private Transform gemsSpawnPosition;
    [SerializeField] private Transform ammoSpawnPosition;
    [SerializeField] private Transform backpackBottomPosition;

    [SerializeField] private Transform currencyContainer;
    [SerializeField] private Transform emberContainer;

    [SerializeField] private Transform blueOrbUIPrefab;
    [SerializeField] private Transform smallBlueOrbUIPrefab;
    [SerializeField] private Transform redOrbUIPrefab;
    [SerializeField] private Transform smallRedOrbUIPrefab;
    [SerializeField] private Transform greenGemUIPrefab;
    [SerializeField] private Transform redGemUIPrefab;
    [SerializeField] private Transform ammoUIPrefab;
    [SerializeField] private Transform emberUIPrefab;

    [SerializeField] private int smallOrbValue = 5;

    [SerializeField] int debugInitialBigOrbs = 10;
    [SerializeField] int debugInitialSmallOrbs = 0;
    [SerializeField] int debugInitialBigRedOrbs = 10;
    [SerializeField] int debugInitialSmallRedOrbs = 10;
    [SerializeField] int debugInitialAmmo = 3;

    List<Currency_UI> currenciesInBag = new List<Currency_UI>();

    private PayCurrencyUI currentPayCurrencyUI;
    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyDropped;
    public event EventHandler<OnCurrencyTryPayEventArgs> OnCurrencyTryPay;
    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyCollected;
    public event EventHandler OnCurrencyFailedToDrop;
    public class OnCurrencyTryPayEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
        public PayCurrencyTemplateWorldUI destionationOrbTemplate;
        public float currencyIndexNormalized;
    }
    public class OnCurrencyDroppedEventArgs : EventArgs {
        public Currency_UI currencyUIDropped;
    }

    private bool payingCurrencyJustCanceled;
    private bool formingBigOrb;
    private bool formingBigOrbCanceled;
    private bool tryingToDropOrb;
    private bool justInteractedWithStructure;
    private bool structureFunctionJustUsed;

    private float tryingToDropOrbTimer;
    private float tryingToDropOrbHoldTime = .2f;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyStructurePrimaryFunctionUsed += Structure_OnAnyStructureFunctionUsed;

        if(SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB) {
            List<PlayerCurrencies.CurrencyType> currencyTypes = new List<PlayerCurrencies.CurrencyType> {
                PlayerCurrencies.CurrencyType.bigBlueOrb,
                PlayerCurrencies.CurrencyType.smallBlueOrb,
                PlayerCurrencies.CurrencyType.bigRedOrb,
                PlayerCurrencies.CurrencyType.smallRedOrb,
                PlayerCurrencies.CurrencyType.ammo,
            };
            List<int> currencyTypesAmount = new List<int> {
                debugInitialBigOrbs,
                debugInitialSmallOrbs,
                debugInitialBigRedOrbs,
                debugInitialSmallRedOrbs,
                debugInitialAmmo,
            };

            AddMultipleCurrencies(currencyTypes, currencyTypesAmount);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.greenGem);
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.redGem);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.ammo);
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.bigRedOrb);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.smallBlueOrb);
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.smallRedOrb);
        }
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

        if (currencyType == PlayerCurrencies.CurrencyType.greenGem) {
            currencyTransform = Instantiate(greenGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.redGem) {
            currencyTransform = Instantiate(redGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
            currencyTransform = Instantiate(ammoUIPrefab, ammoSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ember) {
            currencyTransform = Instantiate(emberUIPrefab, emberContainer.position, Quaternion.identity, emberContainer);
            currencyTransform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }

        Currency_UI currencyUICollected = currencyTransform.GetComponent<Currency_UI>();
        //currencyUICollected.SetBackpackBottomPosition(backpackBottomPosition);
        OnCurrencyCollected?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUICollected
        });

        foreach(Currency_UI currency in currenciesInBag) {
            currency.SetCurrencyRbMovable();
        }

        currenciesInBag.Add(currencyUICollected);
    }

    public void AddCurrencyAmount(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
        Debug.Log("Add " + currencyType + " " + currencyAmount);
        StartCoroutine(AddCurrencyCoroutine(currencyType, currencyAmount));
    }

    public void AddMultipleCurrencies(List<PlayerCurrencies.CurrencyType> currencyTypeList, List<int> currencyAmountList) {
        StartCoroutine(AddMultipleCurrenciesCoroutine(currencyTypeList, currencyAmountList));
    }

    private IEnumerator AddMultipleCurrenciesCoroutine(List<PlayerCurrencies.CurrencyType> currencyTypeList, List<int> currencyAmountList) {
        int i = 0;
        float delayBetweenEachCurrencyType = .75f;

        foreach(PlayerCurrencies.CurrencyType currencyType in currencyTypeList) {
            int currencyTypeAmount = currencyAmountList[i];
            float delay = currencyTypeAmount * .15f;

            StartCoroutine(AddCurrencyCoroutine(currencyType, currencyTypeAmount));

            yield return new WaitForSeconds(delay + delayBetweenEachCurrencyType);
            i++;
        }
    }

    private IEnumerator AddCurrencyCoroutine(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
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

        foreach (Currency_UI currency in currenciesInBag) {
            currency.SetCurrencyRbMovable();
        }
    }

    public void RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
        for(int i=0; i< currencyAmount; i++) {
            List<Currency_UI> currenciesOfType = GetCurrenciesInBagOfType(currencyType);
            DropCurrencyFromBag(currenciesOfType[currenciesOfType.Count - 1]);
        }

        foreach (Currency_UI currency in currenciesInBag) {
            currency.SetCurrencyRbMovable();
        }
    }

    public void RemoveCurrencyUIFromInventoryList(Currency_UI currencyUI) {
        currenciesInBag.Remove(currencyUI);
    }

    public void CurrencyFellFromBag(Currency_UI currencyUI) {
        PlayerCurrencies.Instance.CurrencyFellFromBag(currencyUI.GetCurrencyType());
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
        float currencyIndexNormalized = currentPayCurrencyUI.GetCurrencyIndexNormalized();

        List<Currency_UI> currenciesInBagOfType = GetCurrenciesInBagOfType(currencyTypeToPay);

        if (currenciesInBagOfType.Count > 0 ) {
            currenciesInBagOfType[currenciesInBagOfType.Count-1].RemoveFromBag();

            OnCurrencyTryPay?.Invoke(this, new OnCurrencyTryPayEventArgs {
                currencyType = currencyTypeToPay,
                destionationOrbTemplate = currencyTemplateUI,
                currencyIndexNormalized = currencyIndexNormalized
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
        if (justInteractedWithStructure) {
            justInteractedWithStructure = false;
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
        justInteractedWithStructure = true;
    }

    private void Structure_OnAnyStructureFunctionUsed(object sender, EventArgs e) {
        justInteractedWithStructure = true;
    }

    public int GetSmallOrbValue() {
        return smallOrbValue;
    }

    public bool GetHasBigOrb() {
        return GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.bigBlueOrb).Count > 0;
    }

    public List<Vector3> GetCurrencyPositions(PlayerCurrencies.CurrencyType currencyType) {
        List<Vector3> currencyPosition = new List<Vector3>();

        foreach (Currency_UI currency in GetCurrenciesInBagOfType(currencyType)) {
            currencyPosition.Add(currency.transform.position);

        }
        return currencyPosition;
    }

    public void LoadCurrencies(PlayerCurrencies.CurrencyType currencyType, List<Vector3> currencyPositions) {
        Transform prefab = null;
        Debug.Log("LoadCurrencies");

        if (currencyType == PlayerCurrencies.CurrencyType.greenGem) {
            prefab = greenGemUIPrefab;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.redGem) {
            prefab = redGemUIPrefab;
        }

        foreach (Vector3 position in currencyPositions) {
            Currency_UI currencyUI = Instantiate(prefab, position, Quaternion.identity, currencyContainer).GetComponent<Currency_UI>();
            currenciesInBag.Add(currencyUI);
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractHeldDown -= GameInput_OnPlayerInteractHeldDown;
    }

}
