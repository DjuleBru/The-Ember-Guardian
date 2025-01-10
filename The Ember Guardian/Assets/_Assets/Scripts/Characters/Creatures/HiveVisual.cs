using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveVisual : MonoBehaviour
{

    [SerializeField] protected Transform mobHitPS_Splatter;
    [SerializeField] protected Transform mobHitPS_Splatter_Continuous;

    private CreatureSpawnerContinuous creatureSpawnerContinuous;

    private void Awake() {
        creatureSpawnerContinuous = GetComponentInParent<CreatureSpawnerContinuous>();
    }

    private void Start() {
        creatureSpawnerContinuous.OnHitPSInstatiated += CreatureSpawnerContinuous_OnHitPSInstatiated;
    }

    private void CreatureSpawnerContinuous_OnHitPSInstatiated(object sender, CreatureSpawnerContinuous.OnHitPSInstatiatedEventArgs e) {
        Vector3 localPosition = new Vector3(transform.position.x, e.height, 0);

        Instantiate(mobHitPS_Splatter, localPosition, Quaternion.Euler(0, 0, e.angle), transform); // Particules pour impact normal
        Instantiate(mobHitPS_Splatter_Continuous, localPosition, Quaternion.Euler(0, 0, e.angle), transform); // Particules pour impact normal
    }
}
