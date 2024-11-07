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
    private bool insideCamp;

    private float deadTimer;
    private float respawnTime = 5f;

    private int playerMaxHealth = 7;
    private int playerHealth;

    public event EventHandler OnPlayerEnteredCamp;
    public event EventHandler OnPlayerExitedCamp;
    public event EventHandler OnPlayerDamaged;
    public event EventHandler<OnPlayerHealedEventArgs> OnPlayerHealed;
    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerRespawned;

    public class OnPlayerHealedEventArgs : EventArgs {
        public int healAmount;
    }

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        playerHealth = playerMaxHealth;
    }

    private void Update() {
        CheckExitingCamp();

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
        playerHealth -= 1;

        if(playerHealth <= 0) {
            Die();
        }

        OnPlayerDamaged?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAim>().enabled = false;
        GetComponent<PlayerShoot>().enabled = false;
        GetComponent<PlayerCurrencies>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        OnPlayerDied?.Invoke(this, EventArgs.Empty);

        dead = true;
    }

    private IEnumerator RespawnCoroutine() {
        playerHealth = playerMaxHealth;

        Vector2 respawnPosition = new Vector2(Tent.Instance.transform.position.x, transform.position.y);
        transform.position = respawnPosition;

        yield return new WaitForSeconds(1f);

        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerAim>().enabled = true;
        GetComponent<PlayerShoot>().enabled = true;
        GetComponent<PlayerCurrencies>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        OnPlayerRespawned?.Invoke(this, EventArgs.Empty);

        dead = false;
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
}
