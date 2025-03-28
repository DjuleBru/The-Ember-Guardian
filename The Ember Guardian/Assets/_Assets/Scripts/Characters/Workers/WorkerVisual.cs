using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorkerVisual : MobVisual {

    private Worker worker;
    private WorkerAI workerAI;
    private HunterJob hunterJob;
    private GuardJob guardJob;
    private MinerJob minerJob;
    private JoblessJob joblessJob;

    [SerializeField] private WorkerInteractionCollider interactionCollider;

    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Light2D bodySpriteLight;
    [SerializeField] private SpriteRenderer workerBodySpriteRenderer;
    [SerializeField] private SpriteRenderer workerWeaponSpriteRenderer;
    [SerializeField] private SpriteRenderer workerWeaponGlowSpriteRenderer;
    [SerializeField] private SpriteRenderer workerStatusSpriteRenderer;
    [SerializeField] private Animator workerStatusAnimator;
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite exclamationMarkSprite;
    [SerializeField] private Sprite hoveredSprite;

    private float unHoveredBodySpriteLightIntensity = .9f;
    private float hoveredBodySpriteLightIntensity = 1.1f;

    private bool workerBlockedByCreatures;
    private bool hunterFoundAnimal;

    protected override void Awake() {
        base.Awake();
        worker = GetComponentInParent<Worker>();
        workerAI = GetComponentInParent<WorkerAI>();
        hunterJob = GetComponentInParent<HunterJob>();
        guardJob = GetComponentInParent<GuardJob>();
        minerJob = GetComponentInParent<MinerJob>();
        joblessJob = GetComponentInParent<JoblessJob>();
        workerStatusSpriteRenderer.sprite = null;

        worker.OnMobDied += Worker_OnMobDied;
        worker.OnWorkerHovered += Worker_OnWorkerHovered;
        worker.OnWorkerUnhovered += Worker_OnWorkerUnhovered;
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;

        hunterJob.OnHunterFindsNoAnimal += HunterJob_OnHunterFindsNoAnimal;
        hunterJob.OnHunterChangedState += HunterJob_OnHunterChangedState;
        hunterJob.OnHunterFoundAnimal += HunterJob_OnHunterFoundAnimal;
        minerJob.OnMinerChangedState += MinerJob_OnMinerChangedState;

        joblessJob.OnJoblessBlockedByCreatures += JoblessJob_OnJoblessBlockedByCreatures;
        joblessJob.OnJoblessNotBlockedByCreatures += JoblessJob_OnJoblessNotBlockedByCreatures;


        workerWeaponSpriteRenderer.sortingOrder = currentMaxSortingOrder+1;
        workerWeaponGlowSpriteRenderer.sortingOrder = currentMaxSortingOrder+2;
    }

    private void Start() {     
        WorkerManager.Instance.OnClosestWorkerChanged += WorkerManager_OnClosestWorkerChanged;
    }

    private void WorkerManager_OnClosestWorkerChanged(object sender, WorkerManager.OnClosestWorkerChangedEventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;

        if (e.newClosestWorker == worker) {
            ChangeStatusSprite(hoveredSprite);
            bodySpriteLight.intensity = hoveredBodySpriteLightIntensity;
        } else {
            bodySpriteLight.intensity = unHoveredBodySpriteLightIntensity;
            workerBodySpriteRenderer.material = emptyMaterial;
            workerStatusSpriteRenderer.sprite = null;
        }
    }

    private void Worker_OnWorkerUnhovered(object sender, System.EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;

        workerBodySpriteRenderer.material = emptyMaterial;
        workerStatusSpriteRenderer.sprite = null;
        bodySpriteLight.intensity = unHoveredBodySpriteLightIntensity;
    }

    private void Worker_OnWorkerHovered(object sender, System.EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;

        ChangeStatusSprite(hoveredSprite);
        bodySpriteLight.intensity = hoveredBodySpriteLightIntensity;
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, System.EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;

        bodySpriteLight.intensity = unHoveredBodySpriteLightIntensity;
        workerBodySpriteRenderer.material = emptyMaterial;
        workerStatusSpriteRenderer.sprite = null;
    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = null;
    }

    private void JoblessJob_OnJoblessNotBlockedByCreatures(object sender, System.EventArgs e) {
        workerStatusSpriteRenderer.sprite = null;
    }

    private void JoblessJob_OnJoblessBlockedByCreatures(object sender, System.EventArgs e) {
        ChangeStatusSprite(exclamationMarkSprite);
    }

    private void MinerJob_OnMinerChangedState(object sender, System.EventArgs e) {
        MinerJob.MinerState state = minerJob.GetState();

        if (state == MinerJob.MinerState.blockedByCreatures) {
            if (!workerBlockedByCreatures) {
                workerBlockedByCreatures = true;
                ChangeStatusSprite(exclamationMarkSprite);
            }
        }
        else {
            if(workerBlockedByCreatures) {
                workerStatusSpriteRenderer.sprite = null;
                workerBlockedByCreatures = false;
            }
        }
    }


    private void HunterJob_OnHunterChangedState(object sender, System.EventArgs e) {
        HunterJob.HunterState state = hunterJob.GetState();

        if(state == HunterJob.HunterState.blockedByCreatures) {
            if(!workerBlockedByCreatures) {
                workerBlockedByCreatures = true;
                ChangeStatusSprite(exclamationMarkSprite);
            }
        } else {
            if (workerBlockedByCreatures) {
                workerStatusSpriteRenderer.sprite = null;
                workerBlockedByCreatures = false;
            }
        }
        if(state == HunterJob.HunterState.headingToGuard) {
            hunterFoundAnimal = true;
            workerStatusSpriteRenderer.sprite = null;
        }
    }

    private void HunterJob_OnHunterFindsNoAnimal(object sender, System.EventArgs e) {
        if(hunterFoundAnimal) {
            ChangeStatusSprite(questionMarkSprite);
            hunterFoundAnimal = false;
        }
    }

    private void HunterJob_OnHunterFoundAnimal(object sender, System.EventArgs e) {
        if(!hunterFoundAnimal) {
            hunterFoundAnimal = true;
            workerStatusSpriteRenderer.sprite = null;
        }
    }


    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        if(workerAI.GetJob() != WorkerAI.JobTypes.wild) {
            workerBodySpriteRenderer.material = emptyMaterial;
        }
    }

    private void ChangeStatusSprite(Sprite sprite) {
        workerStatusSpriteRenderer.sprite = sprite;
        workerStatusAnimator.SetTrigger("Changed");
    }

    private void OnDestroy() {
        WorkerManager.Instance.OnClosestWorkerChanged -= WorkerManager_OnClosestWorkerChanged;
    }
}
