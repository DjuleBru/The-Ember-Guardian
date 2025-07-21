using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorkerVisual : MobVisual {

    private Worker worker;
    private WorkerAI workerAI;
    private HunterJob hunterJob;
    private GuardJob guardJob;
    private MinerJob minerJob;
    private EngineerJob engineerJob;
    private JoblessJob joblessJob;
    private WorkerMovement workerMovement;

    [SerializeField] private WorkerInteractionCollider interactionCollider;

    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material blackAndWhiteMaterial;
    [SerializeField] private Material weaponGlowMaterial;
    [SerializeField] private Light2D bodySpriteLight;
    [SerializeField] private SpriteRenderer workerBodySpriteRenderer;
    [SerializeField] private SpriteRenderer workerWeaponSpriteRenderer;
    [SerializeField] private SpriteRenderer workerWeaponGlowSpriteRenderer;
    [SerializeField] private SpriteRenderer workerStatusSpriteRenderer;
    [SerializeField] private Animator workerStatusAnimator;
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite exclamationMarkSprite;
    [SerializeField] private Sprite hoveredSprite;
    [SerializeField] private GameObject holdingCurrencyGO;
    [SerializeField] private ParticleSystem engineerWrenchPS;

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
        engineerJob = GetComponentInParent<EngineerJob>();
        workerMovement = GetComponentInParent<WorkerMovement>();
        ChangeStatusSprite(null);

        worker.OnMobDied += Worker_OnMobDied;
        worker.OnWorkerHovered += Worker_OnWorkerHovered;
        worker.OnWorkerUnhovered += Worker_OnWorkerUnhovered;
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;
        worker.OnWorkerCollectedCurrency += Worker_OnWorkerCollectedCurrency;
        worker.OnWorkerDroppedCurrency += Worker_OnWorkerDroppedCurrency;
        worker.OnWorkerDroppedAllCurrencies += Worker_OnWorkerDroppedAllCurrencied;
        workerMovement.OnTPEnded += WorkerMovement_OnTPEnded;
        workerMovement.OnTPStarted += WorkerMovement_OnTPStarted;

        engineerJob.OnEngineerChangedState += EngineerJob_OnEngineerChangedState;
        engineerJob.OnEngineerHideTool += EngineerJob_OnEngineerHideTool;
        engineerJob.OnEngineerHideVisual += EngineerJob_OnEngineerHideVisual;
        engineerJob.OnEngineerTurnsWrench += EngineerJob_OnEngineerTurnsWrench;

        hunterJob.OnHunterFindsNoAnimal += HunterJob_OnHunterFindsNoAnimal;
        hunterJob.OnHunterChangedState += HunterJob_OnHunterChangedState;
        hunterJob.OnHunterFoundAnimal += HunterJob_OnHunterFoundAnimal;
        minerJob.OnMinerChangedState += MinerJob_OnMinerChangedState;

        joblessJob.OnJoblessBlockedByCreatures += JoblessJob_OnJoblessBlockedByCreatures;
        joblessJob.OnJoblessNotBlockedByCreatures += JoblessJob_OnJoblessNotBlockedByCreatures;

        interactionCollider.OnPlayerTriggeredOut += InteractionCollider_OnPlayerTriggeredOut;
        interactionCollider.OnPlayerTriggeredIn += InteractionCollider_OnPlayerTriggeredIn;

        workerWeaponSpriteRenderer.sortingOrder = currentMaxSortingOrder+1;
        workerWeaponGlowSpriteRenderer.sortingOrder = currentMaxSortingOrder+2;
        holdingCurrencyGO.SetActive(false);

        workerBodySpriteRenderer.material = blackAndWhiteMaterial;
        workerWeaponSpriteRenderer.material = blackAndWhiteMaterial;
        workerWeaponGlowSpriteRenderer.material = blackAndWhiteMaterial;
    }


    private void EngineerJob_OnEngineerTurnsWrench(object sender, System.EventArgs e) {
        StartCoroutine(TriggerWrenchPSAfterDelay());
    }

    private IEnumerator TriggerWrenchPSAfterDelay() {
        yield return new WaitForSeconds(.2f);
        engineerWrenchPS.Play();
    }

    private void EngineerJob_OnEngineerHideVisual(object sender, System.EventArgs e) {
        ShowBodyVisual(false);
    }
    private void WorkerMovement_OnTPStarted(object sender, System.EventArgs e) {
        ShowBodyVisual(false);
    }

    private void WorkerMovement_OnTPEnded(object sender, System.EventArgs e) {
        ShowBodyVisual(true);
    }

    private void EngineerJob_OnEngineerHideTool(object sender, System.EventArgs e) {
        ShowWeapon(false);
    }

    private void EngineerJob_OnEngineerChangedState(object sender, System.EventArgs e) {
        RefreshStatusAndWeaponSprite();
    }

    private void ShowAllVisuals(bool show) {
        ShowWeapon(show);
        ShowBodyVisual(show);
        workerStatusSpriteRenderer.enabled = show;
        holdingCurrencyGO.GetComponent<SpriteRenderer>().enabled = show;
    }

    private void ShowWeapon(bool show) {
        workerWeaponSpriteRenderer.enabled = show;
        workerWeaponGlowSpriteRenderer.enabled = show;
    }
    private void ShowBodyVisual(bool show) {
        ShowWeapon(show);
        workerBodySpriteRenderer.enabled = show;
    }

    private void Worker_OnWorkerDroppedCurrency(object sender, System.EventArgs e) {
        if(worker.GetTotalCurrencyAmount() == 0) {
            holdingCurrencyGO.SetActive(false);
        }
    }

    private void Worker_OnWorkerDroppedAllCurrencied(object sender, System.EventArgs e) {
        holdingCurrencyGO.SetActive(false);
    }

    private void Worker_OnWorkerCollectedCurrency(object sender, System.EventArgs e) {
        holdingCurrencyGO.SetActive(true);
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
            ChangeStatusSprite(null);
        }
    }

    private void Worker_OnWorkerUnhovered(object sender, System.EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;

        workerBodySpriteRenderer.material = emptyMaterial;
        bodySpriteLight.intensity = unHoveredBodySpriteLightIntensity;
        RefreshStatusAndWeaponSprite();
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

        if (workerAI.GetFollowingPlayer()) {
            ChangeStatusSprite(null);
        }
        else {
            RefreshStatusAndWeaponSprite();
        }
    }

    private void Worker_OnMobDied(object sender, System.EventArgs e) {
        ChangeStatusSprite(null);
    }

    private void JoblessJob_OnJoblessNotBlockedByCreatures(object sender, System.EventArgs e) {
        ChangeStatusSprite(null);
    }

    private void JoblessJob_OnJoblessBlockedByCreatures(object sender, System.EventArgs e) {
        ChangeStatusSprite(exclamationMarkSprite);
    }

    private void MinerJob_OnMinerChangedState(object sender, System.EventArgs e) {
        RefreshStatusAndWeaponSprite();
    }

    private void HunterJob_OnHunterChangedState(object sender, System.EventArgs e) {
        RefreshStatusAndWeaponSprite();
        
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
            ChangeStatusSprite(null);
        }
    }

    private void InteractionCollider_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!worker.GetRecruited()) return;

        if (workerBlockedByCreatures) {
            ChangeStatusSprite(null);
        }

        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            if (!hunterFoundAnimal) {
                ChangeStatusSprite(null);
            }
        }
       
    }

    private void InteractionCollider_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (!worker.GetRecruited()) return;
        if (workerAI.GetFollowingPlayer()) return;

        if (workerBlockedByCreatures) {
            ChangeStatusSprite(exclamationMarkSprite);
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            if (!hunterFoundAnimal) {
                ChangeStatusSprite(questionMarkSprite);
            }
        };
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        if(workerAI.GetJob() != WorkerAI.JobTypes.wild) {
            workerBodySpriteRenderer.material = emptyMaterial;
            workerWeaponSpriteRenderer.material = emptyMaterial;
            workerWeaponGlowSpriteRenderer.material = weaponGlowMaterial;
        }
    }

    private void RefreshStatusAndWeaponSprite() {
        WorkerAI.JobTypes job = workerAI.GetJob();
        ChangeStatusSprite(null);

        if (workerAI.GetFollowingPlayer()) return;

        switch (job) {
            case WorkerAI.JobTypes.hunter:
                HunterJob.HunterState state = hunterJob.GetState();

                if (state == HunterJob.HunterState.blockedByCreatures) {
                    workerBlockedByCreatures = true;
                    ChangeStatusSprite(exclamationMarkSprite);
                    return;
                    
                } else {
                    workerBlockedByCreatures = false;
                }

                if (state == HunterJob.HunterState.headingToGuard) {
                    hunterFoundAnimal = true;
                    ChangeStatusSprite(null);
                    return;
                }

                if(!hunterFoundAnimal) {
                    ChangeStatusSprite(questionMarkSprite);
                }

                break;
            case WorkerAI.JobTypes.miner:
                MinerJob.MinerState minerState = minerJob.GetState();

                if (minerState == MinerJob.MinerState.blockedByCreatures) {

                    workerBlockedByCreatures = true;
                    ChangeStatusSprite(exclamationMarkSprite);
                }
                else {
                    workerBlockedByCreatures = false;
                }

                if (minerState == MinerJob.MinerState.Mining) {
                    if (minerJob.GetScavengableAssigned().GetIsMine()) {
                        ShowAllVisuals(false);
                    }
                }
                else {
                    ShowAllVisuals(true);
                }
                break;

            case WorkerAI.JobTypes.engineer:
                EngineerJob.EngineerState engineerState = engineerJob.GetState();
                if (engineerState == EngineerJob.EngineerState.blockedByCreatures) {
                    workerBlockedByCreatures = true;
                    ChangeStatusSprite(exclamationMarkSprite);
                }
                else {
                    workerBlockedByCreatures = false;
                }

                if (engineerState == EngineerJob.EngineerState.droppingCurrency) return;

                if (engineerState == EngineerJob.EngineerState.idle) {
                    ShowWeapon(true);
                    ShowBodyVisual(true);
                }

                break;

            case WorkerAI.JobTypes.guard:
                break;

        }
        
    }

    private void ChangeStatusSprite(Sprite sprite) {
        //Debug.Log("ChangeStatusSprite " + sprite);
        workerStatusSpriteRenderer.sprite = sprite;
        workerStatusAnimator.SetTrigger("Changed");
    }

    private void OnDestroy() {
        WorkerManager.Instance.OnClosestWorkerChanged -= WorkerManager_OnClosestWorkerChanged;
    }
}
