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

    private float maxDistanceToHear = 20f;

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
        GetComponent<SoundVolume2D>().SetSoundVolume2DActiveAfterDelay(true, 0);
        audioSource.PlayOneShot(currencyInstantiatedAudioClip, sfxVolume * 1.5f);
    }

    private void AmmoCrafter_OnAmmoCraftingStarted(object sender, CurrencyCrafter.OnNewCurrencyBatchCraftingStartedEventArgs e) {
        if(e.triggerStartCraftingSFX) {
            GetComponent<SoundVolume2D>().SetSoundVolume2DActiveAfterDelay(true, 0);

            float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);
            float distNorm = distanceToPlayer / maxDistanceToHear;
            float volumeMultiplier = 1f - Mathf.Clamp01(distNorm);

            audioSource.PlayOneShot(startCraftingAudioClip, sfxVolume * masterVolume* volumeMultiplier * 2);
        }

        if (currencyCrafter.GetCraftingCurrency()) return;

        StartCoroutine(StartPlayingLoop());
    }

    private IEnumerator StartPlayingLoop() {
        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(false, 0);
        yield return new WaitForSeconds(1f);
        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(true, 0);
        craftingAudioSource.volume = .3f * sfxVolume * masterVolume;
        craftingAudioSource.Play();
    }

    private void AmmoCrafter_OnAmmoCraftingEnded(object sender, System.EventArgs e) {
        craftingAudioSource.Stop();

        float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);
        float distNorm = distanceToPlayer / maxDistanceToHear;
        float volumeMultiplier = 1f - Mathf.Clamp01(distNorm);

        audioSource.PlayOneShot(endCraftingAudioClip, sfxVolume * masterVolume * volumeMultiplier * 2);
        craftingAudioSourceSoundVolume2D.SetSoundVolume2DActiveAfterDelay(false, endCraftingAudioClip.length);
        GetComponent<SoundVolume2D>().SetSoundVolume2DActiveAfterDelay(false, endCraftingAudioClip.length);
    }
}
