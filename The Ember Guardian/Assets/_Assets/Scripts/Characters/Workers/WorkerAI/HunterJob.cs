using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HunterJob : MonoBehaviour, IJobBehavior {

    private MobMovement mobMovement;
    private MobAttack hunterAttack;
    private Animal targetAnimal;

    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float headToCampMoveSpeed = 2f;
    [SerializeField] private float trackAnimalMoveSpeed = 2f;
    [SerializeField] private float roamChangeDestinationRate = 5f;

    [SerializeField] private float huntingRange = 4f;

    private float maxAnimalTargetingDistanceToCampOuterPoint = 20f;

    private float roamTimer;
    private float checkClosestAnimalTimer;
    private float checkClosestAnimalRate = 1f;

    private bool hasSetSpeed;
    private bool hasHitAnimal;

    private List<Collectible> orbsToCollect = new List<Collectible>();

    public enum HunterState {
        idle,
        headingToHunt,
        pickingUpOrbs,
        hunting,
    }

    private HunterState state;

    private void Update() {
        return;
        if (targetAnimal != null) {
            Debug.DrawLine(mobMovement.transform.position, targetAnimal.transform.position, Color.red);
        }

        if(state != HunterState.pickingUpOrbs) {
            if (!hasHitAnimal) {
                CheckClosestAnimal();
            }
        }

        switch (state) {

            case HunterState.idle:

                Roam();

            break;

            case HunterState.headingToHunt:

                if (targetAnimal == null) return;

                HeadToTargetAnimal();
                if(TargetAnimalIsInHuntingRange()) {
                    ChangeState(HunterState.hunting);
                };

                break;

            case HunterState.pickingUpOrbs:
                HeadToPickUpClosestOrb();
                break;

            case HunterState.hunting:

                if (!TargetAnimalIsInHuntingRange()) {
                    hunterAttack.RemoveAttackTarget();
                    ChangeState(HunterState.headingToHunt);
                    return;
                };

                //hunterAttack.SetAttackTargetTransform(targetAnimal);

                break;

       }

    }

    private bool TargetAnimalIsInHuntingRange() {
        if(Mathf.Abs(targetAnimal.transform.position.x - mobMovement.transform.position.x) < huntingRange) {
            return true;
        } else {
            return false;
        }
    }

    public void Roam() {
        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer < 0) {
            roamTimer = roamChangeDestinationRate;
            RoamBehavior.RoamAroundPoint(mobMovement, 3f , CampZoneManager.Instance.GetClosestExteriorZoneLimit(mobMovement.transform.position));
        }
    }

    private bool CheckClosestAnimal() {
        checkClosestAnimalTimer -= Time.deltaTime;

        if(checkClosestAnimalTimer < 0 ) {
            checkClosestAnimalTimer = checkClosestAnimalRate;

            Animal newTargetAnimal = AnimalManager.Instance.GetClosestAnimalInRadius(mobMovement.transform.position, maxAnimalTargetingDistanceToCampOuterPoint);

            if(newTargetAnimal == null) {
                targetAnimal = null;
                ChangeState(HunterState.idle);
                return false;

            } else {
                TargetAnimal(newTargetAnimal);
                ChangeState(HunterState.headingToHunt);
                return true;
            }
        }

        return false;
    }

    public void TargetAnimal(Animal newTargetAnimal) {
        if (targetAnimal == newTargetAnimal) return;

        if (targetAnimal != null) {
            newTargetAnimal.OnAnimalDroppedCollectibles -= TargetAnimal_OnAnimalDroppedCollectibles;
        }

        targetAnimal = newTargetAnimal;
        targetAnimal.OnAnimalDroppedCollectibles += TargetAnimal_OnAnimalDroppedCollectibles;
        targetAnimal.OnMobDamageTaken += TargetAnimal_OnMobDamageTaken;
    }

    private void TargetAnimal_OnAnimalDroppedCollectibles(object sender, Animal.OnAnimalDroppedCollectibleEventArgs e) {
        orbsToCollect = e.collectibleDroppedList;

        foreach(Collectible collectible in orbsToCollect) {
            collectible.OnCollectibleDestroyed += Collectible_OnCollectibleDestroyed;
        }

        mobMovement.SetMoveTarget(targetAnimal.transform.position);

        targetAnimal = null;
        hasHitAnimal = false;

        ChangeState(HunterState.pickingUpOrbs);
    }

    private void TargetAnimal_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        hasHitAnimal = true;
    }

    public void HeadToTargetAnimal() {

        if (!hasSetSpeed) {
            mobMovement.SetMoveSpeed(roamMoveSpeed);
            hasSetSpeed = true;
        }

        Vector3 targetDestination = new Vector3(targetAnimal.transform.position.x, 0, 0);
            
        mobMovement.SetMoveTarget(targetDestination);
        hasSetSpeed = false;
    }

    public void HeadToPickUpClosestOrb() {

        if (orbsToCollect.Count == 0) {
            ChangeState(HunterState.headingToHunt);
            return;
        }

        Collectible orbToCollect = orbsToCollect[0];
        Vector3 targetDestination = new Vector3(orbToCollect.transform.position.x, 0, 0);

        mobMovement.SetMoveTarget(targetDestination);

    }

    private void ChangeState(HunterState newState) {
        mobMovement.SetMoveTarget(mobMovement.transform.position);
        state = newState;
    }

    private void Collectible_OnCollectibleDestroyed(object sender, System.EventArgs e) {
        Collectible collectible = sender as Collectible;
        RemoveOrbToCollect(collectible);
    }

    public void RemoveOrbToCollect(Collectible collectible) {
        if(orbsToCollect.Contains(collectible)) {
            orbsToCollect.Remove(collectible);
        }
    }

    public List<Collectible> GetOrbsToCollectList() {
        return orbsToCollect;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {

    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        checkClosestAnimalTimer = 0;

        targetAnimal = null;
        hasHitAnimal = false;
        
        ChangeState(HunterState.headingToHunt);
    }

    private void WorkerMovement_OnDestinationReached(object sender, System.EventArgs e) {

    }

    private void OnEnable() {
        mobMovement = GetComponent<MobMovement>();
        hunterAttack = GetComponent<MobAttack>();

        mobMovement.OnDestinationReached += WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;


        mobMovement.SetMoveTarget(mobMovement.transform.position);

        if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
            checkClosestAnimalTimer = 0;
            ChangeState(HunterState.headingToHunt);
        }
    }
    private void OnDisable() {
        hasSetSpeed = false;
        mobMovement.OnDestinationReached -= WorkerMovement_OnDestinationReached;
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
    }

}
