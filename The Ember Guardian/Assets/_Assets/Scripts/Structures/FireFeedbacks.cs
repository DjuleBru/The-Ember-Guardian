using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFeedbacks : MonoBehaviour
{
    private Fire fire;
    [SerializeField] private MMF_Player damagedFeedbacks;

    private void Awake() {
        fire = GetComponentInParent<Fire>();
    }

    private void Start() {
        fire.OnFireDamageTaken += Fire_OnFireDamageTaken;
    }

    private void Fire_OnFireDamageTaken(object sender, System.EventArgs e) {
        damagedFeedbacks.PlayFeedbacks();
    }
}
