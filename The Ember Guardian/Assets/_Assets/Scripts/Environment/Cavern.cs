using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavern : MonoBehaviour
{
    [SerializeField] private AudioClip cavernAudioClip;
    [SerializeField] private float cavernAudioVolume;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Player>() != null) {
            BackgroundSoundsManager.Instance.SetInCavern(true, cavernAudioClip, cavernAudioVolume);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            BackgroundSoundsManager.Instance.SetInCavern(false, cavernAudioClip, cavernAudioVolume);
        }
    }
}
