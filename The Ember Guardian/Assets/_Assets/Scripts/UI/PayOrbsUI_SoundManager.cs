using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayOrbsUI_SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] payOrbsUIAudioClips;

    //[SerializeField] private float initialPitch = 0;
    //[SerializeField] private float pitchIncreasePerOrb = .2f;
    private float pitch;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        PayOrbsUI.OnAnySingleOrbPaid += PayOrbsUI_OnSingleOrbFilled1;
    }

    private void PayOrbsUI_OnSingleOrbFilled1(object sender, PayOrbsUI.OnSingleOrbFilledEventArgs e) {

        Structure structureFromWhichOrbWasPaid = (sender as PayOrbsUI).GetComponent<Structure>();
        if(structureFromWhichOrbWasPaid != null ) {
            if (structureFromWhichOrbWasPaid.GetCurrentStructureInteractionType() == Structure.StructureInteractionType.function) {
                return;
            }
        }
        

        //pitch = initialPitch + e.orbIndex*pitchIncreasePerOrb;
        //audioSource.pitch = pitch;

        AudioClip audioClip = payOrbsUIAudioClips[Random.Range(0, payOrbsUIAudioClips.Length)];
        audioSource.PlayOneShot(audioClip);
    }

}
