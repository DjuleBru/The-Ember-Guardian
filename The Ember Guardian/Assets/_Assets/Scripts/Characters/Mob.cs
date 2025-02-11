using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob : MonoBehaviour, IDamageable
{
    [SerializeField] protected Transform projectileTarget;
    [SerializeField] protected Transform projectileParent;
    [SerializeField] protected Transform dropSpawnPoint;
    [SerializeField] protected Transform mobHitPS_Splatter;
    [SerializeField] protected Transform mobHitPS_Splatter_Continuous;
    [SerializeField] protected Transform mobHitPS_Splatter_Crit;
    [SerializeField] protected Transform mobHitPS_Front;

    protected MobSpawner mobSpawner;

    protected List<Collectible> collectiblesDropped = new List<Collectible>();
    public class OnMobDroppedCollectibleEventArgs {
        public List<Collectible> collectibleDroppedList;
    }

    protected int health;
    protected bool dead;

    public event EventHandler OnMobDied;
    public static event EventHandler OnAnyMobDied;
    public event EventHandler<OnMobDamageTakenEventArgs> OnMobDamageTaken;
    public event EventHandler<OnMobDamageTakenEventArgs> OnMobCritDamageTaken;
    public event EventHandler OnAnyMobCritDamageTaken;
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
        public Transform damageOriginTransform;
    }

    public void SetMobSpawner(MobSpawner mobSpawner) {
        this.mobSpawner = mobSpawner;
    }

    public MobSpawner GetMobSpawner() {
        return mobSpawner;
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false) {
        if (health <= 0) return;
        
        if(critHit) {
            damage *= 2;
            health -= damage;
            OnMobCritDamageTaken?.Invoke(this, new OnMobDamageTakenEventArgs {
                damageOriginTransform = damageSource,
            });
        } else {
            health -= damage;
        }

        OnMobDamageTaken?.Invoke(this, new OnMobDamageTakenEventArgs {
            damageOriginTransform = damageSource,
        });

        if (health <= 0) {
            Die();
        }
    }

    public void InstantiateHitPS(float angle, float height, bool critHit) {
        Vector3 localPosition = new Vector3(transform.position.x,height,0);

        if(!critHit) {
            Instantiate(mobHitPS_Splatter, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
        } else {
            Instantiate(mobHitPS_Splatter_Crit, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
        }

        //Instantiate(mobHitPS_Front, localPosition, Quaternion.identity, transform); // Particules pour impact normal
        Instantiate(mobHitPS_Splatter_Continuous, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
    }

    public virtual void Die() {
        dead = true;
        OnMobDied?.Invoke(this, EventArgs.Empty);
        OnAnyMobDied?.Invoke(this, EventArgs.Empty);
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

    public bool GetDead() {
        return dead;
    }

    public void InvokeOnMobDroppedCollectibles(List<Collectible> collectibleDroppedList) {
        OnMobDroppedCollectibles?.Invoke(this, new OnMobDroppedCollectibleEventArgs {
            collectibleDroppedList = collectiblesDropped
        });
    }
}
