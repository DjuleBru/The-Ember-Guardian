using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyCrafterVisual : StructureVisual
{
    [SerializeField] private StructureUI ammoCrafterUI;
    [SerializeField] private GameObject craftCurrency_PayOrbsGameObject;
    [SerializeField] private GameObject craftSpecialCurrency_PayOrbsGameObject;
    [SerializeField] private GameObject craftCurrency_craftingCurrencyGameObject;
    [SerializeField] private GameObject collectCurrencyGameObject;

    [SerializeField] private RectTransform currencyBarTemplate;
    [SerializeField] private Image currencyBarTemplateImage;
    [SerializeField] private Image currencyBarTemplateBackgroundImage;
    [SerializeField] private Sprite ammoSprite;
    [SerializeField] private Sprite ammoSpecialSprite;

    [SerializeField] private RectTransform currencyBatchContainer;
    [SerializeField] private RectTransform currencyBatchTemplate;


    private List<RectTransform> currencyBatchTemplateList = new List<RectTransform>();

    private Animator crafterAnimator;

    private int currentBatch;
    private bool craftingCurrency;
    private CurrencyCrafter currencyCrafter;
    private List<CurrencyCrafterVisual_CurrencyBarTemplate> currencyBarTemplateList = new List<CurrencyCrafterVisual_CurrencyBarTemplate>();

    protected override void Awake() {
        base.Awake();
        currencyCrafter = GetComponentInParent<CurrencyCrafter>();
        crafterAnimator = GetComponent<Animator>();

        craftCurrency_PayOrbsGameObject.SetActive(true);
    }

    protected override void Start() {
        base.Start();

        currencyCrafter.OnCurrencyCraftingEnded += CurrencyCrafter_OnAmmoCraftingEnded;
        currencyCrafter.OnNewCurrencyBatchCraftingStarted += CurrencyCrafter_OnNewCurrencyBatchCraftingStarted;
        currencyCrafter.OnPlayerCollectedCurrency += CurrencyCrafter_OnPlayerCollectedAmmo;
        currencyCrafter.OnMaxCurrencyBatchCraftingStarted += AmmoCrafter_OnMaxCurrencyBatchCraftingStarted;
        ammoCrafterUI.OnStructureDisplayedFunctionChanged += AmmoCrafterUI_OnStructureDisplayedFunctionChanged;
        currencyCrafter.OnPlayerTriggeredIn += AmmoCrafter_OnPlayerTriggeredIn;
        currencyCrafter.OnPlayerTriggeredOut += AmmoCrafter_OnPlayerTriggeredOut;
        currencyCrafter.OnWorkerStartedRefilling += CurrencyCrafter_OnWorkerStartedRefilling;
        currencyCrafter.OnCurrencyTypeBeingCraftedLoaded += CurrencyCrafter_OnCurrencyTypeBeingCraftedLoaded;
        currencyCrafter.OnWorkerCollectedCurrency += CurrencyCrafter_OnWorkerCollectedCurrency;
        StructureStats.Instance.OnStructureStatsUpdated += StructureStats_OnStructureStatsUpdated;

        if (craftSpecialCurrency_PayOrbsGameObject != null) {
            craftSpecialCurrency_PayOrbsGameObject.SetActive(true);
        }

        RefreshCurrencyBarVisuals();
        HighlightStructureFunctionIcon(true);

        ShowCollectCurrencyInstructionAnimator(false);
    }

    private void StructureStats_OnStructureStatsUpdated(object sender, System.EventArgs e) {
        ResetCurrencyBarVisuals();
    }

    private void ShowCollectCurrencyInstructionAnimator(bool show) {
        collectCurrencyGameObject.SetActive(show);
    }

    private void AmmoCrafter_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (currencyCrafter.GetCraftingCurrency()) return;

        if (currencyCrafter.GetCraftedCurrency()) {
            ShowCollectCurrencyInstructionAnimator(false);
            return;
        }

        craftCurrency_craftingCurrencyGameObject.SetActive(false);

    }

    private void AmmoCrafter_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (currencyCrafter.GetCraftingCurrency()) return;

        craftCurrency_craftingCurrencyGameObject.SetActive(true);

        if(currencyCrafter.GetCraftedCurrency()) {
            ShowCollectCurrencyInstructionAnimator(true);
        }
    }
    private void CurrencyCrafter_OnWorkerStartedRefilling(object sender, System.EventArgs e) {
        if (currencyCrafter.GetCraftingCurrency()) return;

        craftCurrency_craftingCurrencyGameObject.SetActive(true);
    }

    protected void Update() {

        if(craftingCurrency) {

            float craftingProgression = currencyCrafter.GetAmmoCraftTimerNormalized();
            RefreshCurrencyBarProgression(craftingProgression);

        }
    }

    private void CurrencyCrafter_OnWorkerCollectedCurrency(object sender, System.EventArgs e) {
        RefreshCurrencyBarVisuals();
    }

    private void AmmoCrafterUI_OnStructureDisplayedFunctionChanged(object sender, System.EventArgs e) {
        if (craftingCurrency) return;
        if (currencyCrafter.GetCraftedCurrency()) return;
        RefreshCurrencyBarVisuals();
    }

    private void CurrencyCrafter_OnCurrencyTypeBeingCraftedLoaded(object sender, System.EventArgs e) {
        RefreshCurrencyBarVisuals();
    }

    private void CurrencyCrafter_OnPlayerCollectedAmmo(object sender, System.EventArgs e) {
        craftCurrency_PayOrbsGameObject.SetActive(true);
        if (craftSpecialCurrency_PayOrbsGameObject != null) {
            craftSpecialCurrency_PayOrbsGameObject.SetActive(true);
        }
        RefreshCurrencyBarVisuals();

        ShowCollectCurrencyInstructionAnimator(false);
    }

    private void CurrencyCrafter_OnNewCurrencyBatchCraftingStarted(object sender, System.EventArgs e) {
        RefreshCurrencyBarVisuals();
        HighlightStructureFunctionIcon(false);

        if(crafterAnimator != null) {
            crafterAnimator.ResetTrigger("Idle");
            crafterAnimator.SetTrigger("Crafting");
        }
        craftingCurrency = true;
    }

    private void AmmoCrafter_OnMaxCurrencyBatchCraftingStarted(object sender, System.EventArgs e) {
        craftCurrency_PayOrbsGameObject.SetActive(false);

        if(craftSpecialCurrency_PayOrbsGameObject != null) {
            craftSpecialCurrency_PayOrbsGameObject.SetActive(false);
        }
    }

    private void CurrencyCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {
        craftingCurrency = false;
        foreach(CurrencyCrafterVisual_CurrencyBarTemplate ammoTemplate in currencyBarTemplateList) {
            ammoTemplate.SetCrafted();
        }

        HighlightStructureFunctionIcon(true);

        if (crafterAnimator != null) {
            crafterAnimator.ResetTrigger("Crafting");
            crafterAnimator.SetTrigger("Idle");
        }

        if(currencyCrafter.GetPlayerInTriggerArea()) {
            ShowCollectCurrencyInstructionAnimator(true);
        }

    }

    private void RefreshCurrencyBarProgression(float craftingProgression) {
        float totalBars = currencyBarTemplateList.Count * craftingProgression;

        for (int i = 0; i < totalBars; i++) {
            if (i < Mathf.FloorToInt(totalBars)) {
                currencyBarTemplateList[i].SetFillAmount(1f);
            }
            else if (i == Mathf.FloorToInt(totalBars)) {
                currencyBarTemplateList[i].SetFillAmount(totalBars - Mathf.Floor(totalBars));
            }
            else {
                currencyBarTemplateList[i].SetFillAmount(0f);
            }
        }

    }

    private void RefreshCurrencyBarVisuals() {

        currencyBarTemplate.gameObject.SetActive(true);
        currencyBatchTemplate.gameObject.SetActive(true);

        if(currencyCrafter.GetCurrentBatch() == 0) {
            //First batch launched
            currencyBarTemplateList.Clear();
            currencyBatchTemplateList.Clear();

            foreach (RectTransform child in currencyBatchContainer) {
                if (child == currencyBatchTemplate) continue;
                Destroy(child.gameObject);
            }
        }

        Transform batchTemplate = Instantiate(currencyBatchTemplate, currencyBatchContainer);

        batchTemplate.gameObject.SetActive(true);
        batchTemplate.GetComponent<RectTransform>().SetAsFirstSibling();
        int currencyCount = currencyCrafter.GetCurrencyCraftAmount();

        foreach (RectTransform child in batchTemplate) {
            if (child == currencyBarTemplate) continue;
            Destroy(child.gameObject);
        }

        RectTransform rt = currencyBarTemplate.GetComponent<RectTransform>();

        if (currencyCrafter.GetCurrencyTypeCrafted() == PlayerCurrencies.CurrencyType.ammo || currencyCrafter.GetCurrencyTypeCrafted() == PlayerCurrencies.CurrencyType.ammo_special) {

            //Debug.Log(currencyCrafter.GetCurrentStructureInteractionType());

            if (currencyCrafter.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.primaryFunction) {

                currencyBarTemplateImage.sprite = ammoSprite;
                currencyBarTemplateBackgroundImage.sprite = ammoSprite;

                rt.sizeDelta = new Vector2(.2f, .5f);
            }
            if (currencyCrafter.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.secondaryFunction) {

                currencyBarTemplateImage.sprite = ammoSpecialSprite;
                currencyBarTemplateBackgroundImage.sprite = ammoSpecialSprite;


                rt.sizeDelta = new Vector2(.3f, .5f);
            }
        }

        for (int i = 0; i < currencyCount; i++) {
            CurrencyCrafterVisual_CurrencyBarTemplate ammoBar = Instantiate(currencyBarTemplate, batchTemplate).GetComponent<CurrencyCrafterVisual_CurrencyBarTemplate>();
            ammoBar.SetFillAmount(0f);
            currencyBarTemplateList.Add(ammoBar);
        }

        currencyBarTemplate.gameObject.SetActive(false);
        currencyBatchTemplate.gameObject.SetActive(false);
    }

    private void ResetCurrencyBarVisuals() {
        currencyBarTemplate.gameObject.SetActive(true);
        currencyBatchTemplate.gameObject.SetActive(true);

        // --- CLEAR COMPLET ---
        foreach (RectTransform child in currencyBatchContainer) {
            if (child == currencyBatchTemplate) continue;
            Destroy(child.gameObject);
        }
        currencyBarTemplateList.Clear();
        currencyBatchTemplateList.Clear();
        // ---------------------

        Transform batchTemplate = Instantiate(currencyBatchTemplate, currencyBatchContainer);
        batchTemplate.gameObject.SetActive(true);
        batchTemplate.GetComponent<RectTransform>().SetAsFirstSibling();

        int currencyCount = currencyCrafter.GetCurrencyCraftAmount();

        foreach (RectTransform child in batchTemplate) {
            if (child == currencyBarTemplate) continue;
            Destroy(child.gameObject);
        }

        RectTransform rt = currencyBarTemplate.GetComponent<RectTransform>();

        if (currencyCrafter.GetCurrencyTypeCrafted() == PlayerCurrencies.CurrencyType.ammo ||
            currencyCrafter.GetCurrencyTypeCrafted() == PlayerCurrencies.CurrencyType.ammo_special) {

            if (currencyCrafter.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.primaryFunction) {
                currencyBarTemplateImage.sprite = ammoSprite;
                currencyBarTemplateBackgroundImage.sprite = ammoSprite;
                rt.sizeDelta = new Vector2(.2f, .5f);
            }
            if (currencyCrafter.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.secondaryFunction) {
                currencyBarTemplateImage.sprite = ammoSpecialSprite;
                currencyBarTemplateBackgroundImage.sprite = ammoSpecialSprite;
                rt.sizeDelta = new Vector2(.3f, .5f);
            }
        }

        for (int i = 0; i < currencyCount; i++) {
            CurrencyCrafterVisual_CurrencyBarTemplate bar = Instantiate(currencyBarTemplate, batchTemplate)
                .GetComponent<CurrencyCrafterVisual_CurrencyBarTemplate>();
            bar.SetFillAmount(0f);
            currencyBarTemplateList.Add(bar);
        }

        currencyBarTemplate.gameObject.SetActive(false);
        currencyBatchTemplate.gameObject.SetActive(false);
    }
}
