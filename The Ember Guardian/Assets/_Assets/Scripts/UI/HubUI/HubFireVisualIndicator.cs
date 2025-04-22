using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubFireVisualIndicator : MonoBehaviour
{

    public static HubFireVisualIndicator Instance;

    private bool playerExtractedEmber;
    private Fire hubFire;
    [SerializeField] private GameObject fireIndicator;

    private void Awake() {
        Instance = this;

        hubFire = GetComponentInParent<Fire>();
        hubFire.OnFireEmberExtracted += HubFire_OnFireEmberExtracted;
        hubFire.OnFireEmberExtractionStarted += HubFire_OnFireEmberExtractionStarted;
        hubFire.OnFireEmberExtractionStopped += HubFire_OnFireEmberExtractionStopped;
        fireIndicator.gameObject.SetActive(false);

    }

    private void HubFire_OnFireEmberExtracted(object sender, System.EventArgs e) {
        playerExtractedEmber = true;
    }

    private void HubFire_OnFireEmberExtractionStopped(object sender, System.EventArgs e) {
        if (playerExtractedEmber) return;
        fireIndicator.gameObject.SetActive(true);

    }

    private void HubFire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        fireIndicator.gameObject.SetActive(false);
    }

    public void SetIndicatorActive() {
        fireIndicator.gameObject.SetActive(true);
    }
}
