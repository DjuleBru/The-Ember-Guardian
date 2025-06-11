using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbExtractor : Structure
{
    [SerializeField] private Transform orbSpawnPosition;
    [SerializeField] private float orbExtractionDuration = 60f;
    private float orbExtractionRate = 1f;
    private float orbExtractionTimer;

    private bool crafting;

    protected override void Start() {
        base.Start();
        orbExtractionTimer = orbExtractionDuration;
    }

    private void Update() {
        orbExtractionTimer -= Time.deltaTime * orbExtractionRate;
        if(orbExtractionTimer <= 0) {
            ExtractOrb();
            orbExtractionTimer = orbExtractionDuration;
        }
    }

    private void ExtractOrb() {
        Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(PlayerCurrencies.CurrencyType.bigBlueOrb), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();
        collectible.ApplyRandomForce(-1, 1, 2, 4);
        collectible.SetCanBePickedUpByWorkerAfterDelay(2f);
    }
}
