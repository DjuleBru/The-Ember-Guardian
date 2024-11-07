using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeSound : MonoBehaviour
{
    private AudioSource barricadeAudioSource;

    [SerializeField] private Barricade barricade;
    [SerializeField] private BarricadeVisual barricadeVisual;

    [SerializeField] private AudioClip[] damagedAudioClips;
    [SerializeField] private AudioClip[] spriteFellAudioClips;
    [SerializeField] private AudioClip[] destroyedAudioClips;
    [SerializeField] private AudioClip[] repairedAudioClips;

    private void Awake() {
        barricadeAudioSource = GetComponent<AudioSource>(); 
    }

    private void Start() {
        barricade.OnBarricadeDamageTaken += Barricade_OnBarricadeDamageTaken;
        barricade.OnBarricadeDestroyed += Barricade_OnBarricadeDestroyed;
        barricade.OnBarricadeRepaired += Barricade_OnBarricadeRepaired;
        barricadeVisual.OnBarricadeSpriteFell += BarricadeVisual_OnBarricadeSpriteFell;
    }


    private void BarricadeVisual_OnBarricadeSpriteFell(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(spriteFellAudioClips[Random.Range(0, spriteFellAudioClips.Length)], .75f);
    }

    private void Barricade_OnBarricadeRepaired(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(repairedAudioClips[Random.Range(0, repairedAudioClips.Length)]);
    }

    private void Barricade_OnBarricadeDestroyed(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(destroyedAudioClips[Random.Range(0, destroyedAudioClips.Length)], .75f);
    }

    private void Barricade_OnBarricadeDamageTaken(object sender, System.EventArgs e) {
        if (barricade.GetBarricadeHealthNormalized() <= 0) return; 
        barricadeAudioSource.PlayOneShot(damagedAudioClips[Random.Range(0, damagedAudioClips.Length)], .5f);
    }
}
