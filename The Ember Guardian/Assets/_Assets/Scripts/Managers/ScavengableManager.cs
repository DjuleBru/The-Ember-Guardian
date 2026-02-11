using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableManager : MonoBehaviour
{
    public static ScavengableManager Instance;

    [SerializeField] private bool unlockScavengablesInThisLevel;
    [SerializeField] private bool scavengablesUnlocked = true;
    private List<IScavengable> scavengablesInLevelList = new List<IScavengable>();

    private void Awake() {
        Instance = this;

        scavengablesUnlocked = ES3.Load("scavengablesUnlocked", false);
    }

    private void Start() {
        foreach(IScavengable scavengable in GetComponentsInChildren<IScavengable>()) {
            scavengablesInLevelList.Add(scavengable);
        }

        if(!scavengablesUnlocked) {
            foreach (IScavengable scavengable in scavengablesInLevelList) {
                scavengable.SetScavengableUnlocked(false);
            }
        }

        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        if (!scavengablesUnlocked && unlockScavengablesInThisLevel) {
            foreach (IScavengable scavengable in scavengablesInLevelList) {
                scavengable.SetScavengableUnlocked(true);
            }

            ES3.Save("scavengablesUnlocked", true);
        }
    }

    public List<IScavengable> GetAvailableScavengableList() {

        List<IScavengable> scavengablesToScavenge = new List<IScavengable>();

        foreach(IScavengable scavengable in  scavengablesInLevelList) {
            if (scavengable.GetDepleted()) continue;
            if (scavengable.GetMaxMinersAssigned()) continue;
            if (!scavengable.GetMarkedToScavenge()) continue;

            scavengablesToScavenge.Add(scavengable);
        }

        return scavengablesToScavenge;
    }

    public List<Scavengable> GetScavengablesList() {
        List<Scavengable> allScavengables = new List<Scavengable>();

        foreach (IScavengable scavengable in scavengablesInLevelList) {
            if (scavengable is Scavengable) {
                allScavengables.Add(scavengable as Scavengable);
            }
        }

        return allScavengables;
    }

    public IScavengable GetClosestHighestPriorityScavengableToScavenge(MinerJob requestingMiner) {
        List<IScavengable> scavengables = GetAvailableScavengableList();
        List<MinerJob> allMiners = WorkerManager.Instance.GetRecruitedMiners();

        Dictionary<MinerJob, (IScavengable scavengable, int priority, float distance)> pendingAssignments = new();

        foreach (IScavengable scavengable in scavengables) {
            if (!scavengable.GetScavengingActive()) continue;

            int scavPriority = scavengable.GetMiningPriority();
            Vector3 scavPos = (scavengable as MonoBehaviour).transform.position;

            MinerJob closestMiner = null;
            float closestDistance = Mathf.Infinity;

            foreach (MinerJob miner in allMiners) {
                if (miner.GetScavengableAssigned() != null) continue;

                float dist = Mathf.Abs(miner.transform.position.x - scavPos.x);

                if (dist < closestDistance) {
                    closestDistance = dist;
                    closestMiner = miner;
                }
            }

            if (closestMiner == null) continue;

            if (!pendingAssignments.TryGetValue(closestMiner, out var currentAssignment)) {
                pendingAssignments[closestMiner] = (scavengable, scavPriority, closestDistance);
            }
            else {
                // Comparer les priorités, puis les distances si égalité
                if (scavPriority < currentAssignment.priority ||
                    (scavPriority == currentAssignment.priority && closestDistance < currentAssignment.distance)) {
                    pendingAssignments[closestMiner] = (scavengable, scavPriority, closestDistance);
                }
            }
        }

        if (pendingAssignments.TryGetValue(requestingMiner, out var assigned)) {
            return assigned.scavengable;
        }

        return null;
    }

    public IScavengable GetClosestHighestPriorityScavengableToScavenge(Vector3 minerPosition) {
        List<IScavengable> scavengabledToScavenge = GetAvailableScavengableList();

        IScavengable closestNonMine = null;
        float closestNonMineDistance = Mathf.Infinity;

        IScavengable closestMine = null;
        float closestMineDistance = Mathf.Infinity;

        foreach (IScavengable scavengable in scavengabledToScavenge) {
            float distance = Mathf.Abs((scavengable as MonoBehaviour).transform.position.x - minerPosition.x);

            if (scavengable.GetIsMine()) {
                if (!scavengable.GetScavengingActive()) {
                    continue; // ignorer les mines inactives
                }

                if (distance < closestMineDistance) {
                    closestMine = scavengable;
                    closestMineDistance = distance;
                }
            }
            else {
                if (!scavengable.GetScavengingActive()) {
                    continue; // ignorer les mines inactives
                }

                if (distance < closestNonMineDistance) {
                    closestNonMine = scavengable;
                    closestNonMineDistance = distance;
                }
            }
        }

        if (closestNonMine != null) {
            return closestNonMine;
        }
        else {
            return closestMine;
        }
    }

    public void AddScavengable(Scavengable scav) {
        scavengablesInLevelList.Add(scav);
    }
}
