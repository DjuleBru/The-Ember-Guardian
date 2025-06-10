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
    [SerializeField] protected bool useHitXPosition;

    protected MobSpawner mobSpawner;

    protected List<Collectible> collectiblesDropped = new List<Collectible>();
    public class OnMobDroppedCollectibleEventArgs {
        public List<Collectible> collectibleDroppedList;
    }

    protected Rigidbody2D rb;
    protected int health;
    protected int maxHealth;
    protected bool dead;
    protected bool inObstacleTriggerArea;

    public event EventHandler OnMobDied;
    public static event EventHandler OnAnyMobDied;
    public event EventHandler OnMobHitObstacle;
    public event EventHandler<OnMobDamageTakenEventArgs> OnMobDamageTaken;
    public event EventHandler<OnMobDamageTakenEventArgs> OnMobCritDamageTaken;
    public static event EventHandler OnAnyMobCritDamageTaken;
    public event EventHandler<OnMobDroppedCollectibleEventArgs> OnMobDroppedCollectibles;

    protected float inObstacleTriggerAreaSendEventRate = 1f;
    protected float inObstacleTriggerAreaSendEventTimer;

    protected virtual void Update() {
        if(inObstacleTriggerArea) {
            inObstacleTriggerAreaSendEventTimer -= Time.deltaTime;
            if(inObstacleTriggerAreaSendEventTimer < 0) {
                inObstacleTriggerAreaSendEventTimer = inObstacleTriggerAreaSendEventRate;
                OnMobHitObstacle?.Invoke(this, EventArgs.Empty);
            }
        }
    }

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

    public virtual void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {
        if (health <= 0) return;
       
        if(critHit) {
            damage *= 2;
            health -= damage;
            OnMobCritDamageTaken?.Invoke(this, new OnMobDamageTakenEventArgs {
                damageOriginTransform = damageSource,
            });
            OnAnyMobCritDamageTaken?.Invoke(this, EventArgs.Empty);
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

    public virtual void HandlePlayerSkillEffects(float angle, float height) {
        if(PlayerSkills.Instance.GetMagmaBullet()) {
            Vector3 localPosition = new Vector3(transform.position.x, height, 0);
            StaticProjectile magmaShot = Instantiate(PlayerSkills.Instance.GetMagmaShotPrefab(), localPosition, Quaternion.Euler(0, 0, angle)).GetComponent<StaticProjectile>();
            magmaShot.Initialize(PlayerAim.Instance.GetAimDirFloat(), null, PlayerSkills.Instance.GetMagmaShotDamage(), true);
            magmaShot.InitializeBurning(PlayerSkills.Instance.GetMagmaShotBulletBurnDuration());
            magmaShot.GetComponent<StaticProjectileSounds>().TriggerProjectileSFX();
        }
    }

    public void TakeKnockback(float knockback, Vector2 knockBackDirNormalized) {
        if (dead) return;
        Vector2 knockBackForce = knockBackDirNormalized * knockback;
        rb.AddForce(knockBackForce, ForceMode2D.Impulse);
    }

    public void InstantiateHitPS(float angle, float height, bool critHit, int damage, float xPosition) {
        Vector3 localPosition = new Vector3(transform.position.x,height,0);
        if(useHitXPosition) {
            localPosition.x = xPosition;
        }

        if(!critHit) {
            Instantiate(mobHitPS_Splatter, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
        } else {
            Instantiate(mobHitPS_Splatter_Crit, localPosition, Quaternion.Euler(0, 0, angle), transform); // Particules pour impact normal
        }

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
    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Obstacle>() != null) {
            Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
            if (obstacle.GetBuilt()) return;
            OnMobHitObstacle?.Invoke(this, EventArgs.Empty);
            inObstacleTriggerArea = true;
        
        }
        if (collision.gameObject.GetComponent<EndLevelCollider>() != null) {
            OnMobHitObstacle?.Invoke(this, EventArgs.Empty);
            inObstacleTriggerArea = true;
        }
    }
    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Obstacle>() != null) {
            inObstacleTriggerArea = false;
        }
        if (collision.gameObject.GetComponent<EndLevelCollider>() != null) {
            inObstacleTriggerArea = false;
        }
    }
}
