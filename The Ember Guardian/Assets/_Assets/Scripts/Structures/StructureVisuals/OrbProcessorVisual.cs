using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbProcessorVisual : StructureVisual
{
    [SerializeField] private SpriteRenderer orbContainerSpriteRenderer;
    [SerializeField] private List<Sprite> orbContainerFillingSpriteList;

    [SerializeField] private GameObject craftOrbs_PayCurrenciesGameObject;
    [SerializeField] private GameObject craftOrbs_craftingOrbsGameObject;

    [SerializeField] private RectTransform ammoBarContainer;
    [SerializeField] private RectTransform ammoBarTemplate;

    private OrbProcessor orbProcessor;
    private bool craftingOrb;
    private int currentSpriteIndex;

    private List<CurrencyCrafterVisual_CurrencyBarTemplate> orbsTemplateList = new List<CurrencyCrafterVisual_CurrencyBarTemplate>();

    protected override void Awake() {
        base.Awake();
        orbProcessor = GetComponentInParent<OrbProcessor>();
    }

    protected override void Start() {
        base.Start();
        orbProcessor.OnPlayerCollectedOrb += OrbProcessor_OnPlayerCollectedOrb;
        orbProcessor.OnOrbCraftingStarted += OrbProcessor_OnOrbCraftingStarted;
        orbProcessor.OnOrbCraftingEnded += OrbProcessor_OnOrbCraftingEnded;
    }

    private void OrbProcessor_OnOrbCraftingEnded(object sender, System.EventArgs e) {
        craftingOrb = false;
        foreach (CurrencyCrafterVisual_CurrencyBarTemplate orbTemplate in orbsTemplateList) {
            orbTemplate.SetCrafted();
        }

        HighlightStructureFunctionIcon(true);
    }

    private void OrbProcessor_OnOrbCraftingStarted(object sender, System.EventArgs e) {
        craftOrbs_PayCurrenciesGameObject.SetActive(false);
        craftOrbs_craftingOrbsGameObject.SetActive(true);
        RefreshOrbsCraftinsVisuals();
        HighlightStructureFunctionIcon(false);
    }

    private void Update() {
        if(orbProcessor.GetCraftingOrb()) {
           float craftAmountNormalized = orbProcessor.GetOrbCraftTimerNormalized();
            int newSpriteIndex = Mathf.RoundToInt(craftAmountNormalized * orbContainerFillingSpriteList.Count);

            if(currentSpriteIndex != newSpriteIndex) {
                currentSpriteIndex = newSpriteIndex;
                orbContainerSpriteRenderer.sprite = orbContainerFillingSpriteList[currentSpriteIndex];
            }
        }
    }

    private void OrbProcessor_OnPlayerCollectedOrb(object sender, System.EventArgs e) {
        orbContainerSpriteRenderer.sprite = orbContainerFillingSpriteList[0];
    }

    private void RefreshOrbsCraftinsVisuals() {
        orbsTemplateList.Clear();
        ammoBarTemplate.gameObject.SetActive(true);

        int ammoCount = orbProcessor.GetOrbCraftAmount();
        foreach (RectTransform child in ammoBarContainer) {
            if (child == ammoBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ammoCount; i++) {
            CurrencyCrafterVisual_CurrencyBarTemplate ammoBar = Instantiate(ammoBarTemplate, ammoBarContainer).GetComponent<CurrencyCrafterVisual_CurrencyBarTemplate>();
            ammoBar.SetFillAmount(0f);
            orbsTemplateList.Add(ammoBar);
        }

        ammoBarTemplate.gameObject.SetActive(false);
    }

}
