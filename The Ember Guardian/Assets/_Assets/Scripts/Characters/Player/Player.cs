using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    public static Player Instance;

    [SerializeField] private Transform projectileTarget;
    [SerializeField] private Transform carryingFlagPosition;

    private Rigidbody2D rb;
    private bool dead;
    private bool damagedRecently;
    private bool insideCamp;
    private bool hasHPRegen;
    private bool canDropOrbOnTheFloor = true;
    private bool interactingWithOtherObject;
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

    public void SetInteractingWithOtherObject(bool interactingWithOtherObject) {
        this.interactingWithOtherObject = interactingWithOtherObject;
        canDropOrbOnTheFloor = !interactingWithOtherObject;
    }

    public bool GetCanDropOrbOnTheFloor() {
        return canDropOrbOnTheFloor && !interactingWithOtherObject;
    }

    public void AddKnockBack(Vector2 knockbackDir) {
        rb.AddForce(knockbackDir, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit = false) {
        if (damagedRecently) return;
        if (dead) return;

        if (ShieldTanksDamage(damage, damageSource)) return;

        playerHealth -= 1;

        if(playerHealth <= 0) {
            Die();
        }

        damagedTimer = PlayerStats.Instance.GetDamagedImmunityTime();
        damagedRecently = true;

        OnPlayerDamaged?.Invoke(this, EventArgs.Empty);
    }

    private bool ShieldTanksDamage(int damage, Transform damageSource) {
        if(PlayerSkills.Instance.GetPassiveShield().GetShieldActive()) {
            PlayerSkills.Instance.GetPassiveShield().TakeDamage(damage, damageSource, false);
            return true;
        } else {
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
        GetComponent<PlayerCurrencies>().enabled = false;
        PlayerShoot.Instance.SetCanShoot(false);
        SetCanDropOrbOnTheFloor(false);

        if(SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerCurrencies.Instance.DropEmber();
        }

        OnPlayerDied?.Invoke(this, EventArgs.Empty);

        dead = true;
    }

    private IEnumerator RespawnCoroutine() {

        Vector2 respawnPosition = new Vector2();

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {

            respawnPosition = Tutorial.Instance.GetRespawnPosition();
            playerHealth = PlayerStats.Instance.GetPlayerMaxHP();

        } else {

            respawnPosition = new Vector2(Tent.Instance.transform.position.x, transform.position.y);
            playerHealth = PlayerStats.Instance.GetPlayerRespawnHealth();

        }

        transform.position = respawnPosition;

        yield return new WaitForSeconds(1f);

        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerAim>().enabled = true;
        GetComponent<PlayerShoot>().enabled = true;
        PlayerShoot.Instance.SetCanShoot(true);
        SetCanDropOrbOnTheFloor(true);

        OnPlayerRespawned?.Invoke(this, EventArgs.Empty);

        dead = false;
    }

    public void MoveOnTeleporter(Transform teleporterPlayerPosition) {
        canMove = false;

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<PlayerCurrencies>().enabled = false;
        PlayerShoot.Instance.SetCanShoot(false);
        SetCanDropOrbOnTheFloor(false);
        transform.position = teleporterPlayerPosition.position;
    }

    public void ReleasePlayerFromTeleporter() {
        canMove = true;

        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerCurrencies>().enabled = true;
        PlayerShoot.Instance.SetCanShoot(true);
        SetCanDropOrbOnTheFloor(true);
    }

    public void StartInteractingWithMerchant() {
        canMove = false;
        GetComponent<PlayerMovement>().enabled = false;
        PlayerShoot.Instance.SetCanShoot(false);
    }
    
    public void StopInteractingWithMerchant() {
        canMove = true;

        GetComponent<PlayerMovement>().enabled = true;
        PlayerShoot.Instance.SetCanShoot(true);
    }

    #endregion

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public Transform GetCarryingFlagPosition() {
        return carryingFlagPosition;
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
