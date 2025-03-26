using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeSound : StructureSounds
{
    private AudioSource barricadeAudioSource;
    [SerializeField] private AudioSource worldAudioSource;

    [SerializeField] private Barricade barricade;
    [SerializeField] private BarricadeVisual barricadeVisual;

    [SerializeField] private AudioClip turnOnOffLight;
    [SerializeField] private AudioClip warningBreachedAudioClip;
    [SerializeField] private AudioClip warningDingAudioClip;
    [SerializeField] private AudioClip[] damagedAudioClips;
    [SerializeField] private AudioClip[] spriteFellAudioClips;
    [SerializeField] private AudioClip[] destroyedAudioClips;
    [SerializeField] private AudioClip[] repairedAudioClips;

    protected override void Awake() {
        base.Awake();
        barricadeAudioSource = GetComponent<AudioSource>(); 
    }

    protected override void Start() {
        base.Start();
        barricade.OnBarricadeDamageTaken += Barricade_OnBarricadeDamageTaken;
        barricade.OnBarricadeDestroyed += Barricade_OnBarricadeDestroyed;
        barricade.OnBarricadeRepaired += Barricade_OnBarricadeRepaired;
        barricadeVisual.OnBarricadeSpriteFell += BarricadeVisual_OnBarricadeSpriteFell;
        barricade.OnBarricadeLightSwitched += Barricade_OnBarricadeLightSwitched;
        barricade.OnBarricadeBreached += Barricade_OnBarricadeBreached;
    }

    private void Barricade_OnBarricadeBreached(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(warningBreachedAudioClip,  sfxVolume);
    }

    private void Barricade_OnBarricadeLightSwitched(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(turnOnOffLight, .75f * sfxVolume);
    }

    private void BarricadeVisual_OnBarricadeSpriteFell(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(spriteFellAudioClips[Random.Range(0, spriteFellAudioClips.Length)], .75f * sfxVolume);
    }

    private void Barricade_OnBarricadeRepaired(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(repairedAudioClips[Random.Range(0, repairedAudioClips.Length)], sfxVolume);
    }

    private void Barricade_OnBarricadeDestroyed(object sender, System.EventArgs e) {
        barricadeAudioSource.PlayOneShot(destroyedAudioClips[Random.Range(0, destroyedAudioClips.Length)], .75f * sfxVolume);
    }

    private void Barricade_OnBarricadeDamageTaken(object sender, System.EventArgs e) {
        if (barricade.GetBarricadeHealthNormalized() <= 0) return;
        worldAudioSource.PlayOneShot(damagedAudioClips[Random.Range(0, damagedAudioClips.Length)], .5f * sfxVolume);
    }

    public void TriggerBarricadeBreachedWarningDing() {
        worldAudioSource.PlayOneShot(warningDingAudioClip, .5f * sfxVolume);
    }
}
