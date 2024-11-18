using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyCrafterVisual : StructureVisual
{
    [SerializeField] private GameObject craftCurrency_PayOrbsGameObject;
    [SerializeField] private GameObject craftCurrency_craftingCurrencyGameObject;

    [SerializeField] private RectTransform currencyBarContainer;
    [SerializeField] private RectTransform currencyBarTemplate;

    private bool craftingAmmo;
    private CurrencyCrafter ammoCrafter;
    private List<CurrencyCrafterVisual_CurrencyBarTemplate> currencyBarTemplateList = new List<CurrencyCrafterVisual_CurrencyBarTemplate>();

    protected override void Awake() {
        base.Awake();
        ammoCrafter = GetComponentInParent<CurrencyCrafter>();
    }

    protected override void Start() {
        base.Start();

        ammoCrafter.OnCurrencyCraftingEnded += CurrencyCrafter_OnAmmoCraftingEnded;
        ammoCrafter.OnCurrencyCraftingStarted += CurrencyCrafter_OnAmmoCraftingStarted;
        ammoCrafter.OnPlayerCollectedCurrency += CurrencyCrafter_OnPlayerCollectedAmmo;

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

    private void CurrencyCrafter_OnAmmoCraftingStarted(object sender, System.EventArgs e) {
        craftCurrency_PayOrbsGameObject.SetActive(false);
        craftCurrency_craftingCurrencyGameObject.SetActive(true);
        RefreshCurrencyBarVisuals();
        HighlightStructureFunctionIcon(false);

        craftingAmmo = true;
    }

    private void CurrencyCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {

        craftingAmmo = false;
        foreach(CurrencyCrafterVisual_CurrencyBarTemplate ammoTemplate in currencyBarTemplateList) {
            ammoTemplate.SetGlowMaterial();
        }

        HighlightStructureFunctionIcon(true);

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
        currencyBarTemplateList.Clear();
        currencyBarTemplate.gameObject.SetActive(true);

        int ammoCount = ammoCrafter.GetAmmoCraftAmount();
        foreach (RectTransform child in currencyBarContainer) {
            if (child == currencyBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ammoCount; i++) {
            CurrencyCrafterVisual_CurrencyBarTemplate ammoBar = Instantiate(currencyBarTemplate, currencyBarContainer).GetComponent<CurrencyCrafterVisual_CurrencyBarTemplate>();
            ammoBar.SetFillAmount(0f);
            currencyBarTemplateList.Add(ammoBar);
        }

        currencyBarTemplate.gameObject.SetActive(false);
    }

}
