using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleVisual : MonoBehaviour
{
    private Collectible collectible;
    [SerializeField] private MMF_Player enterSlotFeedbacks;

    private void Awake() {
        collectible = GetComponentInParent<Collectible>();
    }

    private void Start() {
        collectible.OnCollectibleEnteredSlot += Collectible_OnCollectibleEnteredSlot;
    }

    private void Collectible_OnCollectibleEnteredSlot(object sender, System.EventArgs e) {
        enterSlotFeedbacks.PlayFeedbacks();
    }
}
