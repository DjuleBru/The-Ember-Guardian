using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScavengableVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer glowSpriteRenderer;
    [SerializeField] private Transform minerAssignedContainer;
    [SerializeField] private Transform minerAssignedTemplate;
    [SerializeField] private Sprite depletedSprite;
    [SerializeField] private GameObject effortRequiredGO;
    [SerializeField] private Image effortRequiredImage;
    [SerializeField] private Gradient effortRequiredGradient;
    [SerializeField] private GameObject miningStatusGO;
    [SerializeField] private TextMeshProUGUI cancelText;
    [SerializeField] private Image miningStatusImage;
    [SerializeField] private Sprite miningImage;
    [SerializeField] private Sprite notMiningImage;
    [SerializeField] private Color miningColor;
    [SerializeField] private Color notMiningColor;

    [SerializeField] private Material unhoveredMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Animator scavengableUIAnimator;
    [SerializeField] private GameObject orbCostUIGO;

    private IScavengable scavengable;

    private void Awake() {
        scavengable = GetComponentInParent<IScavengable>();
    }

    private void Start() {
        scavengable.OnPlayerTriggerIn += Scavengable_OnPlayerTriggerIn;
        scavengable.OnPlayerTriggerOut += Scavengable_OnPlayerTriggerOut;
        scavengable.OnScavengableDepleted += Scavengable_OnScavengableDepleted;
        scavengable.OnScavengableMarkedToScavenge += Scavengable_OnScavengableMarkedToScavenge;
        scavengable.OnMinerStartsMining += Scavengable_OnMinerStartsMining;
        scavengable.OnMinerStopsMining += Scavengable_OnMinerStopsMining;
        scavengable.OnMinerExtractedResourceFromMine += Scavengable_OnMinerExtractedResourceFromMine;
        scavengable.OnActivatedMining += Scavengable_OnActivatedMining;
        scavengable.OnDeactivatedMining += Scavengable_OnDeactivatedMining;

        for (int i = 0; i < scavengable.GetMaxMinerAmount(); i++) {
            Transform template = Instantiate(minerAssignedTemplate, minerAssignedContainer);
        }
        minerAssignedContainer.gameObject.SetActive(false);

        if (effortRequiredGO != null) {
            effortRequiredGO.SetActive(false);
        }
        if (miningStatusGO != null) {
            miningStatusGO.SetActive(false);
        }
    }

    private void Scavengable_OnDeactivatedMining(object sender, System.EventArgs e) {
        miningStatusImage.sprite = notMiningImage;
        miningStatusImage.color = notMiningColor;

        if(glowSpriteRenderer != null) {
            glowSpriteRenderer.material.SetFloat("_Glow", 0f);
        }
    }

    private void Scavengable_OnActivatedMining(object sender, System.EventArgs e) {
        miningStatusImage.sprite = miningImage;
        miningStatusImage.color = miningColor;

        if (glowSpriteRenderer != null) {
            glowSpriteRenderer.material.SetFloat("_Glow", 2f);
        }
    }

    private void Scavengable_OnMinerExtractedResourceFromMine(object sender, System.EventArgs e) {
        RefreshEffortRequired();
    }

    private void Scavengable_OnMinerStopsMining(object sender, System.EventArgs e) {
        RefreshMinersInMine();
    }

    private void Scavengable_OnMinerStartsMining(object sender, System.EventArgs e) {
        RefreshMinersInMine();
    }

    private void RefreshMinersInMine() {
        foreach(Transform child in minerAssignedContainer) {
            if (child == minerAssignedTemplate) continue;
            Destroy(child.gameObject);
        }

        minerAssignedTemplate.gameObject.SetActive(true);
        int minersMining = scavengable.GetMinerAmountMining();
        for (int i  = 0; i < scavengable.GetMaxMinerAmount(); i++) {
            Transform template = Instantiate(minerAssignedTemplate, minerAssignedContainer);
            if(i < minersMining) {
                template.Find("MinersMiningTemplate").gameObject.SetActive(true);
            }
        }
        minerAssignedTemplate.gameObject.SetActive(false);
    }

    private void RefreshEffortRequired() {
        float normalized = scavengable.GetTimeToExtractOneResourceNormalized();
        effortRequiredImage.fillAmount = normalized;
        effortRequiredImage.color = effortRequiredGradient.Evaluate(normalized);
    }

    private void Scavengable_OnScavengableMarkedToScavenge(object sender, System.EventArgs e) {
        orbCostUIGO.gameObject.SetActive(false);
        minerAssignedContainer.gameObject.SetActive(true);

        if (effortRequiredGO != null) {
            effortRequiredGO.SetActive(true);
        }
        if (miningStatusGO != null) {
            miningStatusGO.SetActive(true);
            cancelText.text = LocalizationManager.Instance.GetLocalizedText("mine_active");
            LayoutRebuilder.ForceRebuildLayoutImmediate(cancelText.GetComponent<RectTransform>());
        }
    }

    private void Scavengable_OnScavengableDepleted(object sender, System.EventArgs e) {

        if(depletedSprite != null) {
            bodySpriteRenderer.sprite = depletedSprite;
        }

        scavengableUIAnimator.gameObject.SetActive(false);
    }

    private void Scavengable_OnPlayerTriggerOut(object sender, System.EventArgs e) {
        scavengableUIAnimator.SetTrigger("Hide");

        if (scavengable.GetMarkedToScavenge()) return;

        if(bodySpriteRenderer != null) {
            bodySpriteRenderer.material = unhoveredMaterial;
        }
    }

    private void Scavengable_OnPlayerTriggerIn(object sender, System.EventArgs e) {
        scavengableUIAnimator.SetTrigger("Show");

        if (scavengable.GetMarkedToScavenge()) return;

        if (bodySpriteRenderer != null) {
            bodySpriteRenderer.material = hoveredMaterial;
        }
    }
}
