using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrafterSound : MonoBehaviour
{
    private AmmoCrafter ammoCrafter;
    private AudioSource audioSource;
    [SerializeField] private AudioClip startCraftingAudioClip;
    [SerializeField] private AudioClip endCraftingAudioClip;

    private void Awake() {
        ammoCrafter = GetComponentInParent<AmmoCrafter>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ammoCrafter.OnAmmoCraftingEnded += AmmoCrafter_OnAmmoCraftingEnded;
        ammoCrafter.OnAmmoCraftingStarted += AmmoCrafter_OnAmmoCraftingStarted;
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
