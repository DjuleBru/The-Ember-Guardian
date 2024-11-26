using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayCurrencyUI_SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] payOrbsUIAudioClips;
    [SerializeField] private AudioClip[] paySmallOrbsUIAudioClips;
    [SerializeField] private AudioClip[] payRedOrbsUIAudioClips;
    [SerializeField] private AudioClip[] paySmallRedOrbsUIAudioClips;
    [SerializeField] private AudioClip payEmberAudioClip;

    private float pitch;
    private float sfxVolume;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
        SettingsManager.Instance.OnSfxVolumeChanged += SettingsManager_OnSfxVolumeChanged;
        PayCurrencyUI.OnAnySingleCurrencyPaid += PayOrbsUI_OnSingleOrbFilled1;
    }

    private void SettingsManager_OnSfxVolumeChanged(object sender, System.EventArgs e) {
        sfxVolume = SettingsManager.Instance.GetSfxVolume();
    }

    private void PayOrbsUI_OnSingleOrbFilled1(object sender, PayCurrencyUI.OnSingleOrbFilledEventArgs e) {


        AudioClip audioClip = payOrbsUIAudioClips[Random.Range(0, payOrbsUIAudioClips.Length)];

        if ((sender as PayCurrencyUI).GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay() == PlayerCurrencies.CurrencyType.ember) {
            audioClip = payEmberAudioClip;
            audioSource.PlayOneShot(audioClip, sfxVolume);
            return;
        }

        if ((sender as PayCurrencyUI).GetIsLastCurrencyPaid()) return;

        
        if((sender as PayCurrencyUI).GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            audioClip = paySmallOrbsUIAudioClips[Random.Range(0, paySmallOrbsUIAudioClips.Length)];
        }

        if ((sender as PayCurrencyUI).GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay() == PlayerCurrencies.CurrencyType.bigRedOrb) {
            audioClip = payRedOrbsUIAudioClips[Random.Range(0, payRedOrbsUIAudioClips.Length)];
        }

        if ((sender as PayCurrencyUI).GetCurrentCurrencyTemplateWorldUI().GetCurrencyTypeToPay() == PlayerCurrencies.CurrencyType.smallRedOrb) {
            audioClip = paySmallRedOrbsUIAudioClips[Random.Range(0, paySmallRedOrbsUIAudioClips.Length)];
        }

        audioSource.PlayOneShot(audioClip, sfxVolume);

    }

    private void OnDestroy() {
        PayCurrencyUI.OnAnySingleCurrencyPaid -= PayOrbsUI_OnSingleOrbFilled1;
    }
}
