using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyCrafterVisual : StructureVisual
{
    [SerializeField] private GameObject craftCurrency_PayOrbsGameObject;
    [SerializeField] private GameObject craftCurrency_craftingCurrencyGameObject;

    [SerializeField] private RectTransform currencyBarTemplate;

    [SerializeField] private RectTransform currencyBatchContainer;
    [SerializeField] private RectTransform currencyBatchTemplate;
    private List<RectTransform> currencyBatchTemplateList = new List<RectTransform>();

    private Animator crafterAnimator;

    private int currentBatch;
    private bool craftingAmmo;
    private CurrencyCrafter ammoCrafter;
    private List<CurrencyCrafterVisual_CurrencyBarTemplate> currencyBarTemplateList = new List<CurrencyCrafterVisual_CurrencyBarTemplate>();

    protected override void Awake() {
        base.Awake();
        ammoCrafter = GetComponentInParent<CurrencyCrafter>();
        crafterAnimator = GetComponent<Animator>();
    }

    protected override void Start() {
        base.Start();

        ammoCrafter.OnCurrencyCraftingEnded += CurrencyCrafter_OnAmmoCraftingEnded;
        ammoCrafter.OnNewCurrencyBatchCraftingStarted += CurrencyCrafter_OnNewCurrencyBatchCraftingStarted;
        ammoCrafter.OnPlayerCollectedCurrency += CurrencyCrafter_OnPlayerCollectedAmmo;
        ammoCrafter.OnMaxCurrencyBatchCraftingStarted += AmmoCrafter_OnMaxCurrencyBatchCraftingStarted;

        craftCurrency_PayOrbsGameObject.SetActive(true);
        craftCurrency_craftingCurrencyGameObject.SetActive(false);
        HighlightStructureFunctionIcon(true);
    }


    protected void Update() {

        if(craftingAmmo) {

            float craftingProgression = ammoCrafter.GetAmmoCraftTimerNormalized();
            RefreshCurrencyBarProgression(craftingProgression);

        }
    }

    private void CurrencyCrafter_OnPlayerCollectedAmmo(object sender, System.EventArgs e) {
        craftCurrency_PayOrbsGameObject.SetActive(true);
        craftCurrency_craftingCurrencyGameObject.SetActive(false);
    }

    private void CurrencyCrafter_OnNewCurrencyBatchCraftingStarted(object sender, System.EventArgs e) {
        craftCurrency_craftingCurrencyGameObject.SetActive(true);
        RefreshCurrencyBarVisuals();
        HighlightStructureFunctionIcon(false);

        if(crafterAnimator != null) {
            crafterAnimator.ResetTrigger("Idle");
            crafterAnimator.SetTrigger("Crafting");
        }
        craftingAmmo = true;
    }

    private void AmmoCrafter_OnMaxCurrencyBatchCraftingStarted(object sender, System.EventArgs e) {
        craftCurrency_PayOrbsGameObject.SetActive(false);
    }

    private void CurrencyCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {

        craftingAmmo = false;
        foreach(CurrencyCrafterVisual_CurrencyBarTemplate ammoTemplate in currencyBarTemplateList) {
            ammoTemplate.SetGlowMaterial();
        }

        HighlightStructureFunctionIcon(true);

        if (crafterAnimator != null) {
            crafterAnimator.ResetTrigger("Crafting");
            crafterAnimator.SetTrigger("Idle");
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
        Debug.Log("GetCurrentBatch " + ammoCrafter.GetCurrentBatch());

        currencyBarTemplate.gameObject.SetActive(true);
        currencyBatchTemplate.gameObject.SetActive(true);

        if (ammoCrafter.GetCurrentBatch() == 0) {
            //First batch launched
            Debug.Log("clear currencyBarTemplateList");
            currencyBarTemplateList.Clear();
            currencyBatchTemplateList.Clear();
            foreach (RectTransform child in currencyBatchContainer) {
                if (child == currencyBatchTemplate) continue;
                Destroy(child.gameObject);
            }
        }

        Transform batchTemplate = Instantiate(currencyBatchTemplate, currencyBatchContainer);

        batchTemplate.gameObject.SetActive(true);
        int ammoCount = ammoCrafter.GetAmmoCraftAmount();
        foreach (RectTransform child in batchTemplate) {
            if (child == currencyBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ammoCount; i++) {
            CurrencyCrafterVisual_CurrencyBarTemplate ammoBar = Instantiate(currencyBarTemplate, batchTemplate).GetComponent<CurrencyCrafterVisual_CurrencyBarTemplate>();
            ammoBar.SetFillAmount(0f);
            currencyBarTemplateList.Add(ammoBar);
        }

        currencyBarTemplate.gameObject.SetActive(false);
        currencyBatchTemplate.gameObject.SetActive(false);
    }

}
