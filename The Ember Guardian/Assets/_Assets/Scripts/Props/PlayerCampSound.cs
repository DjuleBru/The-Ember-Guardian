using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCampSound : SoundObject {

    [SerializeField] private AudioClip idleAudioClip;
    [SerializeField] private AudioClip[] buildAudioClips;
    [SerializeField] protected AudioSource buildAudioSource;
    protected AudioSource audioSource;

    protected bool initialCampBackgroundBuilt;

    protected void Awake() {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = idleAudioClip;
        audioSource.volume = 0f;
    }

    protected override void Start() {
        base.Start();
        PlayerCampVisual.Instance.OnCampBackgroundBuilt += PlayerCampVIsual_OnCampBackgroundBuilt;
        PlayerCampVisual.Instance.OnCampBackgroundBuild_Start += PlayerCampVisual_OnCampBackgroundBuild_Start;
    }

    private void PlayerCampVisual_OnCampBackgroundBuild_Start(object sender, System.EventArgs e) {
        FadeIn(audioSource, .2f, sfxVolume);
    }


    private void PlayerCampVIsual_OnCampBackgroundBuilt(object sender, System.EventArgs e) {
        FadeOut(audioSource, 1f);
        buildAudioSource.PlayOneShot(buildAudioClips[Random.Range(0, buildAudioClips.Length)], .75f * sfxVolume);
    }


}
