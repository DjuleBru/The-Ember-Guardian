using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInteractionCollider : MonoBehaviour
{
    private Worker worker;
    private WorkerAI workerAI;
    private bool workerCanBeOrdered;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    private bool interactionWithWorkersUnlocked;

    private void Start() {
        interactionWithWorkersUnlocked = MetaProgressionManager.Instance.GetInteractionWithWorkersUnlocked();
        if (!interactionWithWorkersUnlocked) return;

        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;

        workerAI = GetComponentInParent<WorkerAI>();
        worker = GetComponentInParent<Worker>();
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }

    private void WorkerAI_OnJobChanged(object sender, EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        StartCoroutine(SetWorkerCanBeOrderedAfterDelay(1f));
    }

    private IEnumerator SetWorkerCanBeOrderedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        workerCanBeOrdered = true;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
        if (!workerCanBeOrdered) return;
        if (workerAI.GetFollowingPlayer()) return;
        if (WorkerFollowPlayerHandler.Instance.GetHoveringWorkers()) return;
        if (WorkerManager.Instance.GetClosestWorkerInPlayerInteractionArea() == null || WorkerManager.Instance.GetClosestWorkerInPlayerInteractionArea() != worker) return;

        WorkerManager.Instance.RemoveWorkerFromPlayerInteractionArea(worker, false);
        workerAI.SetFollowingPlayer(true, true);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);

        if (!interactionWithWorkersUnlocked) return;

        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        if (workerAI.GetFollowingPlayer()) return;
        if (WorkerFollowPlayerHandler.Instance.GetHoveringWorkers()) return;
        if (!workerCanBeOrdered) return;

        WorkerManager.Instance.AddWorkerToPlayerInteractionArea(worker);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);

        if (!interactionWithWorkersUnlocked) return;

        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        if (workerAI.GetFollowingPlayer()) return;
        if (!workerCanBeOrdered) return;

        WorkerManager.Instance.RemoveWorkerFromPlayerInteractionArea(worker, true);
    }
}
