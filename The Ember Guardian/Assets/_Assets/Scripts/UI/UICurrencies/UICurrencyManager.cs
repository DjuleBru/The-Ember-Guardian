using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurrencyManager : MonoBehaviour
{

    public static UICurrencyManager PlayerInventoryUI;
    public static UICurrencyManager HubInventoryUI;

    [SerializeField] private bool isPlayerInventory;
    [SerializeField] private bool isHubInventory;

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
    [SerializeField] private Transform blueGemUIPrefab;
    [SerializeField] private Transform yellowGemUIPrefab;
    [SerializeField] private Transform purpleGemUIPrefab;
    [SerializeField] private Transform cyanGemUIPrefab;
    [SerializeField] private Transform ammoUIPrefab;
    [SerializeField] private Transform ammoSpecialUIPrefab;
    [SerializeField] private Transform emberUIPrefab;
    [SerializeField] private Transform bearTrapUIPrefab;
    [SerializeField] private Transform bladeTrapUIPrefab;
    [SerializeField] private Transform shockerEjectorUIPrefab;
    [SerializeField] private Transform smokeEjectorUIPrefab;
    [SerializeField] private Transform spikeEjectorUIPrefab;
    [SerializeField] private Transform spikeUIPrefab;
    [SerializeField] private Transform fireEjectorUIPrefab;

    [SerializeField] private int smallOrbValue = 5;

    private int initialBigBlueOrbs;
    private bool allowDebugInputs;
    [SerializeField] int debugInitialBigOrbs = 10;
    [SerializeField] int debugInitialSmallOrbs = 0;
    [SerializeField] int debugInitialBigRedOrbs = 10;
    [SerializeField] int debugInitialSmallRedOrbs = 10;
    [SerializeField] int debugInitialAmmo = 3;

    List<Currency_UI> currenciesInBag = new List<Currency_UI>();

    private PayCurrencyUI currentPayCurrencyUI;
    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyDropped;
    public event EventHandler<OnCurrencyDroppedEventArgs> OnCurrencyRemovedFromBag;
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
    private bool justInteractedWithStructure;

    private bool tryingToDropOrb;
    private float tryingToDropOrbTimer;
    private float tryingToDropOrbMaxHoldTime = .5f;

    private void Awake() {
        if(isPlayerInventory) {
            PlayerInventoryUI = this;
        }
        if (isHubInventory) {
            HubInventoryUI = this;
        }
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractHeldDown += GameInput_OnPlayerInteractHeldDown;
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyStructurePrimaryFunctionUsed += Structure_OnAnyStructureFunctionUsed;


        allowDebugInputs = DebugManager.Instance.GetAllowDebugInputs_CurrencyUIManager();

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {

            if(allowDebugInputs) {
                AddDebugCurrency();
            }

            StartCoroutine(AddInitialCurrencies());
        }
    }

    private void Update() {
        if(tryingToDropOrb) {
            tryingToDropOrbTimer += Time.deltaTime;
            if(tryingToDropOrbTimer > tryingToDropOrbMaxHoldTime) {
                tryingToDropOrb = false;
            }
        }
        if(allowDebugInputs) {
            HandleDebugInputs();
        }
        
    }
    private void HandleDebugInputs() {
        if (Input.GetKeyDown(KeyCode.G)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.greenGem);
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.redGem);
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.cyanGem);
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.blueGem);
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.yellowGem);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.purpleGem);
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.ammo);
            AddCurrencyInBag(PlayerCurrencies.CurrencyType.ammo_special);
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

    private void AddDebugCurrency() {
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

    private IEnumerator AddInitialCurrencies() {
        yield return new WaitForSeconds(6f);

        List<TrapSO> unlockedTraps = TrapManager.Instance.GetUnlockedTraps();
        TrapSO randomTrapSO = unlockedTraps[UnityEngine.Random.Range(0,unlockedTraps.Count)];
        PlayerCurrencies.CurrencyType randomTrap = CurrenciesManager.Instance.GetTrapCurrencyType(randomTrapSO.trapType);

        PlayerCurrencies.CurrencyType initialAmmoType = PlayerCurrencies.CurrencyType.ammo;
        int ammoAmount = PlayerStats.Instance.GetStartLevelAmmo();
        bool hasOnlySpecialAmmo = PlayerShoot.Instance.GetHasOnlySpecialAmmo();

        if(hasOnlySpecialAmmo) {
            initialAmmoType = PlayerCurrencies.CurrencyType.ammo_special;
            ammoAmount /= 2;
        }

        List<PlayerCurrencies.CurrencyType> currencyTypes = new List<PlayerCurrencies.CurrencyType> {
                PlayerCurrencies.CurrencyType.bigBlueOrb,
                initialAmmoType,
                randomTrap,
            };

        List<int> currencyTypesAmount = new List<int> {
            PlayerStats.Instance.GetStartLevelOrbs(),
            ammoAmount,
            StructureStats.Instance.GetStartWithRandomTrapAmount(),
            };



        AddMultipleCurrencies(currencyTypes, currencyTypesAmount);
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

        if (currencyType == PlayerCurrencies.CurrencyType.cyanGem) {
            currencyTransform = Instantiate(cyanGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.redGem) {
            currencyTransform = Instantiate(redGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.blueGem) {
            currencyTransform = Instantiate(blueGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.yellowGem) {
            currencyTransform = Instantiate(yellowGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.purpleGem) {
            currencyTransform = Instantiate(purpleGemUIPrefab, gemsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
            currencyTransform = Instantiate(ammoUIPrefab, ammoSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ammo_special) {
            currencyTransform = Instantiate(ammoSpecialUIPrefab, ammoSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        if (currencyType == PlayerCurrencies.CurrencyType.ember) {
            currencyTransform = Instantiate(emberUIPrefab, emberContainer.position, Quaternion.identity, emberContainer);
            currencyTransform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.bearTrap) {
            currencyTransform = Instantiate(bearTrapUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        if (currencyType == PlayerCurrencies.CurrencyType.bladeTrap) {
            currencyTransform = Instantiate(bladeTrapUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        if (currencyType == PlayerCurrencies.CurrencyType.smokeEjector) {
            currencyTransform = Instantiate(smokeEjectorUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        if (currencyType == PlayerCurrencies.CurrencyType.spikeEjector) {
            currencyTransform = Instantiate(spikeEjectorUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        if (currencyType == PlayerCurrencies.CurrencyType.shockerEjector) {
            currencyTransform = Instantiate(shockerEjectorUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        if (currencyType == PlayerCurrencies.CurrencyType.spikes) {
            currencyTransform = Instantiate(spikeUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        if (currencyType == PlayerCurrencies.CurrencyType.fireEjector) {
            currencyTransform = Instantiate(fireEjectorUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, currencyContainer);
            currencyTransform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }

        Currency_UI currencyUICollected = currencyTransform.GetComponent<Currency_UI>();

        if(this == HubInventoryUI) {
            currencyTransform.localScale *= 1.5f;
            currencyUICollected.PurifyGem();
        }

        //foreach (Currency_UI currency in currenciesInBag) {
        //    currency.SetCurrencyRbMovable();
        //}

        currenciesInBag.Add(currencyUICollected);

        OnCurrencyCollected?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUICollected
        });
    }

    public void AddCurrencyAmount(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
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
    public void RemoveMultipleCurrencies(List<PlayerCurrencies.CurrencyType> currencyTypeList, List<int> currencyAmountList) {
        StartCoroutine(RemoveMultipleCurrenciesCoroutine(currencyTypeList, currencyAmountList));
    }

    private IEnumerator RemoveMultipleCurrenciesCoroutine(List<PlayerCurrencies.CurrencyType> currencyTypeList, List<int> currencyAmountList) {
        int i = 0;
        float delayBetweenEachCurrencyType = .75f;

        foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeList) {
            int currencyTypeAmount = currencyAmountList[i];
            float delay = currencyTypeAmount * .15f;

            StartCoroutine(RemoveCurrencyCoroutine(currencyType, currencyTypeAmount));

            yield return new WaitForSeconds(delay + delayBetweenEachCurrencyType);
            i++;
        }
    }

    private IEnumerator RemoveCurrencyCoroutine(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
        for (int i = 0; i < currencyAmount; i++) {
            if (GetCurrenciesInBagOfType(currencyType).Count == 0) continue;
            RemoveCurrencyFromBag(currencyType, 1);
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
    public List<Currency_UI> GetCurrenciesInBagOfCategory(PlayerCurrencies.CurrencyCategory category) {
        List<Currency_UI> currencyList = new List<Currency_UI>();

        foreach (Currency_UI currency in currenciesInBag) {
            if (CurrenciesManager.Instance.GetCurrencyCategory(currency.GetCurrencyType()) == category) {
                currencyList.Add(currency);
            }
        }

        return currencyList;
    }

    public void DropNextCurrencyInBag(PlayerCurrencies.CurrencyType currencyType) {
        List<Currency_UI> currenciesOfType = GetCurrenciesInBagOfType(currencyType);
        DropCurrencyFromBag(currenciesOfType[currenciesOfType.Count-1]);
    }

    private void DropCurrencyFromBag(Currency_UI currencyUI) {
        currencyUI.RemoveFromBag(this);

        OnCurrencyDropped?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUI
        });

        //for(int i = 0; i <= 10; i++) {
        //    currenciesInBag[i].SetCurrencyRbMovable();
        //}
    }

    public void RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType currencyType, int currencyAmount) {
        for(int i=0; i< currencyAmount; i++) {
            List<Currency_UI> currenciesOfType = GetCurrenciesInBagOfType(currencyType);
            DropCurrencyFromBag(currenciesOfType[currenciesOfType.Count - 1]);
        }

        //foreach (Currency_UI currency in currenciesInBag) {
        //    currency.SetCurrencyRbMovable();
        //}
    }
    public void RemoveAllCurrenciesFromBag() {
        List<Currency_UI> currenciesInBagCopy = new List<Currency_UI>();

        foreach (Currency_UI currency in currenciesInBag) {
            currenciesInBagCopy.Add(currency);
        }

        foreach (Currency_UI currency in currenciesInBagCopy) {
            RemoveCurrencyUIFromInventoryList(currency);
            currency.DestroyCurrency();
        }
    }

    public void RemoveCurrencyUIFromInventoryList(Currency_UI currencyUI) {
        currenciesInBag.Remove(currencyUI);
        OnCurrencyRemovedFromBag?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUI
        });
    }

    public void CurrencyFellFromBag(Currency_UI currencyUI) {
        PlayerCurrencies.Instance.CurrencyFellFromBag(currencyUI.GetCurrencyType());
        currenciesInBag.Remove(currencyUI);
        currencyUI.SetFellFromBag();

        OnCurrencyRemovedFromBag?.Invoke(this, new OnCurrencyDroppedEventArgs {
            currencyUIDropped = currencyUI
        });
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
            currenciesInBagOfType[currenciesInBagOfType.Count-1].RemoveFromBag(this);

            OnCurrencyTryPay?.Invoke(this, new OnCurrencyTryPayEventArgs {
                currencyType = currencyTypeToPay,
                destionationOrbTemplate = currencyTemplateUI,
                currencyIndexNormalized = currencyIndexNormalized
            });

        } else {
            OnCurrencyFailedToDrop?.Invoke(this, EventArgs.Empty);
            currentPayCurrencyUI.ResetCurrencyPayment();
            PlayerCurrencies.Instance.CancelCurrencyPayment(currentPayCurrencyUI.GetCurrenciesFailedToPayFallInWater());
        }
    }

    private void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        tryingToDropOrb = true;
        tryingToDropOrbTimer = 0;
    }

    private void GameInput_OnPlayerInteractHeldDown(object sender, EventArgs e) {

    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        // At end of frame because SetPayingCurrency would run before and payingCurrencyJustCanceled hadn't updated
        StartCoroutine(InteractCanceledAfterFrame());
    }

    private IEnumerator InteractCanceledAfterFrame() {
        yield return new WaitForEndOfFrame();

        if (justInteractedWithStructure) {
            justInteractedWithStructure = false;
            yield break;
        };

        if (payingCurrencyJustCanceled) {
            payingCurrencyJustCanceled = false;
            yield break;
        }

        if (tryingToDropOrb && Player.Instance.GetCanDropOrbOnTheFloor()) {
            if (GetHasBigOrb()) {
                DropNextCurrencyInBag(PlayerCurrencies.CurrencyType.bigBlueOrb);
            }
            else {
                OnCurrencyFailedToDrop?.Invoke(this, EventArgs.Empty);
            }
        }
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
            if (currency == null) continue;
            currencyPosition.Add(currency.transform.position);

        }
        return currencyPosition;
    }

    public void LoadCurrencies(PlayerCurrencies.CurrencyType currencyType, List<Vector3> currencyPositions) {
        Transform prefab = null;

        if (currencyType == PlayerCurrencies.CurrencyType.greenGem) {
            prefab = greenGemUIPrefab;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.redGem) {
            prefab = redGemUIPrefab;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.blueGem) {
            prefab = blueGemUIPrefab;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.yellowGem) {
            prefab = yellowGemUIPrefab;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.purpleGem) {
            prefab = purpleGemUIPrefab;
        }

        if (currencyType == PlayerCurrencies.CurrencyType.cyanGem) {
            prefab = cyanGemUIPrefab;
        }

        foreach (Vector3 position in currencyPositions) {
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)); // Rotation aléatoire sur l'axe Z
            Currency_UI currencyUI = Instantiate(prefab, position, randomRotation, currencyContainer).GetComponent<Currency_UI>();
            currenciesInBag.Add(currencyUI);

            if(isHubInventory) {
                currencyUI.transform.localScale *= 1.5f;
                currencyUI.PurifyGem();
            }

            if (isPlayerInventory) {
                currencyUI.SetCurrencyLoaded();
            }
        }

    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractStarted;
        GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractHeldDown -= GameInput_OnPlayerInteractHeldDown;
    }

}
