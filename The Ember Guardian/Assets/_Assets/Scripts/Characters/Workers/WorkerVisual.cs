using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerVisual : MobVisual {

    private WorkerAI workerAI;

    [SerializeField] private Material emptyMaterial;
    [SerializeField] private SpriteRenderer workerBodySpriteRenderer;

    protected override void Awake() {
        base.Awake();
        workerAI = GetComponentInParent<WorkerAI>();
    }

    private void Start() {
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {
        if(workerAI.GetJob() != WorkerAI.JobTypes.wild) {
            workerBodySpriteRenderer.material = emptyMaterial;
        }
    }
}
