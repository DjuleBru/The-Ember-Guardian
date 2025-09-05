using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCrafterSound : StructureSounds
{
    private CurrencyCrafter currencyCrafter;
    private SoundVolume2D soundVolume2D;
    [SerializeField] private AudioClip startCraftingAudioClip;
    [SerializeField] private AudioClip endCraftingAudioClip;
    [SerializeField] private AudioClip currencyInstantiatedAudioClip;

    protected override void Awake() {
        base.Awake();
        currencyCrafter = GetComponentInParent<CurrencyCrafter>();
        soundVolume2D = GetComponent<SoundVolume2D>();
    }

    protected override void Start()
    {
        base.Start();
        currencyCrafter.OnCurrencyCraftingEnded += AmmoCrafter_OnAmmoCraftingEnded;
        currencyCrafter.OnNewCurrencyBatchCraftingStarted += AmmoCrafter_OnAmmoCraftingStarted;
        currencyCrafter.OnCurrencyInstantiated += AmmoCrafter_OnCurrencyInstantiated;
    }

    private void AmmoCrafter_OnCurrencyInstantiated(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(currencyInstantiatedAudioClip, sfxVolume);
    }

    private void AmmoCrafter_OnAmmoCraftingStarted(object sender, System.EventArgs e) {
        soundVolume2D.SetSoundVolume2DActiveAfterDelay(true, 0);

        audioSource.PlayOneShot(startCraftingAudioClip, sfxVolume);
        audioSource.volume = 1f * sfxVolume;

        if (currencyCrafter.GetCraftingCurrency()) return;

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
        soundVolume2D.SetSoundVolume2DActiveAfterDelay(false, endCraftingAudioClip.length);
    }
}
