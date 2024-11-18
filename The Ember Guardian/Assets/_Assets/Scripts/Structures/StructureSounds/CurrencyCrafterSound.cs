using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCrafterSound : MonoBehaviour
{
    private CurrencyCrafter currencyCrafter;
    private AudioSource audioSource;
    [SerializeField] private AudioClip startCraftingAudioClip;
    [SerializeField] private AudioClip endCraftingAudioClip;
    [SerializeField] private AudioClip currencyInstantiatedAudioClip;

    private void Awake() {
        currencyCrafter = GetComponentInParent<CurrencyCrafter>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currencyCrafter.OnCurrencyCraftingEnded += AmmoCrafter_OnAmmoCraftingEnded;
        currencyCrafter.OnCurrencyCraftingStarted += AmmoCrafter_OnAmmoCraftingStarted;
        currencyCrafter.OnCurrencyInstantiated += AmmoCrafter_OnCurrencyInstantiated;
    }

    private void AmmoCrafter_OnCurrencyInstantiated(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(currencyInstantiatedAudioClip);
    }

    private void AmmoCrafter_OnAmmoCraftingStarted(object sender, System.EventArgs e) {
        audioSource.volume = 1f;
        audioSource.PlayOneShot(startCraftingAudioClip);
        StartCoroutine(StartPlayingLoop());
    }

    private IEnumerator StartPlayingLoop() {
        yield return new WaitForSeconds(1f);
        audioSource.volume = .3f;
        audioSource.Play();
    }

    private void AmmoCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {
        audioSource.volume = 1f;
        audioSource.PlayOneShot(endCraftingAudioClip);
        audioSource.Stop();
    }
}
