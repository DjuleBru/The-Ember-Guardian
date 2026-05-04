using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PlayerSkills : MonoBehaviour
{
    public static PlayerSkills Instance;

    [SerializeField] private PassiveShield passiveShield;
    [SerializeField] private Transform magmaShotPrefab;
    [SerializeField] private Transform fireDashPrefab;
    [SerializeField] private List<SkillSO> allSkillsSOList;

    private List<SkillItem> passiveSkillList = new List<SkillItem>();
    private List<SkillItem> activeSkillList = new List<SkillItem>();
    private SkillItem activeSkillLeft;
    private SkillItem activeSkillRight;

    private float rightSkillCooldownTimer;
    private float rightSkillCooldown;
    private float leftSkillCooldownTimer;
    private float leftSkillCooldown;

    private bool leftSkillReady;
    private bool leftSkillRunning;
    private bool rightSkillReady;
    private bool rightSkillRunning;

    private bool moveSpeedBuffActive;
    private float moveSpeedBuffAmount = 1.4f;
    private float moveSpeedBuffTimer;

    private bool shootCooldownBuffActive;
    private float shootCooldownBuffTimer;
    private float shootCooldownBuffValue;
    private float shootCooldownBuffDuration = 8f;

    private bool magmaShotBulletActive;
    private float magmaShotBulletTimer;
    private float magmaShotBulletSkillDuration;
    private int magmaShotBulletBurnDuration = 2;

    private bool healOnKillsActive;
    private float healOnKillsTimer;
    private float healOnKillsSkillDuration = 15f;
    private int healPipsPerKill = 1;
    private int pipsToHeal1Health = 5;

    private bool fuelFireOnKillsActive;
    private float fuelFireOnKillsTimer;
    private float fuelFireOnKillsSkillDuration = 15f;
    private int fuelPipsPerKill = 1;
    private int fuelPerPip = 1;

    private bool shootOnReload;
    private int shootOnReloadShotAmount;
    private bool meleeAttackMagmaShot;
    private bool lastBulletDealsMoreDamage;
    private float lastBulletDealsMoreDamageBuff;
    private int magmaShotDamage = 5;
    private int meleeAttackMagmaShotBurnDuration = 2;

    private bool fireDashRoll;
    private int fireDashRollDamage = 5;
    private int fireDashRollBurnDuration = 2;

    private int darkFlameDamage = 10;
    private int darkSwordDamage;
    private float darkSwordStunDuration = 1.5f;
    private int darkFlameBurnAmount;
    private int darkMinePoisonAmount;
    private int darkMineDamage = 30;
    private int reaperDamage = 50;

    private List<Fire> firesEntered = new List<Fire>();
    private bool enteredLight;
    private bool increasedDamageInFireLight;
    private bool damageInFireLightCurrentlyBuffed;
    private float damageBuffInFireLight;
    private bool increasedDamageOutFireLight;
    private bool damageOutFireLightCurrentlyBuffed;
    private float damageBuffOutFireLight;

    private bool workerAttackSpeedCurrentlyBuffed;
    private float workerAttackSpeedBuffDuration = 15f;
    private float workerAttackSpeedBuffTimer;
    private float workerAttackSpeedBuffAmount;

    private bool isPaused;

    private float leftSkillMinActiveTime = .7f;
    private float rightSkillMinActiveTime = .7f;

    private float leftSkillActiveTimer;
    private float rightSkillActiveTimer;

    public event EventHandler OnPlayerInFireLightBuffedDmg;
    public event EventHandler OnPlayerInFireLightDebuffedDmg;
    public event EventHandler OnPlayerOutFireLightBuffedDmg;
    public event EventHandler OnPlayerOutFireLightDebuffedDmg;
    public event EventHandler OnPlayerInFireLightBuffRefresh;
    public event EventHandler OnPlayerOutFireLightBuffRefresh;

    public event EventHandler<OnSkillAddedEventArgs> OnActiveSkillActivated;
    public event EventHandler<OnSkillDeactivatedArgs> OnActiveSkillDeactivated;
    public event EventHandler<OnSkillAddedEventArgs> OnActiveSkillAdded;
    public event EventHandler<OnSkillAddedEventArgs> OnActiveSkillRemoved;
    public event EventHandler<OnSkillAddedEventArgs> OnPassiveSkillAdded;
    public event EventHandler OnActiveSkillReady;
    public event EventHandler OnLeftActiveSkillActivated;
    public event EventHandler OnLeftActiveSkillDeactivated;
    public event EventHandler OnRightActiveSkillActivated;
    public event EventHandler OnRightActiveSkillDeactivated;
    public event EventHandler OnInitialSkillsInitialized;

    public class OnSkillAddedEventArgs : EventArgs {
        public SkillItem skillItemAdded;
        public bool triggerAddSFX;
    }
    public class OnSkillDeactivatedArgs : EventArgs {
        public SkillItem.SkillType skillTypeDeactivated;
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerLeftSkillPerformed += GameInput_OnPlayerLeftSkillPerformed;
        GameInput.Instance.OnPlayerRightSkillPerformed += GameInput_OnPlayerRightSkillPerformed;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;

        if (DayNightManager.Instance != null) {
            DayNightManager.Instance.OnCyclePausedByMerchantTalk += DayNightManager_OnCyclePausedByMerchantTalk;
            DayNightManager.Instance.OnCycleUnpaused += DayNightManager_OnCycleUnpaused;
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {

            if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;
            StartCoroutine(SetPlayerInitialSkills());

        }
    }

    private void DayNightManager_OnCycleUnpaused(object sender, EventArgs e) {
        isPaused = false;
    }

    private void DayNightManager_OnCyclePausedByMerchantTalk(object sender, EventArgs e) {
        isPaused = true;
    }

    private IEnumerator SetPlayerInitialSkills() {
        List<SkillSO> unlockedActiveSkillSOList = PlayerSave.Instance.GetActiveSkillsUnlocked();
        List<SkillSO> unlockedPassiveSkillSOList = PlayerSave.Instance.GetPassiveSkillsUnlocked();
        int initialActiveSkillLevel = PlayerStats.Instance.GetStartWithRandomActiveSkillLevel();
        int initialPassiveSkillLevel = PlayerStats.Instance.GetStartWithRandomPassiveSkillLevel();

        SkillSO randomActiveSkillSO = unlockedActiveSkillSOList[UnityEngine.Random.Range(0, unlockedActiveSkillSOList.Count)];
        SkillSO randomPassiveSkillSO = unlockedPassiveSkillSOList[UnityEngine.Random.Range(0, unlockedPassiveSkillSOList.Count)];

        yield return new WaitForSeconds(8.5f);

        if(initialActiveSkillLevel != 0) {
            SkillItem skillItem = new SkillItem();
            skillItem.Initialize(randomActiveSkillSO);
            skillItem.currentLevel = initialActiveSkillLevel;
            AddActiveSkill(skillItem);
        }

        yield return new WaitForSeconds(1f);

        if (initialPassiveSkillLevel != 0) {
            SkillItem skillItem = new SkillItem();
            skillItem.Initialize(randomPassiveSkillSO);
            skillItem.currentLevel = initialPassiveSkillLevel;
            AddPassiveSkill(skillItem, true, true);
        }


        yield return new WaitForSeconds(.5f);

        OnInitialSkillsInitialized?.Invoke(this, EventArgs.Empty);
    }

    public void SetPlayerSkillsInitialized() {
        StartCoroutine(SetPlayerSkillsInitializedAfterDelay(1f));
    }

    private IEnumerator SetPlayerSkillsInitializedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        OnInitialSkillsInitialized?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        if (isPaused) return;

        if (leftSkillRunning) {
            leftSkillActiveTimer += Time.deltaTime;
        }

        if (rightSkillRunning) {
            rightSkillActiveTimer += Time.deltaTime;
        }

        HandleActiveSkillsCooldowns();
        HandleActiveMoveSpeedBuff();
        HandleActiveShootCooldownBuff();
        HandleActiveMagmaShotBullet();
        HandleFuelOnKills();
        HandleHealOnKills();
        HandleWorkerAttackSpeedBuff();
    }

    private void GameInput_OnPlayerRightSkillPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammedAndNextInputSequence(GameInput.Binding.ability2)) return;

        if (activeSkillRight == null) return;

        if (rightSkillRunning) {

            if (rightSkillActiveTimer < rightSkillMinActiveTime) {
                return;
            }

            SkillItem.SkillType skillType = activeSkillRight.skillType;
            HandleActiveSkillDeactivation(skillType);
            return;
        }

        if (!rightSkillReady) return;

        ActivateActiveSkill(activeSkillRight, false);
        rightSkillReady = false;
    }

    private void GameInput_OnPlayerLeftSkillPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammedAndNextInputSequence(GameInput.Binding.ability1)) return;

        if (activeSkillLeft == null) return;

        if (leftSkillRunning) {

            if (leftSkillActiveTimer < leftSkillMinActiveTime) {
                return;
            }

            SkillItem.SkillType skillType = activeSkillLeft.skillType;
            HandleActiveSkillDeactivation(skillType);
            return;
        }

        if (!leftSkillReady) return;

        ActivateActiveSkill(activeSkillLeft, true);
        leftSkillReady = false;
    }

    #region ACTIVE SKILLS TIMERS
    private void HandleActiveSkillsCooldowns() {
        if (activeSkillLeft != null && !leftSkillReady && !leftSkillRunning) {
            leftSkillCooldownTimer -= Time.deltaTime;
            if (leftSkillCooldownTimer <= 0) {
                leftSkillReady = true;
                OnActiveSkillReady?.Invoke(this, EventArgs.Empty);
            }
        }

        if (activeSkillRight != null && !rightSkillReady && !rightSkillRunning) {

            rightSkillCooldownTimer -= Time.deltaTime;
            if (rightSkillCooldownTimer <= 0) {
                rightSkillReady = true;
                OnActiveSkillReady?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void HandleActiveMoveSpeedBuff() {
        if (!moveSpeedBuffActive) return;

        moveSpeedBuffTimer -= Time.deltaTime;

        if (moveSpeedBuffTimer <= 0) {
            HandleActiveSkillDeactivation(SkillItem.SkillType.activeMoveSpeedBuff);
        }
    }

    private void HandleActiveShootCooldownBuff() {
        if (!shootCooldownBuffActive) return;

        shootCooldownBuffTimer -= Time.deltaTime;

        if (shootCooldownBuffTimer <= 0) {
            HandleActiveSkillDeactivation(SkillItem.SkillType.activeShootSpeedBuff);
        }
    }
    private void HandleActiveMagmaShotBullet() {
        if (!magmaShotBulletActive) return;

        magmaShotBulletTimer -= Time.deltaTime;

        if (magmaShotBulletTimer <= 0) {
            HandleActiveSkillDeactivation(SkillItem.SkillType.activeMagmaShotBullet);
        }
    }
    private void HandleFuelOnKills() {
        if (!fuelFireOnKillsActive) return;

        fuelFireOnKillsTimer -= Time.deltaTime;

        if (fuelFireOnKillsTimer <= 0) {
            HandleActiveSkillDeactivation(SkillItem.SkillType.activeFeedFireOnKills);
        }
    }
    private void HandleHealOnKills() {
        if (!healOnKillsActive) return;

        healOnKillsTimer -= Time.deltaTime;

        if (healOnKillsTimer <= 0) {
            HandleActiveSkillDeactivation(SkillItem.SkillType.activeHealOnKills);
        }
    }
    private void HandleWorkerAttackSpeedBuff() {
        if (!workerAttackSpeedCurrentlyBuffed) return;

        workerAttackSpeedBuffTimer -= Time.deltaTime;

        if (workerAttackSpeedBuffTimer <= 0) {
            HandleActiveSkillDeactivation(SkillItem.SkillType.activeWorkerAttackSpeedBuff);
        }
    }
    private void HandleActiveSkillDeactivation(SkillItem.SkillType skillType) {

        if (activeSkillLeft != null && activeSkillLeft.skillType == skillType) {
            leftSkillRunning = false;
            leftSkillCooldownTimer = leftSkillCooldown;
            OnLeftActiveSkillDeactivated?.Invoke(this, EventArgs.Empty);
        }

        if (activeSkillRight != null && activeSkillRight.skillType == skillType) {
            rightSkillRunning = false;
            rightSkillCooldownTimer = rightSkillCooldown;
            OnRightActiveSkillDeactivated?.Invoke(this, EventArgs.Empty);
        }

        if (skillType == SkillItem.SkillType.activeMoveSpeedBuff) {
            PlayerMovement.Instance.DebuffMoveSpeed("activeMoveSpeedBuff", moveSpeedBuffAmount);
            moveSpeedBuffActive = false;
        }

        if (skillType == SkillItem.SkillType.activeShootSpeedBuff) {
            PlayerStats.Instance.DebuffShootCooldown(shootCooldownBuffValue);
            shootCooldownBuffActive = false;
        }

        if (skillType == SkillItem.SkillType.activeMagmaShotBullet) {
            magmaShotBulletActive = false;
        }

        if (skillType == SkillItem.SkillType.activeHealOnKills) {
            healOnKillsActive = false;
        }

        if (skillType == SkillItem.SkillType.activeFeedFireOnKills) {
            fuelFireOnKillsActive = false;
        }
        if (skillType == SkillItem.SkillType.activeWorkerAttackSpeedBuff) {
            workerAttackSpeedCurrentlyBuffed = false;
        }

        OnActiveSkillDeactivated?.Invoke(this, new OnSkillDeactivatedArgs {
            skillTypeDeactivated = skillType,
        });
    }

    #endregion

    #region SKILLS ADDING & ACTIVATION
    public void ActivateActiveSkill(SkillItem skillItem, bool isLeftSkill) {

        if (isLeftSkill) {
            leftSkillRunning = true;
            leftSkillActiveTimer = 0f;
            OnLeftActiveSkillActivated?.Invoke(this, EventArgs.Empty);
        }
        else {
            rightSkillRunning = true;
            rightSkillActiveTimer = 0f;
            OnRightActiveSkillActivated?.Invoke(this, EventArgs.Empty);
        }
        Debug.Log("skillItem.currentLevel " + skillItem.currentLevel);
        float skillBuffValue = skillItem.skillSO.activeSkillEffect.GetValueAtLevel(skillItem.currentLevel);

        switch (skillItem.skillType) {

            case SkillItem.SkillType.activeMoveSpeedBuff:

                PlayerMovement.Instance.BuffMoveSpeed("activeMoveSpeedBuff", moveSpeedBuffAmount);

                moveSpeedBuffTimer = skillBuffValue;
                moveSpeedBuffActive = true;

                break;

            case SkillItem.SkillType.activeTeleportation:

                HandleActiveSkillDeactivation(SkillItem.SkillType.activeTeleportation);

                break;

            case SkillItem.SkillType.activeShootSpeedBuff:

                shootCooldownBuffValue = 1 + skillBuffValue / 100;

                PlayerStats.Instance.BuffShootCooldown(shootCooldownBuffValue);
                shootCooldownBuffTimer = shootCooldownBuffDuration;
                shootCooldownBuffActive = true;

                break;

            case SkillItem.SkillType.activeMagmaShotBullet:

                magmaShotBulletSkillDuration = skillBuffValue;

                magmaShotBulletTimer = magmaShotBulletSkillDuration;
                magmaShotBulletActive = true;

                break;

            case SkillItem.SkillType.activeHealOnKills:
                Debug.Log("skillBuffValue " + skillBuffValue);
                healPipsPerKill = (int)skillBuffValue;

                healOnKillsTimer = healOnKillsSkillDuration;
                healOnKillsActive = true;

                break;

            case SkillItem.SkillType.activeFeedFireOnKills:

                fuelPipsPerKill = (int)skillBuffValue;

                fuelFireOnKillsTimer = fuelFireOnKillsSkillDuration;
                fuelFireOnKillsActive = true;

                break;

            case SkillItem.SkillType.activeDarkFlame:

                darkFlameBurnAmount = (int)skillBuffValue;
                HandleActiveSkillDeactivation(SkillItem.SkillType.activeDarkFlame);

                break;
            case SkillItem.SkillType.activeDarkSword:

                darkSwordDamage = (int)skillBuffValue;
                HandleActiveSkillDeactivation(SkillItem.SkillType.activeDarkSword);

                break;
            case SkillItem.SkillType.activeReaper:

                reaperDamage = (int)skillBuffValue;
                HandleActiveSkillDeactivation(SkillItem.SkillType.activeReaper);

                break;
            case SkillItem.SkillType.activePlantMine:

                darkMinePoisonAmount = (int)skillBuffValue;
                HandleActiveSkillDeactivation(SkillItem.SkillType.activePlantMine);

                break;

            case SkillItem.SkillType.activeWorkerAttackSpeedBuff:

                workerAttackSpeedCurrentlyBuffed = true;
                workerAttackSpeedBuffTimer = workerAttackSpeedBuffDuration;
                workerAttackSpeedBuffAmount = (int)skillBuffValue;

            break;
        }

        SetActiveSkillParameters(skillItem);
        OnActiveSkillActivated?.Invoke(this, new OnSkillAddedEventArgs {
            skillItemAdded = skillItem,
        });
    }

    public void AddActiveSkill(SkillItem skillItem, bool triggerSFX = true) {

        if(activeSkillLeft != null && activeSkillLeft.skillType == skillItem.skillType) {
            activeSkillLeft = skillItem;
            leftSkillCooldown = activeSkillLeft.skillSO.activeSkillEffect.GetCooldownAtLevel(skillItem.currentLevel);
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem,
                triggerAddSFX = triggerSFX,
            });
            leftSkillReady = true;
            SetActiveSkillParameters(skillItem);
            return;
        }

        if (activeSkillRight != null && activeSkillRight.skillType == skillItem.skillType) {
            activeSkillRight = skillItem;
            rightSkillCooldown = activeSkillRight.skillSO.activeSkillEffect.GetCooldownAtLevel(skillItem.currentLevel);
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem,
                triggerAddSFX = triggerSFX,
            });
            rightSkillReady = true;
            SetActiveSkillParameters(skillItem);
            return;
        }

        if (activeSkillLeft == null) {
            // Active skill left is null OR player is upgrading left skill

            activeSkillLeft = skillItem;
            leftSkillCooldown = activeSkillLeft.skillSO.activeSkillEffect.GetCooldownAtLevel(skillItem.currentLevel);
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem,
                triggerAddSFX = triggerSFX,
            });
            leftSkillReady = true;
            SetActiveSkillParameters(skillItem);
            return;
        }

        if (activeSkillRight == null) {
            // Active skill right is null OR player is upgrading right skill

            activeSkillRight = skillItem;
            rightSkillCooldown = activeSkillRight.skillSO.activeSkillEffect.GetCooldownAtLevel(skillItem.currentLevel);
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem,
                triggerAddSFX = triggerSFX,
            });
            rightSkillReady = true;
            SetActiveSkillParameters(skillItem);
            return;
        }
    }

    private void SetActiveSkillParameters(SkillItem skillItem) {
        if (skillItem.currentLevel > 1) {
            for (int i = 1; i <= skillItem.currentLevel; i++) {
                SkillItem temp = new SkillItem();
                temp.Initialize(skillItem.skillSO);
                temp.currentLevel = i;
                ApplyActiveSkillEffectInternal(temp);
            }
        }
        else {
            ApplyActiveSkillEffectInternal(skillItem);
        }
    }

    private void ApplyActiveSkillEffectInternal(SkillItem skillItem) {
        float skillBuffValue = skillItem.skillSO.activeSkillEffect.GetValueAtLevel(skillItem.currentLevel);

        switch (skillItem.skillType) {
            case SkillItem.SkillType.activeMoveSpeedBuff:
                moveSpeedBuffTimer = skillBuffValue;
                break;

            case SkillItem.SkillType.activeShootSpeedBuff:
                shootCooldownBuffValue = 1 + skillBuffValue / 100;
                break;

            case SkillItem.SkillType.activeMagmaShotBullet:
                magmaShotBulletSkillDuration = skillBuffValue;
                break;

            case SkillItem.SkillType.activeHealOnKills:
                healPipsPerKill = (int)skillBuffValue;
                break;

            case SkillItem.SkillType.activeFeedFireOnKills:
                fuelPipsPerKill = (int)skillBuffValue;
                break;

            case SkillItem.SkillType.activeDarkFlame:
                darkFlameBurnAmount = (int)skillBuffValue;
                break;

            case SkillItem.SkillType.activeDarkSword:
                darkSwordDamage = (int)skillBuffValue;
                break;

            case SkillItem.SkillType.activeReaper:
                reaperDamage = (int)skillBuffValue;
                break;

            case SkillItem.SkillType.activePlantMine:
                darkMinePoisonAmount = (int)skillBuffValue;
                break;

            case SkillItem.SkillType.activeWorkerAttackSpeedBuff:
                workerAttackSpeedBuffAmount = (int)skillBuffValue;
                break;
        }
    }

    public void AddPassiveSkill(SkillItem skillItem, bool addedFromLoad = false, bool triggerSFX = true) {

        SkillSO skillSO = skillItem.GetSkillSO();
        SkillItem skillItemCopy = new SkillItem();
        skillItemCopy.Initialize(skillSO);
        skillItemCopy.currentLevel = skillItem.currentLevel;

        SkillItem foundSkillItem = null;

        foreach (SkillItem passiveSkill in passiveSkillList) {
            if (passiveSkill.itemName == skillItemCopy.itemName) {
                foundSkillItem = passiveSkill;
            }
        }

        if (foundSkillItem != null) {
            foundSkillItem.currentLevel++;
        }
        else {
            passiveSkillList.Add(skillItemCopy);
        }

        OnPassiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
            skillItemAdded = skillItemCopy,
            triggerAddSFX = triggerSFX,
        });

        ApplyPassiveSkillEffect(skillItem, addedFromLoad);

    }

    public void ApplyPassiveSkillEffect(SkillItem skillItem, bool applyFromLoad) {
        SkillSO skillItemSO = skillItem.GetSkillSO();
        PassiveSkillEffectSO skillEffect = skillItemSO.passiveSkillEffect;

        if (skillEffect == null) {
            Debug.LogWarning($"Passive skill effect is null for {skillItem.itemName}");
            return;
        }

        // On applique les effets cumulés jusqu'au niveau actuel
        float totalBuffValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float previousLevelValue = (skillItem.currentLevel > 1) ? skillEffect.GetValueAtLevel(skillItem.currentLevel - 1) : 0f;
        float relativeBuffEffectValue = totalBuffValue - previousLevelValue;

        // Si le skill commence à un niveau supérieur à 1 (comme au lancement du joueur),
        // on veut appliquer tous les niveaux précédents aussi.

        if(applyFromLoad) {
            ApplyPassiveSkillEffectAbsolute(skillEffect, totalBuffValue);
        } else {
            ApplyPassiveSkillEffectInternal(skillEffect, relativeBuffEffectValue);
        }

    }
    private void ApplyPassiveSkillEffectAbsolute(PassiveSkillEffectSO skillEffect, float absoluteBuffEffectValue) {
        switch (skillEffect.skillType) {
            case SkillItem.SkillType.passiveMoveSpeedBuff:
                PlayerMovement.Instance.BuffMoveSpeed("passiveMoveSpeedBuff", 1 + absoluteBuffEffectValue / 100);
                break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                PlayerStats.Instance.BuffRunAccelerationFactor(absoluteBuffEffectValue / 100);
                break;

            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                PlayerStats.Instance.BuffStamina(absoluteBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveMaxHPIncrease:
                PlayerStats.Instance.BuffMaxHP((int)absoluteBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveHealthRegen:
                PlayerStats.Instance.SetPlayerHealthRegen((int)absoluteBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveAmmoGenerator:
                PlayerStats.Instance.SetPlayerAmmoRegen((int)absoluteBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveChanceToDoubleXPDrop:
                PlayerStats.Instance.BuffChanceToDropx2((int)absoluteBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveShieldGenerator:
                PlayerSkills.Instance.GetPassiveShield().SetShieldRegenTime(absoluteBuffEffectValue);
                PlayerSkills.Instance.GetPassiveShield().UnlockShield();
                break;

            case SkillItem.SkillType.passiveLastBulletDealsTwiceDamage:
                lastBulletDealsMoreDamage = true;
                lastBulletDealsMoreDamageBuff = 1 + absoluteBuffEffectValue / 100;
                break;

            case SkillItem.SkillType.passiveShootOnReload:
                shootOnReload = true;
                Debug.Log("relativeBuffEffectValue " + absoluteBuffEffectValue);
                shootOnReloadShotAmount += (int)absoluteBuffEffectValue;
                Debug.Log("shootOnReloadShotAmount " + shootOnReloadShotAmount);
                break;

            case SkillItem.SkillType.passiveMeleeAttackMagmaShot:
                meleeAttackMagmaShot = true;
                meleeAttackMagmaShotBurnDuration += (int)absoluteBuffEffectValue;
                break;

            case SkillItem.SkillType.passiveDashFireTrail:
                fireDashRoll = true;
                fireDashRollBurnDuration += (int)absoluteBuffEffectValue;
                break;

            case SkillItem.SkillType.passiveDmgIncreaseInLight:
                increasedDamageInFireLight = true;
                damageBuffInFireLight = 1 + absoluteBuffEffectValue / 100;
                if (enteredLight) {
                    OnPlayerInFireLightBuffedDmg?.Invoke(this, EventArgs.Empty);
                    damageInFireLightCurrentlyBuffed = true;
                }
                break;

            case SkillItem.SkillType.passiveDmgIncreaseNotInLight:
                increasedDamageOutFireLight = true;
                damageBuffOutFireLight = 1 + absoluteBuffEffectValue / 100;
                if (!enteredLight) {
                    OnPlayerOutFireLightBuffedDmg?.Invoke(this, EventArgs.Empty);
                    damageOutFireLightCurrentlyBuffed = true;
                }
                break;

            default:
                Debug.LogWarning($"Unhandled passive skill type: {skillEffect.skillType}");
                break;
        }
    }

    private void ApplyPassiveSkillEffectInternal(PassiveSkillEffectSO skillEffect, float relativeBuffEffectValue) {
        switch (skillEffect.skillType) {
            case SkillItem.SkillType.passiveMoveSpeedBuff:
                PlayerMovement.Instance.BuffMoveSpeed("passiveMoveSpeedBuff", 1 + relativeBuffEffectValue / 100);
                break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                PlayerStats.Instance.BuffRunAccelerationFactor(relativeBuffEffectValue / 100);
                break;

            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                PlayerStats.Instance.BuffStamina(relativeBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveMaxHPIncrease:
                PlayerStats.Instance.BuffMaxHP((int)relativeBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveHealthRegen:
                if (relativeBuffEffectValue > 0)
                    PlayerStats.Instance.SetPlayerHealthRegen((int)relativeBuffEffectValue);
                else
                    PlayerStats.Instance.BuffPlayerHealthRegen((int)relativeBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveAmmoGenerator:
                PlayerStats.Instance.BuffPlayerAmmoRegen((int)relativeBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveChanceToDoubleXPDrop:
                PlayerStats.Instance.BuffChanceToDropx2((int)relativeBuffEffectValue);
                break;

            case SkillItem.SkillType.passiveShieldGenerator:
                PlayerSkills.Instance.GetPassiveShield().SetShieldRegenTime(relativeBuffEffectValue);
                PlayerSkills.Instance.GetPassiveShield().UnlockShield();
                break;

            case SkillItem.SkillType.passiveLastBulletDealsTwiceDamage:
                lastBulletDealsMoreDamage = true;
                lastBulletDealsMoreDamageBuff = 1 + relativeBuffEffectValue / 100;
                break;

            case SkillItem.SkillType.passiveShootOnReload:
                shootOnReload = true;
                Debug.Log("relativeBuffEffectValue " + relativeBuffEffectValue);
                shootOnReloadShotAmount += (int)relativeBuffEffectValue;
                Debug.Log("shootOnReloadShotAmount " + shootOnReloadShotAmount);
                break;

            case SkillItem.SkillType.passiveMeleeAttackMagmaShot:
                meleeAttackMagmaShot = true;
                meleeAttackMagmaShotBurnDuration += (int)relativeBuffEffectValue;
                break;

            case SkillItem.SkillType.passiveDashFireTrail:
                fireDashRoll = true;
                fireDashRollBurnDuration += (int)relativeBuffEffectValue;
                break;

            case SkillItem.SkillType.passiveDmgIncreaseInLight:
                increasedDamageInFireLight = true;
                damageBuffInFireLight = 1 + relativeBuffEffectValue / 100;
                if (enteredLight) {
                    OnPlayerInFireLightBuffedDmg?.Invoke(this, EventArgs.Empty);
                    damageInFireLightCurrentlyBuffed = true;
                }
                break;

            case SkillItem.SkillType.passiveDmgIncreaseNotInLight:
                increasedDamageOutFireLight = true;
                damageBuffOutFireLight = 1 + relativeBuffEffectValue / 100;
                if (!enteredLight) {
                    OnPlayerOutFireLightBuffedDmg?.Invoke(this, EventArgs.Empty);
                    damageOutFireLightCurrentlyBuffed = true;
                }
                break;

            default:
                Debug.LogWarning($"Unhandled passive skill type: {skillEffect.skillType}");
                break;
        }
    }
    public void RemoveActiveSkill(SkillItem skillItem) {
        if(activeSkillLeft != null) {
            if (activeSkillLeft.skillType == skillItem.skillType) {
                // Removing left skill

                activeSkillLeft.currentLevel = 0;
                activeSkillLeft = null;
                OnActiveSkillRemoved?.Invoke(this, new OnSkillAddedEventArgs {
                    skillItemAdded = skillItem
                });
                return;

            }
        }
        
        if(activeSkillRight != null) {
            if (activeSkillRight.skillType == skillItem.skillType) {
                // Removing left skill

                activeSkillRight.currentLevel = 0;
                activeSkillRight = null;
                OnActiveSkillRemoved?.Invoke(this, new OnSkillAddedEventArgs {
                    skillItemAdded = skillItem
                });
                return;
            }
        }
    }
 
    #endregion

    #region GET SKILLS
    public SkillItem GetActiveSkillLeft() { return activeSkillLeft; }

    public SkillItem GetActiveSkillRight() { return activeSkillRight; }

    public List<SkillItem> GetActiveSkillList() {

        List<SkillItem> activeSkillList = new List<SkillItem>();

        if (activeSkillLeft != null) {
            activeSkillList.Add(activeSkillLeft);
        }

        if (activeSkillRight != null) {
            activeSkillList.Add(activeSkillRight);
        }

        return activeSkillList;
    }

    public List<SkillItem> GetPassiveSkillList() { return passiveSkillList; }

    #endregion

    #region GET SKILL PARAMETERS
    public PassiveShield GetPassiveShield() { return passiveShield; }

    public float GetShootSpeedBuffDuration() { return shootCooldownBuffDuration; }

    public int GetCurrentSkillLevel(SkillItem skillItem) {
        foreach (SkillItem playerSkillItem in passiveSkillList) {
            if (playerSkillItem.skillType == skillItem.skillType) {
                return playerSkillItem.currentLevel;
            }
        }

        if (activeSkillLeft != null) {
            if (skillItem.skillType == activeSkillLeft.skillType) {
                return activeSkillLeft.currentLevel;
            }
        }

        if (activeSkillRight != null) {
            if (skillItem.skillType == activeSkillRight.skillType) {
                return activeSkillRight.currentLevel;
            }
        }

        return 0;
    }

    public float GetLeftActiveTimerNormalized() {
        return (1 - leftSkillCooldownTimer / leftSkillCooldown);
    }

    public float GetRightActiveTimerNormalized() {
        return (1 - rightSkillCooldownTimer / rightSkillCooldown);
    }
    public int GetMagmaShotDamage() {
        return magmaShotDamage;
    }
    public int GetMeleeAttackMagmaShotBurnDuration() {
        return meleeAttackMagmaShotBurnDuration;
    }
    public int GetMagmaShotBulletBurnDuration() {
        return magmaShotBulletBurnDuration;
    }

    public int GetHealPipsPerKill() {
        return healPipsPerKill;
    }
    public int GetFuelPipsPerKill() {
        return fuelPipsPerKill;
    }

    public int GetFuelPerPip() {
        return fuelPerPip;
    }
    public int GetPipsToHeal1Health() {
        return pipsToHeal1Health;
    }
    public Transform GetMagmaShotPrefab() {
        return magmaShotPrefab;
    }

    public float GetDarkSwordStunDuration() {
        return darkSwordStunDuration;
    }
    #endregion

    #region GET BUFFS ACTIVE
    public bool GetLastBulletDealsMoreDamage() {
        return lastBulletDealsMoreDamage;
    }

    public float GetLastBulletDealsMoreDamageBuff() {
        return lastBulletDealsMoreDamageBuff;
    }
    public bool GetDamageInFireLightCurrentlyBuffed() {
        return damageInFireLightCurrentlyBuffed;
    }
    public bool GetDamageOutFireLightCurrentlyBuffed() {
        return damageOutFireLightCurrentlyBuffed;
    }
    public bool GetIncreasedDamageInFireLight() {
        return increasedDamageInFireLight;
    }
    public bool GetIncreasedDamageOutFireLight() {
        return increasedDamageOutFireLight;
    }
    public float GetDamageBuffInFireLight() {
        return damageBuffInFireLight;
    }
    public float GetDamageBuffOutFireLight() {
        return damageBuffOutFireLight;
    }
    public bool GetShootOnReload() {
        return shootOnReload;
    }

    public int GetShootOnReloadShotAmount() {
        return shootOnReloadShotAmount;
    }
    public bool GetMagmaBulletActive() {
        return magmaShotBulletActive;
    }
    public bool GetMeleeAttackMagmaShot() {
        return meleeAttackMagmaShot;
    }
    public bool GetFuelFireOnKills() {
        return fuelFireOnKillsActive;
    }

    public bool GetHealPlayerOnKills() {
        return healOnKillsActive;
    }

    public int GetDarkSwordDamage() {
        return darkSwordDamage;
    }
    public int GetDarkFlameDamage() {
        return darkFlameDamage;
    }
    public int GetDarkFlameBurnAmount() {
        return darkFlameBurnAmount;
    }
    public int GetDarkMinePoisonAmount() {
        return darkMinePoisonAmount;
    }
    public int GetDarkMineDamage() {
        return darkMineDamage;
    }
    public int GetReaperDamage() {
        return reaperDamage;
    }

    public float GetWorkerAttackSpeedBuffAmount() {
        return workerAttackSpeedBuffAmount;
    }

    #endregion

    #region FIRE DASHING
    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (!fireDashRoll) return;
        StartCoroutine(InstantiateFireDashAfterDelay(.1f));
    }

    private IEnumerator InstantiateFireDashAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        Vector3 positionToInstantiate = transform.position;
        positionToInstantiate.y = 0;
        StaticProjectile fireDash = Instantiate(fireDashPrefab, positionToInstantiate, Quaternion.identity).GetComponent<StaticProjectile>();
        fireDash.Initialize(PlayerAim.Instance.GetAimDirFloat(), null, fireDashRollDamage, true);
        fireDash.InitializeBurning(fireDashRollBurnDuration);
        fireDash.GetComponent<StaticProjectileSounds>().TriggerProjectileSFX();
    }
    #endregion

    #region FIRE LIGHT BUFFS & DEBUFFS
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Fire>() != null) return;
        if (collision.gameObject.GetComponent<FireOrbCollider>() != null) return;

        Fire fire = collision.gameObject.GetComponentInParent<Fire>();

        if (fire != null) {
            if(!firesEntered.Contains(fire)) {
                firesEntered.Add(fire);
            }

            enteredLight = true;

            if (!increasedDamageInFireLight && !increasedDamageOutFireLight) return;
            StartCoroutine(HandleBuffDebuffsFireLight(true, 0f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Fire>() != null) return;
        if (collision.gameObject.GetComponent<FireOrbCollider>() != null) return;

        Fire fire = collision.gameObject.GetComponentInParent<Fire>();
        if (fire != null) {
            if (firesEntered.Contains(fire)) {
                firesEntered.Remove(fire);
            }

            if(firesEntered.Count == 0) {
                enteredLight = false;
                if (!increasedDamageInFireLight && !increasedDamageOutFireLight) return;

                StartCoroutine(HandleBuffDebuffsFireLight(false, 0f));
            }

        }
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        StartCoroutine(HandleBuffDebuffsFireLight(enteredLight, 1f));
    }

    private IEnumerator HandleBuffDebuffsFireLight(bool entered, float delay) {
        yield return new WaitForSeconds(delay);

        if (entered) {
            if (increasedDamageOutFireLight) {
                OnPlayerOutFireLightDebuffedDmg?.Invoke(this, EventArgs.Empty);
                damageOutFireLightCurrentlyBuffed = false;
            }
            if (increasedDamageInFireLight) {
                OnPlayerInFireLightBuffedDmg?.Invoke(this, EventArgs.Empty);
                damageInFireLightCurrentlyBuffed = true;
            }
        }
        else {
            if (increasedDamageInFireLight) {
                OnPlayerInFireLightDebuffedDmg?.Invoke(this, EventArgs.Empty);
                damageInFireLightCurrentlyBuffed = false;
            }
            if (increasedDamageOutFireLight) {
                OnPlayerOutFireLightBuffedDmg?.Invoke(this, EventArgs.Empty);
                damageOutFireLightCurrentlyBuffed = true;
            }
        }
    }

    #endregion

    #region Skill UI Descriptions

    public List<string> GetActiveSkillStatDescriptionList(SkillItem skillItem) {
        List<string> skillStatDescriptionList = new List<string>();

        switch (skillItem.skillType) {
            case SkillItem.SkillType.activeDarkFlame:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_damage"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Burn Duration"));
            break;

            case SkillItem.SkillType.activeDarkSword:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_damage"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Stun Duration"));
                break;

            case SkillItem.SkillType.activeFeedFireOnKills:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Duration"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Fire Pips/Kill"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Fuel/Fire Pip"));
                break;

            case SkillItem.SkillType.activeHealOnKills:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Duration"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Heal Pips/Kill"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Heal Pips To Heal"));
                break;

            case SkillItem.SkillType.activeMagmaShotBullet:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Duration"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_damage"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Burn Duration"));
                break;

            case SkillItem.SkillType.activeMoveSpeedBuff:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Duration"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Movement Speed"));
                break;

            case SkillItem.SkillType.activeShootSpeedBuff:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Duration"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_shotCooldown"));
            break;

            case SkillItem.SkillType.activePlantMine:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_damage"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Poison Damage"));
                break;

            case SkillItem.SkillType.activeReaper:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_damage"));
                break;


            case SkillItem.SkillType.activeTeleportation:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Warp Distance"));
                break;

            case SkillItem.SkillType.activeWorkerAttackSpeedBuff:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Duration"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Worker Attack Speed"));
                break;

        }

        skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("card_cooldown"));

        return skillStatDescriptionList;

    }

    public List<string> GetActiveSkillStatList(SkillItem skillItem, bool calledFromHub = false) {
        List<string> skillStatList = new List<string>();
        ActiveSkillEffectSO skillEffectSO = skillItem.skillSO.activeSkillEffect;
        int skillLevel = GetCurrentSkillLevel(skillItem);

        if(calledFromHub) {
            skillLevel = 1;
        }

        switch (skillItem.skillType) {

            case SkillItem.SkillType.activeDarkFlame:

                skillStatList.Add(darkFlameDamage.ToString());
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");

            break;

            case SkillItem.SkillType.activeDarkSword:

                skillStatList.Add(darkSwordDamage.ToString());
                skillStatList.Add(darkSwordStunDuration.ToString() + "s");

            break;

            case SkillItem.SkillType.activeFeedFireOnKills:

                skillStatList.Add(fuelFireOnKillsSkillDuration.ToString() + "s");
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());
                skillStatList.Add(fuelPerPip.ToString());

                break;

            case SkillItem.SkillType.activeHealOnKills:

                skillStatList.Add(healOnKillsSkillDuration.ToString() + "s");
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());
                skillStatList.Add(pipsToHeal1Health.ToString());

                break;

            case SkillItem.SkillType.activeMagmaShotBullet:

                skillStatList.Add(magmaShotBulletSkillDuration.ToString() + "s");
                skillStatList.Add(magmaShotDamage.ToString());
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());

             break;

            case SkillItem.SkillType.activeMoveSpeedBuff:

                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");
                skillStatList.Add("+" + ((moveSpeedBuffAmount-1)*100).ToString() + "%");

            break;

            case SkillItem.SkillType.activeShootSpeedBuff:

                skillStatList.Add(shootCooldownBuffDuration.ToString() + "s");
                skillStatList.Add("-" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");

            break;

            case SkillItem.SkillType.activePlantMine:

                skillStatList.Add(darkMineDamage.ToString());
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");

            break;

            case SkillItem.SkillType.activeReaper:

                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());

            break;

            case SkillItem.SkillType.activeTeleportation:
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());
            break;

            case SkillItem.SkillType.activeWorkerAttackSpeedBuff:
                skillStatList.Add(workerAttackSpeedBuffDuration.ToString() + "s");
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;
        }

        skillStatList.Add(skillEffectSO.GetCooldownAtLevel(skillLevel).ToString() + "s");

        return skillStatList;
    }

    public List<string> GetPassiveSkillStatDescriptionList(SkillItem skillItem) {
        List<string> skillStatDescriptionList = new List<string>();

        switch (skillItem.skillType) {

            case SkillItem.SkillType.passiveAmmoGenerator:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Ammo Generation Rate"));
            break;

            case SkillItem.SkillType.passiveChanceToDoubleXPDrop:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Creatures x2 Drop Chance"));
            break;

            case SkillItem.SkillType.passiveDashFireTrail:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Damage"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Burn Duration"));
            break;

            case SkillItem.SkillType.passiveDmgIncreaseInLight:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Damage Increase In Light"));
            break;

            case SkillItem.SkillType.passiveDmgIncreaseNotInLight:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Damage Increase Out of Light"));
            break;

            case SkillItem.SkillType.passiveHealthRegen:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Health Regeneration Rate"));
            break;

            case SkillItem.SkillType.passiveLastBulletDealsTwiceDamage:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Last Bullet Damage Buff"));
            break;

            case SkillItem.SkillType.passiveMaxHPIncrease:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Maximum Health"));
            break;

            case SkillItem.SkillType.passiveMeleeAttackMagmaShot:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Skill Damage"));
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Burn Duration"));
                break;

            case SkillItem.SkillType.passiveMoveSpeedBuff:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Movement Speed"));
            break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Sprint Speed"));
            break;

            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Sprint Duration"));
            break;

            case SkillItem.SkillType.passiveShieldGenerator:
                skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Shield Regen Time"));
            break;

            case SkillItem.SkillType.passiveShootOnReload:
                if(skillItem.currentLevel == 1) {
                    skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Free shot"));
                } else {
                    skillStatDescriptionList.Add(LocalizationManager.Instance.GetLocalizedText("Free shot 2+"));
                }


            break;
        }

        return skillStatDescriptionList;

    }
    public List<string> GetPassiveSkillStatList(SkillItem skillItem, bool calledFromHub = false) {
        List<string> skillStatList = new List<string>();
        PassiveSkillEffectSO skillEffectSO = skillItem.skillSO.passiveSkillEffect;
        int skillLevel = GetCurrentSkillLevel(skillItem);

        if(calledFromHub) {
            skillLevel = 1;
        }

        switch (skillItem.skillType) {

            case SkillItem.SkillType.passiveAmmoGenerator:
                skillStatList.Add("1/" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");
            break;
            
            case SkillItem.SkillType.passiveChanceToDoubleXPDrop:
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;

            case SkillItem.SkillType.passiveDashFireTrail:
                skillStatList.Add(fireDashRollDamage.ToString());
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");
                break;

            case SkillItem.SkillType.passiveDmgIncreaseInLight:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;

            case SkillItem.SkillType.passiveDmgIncreaseNotInLight:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;

            case SkillItem.SkillType.passiveHealthRegen:
                skillStatList.Add("1/" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");
            break;

            case SkillItem.SkillType.passiveLastBulletDealsTwiceDamage:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;

            case SkillItem.SkillType.passiveMaxHPIncrease:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString());
            break;

            case SkillItem.SkillType.passiveMeleeAttackMagmaShot:
                skillStatList.Add(magmaShotDamage.ToString());
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());
            break;

            case SkillItem.SkillType.passiveMoveSpeedBuff:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;

            case SkillItem.SkillType.passiveRunAccelerationFactorBuff:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "%");
            break;

            case SkillItem.SkillType.passiveRunMaxTimeBuff:
                skillStatList.Add("+" + skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");
            break;

            case SkillItem.SkillType.passiveShieldGenerator:
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString() + "s");
            break;

            case SkillItem.SkillType.passiveShootOnReload:
                skillStatList.Add(skillEffectSO.GetValueAtLevel(skillLevel).ToString());
            break;
        }

        return skillStatList;
    }
    #endregion

    [Button] 
    private void AddActiveSkillDebug(SkillSO skillSO, int level) {
        SkillItem skillItem = new SkillItem();
        skillItem.Initialize(skillSO);
        skillItem.currentLevel = level;
        AddActiveSkill(skillItem);
    }
    [Button]
    private void AddPassiveSkillDebug(SkillSO skillSO, int level) {
        SkillItem skillItem = new SkillItem();
        skillItem.Initialize(skillSO);
        skillItem.currentLevel = level;
        AddPassiveSkill(skillItem);
    }

    public SkillSO GetSkillSO(SkillItem.SkillType skillType) {
        foreach(SkillSO skillSO in allSkillsSOList) {
            if(skillSO.skillType == skillType) return skillSO;
        }

        return allSkillsSOList[0];
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerLeftSkillPerformed -= GameInput_OnPlayerLeftSkillPerformed;
        GameInput.Instance.OnPlayerRightSkillPerformed -= GameInput_OnPlayerRightSkillPerformed;
        PlayerShoot.Instance.OnPlayerSwappedGun -= PlayerShoot_OnPlayerSwappedGun;
        PlayerMovement.Instance.OnPlayerRoll -= PlayerMovement_OnPlayerRoll;

        if (DayNightManager.Instance != null) {
            DayNightManager.Instance.OnCyclePausedByMerchantTalk -= DayNightManager_OnCyclePausedByMerchantTalk;
            DayNightManager.Instance.OnCycleUnpaused -= DayNightManager_OnCycleUnpaused;
        }
    }
}
