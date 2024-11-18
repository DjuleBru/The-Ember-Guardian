using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayCurrencyUI_SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] payOrbsUIAudioClips;
    [SerializeField] private AudioClip[] paySmallOrbsUIAudioClips;

    private float pitch;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        PayCurrencyUI.OnAnySingleCurrencyPaid += PayOrbsUI_OnSingleOrbFilled1;
    }

    private void PayOrbsUI_OnSingleOrbFilled1(object sender, PayCurrencyUI.OnSingleOrbFilledEventArgs e) {

        AudioClip audioClip = payOrbsUIAudioClips[Random.Range(0, payOrbsUIAudioClips.Length)];
        
        if((sender as PayCurrencyUI).GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            audioClip = paySmallOrbsUIAudioClips[Random.Range(0, paySmallOrbsUIAudioClips.Length)];
        }

        audioSource.PlayOneShot(audioClip);
    }

}
