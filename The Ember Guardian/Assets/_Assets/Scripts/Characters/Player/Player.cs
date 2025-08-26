using QFSW.QC;
using Sirenix.OdinInspector;
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
    [SerializeField] private CapsuleCollider2D playerCollider;

    private Rigidbody2D rb;
    private bool dead;
    private bool damagedRecently;
    private bool insideCamp;
    private bool exploring;
    private bool hasHPRegen;


    private bool inPayCurrencyTriggerArea = false;
    private bool inMerchantTriggerArea = false;
    private bool inOtherInteractableObjectTriggerArea = false;
    private bool inPetDogTriggerArea = false;
    private bool inCurrencyStorageArea = false;

    private bool carryingOtherObject = false;
    private bool cancellingHoveringWorker = false;
    private bool managingWorkers = false;
    private bool interactingWithMerchant = false;
    private bool inTeleporter = false;
    private bool inTeleporterLevelSelectionMenu = false;
    private bool cameraHasOtherTarget = false;
    private bool pettingDog = false;

    private bool tabMenuOpen = false;
    private bool pauseMenuOpen = false;
    private bool videoTipMenuOpen = false;


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
    public event EventHandler OnPlayerStartedExploring;
    public event EventHandler OnPlayerStoppedExploring;
    public event EventHandler<OnPlayerChangedHealthEventArgs> OnPlayerDamaged;
    public event EventHandler OnPlayerDamagedRecentlyEnded;
    public event EventHandler OnPlayerBlinded;
    public event EventHandler<OnPlayerChangedHealthEventArgs> OnPlayerHealed;
    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerRespawned;
    public event EventHandler OnPlayerBackToTentToRespawn;
    public event EventHandler OnPlayerEnteredAnyInteractableTriggerArea;
    public event EventHandler OnPlayerExitedAnyInteractableTriggerArea;
    public event EventHandler OnPlayerStartedInteractingWithAnyInteractable;
    public event EventHandler OnPlayerStoppedInteractingWithAnyInteractable;

    public class OnPlayerChangedHealthEventArgs : EventArgs {
        public int hpChangeAmount;
    }

    private bool isLevelScene;

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        playerHealth = PlayerStats.Instance.GetMaxHP();

        Time.timeScale = 1.0f;
    }

    private void Start() {
        isLevelScene = (SceneLoader.Instance != null && SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB);

        PlayerStats.Instance.OnPlayerMaxHPChanged += PlayerStats_OnPlayerMaxHPChanged;
        PlayerStats.Instance.OnPlayerHPRegenChanged += PlayerStats_OnPlayerHPRegenChanged;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;

        VideoTipUI.Instance.OnVideoTipPanelOpened += VideoTipUI_OnVideoTipPanelOpened;
        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
        PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;


        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
            PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        }

        if(isLevelScene) {
            LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;
        }

        hpRegenTime = PlayerStats.Instance.GetHpRegenTime();
        if(hpRegenTime != 0) {
            hasHPRegen = true;
        }
    }


    private void Update() {
        if (!isLevelScene) return;
        CheckExitingCamp();
        CheckLeavingForExploration();

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
    private void CheckLeavingForExploration() {
        if (!exploring) {

            if (transform.position.x < (CampZoneManager.Instance.GetCampCenterMinLimit() - 20f) || (transform.position.x > CampZoneManager.Instance.GetCampCenterMaxLimit()+15f)) {
                exploring = true;
                OnPlayerStartedExploring?.Invoke(this, EventArgs.Empty);
            }

        }
        else {

            if (transform.position.x > (CampZoneManager.Instance.GetCampCenterMinLimit() - 20f) && transform.position.x < (CampZoneManager.Instance.GetCampCenterMaxLimit() + 15f)) {
                exploring = false;
                OnPlayerStoppedExploring?.Invoke(this, EventArgs.Empty);
            }

        }
    }

    public void AddKnockBack(Vector2 knockbackDir) {
        rb.AddForce(knockbackDir, ForceMode2D.Impulse);
    }

    
    public void TakeDamage(int damage, Transform damageSource, bool critHit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false) {

        if (damagedRecently && !ignoreTemporaryInvincibility) return;
        if (dead) return;
        if (isInvincibleWhileRolling) return;
        if (inTeleporter) return;

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

    [Button]
    public void TakeDamageButton(int damage) {
        int healthLoss = damage;
        playerHealth -= damage;

        if (playerHealth <= 0) {
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
    private void PlayerMovement_OnPlayerCrouchedEnded(object sender, EventArgs e) {
        playerCollider.offset = new Vector2(-0.06608671f, 0.6866874f);
        playerCollider.size = new Vector2(0.4201719f, 1.366405f);
    }

    private void PlayerMovement_OnPlayerCrouched(object sender, EventArgs e) {
        playerCollider.offset = new Vector2(-0.06608671f, 0.5149697f);
        playerCollider.size = new Vector2(0.4201719f, 1.022969f);
    }

    private IEnumerator RollCoroutine() {
        yield return new WaitForSeconds(delayAfterRollStartForInvincibleStart);
        isInvincibleWhileRolling = true;
        yield return new WaitForSeconds(delayAfterRollStartForInvincibleEnd);
        isInvincibleWhileRolling = false;

    }

    #region PLAYER SET CONTROLS RESTRICTIONS

    public void SetCarryinhOtherObject(bool carryingOtherObject) {
        this.carryingOtherObject = carryingOtherObject;
    }

    public void SetManagingWorkers(bool managingWorkers) {
        this.managingWorkers = managingWorkers;

        if(managingWorkers) {
            OnPlayerStartedInteractingWithAnyInteractable?.Invoke(this, EventArgs.Empty);
        } else {
            OnPlayerStoppedInteractingWithAnyInteractable?.Invoke(this, EventArgs.Empty);
        }
    }
    public void SetManagingWorkersAfterFrame(bool managingWorkers) {
        StartCoroutine(SetManagingWorkersAfterFrameeCoroutine(managingWorkers));
    }
    private IEnumerator SetManagingWorkersAfterFrameeCoroutine(bool managingWorkers) {
        yield return new WaitForEndOfFrame();
        this.managingWorkers = managingWorkers;
    }

    public void SetInPayCurrencyArea(bool inPayCurrencyArea) {
        inPayCurrencyTriggerArea = inPayCurrencyArea;

        if(inPayCurrencyArea) {
            OnPlayerEnteredAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        } else {
            OnPlayerExitedAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetInMerchantTriggerArea(bool inMerchantArea) {
        inMerchantTriggerArea = inMerchantArea;

        if (inMerchantArea) {
            OnPlayerEnteredAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnPlayerExitedAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetInOtherInteractableObjectTriggerArea(bool inOtherInteractableObjectArea) {
        inOtherInteractableObjectTriggerArea = inOtherInteractableObjectArea;

        if (inOtherInteractableObjectArea) {
            OnPlayerEnteredAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnPlayerExitedAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetInPetDogTriggerArea(bool inPetDogTriggerArea) {
        this.inPetDogTriggerArea = inPetDogTriggerArea;

        if (inPetDogTriggerArea) {
            OnPlayerEnteredAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnPlayerExitedAnyInteractableTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetPettingDog(bool pettingDog) {
        this.pettingDog = pettingDog;
    }

    public void SetCameraHasOtherTarget(bool cameraHasOtherTarget) {
        this.cameraHasOtherTarget = cameraHasOtherTarget;
    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, EventArgs e) {
        pauseMenuOpen = true;
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, EventArgs e) {
        pauseMenuOpen = false;
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, EventArgs e) {
        tabMenuOpen = false;
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, EventArgs e) {
        tabMenuOpen = true;
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        videoTipMenuOpen = false;
    }

    private void VideoTipUI_OnVideoTipPanelOpened(object sender, EventArgs e) {
        videoTipMenuOpen = true;
    }
    #endregion

    #region GET PLAYER CONTROLS RESTRICTIONS
    public bool GetAllMenusClosed() {
        //Debug.Log("inTeleporterLevelSelectionMenu " + inTeleporterLevelSelectionMenu);
        return !pauseMenuOpen && !tabMenuOpen && !videoTipMenuOpen && !inTeleporterLevelSelectionMenu;
    }

    public bool GetInteractingWithNoOtherObject() {
        return !interactingWithMerchant && !managingWorkers && !inTeleporter;
    }

    public bool GetInNoOtherObjectTriggerArea() {
        return !inPayCurrencyTriggerArea && !inMerchantTriggerArea && !inOtherInteractableObjectTriggerArea;
    }

    public bool GetCanPetDog() {
        return GetAllMenusClosed() && GetInteractingWithNoOtherObject() && !dead && !cameraHasOtherTarget && !carryingOtherObject && !inPayCurrencyTriggerArea && !inMerchantTriggerArea && !inOtherInteractableObjectTriggerArea;
    }

    public bool GetCanDropOrbOnTheFloor() {
        return GetAllMenusClosed() && GetInteractingWithNoOtherObject() && GetInNoOtherObjectTriggerArea() && !dead && !cameraHasOtherTarget && !carryingOtherObject;
    }

    public bool GetPlayerControlInputsEnabled() {
        // Move, aim, shoot
        return GetAllMenusClosed() && GetInteractingWithNoOtherObject() && !dead && !cameraHasOtherTarget && !cameraHasOtherTarget && !pettingDog;
    }

    public bool GetCanInteractWithStructureLocation() {
        return GetAllMenusClosed() && GetPlayerControlInputsEnabled() && !managingWorkers;
    }

    public void Die() {
        bool isTutorial = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial;
        if (!isTutorial) {
            PlayerCurrencies.Instance.SetCarryingEmber(false);
        }

        OnPlayerDied?.Invoke(this, EventArgs.Empty);
        dead = true;

        if(!Fire.Instance.GetInitialFireLit() && !isTutorial) {
            // Fire hasn't been built yet
            LevelManager.Instance.LooseLevel();
        } else {
            StartCoroutine(RespawnCoroutine());
        }
    }

    private void LevelManager_OnLevelFailed(object sender, EventArgs e) {
        DieWithMainFireExtinguished();
    }

    public void DieWithMainFireExtinguished() {
        OnPlayerDied?.Invoke(this, EventArgs.Empty);
        dead = true;
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
        dead = false;
    }

    private IEnumerator HealPlayerCoroutine(int healAmount, float delayBetweenHeals) {
        for (int  i = 1; i <= healAmount; i++) {
            HealPlayer(1);
            yield return new WaitForSeconds(delayBetweenHeals);
        }
    }

    public void MoveOnTeleporter(Transform teleporterPlayerPosition) {
        inTeleporter = true;

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = teleporterPlayerPosition.position;

        OnPlayerStartedInteractingWithAnyInteractable?.Invoke(this, EventArgs.Empty);
    }

    public void ReleasePlayerFromTeleporter() {
        inTeleporter = false;

        OnPlayerStoppedInteractingWithAnyInteractable?.Invoke(this, EventArgs.Empty);
    }

    public void StartInteractingWithMerchant() {
        interactingWithMerchant = true;
        OnPlayerStartedInteractingWithAnyInteractable?.Invoke(this, EventArgs.Empty);
    }
    
    public void StopInteractingWithMerchant() {
        // Set interactingWithMerchant false after frame or dog will react

        StartCoroutine(SetStopInteractingWithMerchantCoroutine());
    }

    private IEnumerator SetStopInteractingWithMerchantCoroutine() {
        yield return new WaitForSeconds(.5f);
        interactingWithMerchant = false;
        OnPlayerStoppedInteractingWithAnyInteractable?.Invoke(this, EventArgs.Empty);
    }

    public void SetInTeleporterLevelSelectionMenu(bool inMenu) {
        inTeleporterLevelSelectionMenu = inMenu;
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

    [Button]
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

    public bool GetInteractingWithMerchant() {
        return interactingWithMerchant;
    }

    public bool GetInCurrencyStorageArea() {
        return inCurrencyStorageArea;
    }
    public void SetInCurrencyStorageArea(bool inArea) {
        this.inCurrencyStorageArea = inArea;
    }
    
    public void BlindPlayer() {
        OnPlayerBlinded?.Invoke(this, EventArgs.Empty);
    }
    
    [Button] 
    public void KillPlayer() {
        Die();
    }

    public void OnDestroy() {
        PlayerStats.Instance.OnPlayerMaxHPChanged -= PlayerStats_OnPlayerMaxHPChanged;
        PlayerStats.Instance.OnPlayerHPRegenChanged -= PlayerStats_OnPlayerHPRegenChanged;
        PlayerMovement.Instance.OnPlayerRoll -= PlayerMovement_OnPlayerRoll;
        VideoTipUI.Instance.OnVideoTipPanelOpened -= VideoTipUI_OnVideoTipPanelOpened;
        VideoTipUI.Instance.OnVideoTipPanelClosed -= VideoTipUI_OnVideoTipPanelClosed;

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerTabMenuUI.Instance.OnPlayerTabClosed -= PlayerTabMenuUI_OnPlayerTabClosed;
            PlayerTabMenuUI.Instance.OnPlayerTabOpened -= PlayerTabMenuUI_OnPlayerTabOpened;
        }

    }
}
