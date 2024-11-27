using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    public static Player Instance;

    [SerializeField] private Transform projectileTarget;

    private Rigidbody2D rb;
    private bool dead;
    private bool damagedRecently;
    private bool insideCamp;
    private bool hasHPRegen;
    private bool canDropOrbOnTheFloor = true;
    private bool canMove = true;

    private float damagedTimer;
    private float deadTimer;
    private float hpRegenTimer;
    private float hpRegenTime;

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
        playerHealth = PlayerStats.Instance.GetPlayerMaxHP();
    }

    private void Start() {
        isLevelScene = (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level);

        PlayerStats.Instance.OnPlayerMaxHPChanged += PlayerStats_OnPlayerMaxHPChanged;
        PlayerStats.Instance.OnPlayerHPRegenChanged += PlayerStats_OnPlayerHPRegenChanged;
    }

    private void Update() {
        if (!isLevelScene) return;
        CheckExitingCamp();

        if(hasHPRegen) {
            hpRegenTimer -= Time.deltaTime;
            if(hpRegenTimer < 0) {
                hpRegenTimer = hpRegenTime;
                HealPlayer(1);
            }
        }

        if (damagedRecently) {
            damagedTimer -= Time.deltaTime;
            if(damagedTimer < 0 ) {
                OnPlayerDamagedRecentlyEnded?.Invoke(this, EventArgs.Empty);
                damagedRecently = false;
            }
        }

        if (dead) {
            deadTimer += Time.deltaTime;

            if(deadTimer > PlayerStats.Instance.GetRespawnTime()) {
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

        if (ShieldTanksDamage(damage, damageSourcePosition)) return;

        playerHealth -= 1;

        if(playerHealth <= 0) {
            Die();
        }

        damagedTimer = PlayerStats.Instance.GetDamagedImmunityTime();
        damagedRecently = true;

        OnPlayerDamaged?.Invoke(this, EventArgs.Empty);
    }

    private bool ShieldTanksDamage(int damage, Vector3 damageSourcePosition) {
        if(PlayerSkills.Instance.GetPassiveShield().GetShieldActive()) {
            PlayerSkills.Instance.GetPassiveShield().TakeDamage(damage, damageSourcePosition);
            Debug.Log("ShieldTanksDamage");
            return true;
        } else {
            Debug.Log("Shield ddoes not TanksDamage");
            return false;
        }
    }

    private void PlayerStats_OnPlayerHPRegenChanged(object sender, EventArgs e) {
        hpRegenTime = PlayerStats.Instance.GetHpRegenTime();
        hasHPRegen = true;
    }

    private void PlayerStats_OnPlayerMaxHPChanged(object sender, EventArgs e) {
        HealPlayer(1);
    }

    #region PLAYER CONTROLS RESTRICTIONS
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
        playerHealth = PlayerStats.Instance.GetPlayerRespawnHealth();

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
        canMove = false;

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<PlayerShoot>().enabled = false;
        GetComponent<PlayerCurrencies>().enabled = false;
        SetCanDropOrbOnTheFloor(false);
        transform.position = teleporterPlayerPosition.position;
    }

    public void ReleasePlayerFromTeleporter() {
        canMove = true;

        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerShoot>().enabled = true;
        GetComponent<PlayerCurrencies>().enabled = true;
        SetCanDropOrbOnTheFloor(true);
    }

    public void StartInteractingWithMerchant() {
        canMove = false;
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerShoot>().enabled = false;
    }
    
    public void StopInteractingWithMerchant() {
        canMove = true;

        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerShoot>().enabled = true;
    }

    #endregion

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }

    public int GetHP() {
        return playerHealth;
    }

    public void RefillPlayerHealth() {
        int healAmount = PlayerStats.Instance.GetPlayerMaxHP() - playerHealth;

        playerHealth = PlayerStats.Instance.GetPlayerMaxHP();
        OnPlayerHealed?.Invoke(this, new OnPlayerHealedEventArgs {
            healAmount = healAmount
        });
    }

    public void HealPlayer(int healAmount) {
        if(playerHealth + healAmount > PlayerStats.Instance.GetPlayerMaxHP()) {
            healAmount = PlayerStats.Instance.GetPlayerMaxHP() - playerHealth;
        }

        if (healAmount == 0) return;

        playerHealth += healAmount;
        OnPlayerHealed?.Invoke(this, new OnPlayerHealedEventArgs {
            healAmount = healAmount
        });
    }

    public bool GetDead() {
        return dead;
    }

    public bool GetCanMove() {
        return canMove;
    }
}
