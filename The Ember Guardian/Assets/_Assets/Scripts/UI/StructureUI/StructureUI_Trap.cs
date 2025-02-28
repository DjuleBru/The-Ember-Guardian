using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI_Trap : StructureUI
{
    [SerializeField] protected Structure_Trap trap;
    [SerializeField] protected Transform rearmTrapPayCurrencyContainer;
    [SerializeField] protected Transform rearmTrapPayCurrencyTemplate;

    protected override void Awake() {
        base.Awake();
        trap.OnTrapDepletedUses += Trap_OnTrapDepletedUses;
        trap.OnTrapRearmPriceChanged += Trap_OnTrapRearmPriceChanged;

        RefreshTrapRearmUI();
    }

    private void Trap_OnTrapRearmPriceChanged(object sender, System.EventArgs e) {
        RefreshTrapRearmUI();
    }

    private void Trap_OnTrapDepletedUses(object sender, System.EventArgs e) {

    }

    private void RefreshTrapRearmUI() {
        int reamAmount = trap.GetTrapSO().rearmPrice;
        rearmTrapPayCurrencyTemplate.gameObject.SetActive(true);

        foreach (Transform child in rearmTrapPayCurrencyContainer) {
            if (child == rearmTrapPayCurrencyTemplate) continue;
            Destroy(child.gameObject);
        }

        for(int i = 0; i < reamAmount; i++) {
            Instantiate(rearmTrapPayCurrencyTemplate, rearmTrapPayCurrencyContainer);
        }

        rearmTrapPayCurrencyTemplate.gameObject.SetActive(false);
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(functionPayOrbsUIList));
    }
}
