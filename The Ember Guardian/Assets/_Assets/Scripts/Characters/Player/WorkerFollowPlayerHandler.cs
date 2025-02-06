using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerFollowPlayerHandler : MonoBehaviour
{
    public static WorkerFollowPlayerHandler Instance;

    private List<Worker> workersFollowingPlayer = new List<Worker>();
    private List<Worker> huntersFollowingPlayer = new List<Worker>();
    private List<Worker> guardsFollowingPlayer = new List<Worker>();
    private List<Worker> minersFollowingPlayer = new List<Worker>();

    private int maxFollowingWorkers = 3;
    private float distanceBetweenFollowingWorkers = 1f;
    private float workersFollowDirection;

    private int workerHoverIndex;
    private bool hoveringFollowingWorkers;
    private bool hoveringReverseOrder;

    private bool interactionWithWorkersUnlocked;

    public event EventHandler OnHoveredFollowingWorkerChanged;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        interactionWithWorkersUnlocked = MetaProgressionManager.Instance.GetInteractionWithWorkersUnlocked();
        if (!interactionWithWorkersUnlocked) return;

        Mob.OnAnyMobDied += Worker_OnAnyMobDied;
        GameInput.Instance.OnHoverWorkersPerformed += GameInput_OnHoverWorkersPerformed;
        GameInput.Instance.OnPlayerLeftRightDirPerformed += GameInput_OnPlayerLeftRightDirPerformed;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        List<Worker> workersToRemove = new List<Worker>();

        foreach (Worker worker in workersFollowingPlayer) {
            workersToRemove.Add(worker);
        }
        foreach (Worker worker in workersToRemove) {
            RemoveFollowingWorker(worker, false);
        }
    }

    public Vector3 GetWorkerFollowPosition(Worker worker) {
        // Trouve l'index du worker dans la liste
        int workerIndex = workersFollowingPlayer.IndexOf(worker);

        // Calcule la position en fonction de l'index et de la distance entre les workers
        Vector3 playerPosition = Player.Instance.transform.position;

        workersFollowDirection = PlayerMovement.Instance.GetLastMoveDir();
        //float moveSpeed = PlayerMovement.Instance.GetMoveSpeed();
        //bool isMovingBackwards = PlayerMovement.Instance.IsMovingBackwards();
        //hoveringReverseOrder = moveSpeed > .25f && isMovingBackwards;

        float distanceToPlayer = distanceBetweenFollowingWorkers * (workerIndex + 1) * workersFollowDirection;

        Vector3 followPosition = playerPosition;
        followPosition.x -= distanceToPlayer;

        return followPosition;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!hoveringFollowingWorkers) return;

        Worker workerRemoved = workersFollowingPlayer[workerHoverIndex];
        RemoveFollowingWorker(workerRemoved, true);
        workerRemoved.HoverWorker(false);

        workerHoverIndex = 0;
        if(workersFollowingPlayer.Count > 0) {
            HoverWorker(workersFollowingPlayer[workerHoverIndex]);
        } else {
            StartCoroutine(SetHoveringWorkerAfterFrame(false));
            Player.Instance.EnableControlInputs();
            Player.Instance.SetManagingWorkersAfterFrame(false);
        }
    }

    private IEnumerator SetHoveringWorkerAfterFrame(bool hoveringFollowingWorkers) {
        yield return new WaitForEndOfFrame();

        this.hoveringFollowingWorkers = hoveringFollowingWorkers;
    }

    private void GameInput_OnPlayerLeftRightDirPerformed(object sender, System.EventArgs e) {
        if (!hoveringFollowingWorkers) return;
        if (workersFollowingPlayer.Count == 1) return;
        UnhoverPreviousHoveredWorker(workersFollowingPlayer[workerHoverIndex]);

        float selectDir = GameInput.Instance.GetMovementFloatNormalized();
        selectDir *= workersFollowDirection * -1;

        if(selectDir < 0) {
            workerHoverIndex--;
            if(workerHoverIndex < 0) {
                workerHoverIndex = workersFollowingPlayer.Count-1;
            }

        } else {
            workerHoverIndex++;
            if (workerHoverIndex >= workersFollowingPlayer.Count) {
                workerHoverIndex = 0;
            }

        }

        HoverWorker(workersFollowingPlayer[workerHoverIndex]);
        OnHoveredFollowingWorkerChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnHoverWorkersPerformed(object sender, System.EventArgs e) {
        if (workersFollowingPlayer.Count == 0) return;

        hoveringFollowingWorkers = !hoveringFollowingWorkers;

        Player.Instance.SetManagingWorkers(hoveringFollowingWorkers);

        if(hoveringFollowingWorkers ) {
            StartHoveringWorkers();

        } else {
            Player.Instance.EnableControlInputs();
            UnhoverPreviousHoveredWorker(workersFollowingPlayer[workerHoverIndex]);
        }
    }

    private void StartHoveringWorkers() {
        workerHoverIndex = 0;
        Player.Instance.DisableControlInputs();
        HoverWorker(workersFollowingPlayer[workerHoverIndex]);

    }

    private void HoverWorker(Worker worker) {
        worker.HoverWorker(true);
    }

    private void UnhoverPreviousHoveredWorker(Worker worker) {
        worker.HoverWorker(false);
    }

    private void Worker_OnAnyMobDied(object sender, System.EventArgs e) {
        if (!(sender is Worker)) return;
        Worker worker = (Worker)sender;
        if(workersFollowingPlayer.Contains(worker)) {
            RemoveFollowingWorker(worker, false);
        }
    }

    public void AddFollowingWorker(Worker worker) {
        workersFollowingPlayer.Add(worker);
        
        if(worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.hunter) {
            huntersFollowingPlayer.Add(worker);
        }
        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.guard) {
            guardsFollowingPlayer.Add(worker);
        }
        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.miner) {
            minersFollowingPlayer.Add(worker);
        }

        ReorderWorkerPositions();
    }

    public void RemoveFollowingWorker(Worker worker, bool triggerSFX) {
        worker.GetComponent<WorkerAI>().SetFollowingPlayer(false, triggerSFX);

        workersFollowingPlayer.Remove(worker);

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.hunter) {
            huntersFollowingPlayer.Remove(worker);
        }
        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.guard) {
            guardsFollowingPlayer.Remove(worker);
        }
        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.miner) {
            minersFollowingPlayer.Remove(worker);
        }

        ReorderWorkerPositions();
    }

    private void ReorderWorkerPositions() {
        List<Worker> reorderedWorkers = new List<Worker>();

        reorderedWorkers.AddRange(guardsFollowingPlayer);

        reorderedWorkers.AddRange(huntersFollowingPlayer);

        reorderedWorkers.AddRange(minersFollowingPlayer);

        workersFollowingPlayer = reorderedWorkers;
    }

    public int GetMaxFollowingWorkers() {
        return maxFollowingWorkers;
    }

    public bool GetHoveringWorkers() {
        return hoveringFollowingWorkers;
    }

    private void OnDestroy() {
        Worker.OnAnyMobDied -= Worker_OnAnyMobDied;
        GameInput.Instance.OnHoverWorkersPerformed -= GameInput_OnHoverWorkersPerformed;
        GameInput.Instance.OnPlayerLeftRightDirPerformed -= GameInput_OnPlayerLeftRightDirPerformed;
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractPerformed;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
    }
}
