using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerVisual : MobVisual {

    private Worker worker;
    private WorkerAI workerAI;
    private HunterJob hunterJob;
    private GuardJob guardJob;
    private MinerJob minerJob;
    private JoblessJob joblessJob;

    [SerializeField] private WorkerInteractionCollider interactionCollider;

    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private SpriteRenderer workerBodySpriteRenderer;
    [SerializeField] private SpriteRenderer workerStatusSpriteRenderer;
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite exclamationMarkSprite;
    [SerializeField] private Sprite hoveredSprite;

    protected override void Awake() {
        base.Awake();
        worker = GetComponentInParent<Worker>();
        workerAI = GetComponentInParent<WorkerAI>();
        hunterJob = GetComponentInParent<HunterJob>();
        guardJob = GetComponentInParent<GuardJob>();
        minerJob = GetComponentInParent<MinerJob>();
        joblessJob = GetComponentInParent<JoblessJob>();
        workerStatusSpriteRenderer.sprite = null;
    }

    private void Start() {
        worker.OnMobDied += Worker_OnMobDied;
        worker.OnWorkerHovered += Worker_OnWorkerHovered;
        worker.OnWorkerUnhovered += Worker_OnWorkerUnhovered;
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;

        hunterJob.OnHunterFindsNoAnimal += HunterJob_OnHunterFindsNoAnimal;
        hunterJob.OnHunterChangedState += HunterJob_OnHunterChangedState;
        hunterJob.OnHunterFoundAnimal += HunterJob_OnHunterFoundAnimal;

        guardJob.OnGuardChangedState += GuardJob_OnGuardChangedState;

        minerJob.OnMinerChangedState += MinerJob_OnMinerChangedState;

        joblessJob.OnJoblessBlockedByCreatures += JoblessJob_OnJoblessBlockedByCreatures;
        joblessJob.OnJoblessNotBlockedByCreatures += JoblessJob_OnJoblessNotBlockedByCreatures;

        interactionCollider.OnPlayerTriggeredIn += InteractionCollider_OnPlayerTriggeredIn;
        interactionCollider.OnPlayerTriggeredOut += InteractionCollider_OnPlayerTriggeredOut;
    }

    private void Worker_OnWorkerUnhovered(object sender, System.EventArgs e) {
        workerBodySpriteRenderer.material = emptyMaterial;
        workerStatusSpriteRenderer.sprite = null;
    }

    private void Worker_OnWorkerHovered(object sender, System.EventArgs e) {
        workerBodySpriteRenderer.material = hoveredMaterial;
        workerStatusSpriteRenderer.sprite = hoveredSprite;
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, System.EventArgs e) {
        workerBodySpriteRenderer.material = emptyMaterial;
        workerStatusSpriteRenderer.sprite = null;
    }

    private void InteractionCollider_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        workerBodySpriteRenderer.material = emptyMaterial;
        workerStatusSpriteRenderer.sprite = null;
    }

    private void InteractionCollider_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        workerBodySpriteRenderer.material = hoveredMaterial;
        workerStatusSpriteRenderer.sprite = hoveredSprite;
    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = null;
    }

    private void JoblessJob_OnJoblessNotBlockedByCreatures(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = null;
    }

    private void JoblessJob_OnJoblessBlockedByCreatures(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = exclamationMarkSprite;
    }

    private void MinerJob_OnMinerChangedState(object sender, System.EventArgs e) {
        MinerJob.MinerState state = minerJob.GetState();

        if (state == MinerJob.MinerState.blockedByCreatures) {
            workerStatusSpriteRenderer.sprite = exclamationMarkSprite;
        }
        else {
            workerStatusSpriteRenderer.sprite = null;
        }
    }

    private void GuardJob_OnGuardChangedState(object sender, System.EventArgs e) {
        GuardJob.GuardState state = guardJob.GetState();

        if (state == GuardJob.GuardState.blockedByCreatures) {
            workerStatusSpriteRenderer.sprite = exclamationMarkSprite;
        }
        else {
            workerStatusSpriteRenderer.sprite = null;
        }
    }

    private void HunterJob_OnHunterChangedState(object sender, System.EventArgs e) {
        HunterJob.HunterState state = hunterJob.GetState();

        if(state == HunterJob.HunterState.blockedByCreatures) {
            workerStatusSpriteRenderer.sprite = exclamationMarkSprite;
        } else {
            workerStatusSpriteRenderer.sprite = null;
        }
    }

    private void HunterJob_OnHunterFindsNoAnimal(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = questionMarkSprite;
    }

    private void HunterJob_OnHunterFoundAnimal(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = null;
    }


    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        if(workerAI.GetJob() != WorkerAI.JobTypes.wild) {
            workerBodySpriteRenderer.material = emptyMaterial;
        }
    }
}
