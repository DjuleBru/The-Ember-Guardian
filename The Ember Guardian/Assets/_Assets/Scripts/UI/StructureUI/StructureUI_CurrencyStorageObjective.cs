using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI_CurrencyStorageObjective : StructureUI
{
    [SerializeField] private CurrencyStorage_Objective currencyStorageObj;

    [SerializeField] private GameObject fireProgressBarGameObject;
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarContainer;
    [SerializeField] private RectTransform progressBarTemplate;
    [SerializeField] private RectTransform progressBarBackgroundContainer;
    [SerializeField] private RectTransform progressBarBackgroundTemplate;
    [SerializeField] private Animator fireUIAnimator;

    private CanvasGroup progressBarCanvasGroup;
    private float progressTemplateWidth = .45f;
    private float progressTemplateHeight = .1f;

    protected override void Awake() {
        base.Awake();
        progressBarCanvasGroup = fireProgressBarGameObject.GetComponent<CanvasGroup>();
        currencyStorageObj.OnCurrencyStored += CurrencyStorageObj_OnCurrencyStored;
        currencyStorageObj.OnMaxCurrencyAmountChanged += CurrencyStorageObj_OnMaxCurrencyAmountChanged;
        currencyStorageObj.OnMaxCurrencyStorageIndexLoaded += CurrencyStorageObj_OnMaxCurrencyStorageIndexLoaded;
        currencyStorageObj.OnCurrencyAmountStoredLoaded += CurrencyStorageObj_OnCurrencyAmountStoredLoaded;
        progressBarTemplate.gameObject.SetActive(false);
    }

    private void CurrencyStorageObj_OnCurrencyAmountStoredLoaded(object sender, System.EventArgs e) {
        RefreshProgressBar();
    }

    private void CurrencyStorageObj_OnMaxCurrencyStorageIndexLoaded(object sender, System.EventArgs e) {
        RefreshBackgroundBar(currencyStorageObj.GetMaxCurrencyAmountStored());
    }

    protected override void Start() {
        base.Start();
        RefreshBackgroundBar(currencyStorageObj.GetMaxCurrencyAmountStored());
    }

    private void CurrencyStorageObj_OnCurrencyStored(object sender, System.EventArgs e) {
        AddProgressBar();
    }

    private void CurrencyStorageObj_OnMaxCurrencyAmountChanged(object sender, System.EventArgs e) {
        RefreshBackgroundBar(currencyStorageObj.GetMaxCurrencyAmountStored());
        StartCoroutine(RemoveAllProgressBars());
    }

    protected override void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredOut(sender, e);
        fireProgressBarGameObject.SetActive(false);
    }

    protected override void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        base.Structure_OnPlayerTriggeredIn(sender, e);
        fireProgressBarGameObject.SetActive(true);
    }

    private void RefreshBackgroundBar(int barAmount) {
        progressBarBackgroundTemplate.gameObject.SetActive(true);

        foreach (RectTransform child in progressBarBackgroundContainer) {
            if (child == progressBarBackgroundTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < barAmount; i++) {
            Instantiate(progressBarBackgroundTemplate, progressBarBackgroundContainer);
        }

        progressBar.sizeDelta = new Vector2(progressTemplateWidth, currencyStorageObj.GetMaxCurrencyAmountStored() * progressTemplateHeight);


        progressBarBackgroundTemplate.gameObject.SetActive(false);
    }

    private void AddProgressBar() {
        PlayerUI_TickTemplate fireTick = Instantiate(progressBarTemplate, progressBarContainer).GetComponent<PlayerUI_TickTemplate>();

        fireTick.gameObject.SetActive(true);
        PlayerUI_TickTemplate[] fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        fireTickArray[0].AddTick();

    }

    private void RefreshProgressBar() {
        Debug.Log("RefreshProgressBar");
        foreach(Transform child in  progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currencyStorageObj.GetCurrencyAmountStored(); i++) {
            Instantiate(progressBarTemplate, progressBarContainer).gameObject.SetActive(true);
        }
    }

    private IEnumerator RemoveAllProgressBars() {
        PlayerUI_TickTemplate[] fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        int barsToRemove = fireTickArray.Length;

        for (int i = 0; i < barsToRemove; i++) {
            fireTickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();

            fireTickArray[0].RemoveTick();
            fireTickArray[0].transform.SetParent(this.transform);

            yield return new WaitForSeconds(.025f);
        }
    }

}
