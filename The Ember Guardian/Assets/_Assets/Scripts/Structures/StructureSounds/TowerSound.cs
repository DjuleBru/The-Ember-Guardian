using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSound : MonoBehaviour
{
    [SerializeField] private Tower tower;
    [SerializeField] private AudioClip hunterAssignAudioClip;
    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        tower.OnHunterAssigned += Tower_OnHunterAssigned;
    }

    private void Tower_OnHunterAssigned(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(hunterAssignAudioClip);
    }
}
