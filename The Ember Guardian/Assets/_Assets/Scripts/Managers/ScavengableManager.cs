using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableManager : MonoBehaviour
{
    public static ScavengableManager Instance;

    private List<IScavengable> scavengablesInLevelList = new List<IScavengable>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach(IScavengable scavengable in GetComponentsInChildren<IScavengable>()) {
            scavengablesInLevelList.Add(scavengable);
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
}
