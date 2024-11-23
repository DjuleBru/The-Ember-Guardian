using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{

    public static Player Instance;

    [SerializeField] private Transform projectileTarget;

    private Rigidbody2D rb;
    private bool canDropOrbOnTheFloor = true;
    private bool dead;
    private bool damagedRecently;
    private bool insideCamp;

    private float damagedTimer;
    private float damagedImmunityTime = 1.5f;
    private float deadTimer;
    private float respawnTime = 5f;

    private int playerMaxHealth = 7;
    private int playerRespawnHealth = 3;
    private int playerHealth;

    public event EventHandler OnPlayerEnteredCamp;
    public event EventHandler OnPlayerExitedCamp;
    public event EventHandler OnPlayerDamaged;
    public event EventHandler OnPlayerDamagedRecentlyEnded;
    public event EventHandler<OnPlayerHealedEventArgs> OnPlayerHealed;
    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerRespawned;
    public event EventHandler OnPlayerTeleported;

    public class OnPlayerHealedEventArgs : EventArgs {
        public int healAmount;
    }
    private bool isLevelScene;

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        playerHealth = playerMaxHealth;
    }

    private void Start() {
        isLevelScene = (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level);
    }

    private void Update() {
        if (!isLevelScene) return;
        CheckExitingCamp();

        if (damagedRecently) {
            damagedTimer -= Time.deltaTime;
            if(damagedTimer < 0 ) {
                OnPlayerDamagedRecentlyEnded?.Invoke(this, EventArgs.Empty);
                damagedRecently = false;
            }
        }

        if (dead) {
            deadTimer += Time.deltaTime;

            if(deadTimer > respawnTime) {
                StartCoroutine(RespawnCoroutine());
                deadTimer = 0;
            }

        }
    }

    private void CheckExitingCamp() {
        if(insideCamp) {

            if(transform.position.x < CampZoneManager.Instance.GetCampCenterMinLimit() || transform.position.x > CampZoneManager.Instance.GetCampCenterMaxLimit()) {
                insideCamp = false;
                OnPlayerExitedCamp?.Invoke(this, EventArgs.Empty);
            }

        } else {

            if (transform.position.x > CampZoneManager.Instance.GetCampCenterMinLimit() && transform.position.x < CampZoneManager.Instance.GetCampCenterMaxLimit()) {
                insideCamp = true;
                OnPlayerEnteredCamp?.Invoke(this, EventArgs.Empty);
            }

        }
    }

    public void SetCanDropOrbOnTheFloor(bool canDrop) {
        canDropOrbOnTheFloor = canDrop;
    }

    public bool GetCanDropOrbOnTheFloor() {
        return canDropOrbOnTheFloor;
    }

    public void AddKnockBack(Vector2 knockbackDir) {
        rb.AddForce(knockbackDir, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition) {
        if (damagedRecently) return;
        if (dead) return;

        playerHealth -= 1;

        if(playerHealth <= 0) {
            Die();
        }

        damagedTimer = damagedImmunityTime;
        damagedRecently = true;

        OnPlayerDamaged?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAim>().enabled = false;
        GetComponent<PlayerShoot>().enabled = false;
        GetComponent<PlayerCurrencies>().enabled = false;
        SetCanDropOrbOnTheFloor(false);
        PlayerCurrencies.Instance.DropEmber();

        OnPlayerDied?.Invoke(this, EventArgs.Empty);

        dead = true;
    }

    private IEnumerator RespawnCoroutine() {
        playerHealth = playerRespawnHealth;

        Vector2 respawnPosition = new Vector2(Tent.Instance.transform.position.x, transform.position.y);
        transform.position = respawnPosition;

        yield return new WaitForSeconds(1f);

        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerAim>().enabled = true;
        GetComponent<PlayerShoot>().enabled = true;
        GetComponent<PlayerCurrencies>().enabled = true;
        SetCanDropOrbOnTheFloor(true);

        OnPlayerRespawned?.Invoke(this, EventArgs.Empty);

        dead = false;
    }

    public void MoveOnTeleporter(Transform teleporterPlayerPosition) {
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<PlayerShoot>().enabled = false;
        GetComponent<PlayerCurrencies>().enabled = false;
        GetComponentInChildren<PlayerAnimator>().enabled = false;
        SetCanDropOrbOnTheFloor(false);
        transform.position = teleporterPlayerPosition.position;
    }

    public void ReleasePlayerFromTeleporter() {
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerShoot>().enabled = true;
        GetComponent<PlayerCurrencies>().enabled = true;
        GetComponentInChildren<PlayerAnimator>().enabled = true;
        SetCanDropOrbOnTheFloor(true);
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }

    public int GetMaxHP() {
        return playerMaxHealth;
    }

    public int GetHP() {
        return playerHealth;
    }

    public void RefillPlayerHealth() {
        int healAmount = playerMaxHealth - playerHealth;

        playerHealth = playerMaxHealth;
        OnPlayerHealed?.Invoke(this, new OnPlayerHealedEventArgs {
            healAmount = healAmount
        });
    }

    public bool GetDead() {
        return dead;
    }
}
