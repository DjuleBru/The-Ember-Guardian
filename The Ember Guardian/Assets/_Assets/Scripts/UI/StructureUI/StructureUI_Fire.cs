using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StructureUI_Fire : StructureUI
{
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarContainer;
    [SerializeField] private RectTransform progressBarTemplate;

    private float progressTemplateWidth = .5f;
    private float progressTemplateHeight = .2f;
    private int barAmountPerOrb = 2;

    private float currentFuelLowLimit;
    private float currentFuelHighLimit;
    private float currentFuelDelta;
    private int previousBarAmount;

    private int currentLevelMaxBarAmount;
    private int maxBarAmount;
    private int currentFuelBarDelta;

    private Fire fire;

    protected override void Awake() {
        base.Awake();
        fire = GetComponentInParent<Fire>();

    }
    protected override void Start() {
        base.Start();
        fire.OnFireChangedState += Fire_OnFireChangedState;
    }

    protected void Update() {
        int currentbarAmount = Mathf.FloorToInt((fire.GetCurrentFuelLevel() - currentFuelLowLimit) / fire.GetOrbFuelValue()) * barAmountPerOrb + 1;

        if(currentbarAmount != previousBarAmount) {
            RefreshProgressBar(currentbarAmount);
            previousBarAmount = currentbarAmount;
        }
    }

    protected void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {


        if (e.newState == Fire.State.calm) {
            currentFuelLowLimit = fire.GetCalmFireTreshold();
            currentFuelHighLimit = fire.GetMildFireTreshold();
        }

        if (e.newState == Fire.State.mild) {
            currentFuelLowLimit = fire.GetMildFireTreshold();
            currentFuelHighLimit = fire.GetWildFireTreshold();
        }

        if (e.newState == Fire.State.wild) {
            currentFuelLowLimit = fire.GetWildFireTreshold();
            currentFuelHighLimit = fire.GetInsaneFireTreshold();
        }

        if (e.newState == Fire.State.insane) {
            currentFuelLowLimit = fire.GetInsaneFireTreshold();
            currentFuelHighLimit = fire.GetMaxFireTreshold();
        }

        currentFuelDelta = currentFuelHighLimit - currentFuelLowLimit;

        maxBarAmount = Mathf.FloorToInt(currentFuelDelta / fire.GetOrbFuelValue()) * barAmountPerOrb;
        progressBar.sizeDelta = new Vector2(progressTemplateWidth, maxBarAmount * progressTemplateHeight);
        RefreshFireSlotVisuals();
    }

    private void RefreshProgressBar(int barAmount) {
        progressBarTemplate.gameObject.SetActive(true);

        foreach (RectTransform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        for(int i = 0; i < barAmount; i++) {
            Instantiate(progressBarTemplate, progressBarContainer);
        }

        progressBarTemplate.gameObject.SetActive(false);
    }

    private void RefreshFireSlotVisuals() {

        foreach(GameObject go in levelSlotVisualContainerList) {
            go.SetActive(false);
        }

        if(fire.GetState() == Fire.State.calm) {
            levelSlotVisualContainerList[0].SetActive(true);
        }
        if (fire.GetState() == Fire.State.mild) {
            levelSlotVisualContainerList[1].SetActive(true);
        }
        if (fire.GetState() == Fire.State.wild) {
            levelSlotVisualContainerList[2].SetActive(true);
        }
        if (fire.GetState() == Fire.State.insane) {
            levelSlotVisualContainerList[3].SetActive(true);
        }
    }

}
