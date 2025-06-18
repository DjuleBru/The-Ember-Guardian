using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScavengableVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private Transform minerAssignedContainer;
    [SerializeField] private Transform minerAssignedTemplate;
    [SerializeField] private Transform minerAssignedContainerBackground;
    [SerializeField] private Transform minerAssignedTemplateBackground;
    [SerializeField] private Sprite depletedSprite;
    [SerializeField] private GameObject effortRequiredGO;
    [SerializeField] private Image effortRequiredImage;
    [SerializeField] private Gradient effortRequiredGradient;
    [SerializeField] private GameObject cancelGO;
    [SerializeField] private TextMeshProUGUI cancelText;
    [SerializeField] private Image miningStatusImage;
    [SerializeField] private Sprite miningImage;
    [SerializeField] private Sprite notMiningImage;

    [SerializeField] private Material unhoveredMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Animator scavengableBodyAnimator;
    [SerializeField] private Animator scavengableUIAnimator;
    [SerializeField] private GameObject orbCostUIGO;

    private Scavengable scavengable;

    private void Awake() {
        scavengable = GetComponentInParent<Scavengable>();
        minerAssignedTemplate.gameObject.SetActive(false);
        minerAssignedContainer.gameObject.SetActive(false);
        minerAssignedContainerBackground.gameObject.SetActive(false);

        if(effortRequiredGO != null) {
            effortRequiredGO.SetActive(false);
        }
        if (cancelGO != null) {
            cancelGO.SetActive(false);
        }
    }

    private void Start() {
        scavengable.OnDamageTaken += Scavengable_OnDamageTaken;
        scavengable.OnPlayerTriggerIn += Scavengable_OnPlayerTriggerIn;
        scavengable.OnPlayerTriggerOut += Scavengable_OnPlayerTriggerOut;
        scavengable.OnScavengableDepleted += Scavengable_OnScavengableDepleted;
        scavengable.OnScavengableMarkedToScavenge += Scavengable_OnScavengableMarkedToScavenge;
        scavengable.OnMinerStartsMining += Scavengable_OnMinerEntersMine;
        scavengable.OnMinerStopsMining += Scavengable_OnMinerExitsMine;
        scavengable.OnMinerExtractedResourceFromMine += Scavengable_OnMinerExtractedResourceFromMine;

        for (int i = 0; i < scavengable.GetMaxMinerAmount(); i++) {
            Instantiate(minerAssignedTemplateBackground, minerAssignedContainerBackground);
        }
        minerAssignedTemplateBackground.gameObject.SetActive(false);
    }

    private void Scavengable_OnMinerExtractedResourceFromMine(object sender, System.EventArgs e) {
        RefreshEffortRequired();
    }

    private void Scavengable_OnMinerExitsMine(object sender, System.EventArgs e) {
        RefreshMinersInMine();
    }

    private void Scavengable_OnMinerEntersMine(object sender, System.EventArgs e) {
        RefreshMinersInMine();
    }

    private void RefreshMinersInMine() {
        foreach(Transform child in minerAssignedContainer) {
            if (child == minerAssignedTemplate) continue;
            Destroy(child.gameObject);
        }

        minerAssignedTemplate.gameObject.SetActive(true);
        for (int i  = 0; i < scavengable.GetMinerAmountMining(); i++) {
            Instantiate(minerAssignedTemplate, minerAssignedContainer);
        }
        minerAssignedTemplate.gameObject.SetActive(false);

        if(scavengable.GetMinerAmountMining() > 0) {
            cancelGO.SetActive(true);
        } else {
            cancelGO.SetActive(false);
        }
    }

    private void RefreshEffortRequired() {
        float normalized = scavengable.GetTimeToExtractOneResourceNormalized();
        effortRequiredImage.fillAmount = normalized;
        effortRequiredImage.color = effortRequiredGradient.Evaluate(normalized);
    }

    private void Scavengable_OnDamageTaken(object sender, System.EventArgs e) {
        //scavengableBodyAnimator.SetTrigger("Hit");
    }

    private void Scavengable_OnScavengableMarkedToScavenge(object sender, System.EventArgs e) {
        orbCostUIGO.gameObject.SetActive(false);
        minerAssignedContainer.gameObject.SetActive(true);
        minerAssignedContainerBackground.gameObject.SetActive(true);

        if (effortRequiredGO != null) {
            effortRequiredGO.SetActive(true);
        }
    }

    private void Scavengable_OnScavengableDepleted(object sender, System.EventArgs e) {
        bodySpriteRenderer.sprite = depletedSprite;
    }

    private void Scavengable_OnPlayerTriggerOut(object sender, System.EventArgs e) {
        scavengableUIAnimator.SetTrigger("Hide");

        if (scavengable.GetMarkedToScavenge()) return;

        bodySpriteRenderer.material = unhoveredMaterial;
    }

    private void Scavengable_OnPlayerTriggerIn(object sender, System.EventArgs e) {
        scavengableUIAnimator.SetTrigger("Show");

        if (scavengable.GetMarkedToScavenge()) return;

        bodySpriteRenderer.material = hoveredMaterial;
    }
}
