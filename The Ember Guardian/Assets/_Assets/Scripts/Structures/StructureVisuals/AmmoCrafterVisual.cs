using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCrafterVisual : StructureVisual
{
    [SerializeField] private GameObject craftAmmo_PayOrbsGameObject;
    [SerializeField] private GameObject craftAmmo_craftingAmmoGameObject;

    [SerializeField] private RectTransform ammoBarContainer;
    [SerializeField] private RectTransform ammoBarTemplate;

    private bool craftingAmmo;
    private AmmoCrafter ammoCrafter;
    private List<AmmoCrafterVisual_AmmoBarTemplate> ammoBarTemplateList = new List<AmmoCrafterVisual_AmmoBarTemplate>();

    protected override void Awake() {
        base.Awake();
        ammoCrafter = GetComponentInParent<AmmoCrafter>();
    }

    protected override void Start() {
        base.Start();

        ammoCrafter.OnAmmoCraftingEnded += AmmoCrafter_OnAmmoCraftingEnded;
        ammoCrafter.OnAmmoCraftingStarted += AmmoCrafter_OnAmmoCraftingStarted;
        ammoCrafter.OnPlayerCollectedAmmo += AmmoCrafter_OnPlayerCollectedAmmo;

        craftAmmo_PayOrbsGameObject.SetActive(true);
        craftAmmo_craftingAmmoGameObject.SetActive(false);
        HighlightStructureFunctionIcon(true);
    }

    protected void Update() {

        if(craftingAmmo) {

            float craftingProgression = ammoCrafter.GetAmmoCraftTimerNormalized();
            RefreshAmmoBarProgression(craftingProgression);

        }
    }

    private void AmmoCrafter_OnPlayerCollectedAmmo(object sender, System.EventArgs e) {
        craftAmmo_PayOrbsGameObject.SetActive(true);
        craftAmmo_craftingAmmoGameObject.SetActive(false);
    }

    private void AmmoCrafter_OnAmmoCraftingStarted(object sender, System.EventArgs e) {
        craftAmmo_PayOrbsGameObject.SetActive(false);
        craftAmmo_craftingAmmoGameObject.SetActive(true);
        RefreshAmmoBarVisuals();
        HighlightStructureFunctionIcon(false);

        craftingAmmo = true;
    }

    private void AmmoCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {

        craftingAmmo = false;
        foreach(AmmoCrafterVisual_AmmoBarTemplate ammoTemplate in ammoBarTemplateList) {
            ammoTemplate.SetGlowMaterial();
        }

        HighlightStructureFunctionIcon(true);

    }


    private void RefreshAmmoBarProgression(float craftingProgression) {
        float totalBars = ammoBarTemplateList.Count * craftingProgression;

        for (int i = 0; i < totalBars; i++) {
            if (i < Mathf.FloorToInt(totalBars)) {
                ammoBarTemplateList[i].SetFillAmount(1f);
            }
            else if (i == Mathf.FloorToInt(totalBars)) {
                ammoBarTemplateList[i].SetFillAmount(totalBars - Mathf.Floor(totalBars));
            }
            else {
                ammoBarTemplateList[i].SetFillAmount(0f);
            }
        }

    }

    private void RefreshAmmoBarVisuals() {
        ammoBarTemplateList.Clear();
        ammoBarTemplate.gameObject.SetActive(true);

        int ammoCount = ammoCrafter.GetAmmoCraftAmount();
        foreach (RectTransform child in ammoBarContainer) {
            if (child == ammoBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ammoCount; i++) {
            AmmoCrafterVisual_AmmoBarTemplate ammoBar = Instantiate(ammoBarTemplate, ammoBarContainer).GetComponent<AmmoCrafterVisual_AmmoBarTemplate>();
            ammoBar.SetFillAmount(0f);
            ammoBarTemplateList.Add(ammoBar);
        }

        ammoBarTemplate.gameObject.SetActive(false);
    }

}
