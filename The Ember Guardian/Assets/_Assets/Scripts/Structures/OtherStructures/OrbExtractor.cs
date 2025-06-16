using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbExtractor : Structure
{
    [SerializeField] private Transform orbSpawnPosition;
    [SerializeField] private int drillAmountRequiredToCollectOrb = 10;
    [SerializeField] private Transform engineerWorkingPosition;
    [SerializeField] private Transform engineerDropPosition;

    private int drillIndex;

    public event EventHandler OnExtractorStartedDrilling;
    public event EventHandler OnEngineerStartedWorking;
    public event EventHandler OnEngineerStoppedWorking;

    private float probabilityToExtractBigOrb = .3f;
    private float drillAnimationDuration = 1.5f;
    private float extractionRate = 1f;
    private float extractionRateWithEngineer = 3f;
    private float extractionTimer;
    private float pauseDuration = 8f;

    protected override void Start() {
        base.Start();

        extractionTimer = 2f;
        needsWorking = true;
    }

    private void Update() {
        extractionTimer -= Time.deltaTime * extractionRate;
        if(extractionTimer <= 0) {
            StartCoroutine(Drill());
            extractionTimer = pauseDuration;
        }
    }

    private IEnumerator Drill() {
        drillIndex++;
        OnExtractorStartedDrilling?.Invoke(this, EventArgs.Empty);

        if(engineersAssignedWorking.Count != 0) {
            if(engineersAssignedWorking[0].GetState() == EngineerJob.EngineerState.workingInStructure) {
                engineersAssignedWorking[0].OrbExtractorTriggerDrill();
            }
        }

        yield return new WaitForSeconds(drillAnimationDuration);
        
        if(drillIndex == drillAmountRequiredToCollectOrb) {
            ExtractOrb();
            drillIndex = 0;
        }
    }

    private void ExtractOrb() {
        Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.smallBlueOrb);

        if (UnityEngine.Random.value < probabilityToExtractBigOrb) {
            currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.bigBlueOrb);
        }

        Collectible collectible = Instantiate(currencyPrefab, orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomForce(-2, 2, 4, 6);
        collectible.SetCanBePickedUpByWorkerAfterDelay(2f);
        collectible.SetCollectibleUnInteractable(1f);
    }

    public override void SetEngineerWorking(EngineerJob engineer, bool working) {
        base.SetEngineerWorking(engineer, working);
        needsWorking = !working;

        Debug.Log("SetEngineerWorking " + working);

        if(working) {

            extractionRate = extractionRateWithEngineer;
            engineer.transform.position = engineerWorkingPosition.position;
            OnEngineerStartedWorking?.Invoke(this, EventArgs.Empty);

        } else {

            extractionRate = 1f;
            engineer.transform.position = engineerDropPosition.position;
            OnEngineerStoppedWorking?.Invoke(this, EventArgs.Empty);

        }

    }
}
