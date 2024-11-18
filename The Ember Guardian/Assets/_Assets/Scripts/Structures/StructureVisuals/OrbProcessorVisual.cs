using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbProcessorVisual : StructureVisual
{
    [SerializeField] private SpriteRenderer orbContainerSpriteRenderer;
    [SerializeField] private List<Sprite> orbContainerFillingSpriteList;

    private OrbProcessor bigOrbCrafter;
    private bool craftingOrb;
    private int currentSpriteIndex;

    protected override void Awake() {
        base.Awake();
        bigOrbCrafter = GetComponentInParent<OrbProcessor>();
    }

    protected override void Start() {
        base.Start();
        bigOrbCrafter.OnPlayerCollectedOrb += BigOrbCrafter_OnPlayerCollectedOrb;
    }

    private void Update() {
        if(bigOrbCrafter.GetCraftingOrb()) {
           float craftAmountNormalized = bigOrbCrafter.GetOrbCraftTimerNormalized();
            int newSpriteIndex = Mathf.RoundToInt(craftAmountNormalized * orbContainerFillingSpriteList.Count);

            if(currentSpriteIndex != newSpriteIndex) {
                currentSpriteIndex = newSpriteIndex;
                orbContainerSpriteRenderer.sprite = orbContainerFillingSpriteList[currentSpriteIndex];
            }
        }
    }

    private void BigOrbCrafter_OnPlayerCollectedOrb(object sender, System.EventArgs e) {
        orbContainerSpriteRenderer.sprite = orbContainerFillingSpriteList[0];
    }

}
