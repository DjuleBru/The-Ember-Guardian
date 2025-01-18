using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
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
    private bool interactingWithMerchant;

    private bool isInvincibleWhileRolling = false;
    private float delayAfterRollStartForInvincibleStart = .1f;
    private float delayAfterRollStartForInvincibleEnd = .3f;

    private float damagedTimer;
    private float hpRegenTimer;
    private float hpRegenTime;
    private float dieFuelExtraction = 10f;

    private int playerHealth;

    public event EventHandler OnPlayerEnteredCamp;
    public event EventHandler OnPlayerExitedCamp;
    public event EventHandler OnPlayerDamaged;
    public event EventHandler OnPlayerDamagedRecentlyEnded;
    public event EventHandler<OnPlayerHealedEventArgs> OnPlayerHealed;
    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerRespawned;
    public event EventHandler OnPlayerBackToTentToRespawn;

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
        isLevelScene = (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB);

        PlayerStats.Instance.OnPlayerMaxHPChanged += PlayerStats_OnPlayerMaxHPChanged;
        PlayerStats.Instance.OnPlayerHPRegenChanged += PlayerStats_OnPlayerHPRegenChanged;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
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
        if (interactingWithMerchant) return false;

        return canDropOrbOnTheFloor && !dead;
    }

    public void AddKnockBack(Vector2 knockbackDir) {
        rb.AddForce(knockbackDir, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit = false) {
        if (isInvincibleWhileRolling) { Debug.Log("isInvincibleWhileRolling"); return; } 
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

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine() {
        yield return new WaitForSeconds(delayAfterRollStartForInvincibleStart);
        isInvincibleWhileRolling = true;
        yield return new WaitForSeconds(delayAfterRollStartForInvincibleEnd);
        isInvincibleWhileRolling = false;

    }

    #region PLAYER CONTROLS RESTRICTIONS
    public void Die() {
        SetCanDropOrbOnTheFloor(false);
        DisableControlInputs();

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerCurrencies.Instance.SetCarryingEmber(false);
        }

        OnPlayerDied?.Invoke(this, EventArgs.Empty);
        dead = true;

        if(Fire.Instance.GetCurrentFuelLevel() == 0) {
            StartCoroutine(LevelFailedCoroutine());
        } else {
            StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator LevelFailedCoroutine() {
        yield return new WaitForSeconds(2f);
        SceneLoader.Instance.LoadHub(3f);
    }

    private IEnumerator RespawnCoroutine() {
        yield return new WaitForSeconds(PlayerStats.Instance.GetRespawnTime());

        Vector2 respawnPosition = new Vector2();

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {

            respawnPosition = Tutorial.Instance.GetRespawnPosition();
            playerHealth = PlayerStats.Instance.GetPlayerMaxHP();

        } else {

            respawnPosition = new Vector2(Tent.Instance.transform.position.x, transform.position.y);
            playerHealth = PlayerStats.Instance.GetPlayerRespawnHealth();

        }

        transform.position = respawnPosition;
        OnPlayerBackToTentToRespawn?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2f);

        OnPlayerRespawned?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2f);

        EnableControlInputs();
        SetCanDropOrbOnTheFloor(true);
        dead = false;
    }

    public void MoveOnTeleporter(Transform teleporterPlayerPosition) {
        canMove = false;

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<PlayerCurrencies>().enabled = false;
        PlayerShoot.Instance.SetCanShoot(false);
        SetCanDropOrbOnTheFloor(false);
        transform.position = teleporterPlayerPosition.position;
    }

    public void ReleasePlayerFromTeleporter() {
        canMove = true;

        GetComponent<PlayerCurrencies>().enabled = true;
        PlayerShoot.Instance.SetCanShoot(true);
        SetCanDropOrbOnTheFloor(true);
    }

    public void StartInteractingWithMerchant() {
        interactingWithMerchant = true;
        SetCanDropOrbOnTheFloor(false);
        DisableControlInputs();
    }
    
    public void StopInteractingWithMerchant() {
        SetCanDropOrbOnTheFloor(true);
        // Set interactingWithMerchant false after frame or dog will react

        StartCoroutine(SetStopInteractingWithMerchantCoroutine());
        StartCoroutine(EnableControlInputCoroutine());
    }

    public void DisableControlInputs() {
        canMove = false;

        GetComponent<PlayerAim>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;
        PlayerShoot.Instance.SetCanShoot(false);
    }

    public void EnableControlInputs() {
        StartCoroutine(EnableControlInputCoroutine());
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    private IEnumerator EnableControlInputCoroutine() {
        yield return new WaitForEndOfFrame();
        canMove = true;

        GetComponent<PlayerAim>().enabled = true;
        GetComponent<PlayerMovement>().enabled = true;
        PlayerShoot.Instance.SetCanShoot(true);
    }

    private IEnumerator SetStopInteractingWithMerchantCoroutine() {
        yield return new WaitForSeconds(.5f);
        interactingWithMerchant = false;
    }

    #endregion

    public void SetPosition(Vector3 position) {
        transform.position = position;
    }

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

    public bool GetInteractingWithMerchant() {
        return interactingWithMerchant;
    }
}
