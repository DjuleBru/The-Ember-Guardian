using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavern : MonoBehaviour
{
    [SerializeField] private AudioClip cavernAudioClip;
    [SerializeField] private Fog_Front fog_Front;
    [SerializeField] private float cavernAudioVolume;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Player>() != null) {
            BackgroundSoundsManager.Instance.SetInCavern(true, cavernAudioClip, cavernAudioVolume);
            fog_Front.SetInCavern(true);
            DayNightVisualsManager.Instance.SetInCave(true);
            RainManager.Instance.SetInCavern(true);
            WindManager.Instance.SetInCavern(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            BackgroundSoundsManager.Instance.SetInCavern(false, cavernAudioClip, cavernAudioVolume);
            fog_Front.SetInCavern(false);
            DayNightVisualsManager.Instance.SetInCave(false);
            RainManager.Instance.SetInCavern(false);
            WindManager.Instance.SetInCavern(false);
        }
    }
}
