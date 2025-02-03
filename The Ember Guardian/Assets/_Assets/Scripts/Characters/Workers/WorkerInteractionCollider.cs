using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInteractionCollider : MonoBehaviour
{
    private WorkerAI workerAI;
    private bool playerInTriggerArea;
    private bool workerCanBeOrdered;
    public static int workerInTriggerAreaAmount;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;

        workerAI = GetComponentInParent<WorkerAI>();
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }

    private void WorkerAI_OnJobChanged(object sender, EventArgs e) {
        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        StartCoroutine(SetWorkerCanBeOrderedAfterDelay(1.5f));
    }

    private IEnumerator SetWorkerCanBeOrderedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        workerCanBeOrdered = true;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
        if (!workerCanBeOrdered) return;
        if (!playerInTriggerArea) return;
        if (workerAI.GetFollowingPlayer()) return;
        if (WorkerFollowPlayerHandler.Instance.GetHoveringWorkers()) return;

        workerInTriggerAreaAmount--;
        workerAI.SetFollowingPlayer(true);
        Player.Instance.SetHoveringWorkerAfterFrame(false);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        if (workerAI.GetFollowingPlayer()) return;
        if (WorkerFollowPlayerHandler.Instance.GetHoveringWorkers()) return;

        workerInTriggerAreaAmount++;

        if (!workerCanBeOrdered) return;
        // Check if player is in any other worker's trigger area
        if (workerInTriggerAreaAmount > 1) return;

        playerInTriggerArea = true;
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        Player.Instance.SetHoveringWorker(true);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        playerInTriggerArea = false;

        if (workerAI.GetJob() == WorkerAI.JobTypes.wild || workerAI.GetJob() == WorkerAI.JobTypes.jobless) return;
        if (workerAI.GetFollowingPlayer()) return;
        if (WorkerFollowPlayerHandler.Instance.GetHoveringWorkers()) return;


        workerInTriggerAreaAmount--;
        if (!workerCanBeOrdered) return;

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        Player.Instance.SetHoveringWorker(false);
    }
}
