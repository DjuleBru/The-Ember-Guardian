using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private GameObject markedToScavengeGameObject;
    [SerializeField] private Sprite depletedSprite;

    [SerializeField] private Material unhoveredMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Animator scavengableBodyAnimator;
    [SerializeField] private Animator scavengableUIAnimator;

    private Scavengable scavengable;

    private void Awake() {
        scavengable = GetComponentInParent<Scavengable>();
        markedToScavengeGameObject.SetActive(false);
    }

    private void Start() {
        scavengable.OnDamageTaken += Scavengable_OnDamageTaken;
        scavengable.OnPlayerTriggerIn += Scavengable_OnPlayerTriggerIn;
        scavengable.OnPlayerTriggerOut += Scavengable_OnPlayerTriggerOut;
        scavengable.OnScavengableDepleted += Scavengable_OnScavengableDepleted;
        scavengable.OnScavengableMarkedToScavenge += Scavengable_OnScavengableMarkedToScavenge;
    }

    private void Scavengable_OnDamageTaken(object sender, System.EventArgs e) {
        scavengableBodyAnimator.SetTrigger("Hit");
    }

    private void Scavengable_OnScavengableMarkedToScavenge(object sender, System.EventArgs e) {
        markedToScavengeGameObject.SetActive(true);
        scavengableUIAnimator.gameObject.SetActive(false);
    }

    private void Scavengable_OnScavengableDepleted(object sender, System.EventArgs e) {
        bodySpriteRenderer.sprite = depletedSprite;
        markedToScavengeGameObject.SetActive(false);
    }

    private void Scavengable_OnPlayerTriggerOut(object sender, System.EventArgs e) {
        if (scavengable.GetMarkedToScavenge()) return;

        bodySpriteRenderer.material = unhoveredMaterial;
        scavengableUIAnimator.SetTrigger("Hide");
    }

    private void Scavengable_OnPlayerTriggerIn(object sender, System.EventArgs e) {
        if (scavengable.GetMarkedToScavenge()) return;

        bodySpriteRenderer.material = hoveredMaterial;
        scavengableUIAnimator.SetTrigger("Show");
    }
}
