using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalVisual : MobVisual
{

    [SerializeField] protected SpriteRenderer glowSpriteRenderer;
    [SerializeField] protected float dayGlowAmount = 2f;
    [SerializeField] protected float standardGlowAmount = 5f;

    protected override void Awake() {
        base.Awake();
        glowSpriteRenderer.sortingOrder = bodySpriteRenderer.sortingOrder+1;
    }

    protected void Start() {
        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        glowSpriteRenderer.material.SetFloat("_Glow", standardGlowAmount);
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        glowSpriteRenderer.material.SetFloat("_Glow", dayGlowAmount);
    }
}
