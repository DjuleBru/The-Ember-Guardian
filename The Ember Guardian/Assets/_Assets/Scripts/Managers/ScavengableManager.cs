using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengableManager : MonoBehaviour
{
    public static ScavengableManager Instance;

    private List<Scavengable> scavengablesInLevelList = new List<Scavengable>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach(Scavengable scavengable in GetComponentsInChildren<Scavengable>()) {
            scavengablesInLevelList.Add(scavengable);
        }
    }

    public List<Scavengable> GetAvailableScavengableList() {

        List<Scavengable> scavengablesToScavenge = new List<Scavengable>();

        foreach(Scavengable scavengable in  scavengablesInLevelList) {
            if (scavengable.GetDepleted()) continue;
            if (scavengable.GetMaxMinersAssigned()) continue;
            if (!scavengable.GetMarkedToScavenge()) continue;

            scavengablesToScavenge.Add(scavengable);
        }

        return scavengablesToScavenge;
    }


    public Scavengable GetClosestHighestPriorityScavengableToScavenge(Vector3 minerPosition) {
        List<Scavengable> scavengabledToScavenge = GetAvailableScavengableList();

        Scavengable closestNonMine = null;
        float closestNonMineDistance = Mathf.Infinity;

        Scavengable closestMine = null;
        float closestMineDistance = Mathf.Infinity;

        foreach (Scavengable scavengable in scavengabledToScavenge) {
            float distance = Mathf.Abs(scavengable.transform.position.x - minerPosition.x);

            if (scavengable.GetIsMine()) {
                if (!scavengable.GetMineActive()) {
                    continue; // ignorer les mines inactives
                }

                if (distance < closestMineDistance) {
                    closestMine = scavengable;
                    closestMineDistance = distance;
                }
            }
            else {
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
