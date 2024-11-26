using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCrafterSound : StructureSounds
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

    protected override void Start()
    {
        base.Start();
        currencyCrafter.OnCurrencyCraftingEnded += AmmoCrafter_OnAmmoCraftingEnded;
        currencyCrafter.OnCurrencyCraftingStarted += AmmoCrafter_OnAmmoCraftingStarted;
        currencyCrafter.OnCurrencyInstantiated += AmmoCrafter_OnCurrencyInstantiated;
    }

    private void AmmoCrafter_OnCurrencyInstantiated(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(currencyInstantiatedAudioClip, sfxVolume);
    }

    private void AmmoCrafter_OnAmmoCraftingStarted(object sender, System.EventArgs e) {
        audioSource.volume = 1f * sfxVolume;
        audioSource.PlayOneShot(startCraftingAudioClip, sfxVolume);
        StartCoroutine(StartPlayingLoop());
    }

    private IEnumerator StartPlayingLoop() {
        yield return new WaitForSeconds(1f);
        audioSource.volume = .3f * sfxVolume;
        audioSource.Play();
    }

    private void AmmoCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {
        audioSource.volume = 1f * sfxVolume;
        audioSource.PlayOneShot(endCraftingAudioClip, sfxVolume);
        audioSource.Stop();
    }
}
