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
    

    public Scavengable GetClosestScavengableToScavenge(Vector3 minerPosition) {
        List<Scavengable> scavengabledToScavenge = GetAvailableScavengableList();

        float distanceToClosesyScavengable = Mathf.Infinity;
        Scavengable closestScavengable = null;

        foreach(Scavengable scavengable in scavengabledToScavenge) {
            float distanceToScanvegable = Mathf.Abs(scavengable.transform.position.x - minerPosition.x);
            if (distanceToScanvegable < distanceToClosesyScavengable) {
                closestScavengable = scavengable;
                distanceToClosesyScavengable = distanceToScanvegable;
            }
        }

        return closestScavengable;
    }
}
