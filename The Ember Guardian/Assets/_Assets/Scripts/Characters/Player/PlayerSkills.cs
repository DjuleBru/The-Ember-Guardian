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

    private List<SkillItem> passiveSkillList = new List<SkillItem>();
    private List<SkillItem> activeSkillList = new List<SkillItem>();
    private SkillItem activeSkillLeft;
    private SkillItem activeSkillRight;

    private float rightSkillCooldownTimer;
    private float rightSkillCooldown;
    private float leftSkillCooldownTimer;
    private float leftSkillCooldown;

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
    private int magmaShotBulletBurnAmount = 2;

    private bool healOnKillsActive;
    private float healOnKillsTimer;
    private float healOnKillsSkillDuration = 15f;
    private int healPipsPerKill = 1;

    private bool fuelFireOnKillsActive;
    private float fuelFireOnKillsTimer;
    private float fuelFireOnKillsSkillDuration = 15f;
    private int fuelPipsPerKill = 1;

    private bool leftSkillReady;
    private bool leftSkillRunning;
    private bool rightSkillReady;
    private bool rightSkillRunning;

    private bool shootOnReload;
    private bool meleeAttackMagmaShot;
    private bool lastBulletDealsMoreDamage;
    private float lastBulletDealsMoreDamageBuff;
    private int magmaShotDamage = 5;
    private int meleeAttackMagmaShotBurnAmount = 2;

    private bool fireDashRoll;
    private int fireDashRollDamage = 5;
    private int fireDashRollBurnAmount = 2;

    private bool enteredLight;
    private bool increasedDamageInFireLight;
    private bool damageInFireLightCurrentlyBuffed;
    private float damageBuffInFireLight;
    private bool increasedDamageOutFireLight;
    private bool damageOutFireLightCurrentlyBuffed;
    private float damageBuffOutFireLight;

    public event EventHandler OnPlayerInFireLightBuffedDmg;
    public event EventHandler OnPlayerInFireLightDebuffedDmg;
    public event EventHandler OnPlayerOutFireLightBuffedDmg;
    public event EventHandler OnPlayerOutFireLightDebuffedDmg;
    public event EventHandler OnPlayerInFireLightBuffRefresh;
    public event EventHandler OnPlayerOutFireLightBuffRefresh;

    public event EventHandler<OnSkillAddedEventArgs> OnActiveSkillActivated;
    public event EventHandler<OnSkillDeactivatedArgs> OnActiveSkillDeactivated;
    public event EventHandler<OnSkillAddedEventArgs> OnActiveSkillAdded;
    public event EventHandler<OnSkillAddedEventArgs> OnPassiveSkillAdded;
    public event EventHandler OnActiveSkillReady;
    public event EventHandler OnLeftActiveSkillActivated;
    public event EventHandler OnLeftActiveSkillDeactivated;
    public event EventHandler OnRightActiveSkillActivated;
    public event EventHandler OnRightActiveSkillDeactivated;
    public event EventHandler OnInitialSkillsInitialized;

    public class OnSkillAddedEventArgs : EventArgs {
        public SkillItem skillItemAdded;
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

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            StartCoroutine(SetPlayerInitialSkills());
        }
    }

    private IEnumerator SetPlayerInitialSkills() {
        List<SkillSO> unlockedActiveSkillSOList = PlayerSave.Instance.GetActiveSkillsUnlocked();
        List<SkillSO> unlockedPassiveSkillSOList = PlayerSave.Instance.GetPassiveSkillsUnlocked();
        int initialActiveSkillLevel = PlayerStats.Instance.GetStartWithRandomActiveSkillLevel();
        int initialPassiveSkillLevel = PlayerStats.Instance.GetStartWithRandomPassiveSkillLevel();

        SkillSO randomActiveSkillSO = unlockedActiveSkillSOList[UnityEngine.Random.Range(0, unlockedActiveSkillSOList.Count)];
        SkillSO randomPassiveSkillSO = unlockedPassiveSkillSOList[UnityEngine.Random.Range(0, unlockedPassiveSkillSOList.Count)];

        yield return new WaitForSeconds(3.5f);

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
            AddPassiveSkill(skillItem);
        }


        yield return new WaitForSeconds(.5f);

        OnInitialSkillsInitialized?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        HandleActiveSkillsCooldowns();
        HandleActiveMoveSpeedBuff();
        HandleActiveShootCooldownBuff();
        HandleActiveMagmaShotBullet();
        HandleFuelOnKills();
        HandleHealOnKills();
    }

    private void GameInput_OnPlayerRightSkillPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        if (activeSkillRight == null) return;

        if (rightSkillRunning) {
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

        if (activeSkillLeft == null) return;

        if (leftSkillRunning) {
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
            PlayerMovement.Instance.DebuffMoveSpeed(moveSpeedBuffAmount);
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

        OnActiveSkillDeactivated?.Invoke(this, new OnSkillDeactivatedArgs {
            skillTypeDeactivated = skillType,
        });
    }

    #endregion

    #region SKILLS ADDING & ACTIVATION
    public void ActivateActiveSkill(SkillItem skillItem, bool isLeftSkill) {
        if (isLeftSkill) {
            leftSkillRunning = true;
            OnLeftActiveSkillActivated?.Invoke(this, EventArgs.Empty);
        }
        else {
            rightSkillRunning = true;
            OnRightActiveSkillActivated?.Invoke(this, EventArgs.Empty);
        }
        Debug.Log("skillItem.currentLevel " + skillItem.currentLevel);
        float skillBuffValue = skillItem.skillSO.activeSkillEffect.GetValueAtLevel(skillItem.currentLevel);
        switch (skillItem.skillType) {

            case SkillItem.SkillType.activeMoveSpeedBuff:

                PlayerMovement.Instance.BuffMoveSpeed(moveSpeedBuffAmount);

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

        }

        OnActiveSkillActivated?.Invoke(this, new OnSkillAddedEventArgs {
            skillItemAdded = skillItem,
        });
    }

    public void AddActiveSkill(SkillItem skillItem) {

        if (activeSkillLeft != null && activeSkillRight != null) return;

        if (activeSkillLeft == null || activeSkillLeft.skillType == skillItem.skillType) {
            // Active skill left is null OR player is upgrading left skill

            activeSkillLeft = skillItem;
            leftSkillCooldown = activeSkillLeft.skillSO.activeSkillEffect.GetCooldownAtLevel(skillItem.currentLevel);
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem
            });
            return;
        }

        if (activeSkillRight == null || activeSkillRight.skillType == skillItem.skillType) {
            // Active skill right is null OR player is upgrading right skill

            activeSkillRight = skillItem;
            rightSkillCooldown = activeSkillLeft.skillSO.activeSkillEffect.GetCooldownAtLevel(skillItem.currentLevel);
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem
            });
            return;
        }
    }

    public void AddPassiveSkill(SkillItem skillItem) {

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
            skillItemAdded = skillItemCopy
        });

        ApplyPassiveSkillEffect(skillItem);
    }

    public void ApplyPassiveSkillEffect(SkillItem skillItem) {
        Debug.Log("ApplyPassiveSkillEffect " + skillItem.skillType);
        SkillSO skillItemSO = skillItem.GetSkillSO();
        Debug.Log("skillItemSO " + skillItem.GetSkillSO());
        PassiveSkillEffectSO skillEffect = skillItemSO.passiveSkillEffect;

        float absoluteBuffEffectValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);
        float relativeBuffEffectValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);

        if (skillItem.currentLevel > 1) {
            relativeBuffEffectValue -= skillEffect.GetValueAtLevel(skillItem.currentLevel - 1);
        }

        if (skillEffect != null) {
            switch (skillEffect.skillType) {
                case SkillItem.SkillType.passiveMoveSpeedBuff:

                    PlayerStats.Instance.BuffMoveSpeed(relativeBuffEffectValue / 100);

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

                    PlayerStats.Instance.BuffPlayerHealthRegen((int)relativeBuffEffectValue);

                    break;

                case SkillItem.SkillType.passiveAmmoGenerator:

                    PlayerStats.Instance.BuffPlayerAmmoRegen((int)relativeBuffEffectValue);

                    break;

                case SkillItem.SkillType.passiveChanceToDoubleXPDrop:

                    PlayerStats.Instance.BuffChanceToDropx2((int)relativeBuffEffectValue);

                    break;

                case SkillItem.SkillType.passiveShieldGenerator:

                    passiveShield.UnlockShield(absoluteBuffEffectValue);

                    break;
                case SkillItem.SkillType.passiveLastBulletDealsTwiceDamage:

                    lastBulletDealsMoreDamage = true;
                    lastBulletDealsMoreDamageBuff = 1 + relativeBuffEffectValue / 100;

                    break;

                case SkillItem.SkillType.passiveShootOnReload:

                    shootOnReload = true;

                    break;

                case SkillItem.SkillType.passiveMeleeAttackMagmaShot:

                    meleeAttackMagmaShot = true;
                    meleeAttackMagmaShotBurnAmount += (int)relativeBuffEffectValue;

                    break;

                case SkillItem.SkillType.passiveDashFireTrail:

                    fireDashRoll = true;
                    fireDashRollBurnAmount += (int)relativeBuffEffectValue;

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

                    Debug.LogWarning($"Unhandled skill type: {skillEffect.skillType}");
                    break;
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
    public int GetMeleeAttackMagmaShotBurnAmount() {
        return meleeAttackMagmaShotBurnAmount;
    }
    public int GetMagmaShotBulletBurnAmount() {
        return magmaShotBulletBurnAmount;
    }

    public int GetHealPipsPerKill() {
        return healPipsPerKill;
    }
    public int GetFuelPipsPerKill() {
        return fuelPipsPerKill;
    }
    public Transform GetMagmaShotPrefab() {
        return magmaShotPrefab;
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
    public bool GetMagmaBullet() {
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
        fireDash.InitializeCarriedStatusEffects(true, fireDashRollBurnAmount);
        fireDash.GetComponent<StaticProjectileSounds>().TriggerProjectileSFX();
    }
    #endregion

    #region FIRE LIGHT BUFFS & DEBUFFS
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Fire>() != null) return;
        if (collision.gameObject.GetComponent<FireOrbCollider>() != null) return;

        if (collision.gameObject.GetComponentInParent<Fire>() != null) {
            enteredLight = true;

            if (!increasedDamageInFireLight && !increasedDamageOutFireLight) return;
            StartCoroutine(HandleBuffDebuffsFireLight(true, 0f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Fire>() != null) return;
        if (collision.gameObject.GetComponent<FireOrbCollider>() != null) return;

        if (collision.gameObject.GetComponentInParent<Fire>() != null) {
            enteredLight = false;
            if (!increasedDamageInFireLight && !increasedDamageOutFireLight) return;

            StartCoroutine(HandleBuffDebuffsFireLight(false, 0f));
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

    [Button] 
    private void AddActiveSkillDebug(SkillSO skillSO, int level) {
        SkillItem skillItem = new SkillItem();
        skillItem.Initialize(skillSO);
        skillItem.currentLevel = level;
        AddActiveSkill(skillItem);
    }
    [Button]
    private void AddPassiveSkillDebug(SkillSO skillSO) {
        SkillItem skillItem = new SkillItem();
        skillItem.Initialize(skillSO);
        AddPassiveSkill(skillItem);
    }
}
