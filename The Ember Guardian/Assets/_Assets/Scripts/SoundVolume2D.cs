using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVolume2D : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private float maxDistanceToHear = 20f;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        float distanceToAudioSource = Mathf.Abs(Player.Instance.transform.position.x - transform.position.x);
        float volume = 1 - (distanceToAudioSource / maxDistanceToHear);

        if(volume < 0) volume = 0;

        audioSource.volume = volume;
    }

    public void SetMaxDistanceToHear(float distance) {
        Debug.Log(distance);
        maxDistanceToHear = distance;
    }

}
