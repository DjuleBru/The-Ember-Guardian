using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob : MonoBehaviour, IDamageable
{
    [SerializeField] protected Transform projectileTarget;
    [SerializeField] protected Transform projectileParent;
    [SerializeField] protected Transform dropSpawnPoint;

    protected MobSpawner mobSpawner;

    protected List<Collectible> collectiblesDropped = new List<Collectible>();
    public class OnMobDroppedCollectibleEventArgs {
        public List<Collectible> collectibleDroppedList;
    }

    protected int health;

    public event EventHandler OnMobDied;
    public event EventHandler<OnMobDamageTakenEventArgs> OnMobDamageTaken;
    public event EventHandler<OnMobDroppedCollectibleEventArgs> OnMobDroppedCollectibles;

    protected void SpawnDroppedCurrencies(List<PlayerCurrencies.CurrencyType> currencyTypeList, List<int> dropAmountList) {

        // Chance to drop x2 skill
        int dropMultiplier = 1;
        float dropMultiplierRandom = UnityEngine.Random.Range(0f, 100f);
        if(dropMultiplierRandom < PlayerStats.Instance.GetChanceToDropx2()) {
            dropMultiplier = 2;
        }

        int j = 0;        
        foreach(PlayerCurrencies.CurrencyType currencyType in currencyTypeList) {
            int currencyDropAmount = dropAmountList[j] * dropMultiplier;

            for (int i = 0; i < currencyDropAmount; i++) {
                Transform currencyPrefab = CurrenciesManager.Instance.GetCurrencyPrefab(currencyType);
                Collectible lastBlueOrbDroppedOnTheFloor = Instantiate(currencyPrefab, dropSpawnPoint.position, Quaternion.identity).GetComponent<Collectible>();
                lastBlueOrbDroppedOnTheFloor.ApplyRandomUpwardsForce(3, 6);
                lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(1f);
                lastBlueOrbDroppedOnTheFloor.SetCanBePickedUpByWorker();

                collectiblesDropped.Add(lastBlueOrbDroppedOnTheFloor);
            }
            j++;

        }

    }

    public class OnMobDamageTakenEventArgs {
        public Vector3 damageOriginPosition;
    }

    public void SetMobSpawner(MobSpawner mobSpawner) {
        this.mobSpawner = mobSpawner;
    }

    public MobSpawner GetMobSpawner() {
        return mobSpawner;
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition) {
        if (health <= 0) return;

        health -= damage;
        OnMobDamageTaken?.Invoke(this, new OnMobDamageTakenEventArgs {
            damageOriginPosition = damageSourcePosition,
        });

        if (health <= 0) {
            Die();
        }
    }

    public virtual void Die() {
        OnMobDied?.Invoke(this, EventArgs.Empty);
        GetComponent<MobMovement>().enabled = false;

        if(GetComponent<MobAttack>() != null) {
            GetComponent<MobAttack>().enabled = false;
        }
    }

    protected IEnumerator DestroyGameObjectAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }

    public Transform GetProjectileParent() {
        return projectileParent;
    }

    public void InvokeOnMobDroppedCollectibles(List<Collectible> collectibleDroppedList) {
        OnMobDroppedCollectibles?.Invoke(this, new OnMobDroppedCollectibleEventArgs {
            collectibleDroppedList = collectiblesDropped
        });
    }
}
