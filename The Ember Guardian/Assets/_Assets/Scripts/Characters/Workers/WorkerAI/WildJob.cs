using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildJob : MonoBehaviour, IJobBehavior {

    private Collectible blueOrbAggroed;
    private List<Collectible> blueOrbDroppedByPlayerNearby = new List<Collectible>();

    private Worker worker;  

    [SerializeField] private float distanceToAggroOrb = 7.5f;
    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float blueOrbAggroMoveSpeed = 2.5f;
    [SerializeField] private float roamRadius = 3f;
    [SerializeField] private float roamChangeDestinationRate = 5f;

    private float roamTimer;
    private bool hasSetSpeed;

    private void OnEnable() {
        worker = GetComponent<Worker>();
        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor += PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
        roamTimer = roamChangeDestinationRate;
    }

    private void Update() {
        if (blueOrbAggroed != null) {
            HeadToAggroedBlueOrb();
        }
        else {
            Roam();
        }
    }

    private void OnDisable() {
        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor -= PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
    }

    public void HeadToAggroedBlueOrb() {

        if (!hasSetSpeed) {
            worker.GetComponent<MobMovement>().SetMoveSpeed(blueOrbAggroMoveSpeed);
            hasSetSpeed = true;
        }

        worker.GetComponent<MobMovement>().SetMoveTarget(blueOrbAggroed.transform.position);
    }

    public void UnAggroBlueOrb() {
        if (blueOrbAggroed == null) return;
        blueOrbAggroed.SetAggroedByWildWorker(false, null);
    }

    public void Roam() {

        if (!hasSetSpeed) {
            worker.GetComponent<MobMovement>().SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if(roamTimer < 0 ) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(worker.GetComponent<MobMovement>(), roamRadius, worker.GetMobSpawner().transform.position, false);
        }
    }

    private void PlayerCurrencies_OnBlueOrbDroppedOnTheFloor(object sender, PlayerCurrencies.OnBlueOrbDroppedOnTheFloorEventArgs e) {

        if(Mathf.Abs(Player.Instance.transform.position.x - worker.transform.position.x) < distanceToAggroOrb) {
            blueOrbDroppedByPlayerNearby.Add(e.blueOrbDropped);
            e.blueOrbDropped.OnCollectibleDestroyed += BlueOrbDropped_OnCollectibleDestroyed;

            if (!e.blueOrbDropped.GetAggroedByWildWorker() && blueOrbAggroed == null) {
                // Blue orb has not been aggroed

                e.blueOrbDropped.SetAggroedByWildWorker(true, worker);
                blueOrbAggroed = e.blueOrbDropped;
                
                hasSetSpeed = false;
            }

        }
    }

    private void BlueOrbDropped_OnCollectibleDestroyed(object sender, System.EventArgs e) {
        Collectible blueOrb = sender as Collectible;
        blueOrb.OnCollectibleDestroyed -= BlueOrbDropped_OnCollectibleDestroyed;
        blueOrbDroppedByPlayerNearby.Remove(blueOrb);

        if (blueOrb == blueOrbAggroed) {
            blueOrbAggroed = null;
            hasSetSpeed = false;
        } else {
            RefreshAggroedOrb();
        }

    }

    private void RefreshAggroedOrb() {
        foreach(Collectible collectible in blueOrbDroppedByPlayerNearby) {
            if(!collectible.GetAggroedByWildWorker()) {
                collectible.SetAggroedByWildWorker(true, worker);
                blueOrbAggroed = collectible;
                hasSetSpeed = false;
            }
        }
    }
}
