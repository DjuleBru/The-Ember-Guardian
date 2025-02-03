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

    private int workerHoverIndex;
    private bool hoveringFollowingWorkers;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Mob.OnAnyMobDied += Worker_OnAnyMobDied;
        GameInput.Instance.OnHoverWorkersPerformed += GameInput_OnHoverWorkersPerformed;
        GameInput.Instance.OnPlayerLeftRightDirPerformed += GameInput_OnPlayerLeftRightDirPerformed;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!hoveringFollowingWorkers) return;

        Worker workerRemoved = workersFollowingPlayer[workerHoverIndex];
        RemoveFollowingWorker(workerRemoved);
        workerRemoved.HoverWorker(false);

        workerHoverIndex = 0;
        if(workersFollowingPlayer.Count > 0) {
            HoverWorker(workersFollowingPlayer[workerHoverIndex]);
        } else {
            hoveringFollowingWorkers = false;
            Player.Instance.EnableControlInputs();
        }
    }

    private void GameInput_OnPlayerLeftRightDirPerformed(object sender, System.EventArgs e) {
        if (!hoveringFollowingWorkers) return;
        UnhoverPreviousHoveredWorker(workersFollowingPlayer[workerHoverIndex]);

        float selectDir = GameInput.Instance.GetMovementFloatNormalized();

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
            RemoveFollowingWorker(worker);
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

    public void RemoveFollowingWorker(Worker worker) {
        worker.GetComponent<WorkerAI>().SetFollowingPlayer(false);

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

    public Vector3 GetWorkerFollowPosition(Worker worker) {
        // Trouve l'index du worker dans la liste
        int workerIndex = workersFollowingPlayer.IndexOf(worker);

        // Calcule la position en fonction de l'index et de la distance entre les workers
        Vector3 playerPosition = Player.Instance.transform.position;
        float lastMoveDir = PlayerAim.Instance.GetAimDirFloat();
        float distanceToPlayer = distanceBetweenFollowingWorkers * (workerIndex + 1) * lastMoveDir;

        Vector3 followPosition = playerPosition;
        followPosition.x -= distanceToPlayer;

        return followPosition;
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
    }
}
