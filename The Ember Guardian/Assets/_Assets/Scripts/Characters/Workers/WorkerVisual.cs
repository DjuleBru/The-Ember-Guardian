using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerVisual : MobVisual {

    private Worker worker;

    [SerializeField] private Material emptyMaterial;
    [SerializeField] private SpriteRenderer workerBodySpriteRenderer;

    protected override void Awake() {
        base.Awake();
        worker = GetComponentInParent<Worker>();
    }

    private void Start() {
        worker.OnJobChanged += Worker_OnJobChanged;
    }

    private void Worker_OnJobChanged(object sender, System.EventArgs e) {
        if(worker.GetJob() != Worker.JobTypes.wild) {
            workerBodySpriteRenderer.material = emptyMaterial;
        }
    }
}
