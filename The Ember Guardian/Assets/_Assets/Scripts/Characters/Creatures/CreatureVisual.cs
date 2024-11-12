using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CreatureVisual : MobVisual
{
    private Creature creature;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private GameObject debuffedGameObject;

    protected override void Awake() {
        base.Awake();
        creature = GetComponentInParent<Creature>();
    }

    protected void Start() {
        creature.OnCreatureEnteredLight += Creature_OnCreatureEnteredLight;
        creature.OnCreatureExitedLight += Creature_OnCreatureExitedLight;
        bodySpriteRenderer.material = cleanMaterial;
        debuffedGameObject.SetActive(false);

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {

    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {

    }

    private void Creature_OnCreatureExitedLight(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = cleanMaterial;
        debuffedGameObject.SetActive(false);
    }

    private void Creature_OnCreatureEnteredLight(object sender, System.EventArgs e) {
        //bodySpriteRenderer.material = debuffedMaterial;
        debuffedGameObject.SetActive(true);
    }
}
