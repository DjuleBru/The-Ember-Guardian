using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CollectibleVisual : MonoBehaviour
{
    private Collectible collectible;
    private SpriteRenderer collectibleSpriteRenderer;
    private Animator collectibleAnimator;
    [SerializeField] private bool collectibleHasAnimation;
    [SerializeField] private Light2D collectibleLight;
    [SerializeField] private MMF_Player enterSlotFeedbacks;
    [SerializeField] private GameObject obstructorGameObject;

    private void Awake() {
        collectible = GetComponentInParent<Collectible>();
        collectibleSpriteRenderer = GetComponent<SpriteRenderer>();
        collectibleAnimator = GetComponent<Animator>();

        obstructorGameObject.SetActive(false);
        if (collectibleHasAnimation) {
            collectibleAnimator.SetTrigger("Animate");
        }
    }

    private void Start() {
        collectible.OnCollectibleEnteredSlot += Collectible_OnCollectibleEnteredSlot;
        collectible.OnCollectibleFellFromBag += Collectible_OnCollectibleFellFromBag;
        collectible.OnCollectiblePlouffed += Collectible_OnCollectiblePlouffed;

    }

    private void Collectible_OnCollectiblePlouffed(object sender, System.EventArgs e) {
        StartCoroutine(CollectibleFellInWater());
    }

    private void Collectible_OnCollectibleFellFromBag(object sender, System.EventArgs e) {
        StartCoroutine(CollectibleFellFromBag());
    }

    private IEnumerator CollectibleFellFromBag() {
        collectibleSpriteRenderer.sortingLayerName = "ForeWater";
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator CollectibleFellInWater() {
        if(collectibleLight != null) {
            collectibleLight.enabled = false;
        }
        obstructorGameObject.SetActive(true);
        collectibleAnimator.SetTrigger("Plouf");

        yield return new WaitForSeconds(.05f);
        obstructorGameObject.SetActive(false);

    }

    private void Collectible_OnCollectibleEnteredSlot(object sender, System.EventArgs e) {
        enterSlotFeedbacks.PlayFeedbacks();
    }
}
