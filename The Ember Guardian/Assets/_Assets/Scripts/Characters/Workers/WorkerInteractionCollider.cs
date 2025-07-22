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
        //interactionWithWorkersUnlocked = WorkerStats.Instance.GetInteractionWithWorkersUnlocked();
        interactionWithWorkersUnlocked = false;

        WorkerStats.Instance.OnInteractionsWithWorkersUnlocked += WorkerStats_OnInteractionsWithWorkersUnlocked;

        workerAI = GetComponentInParent<WorkerAI>();
        worker = GetComponentInParent<Worker>();
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;

        if (!interactionWithWorkersUnlocked) return;
        GameInput.Instance.OnCommandWorkerPerformed += GameInput_OnCommandWorkerPerformed;
    }

    private void GameInput_OnCommandWorkerPerformed(object sender, EventArgs e) {
        bool isClosestInteractableWorker = true;

        if (!workerCanBeOrdered) isClosestInteractableWorker = false;
        
        if (WorkerFollowPlayerHandler.Instance.GetMaxFollowedWorkersReached()) isClosestInteractableWorker = false;
        if (workerAI.GetFollowingPlayer()) isClosestInteractableWorker = false;
        if (WorkerFollowPlayerHandler.Instance.GetHoveringWorkers()) isClosestInteractableWorker = false;
        if (WorkerManager.Instance.GetClosestWorkerInPlayerInteractionArea() == null || WorkerManager.Instance.GetClosestWorkerInPlayerInteractionArea() != worker) isClosestInteractableWorker = false;

        if(isClosestInteractableWorker) {
            WorkerManager.Instance.RemoveWorkerFromPlayerInteractionArea(worker);
            workerAI.SetFollowingPlayer(true, true);
        } else {
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
            WorkerManager.Instance.RemoveWorkerFromPlayerInteractionArea(worker);
        }

    }

    private void WorkerAI_OnJobChanged(object sender, EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        StartCoroutine(SetWorkerCanBeOrderedAfterDelay(1f));
    }

    private void WorkerStats_OnInteractionsWithWorkersUnlocked(object sender, EventArgs e) {
        interactionWithWorkersUnlocked = true;
        GameInput.Instance.OnCommandWorkerPerformed += GameInput_OnCommandWorkerPerformed;
    }

    private IEnumerator SetWorkerCanBeOrderedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        workerCanBeOrdered = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);

        if (!interactionWithWorkersUnlocked) return;
        if (WorkerFollowPlayerHandler.Instance.GetMaxFollowedWorkersReached()) return;

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

        WorkerManager.Instance.RemoveWorkerFromPlayerInteractionArea(worker);
    }

    private void OnDestroy() {
        GameInput.Instance.OnCommandWorkerPerformed -= GameInput_OnCommandWorkerPerformed;
    }
}
