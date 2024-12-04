using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_Flicker : MonoBehaviour
{
    [SerializeField] private bool lightFlicker;
    [SerializeField] private AudioSource lightFlickerAudioSource;
    [SerializeField] private AudioClip[] lightFlickerAudioClipArray;
    private Animator lightAnimator;

    private void Awake() {
        lightAnimator = GetComponent<Animator>();

        if(!lightFlicker) {
            lightAnimator.enabled = false;
        } else {
            StartCoroutine(StartFlickerWithRandomDelay());
        }
    }

    public void TriggerFlickerSound() {
        AudioClip audioCLip = lightFlickerAudioClipArray[UnityEngine.Random.Range(0, lightFlickerAudioClipArray.Length)];
        lightFlickerAudioSource.PlayOneShot(audioCLip, .5f);
    }

    private IEnumerator StartFlickerWithRandomDelay() {
        float delay = Random.Range(0f, 5f);

        yield return new WaitForSeconds(delay);
        lightAnimator.enabled = true;
    }

}
