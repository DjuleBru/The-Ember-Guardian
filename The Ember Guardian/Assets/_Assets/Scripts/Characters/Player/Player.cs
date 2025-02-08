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
    private bool hoveringWorker;
    private bool cancellingHoveringWorker;
    private bool managingWorkers;
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
    public event EventHandler<OnPlayerChangedHealthEventArgs> OnPlayerDamaged;
    public event EventHandler OnPlayerDamagedRecentlyEnded;
    public event EventHandler<OnPlayerChangedHealthEventArgs> OnPlayerHealed;
    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerRespawned;
    public event EventHandler OnPlayerBackToTentToRespawn;

    public class OnPlayerChangedHealthEventArgs : EventArgs {
        public int hpChangeAmount;
    }

    private bool isLevelScene;

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        playerHealth = PlayerStats.Instance.GetMaxHP();
    }

    private void Start() {
        isLevelScene = (SceneLoader.Instance != null && SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB);

        PlayerStats.Instance.OnPlayerMaxHPChanged += PlayerStats_OnPlayerMaxHPChanged;
        PlayerStats.Instance.OnPlayerHPRegenChanged += PlayerStats_OnPlayerHPRegenChanged;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;

        hpRegenTime = PlayerStats.Instance.GetHpRegenTime();
        if(hpRegenTime != 0) {
            hasHPRegen = true;
        }
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

    public void AddKnockBack(Vector2 knockbackDir) {
        rb.AddForce(knockbackDir, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage, Transform damageSource, bool critHit = false) {
        if (damagedRecently) return;
        if (dead) return;
        if (isInvincibleWhileRolling) return;

        if (ShieldTanksDamage(damage, damageSource)) return;

        int healthLoss = damage;
        playerHealth -= damage;

        if(playerHealth <= 0) {
            healthLoss = playerHealth + damage;
            playerHealth = 0;
            Die();
        }

        damagedTimer = PlayerStats.Instance.GetDamagedImmunityTime();
        damagedRecently = true;

        OnPlayerDamaged?.Invoke(this, new OnPlayerChangedHealthEventArgs {
            hpChangeAmount = healthLoss
        });
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

    public void SetCanDropOrbOnTheFloor(bool canDrop) {
        canDropOrbOnTheFloor = canDrop;
    }

    public void SetInteractingWithOtherObject(bool interactingWithOtherObject) {
        this.interactingWithOtherObject = interactingWithOtherObject;
    }

    public void SetHoveringWorker(bool hoveringWorker) {
        this.hoveringWorker = hoveringWorker;
    }

    public void ResetHoveringWorkerAfterInteractCanceled() {
        cancellingHoveringWorker = true;
    }

    private void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if(cancellingHoveringWorker) {
            StartCoroutine(SetHoveringWorkerAfterFrameCoroutine(false));
            cancellingHoveringWorker = false;
        }
    }

    private IEnumerator SetHoveringWorkerAfterFrameCoroutine(bool hoveringWorker) {
        yield return new WaitForEndOfFrame();
        this.hoveringWorker = hoveringWorker;
    }
    public void SetManagingWorkers(bool managingWorkers) {
        this.managingWorkers = managingWorkers;
    }
    public void SetManagingWorkersAfterFrame(bool managingWorkers) {
        StartCoroutine(SetManagingWorkersAfterFrameeCoroutine(managingWorkers));
    }
    private IEnumerator SetManagingWorkersAfterFrameeCoroutine(bool managingWorkers) {
        yield return new WaitForEndOfFrame();
        this.managingWorkers = managingWorkers;
    }
    public bool GetCanDropOrbOnTheFloor() {
        return canDropOrbOnTheFloor && !interactingWithMerchant && !interactingWithOtherObject && !hoveringWorker && !managingWorkers && !dead;
    }

    public bool GetCanInteractWithStructureLocation() {
        return !interactingWithOtherObject && !hoveringWorker && !managingWorkers;
    }

    public void Die() {
        SetCanDropOrbOnTheFloor(false);
        DisableControlInputs();

        bool isTutorial = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial;
        if (!isTutorial) {
            PlayerCurrencies.Instance.SetCarryingEmber(false);
        }

        OnPlayerDied?.Invoke(this, EventArgs.Empty);
        dead = true;

        if(Fire.Instance.GetCurrentFuelLevel() == 0 && !isTutorial) {
            // Fire hasn't been built yet
            LevelManager.Instance.LooseLevel();
        } else {
            StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator RespawnCoroutine() {

        yield return new WaitForSeconds(PlayerStats.Instance.GetRespawnTime() - 2f);
        // Move player = move camera

        Vector2 respawnPosition = new Vector2();

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {

            respawnPosition = Tutorial.Instance.GetRespawnPosition();
            RefillPlayerHealth();

        } else {

            respawnPosition = new Vector2(Tent.Instance.transform.position.x, transform.position.y);

        }

        transform.position = respawnPosition;

        yield return new WaitForSeconds(2f);

        OnPlayerBackToTentToRespawn?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2f);

        OnPlayerRespawned?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2.5f);

        float delayBetweenHeals = 1.5f / PlayerStats.Instance.GetPlayerRespawnHP();
        StartCoroutine(HealPlayerCoroutine(PlayerStats.Instance.GetPlayerRespawnHP(), delayBetweenHeals));

        yield return new WaitForSeconds(1.5f);

        EnableControlInputs();
        SetCanDropOrbOnTheFloor(true);
        dead = false;
    }

    private IEnumerator HealPlayerCoroutine(int healAmount, float delayBetweenHeals) {
        for (int  i = 1; i <= healAmount; i++) {
            HealPlayer(1);
            yield return new WaitForSeconds(delayBetweenHeals);
        }
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
        int healAmount = PlayerStats.Instance.GetMaxHP() - playerHealth;

        playerHealth = PlayerStats.Instance.GetMaxHP();
        OnPlayerHealed?.Invoke(this, new OnPlayerChangedHealthEventArgs {
            hpChangeAmount = healAmount
        });
    }

    public void HealPlayer(int healAmount) {
        if(playerHealth + healAmount > PlayerStats.Instance.GetMaxHP()) {
            healAmount = PlayerStats.Instance.GetMaxHP() - playerHealth;
        }

        if (healAmount == 0) return;

        playerHealth += healAmount;
        OnPlayerHealed?.Invoke(this, new OnPlayerChangedHealthEventArgs {
            hpChangeAmount = healAmount
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
