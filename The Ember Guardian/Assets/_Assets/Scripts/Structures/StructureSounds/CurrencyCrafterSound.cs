using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyCrafterSound : StructureSounds
{
    private CurrencyCrafter currencyCrafter;
    [SerializeField] private SoundVolume2D craftingAudioSourceSoundVolume2D;
    [SerializeField] private AudioSource craftingAudioSource;
    [SerializeField] private AudioClip startCraftingAudioClip;
    [SerializeField] private AudioClip endCraftingAudioClip;
    [SerializeField] private AudioClip loopCraftingAudioClip;
    [SerializeField] private AudioClip currencyInstantiatedAudioClip;

    protected override void Awake() {
        base.Awake();
        currencyCrafter = GetComponentInParent<CurrencyCrafter>();
        craftingAudioSource.clip = loopCraftingAudioClip;
        craftingAudioSource.loop = true;
    }

    protected override void Start()
    {
        base.Start();
        currencyCrafter.OnCurrencyCraftingEnded += AmmoCrafter_OnAmmoCraftingEnded;
        currencyCrafter.OnNewCurrencyBatchCraftingStarted += AmmoCrafter_OnAmmoCraftingStarted;
        currencyCrafter.OnCurrencyInstantiated += AmmoCrafter_OnCurrencyInstantiated;

        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(false, 0);
    }

    private void AmmoCrafter_OnCurrencyInstantiated(object sender, System.EventArgs e) {
        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(true, 0f);
        GetComponent<SoundVolume2D>().SetSoundVolume2DActiveAfterDelay(true, 0);
        audioSource.PlayOneShot(currencyInstantiatedAudioClip, sfxVolume);
    }

    private void AmmoCrafter_OnAmmoCraftingStarted(object sender, CurrencyCrafter.OnNewCurrencyBatchCraftingStartedEventArgs e) {
        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(true, 0);
        GetComponent<SoundVolume2D>().SetSoundVolume2DActiveAfterDelay(true, 0);

        if(e.triggerStartCraftingSFX) {
            audioSource.PlayOneShot(startCraftingAudioClip, sfxVolume * masterVolume);
        }

        if (currencyCrafter.GetCraftingCurrency()) return;

        StartCoroutine(StartPlayingLoop());
    }

    private IEnumerator StartPlayingLoop() {
        craftingAudioSource.Stop();
        yield return new WaitForSeconds(1f);
        craftingAudioSource.volume = .3f * sfxVolume * masterVolume;
        craftingAudioSource.Play();
    }

    private void AmmoCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {
        craftingAudioSource.Stop();
        audioSource.PlayOneShot(endCraftingAudioClip, sfxVolume * 2);
        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(false, endCraftingAudioClip.length);
        GetComponent<SoundVolume2D>().SetSoundVolume2DActiveAfterDelay(false, endCraftingAudioClip.length);
    }
}
