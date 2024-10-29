using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildJob : MonoBehaviour, IJobBehavior {

    private Collectible blueOrbAggroed;

    private Worker worker;  

    [SerializeField] private float distanceToAggroOrb = 5f;
    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float blueOrbAggroMoveSpeed = 2.5f;
    [SerializeField] private float roamRadius = 3f;
    [SerializeField] private float roamChangeDestinationRate = 5f;

    private float roamTimer;
    private bool hasSetSpeed;

    private void OnEnable() {
        worker = GetComponent<Worker>();
        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor += PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
    }

    private void Update() {
        if (blueOrbAggroed != null) {
            AggroBlueOrb();
        }
        else {
            Roam();
        }
    }

    private void OnDisable() {
        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor -= PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
    }

    public void AggroBlueOrb() {

        if (!hasSetSpeed) {
            worker.GetComponent<MobMovement>().SetMoveSpeed(blueOrbAggroMoveSpeed);
            hasSetSpeed = true;
        }

        worker.GetComponent<MobMovement>().SetMoveTarget(blueOrbAggroed.transform.position);
    }

    public void Roam() {

        if (!hasSetSpeed) {
            worker.GetComponent<MobMovement>().SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if(roamTimer < 0 ) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(worker.GetComponent<MobMovement>(), roamRadius, worker.GetMobSpawner().transform.position);
        }
    }

    private void PlayerCurrencies_OnBlueOrbDroppedOnTheFloor(object sender, PlayerCurrencies.OnBlueOrbDroppedOnTheFloorEventArgs e) {
        if (e.blueOrbDropped.GetAggroedByWildWorker()) return;
        if (blueOrbAggroed != null) return;

        if(Mathf.Abs(Player.Instance.transform.position.x - worker.transform.position.x) < distanceToAggroOrb) {
            e.blueOrbDropped.SetAggroedByWildWorker();
            blueOrbAggroed = e.blueOrbDropped;
            blueOrbAggroed.OnCollectibleDestroyed += BlueOrbAggroed_OnCollectibleDestroyed;
            hasSetSpeed = false;
        }
    }

    private void BlueOrbAggroed_OnCollectibleDestroyed(object sender, System.EventArgs e) {
        blueOrbAggroed.OnCollectibleDestroyed -= BlueOrbAggroed_OnCollectibleDestroyed;
        blueOrbAggroed = null;
        hasSetSpeed = false;
    }
}
