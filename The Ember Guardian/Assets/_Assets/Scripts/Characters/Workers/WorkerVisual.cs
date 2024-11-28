using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerVisual : MobVisual {

    private WorkerAI workerAI;
    private HunterJob hunterJob;

    [SerializeField] private Material emptyMaterial;
    [SerializeField] private SpriteRenderer workerBodySpriteRenderer;
    [SerializeField] private SpriteRenderer workerStatusSpriteRenderer;
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite exclamationMarkSprite;

    protected override void Awake() {
        base.Awake();
        workerAI = GetComponentInParent<WorkerAI>();
        hunterJob = GetComponentInParent<HunterJob>();
        workerStatusSpriteRenderer.sprite = null;
    }

    private void Start() {
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
        hunterJob.OnHunterFindsNoAnimal += HunterJob_OnHunterFindsNoAnimal;
        hunterJob.OnHunterChangedState += HunterJob_OnHunterChangedState;
        hunterJob.OnHunterFoundAnimal += HunterJob_OnHunterFoundAnimal;
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
        Debug.Log("HunterJob_OnHunterFindsNoAnimal");
        workerStatusSpriteRenderer.sprite = questionMarkSprite;
    }

    private void HunterJob_OnHunterFoundAnimal(object sender, System.EventArgs e) {
        Debug.Log("HunterJob_OnHunterFoundAnimal");
        workerStatusSpriteRenderer.sprite = null;
    }


    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        if(workerAI.GetJob() != WorkerAI.JobTypes.wild) {
            workerBodySpriteRenderer.material = emptyMaterial;
        }
    }
}
