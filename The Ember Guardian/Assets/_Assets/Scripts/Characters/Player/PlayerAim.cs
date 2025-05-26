using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public static PlayerAim Instance;

    private bool autoAimOnMovement;
    [SerializeField] private List<Transform> followAimDirTransformList;
    [SerializeField] private Transform gunTransform;
    [SerializeField] private Transform aimSightTransform;
    [SerializeField] private Transform weaponReticleTransform; // Le réticule du tir effectif

    [SerializeField] private List<Transform> transformAffectedByXScalList;
    [SerializeField] private Transform gunShellPSTransform;
    [SerializeField] private RectTransform ammoBarTransform;
    [SerializeField] private RectTransform ammoBarLeftPosition;
    [SerializeField] private RectTransform ammoBarRightPosition;

    [SerializeField] private LayerMask enemyLayer; // Masque de couche pour les ennemis
    [SerializeField] private LayerMask groundLayer; // Masque de couche pour les ennemis
    [SerializeField] private LayerMask barricadesLayer; // Masque de couche pour les ennemis
    private float autoAimConeAngle = 7.5f; // Angle du cône de visée autour de la direction de visée
    private float detectionRange = 15f; // Portée de détection des ennemis

    private bool isUsingGamepad;
    private bool isAimingSight;
    private bool isRolling = false;
    private bool autoAimActive;
    private bool isAimingCreature;
    private bool isAimingCritZone;

    private float mouseAimAngle;
    private float effectiveAimAngle;

    private float currentPrecisionRadius = 0.3f;
    private float smoothSpeed = 1f;
    private float noiseAmount = 0f;
    private float switchThreshold = 0.02f; // Distance pour considérer qu'on est "arrivé"
    private float weaponRange;

    private float timeToPerfectAccuracy = 3f; // Durée avant précision parfaite
    private float stationaryTimer = 0f;
    private bool perfectPrecisionEventSent;
    private bool playerIsStationary;
    private bool playerIsExhausted;

    private bool lastOffsetWasUp = true;
    private float currentPrecisionModifier = 1;
    private float previousWeaponPrecisionModifier = 1;
    private float weaponPrecisionModifier = 1;
    private float distancePrecisionModifier;
    private float precisionModifierWithDistance;
    private float maxAimDistancePrecisionModifier = 1f;
    private float minAimDistancePrecisionModifier = .1f;
    private float runPrecisionDebuff = 1.5f;
    private float movePrecisionDebuff = 1.25f;
    private float exhaustedPrecisionDebuff = 2.5f;
    private float takeDamagePrecisionDebuff = 3f;
    private float crouchPrecisionBuff = 2;

    private bool justTookDamage;
    private float takeDamageTimeToRecoverPrecision = 1f;
    private float takeDamageTimer;

    private Vector2 smoothedOffset = Vector2.zero;
    private Vector2 currentEffectiveOffsetTarget;
    private bool goToMouseNext = true;

    private float recoilDamping;
    private float currentRecoil;
    private float returnToRestSpeed = 3f;

    private Vector3 aimDir;
    private Vector3 previousGamepadAim = new Vector3(1,0,0);
    private Vector3 previousAimDir = new Vector3(1,0,0);
    private Vector3 pointerTargetOverride;
    private Vector3 weaponReticleWorldPos;
    private Vector2 offsetWithNoiseAndRecoil;
    private RaycastHit2D closestHit;
    private bool hasClosestHit = false;

    private bool limitAimAngle = false;
    private float maxAimAngle = 45f; // Maximum angle from the default aim direction (in degrees)

    public event EventHandler OnXAimDirChanged;
    public event EventHandler OnPlayerAimSightStarted;
    public event EventHandler OnPlayerAimSightEnded;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        float angle = Mathf.Atan2(0, 1) * Mathf.Rad2Deg;

        foreach (Transform transform in followAimDirTransformList) {
            transform.eulerAngles = new Vector3(0, 0, angle);
        }

        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;

        PlayerShoot.Instance.OnPlayerAimedSightStarted += PlayerShoot_OnPlayerAimedSightStarted;
        PlayerShoot.Instance.OnPlayerAimedSightEnded += PlayerShoot_OnPlayerAimedSightEnded;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
        PlayerMovement.Instance.OnPlayerRunStarted += PlayerMovement_OnPlayerRunStarted;
        PlayerMovement.Instance.OnPlayerRunStopped += PlayerMovement_OnPlayerRunStopped;
        PlayerMovement.Instance.OnPlayerCrouched += PlayerMovement_OnPlayerCrouched;
        PlayerMovement.Instance.OnPlayerCrouchedEnded += PlayerMovement_OnPlayerCrouchedEnded;
        PlayerMovement.Instance.OnPlayerMoveStarted += PlayerMovement_OnPlayerMoveStarted;
        PlayerMovement.Instance.OnPlayerMoveStopped += PlayerMovement_OnPlayerMoveStopped;
        PlayerMovement.Instance.OnPlayerExhaustionStarted += PlayerMovement_OnPlayerExhaustionStarted;
        PlayerMovement.Instance.OnPlayerExhaustionStopped += PlayerMovement_OnPlayerExhaustionStopped;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        SettingsManager.Instance.OnAimAssistChanged += SettingsManager_OnAimAssistChanged;
        SettingsManager.Instance.OnAutoAlignAimWithMovementChanged += SettingsManager_OnAutoAlignAimWithMovementChanged;

        autoAimOnMovement = SettingsManager.Instance.GetAlignAimWithMovement();
        autoAimActive = SettingsManager.Instance.GetAimAssist();
        isUsingGamepad = GameInput.Instance.IsUsingGamepad();
    }


    private void LateUpdate() {
        HandleJustTookDamagePrecisionDebuff();

        if (isRolling) return;
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        weaponRange = PlayerShoot.Instance.GetHeldGun().GetRange();
        if (isUsingGamepad) {
            HandleAimGamepad(GameInput.Instance.GetAimInput());
        }
        else {
            HandleAimMouse();
        }

        HandleRecoil();
    }

    private void RefreshPrecisionVariables() {
        GunSO gunSO = PlayerShoot.Instance.GetHeldGunSO();

        weaponPrecisionModifier = gunSO.weaponPrecisionMultiplier;

        DebuffPrecision(previousWeaponPrecisionModifier);
        BuffPrecision(weaponPrecisionModifier);

        previousWeaponPrecisionModifier = weaponPrecisionModifier;
    }
    private void SettingsManager_OnAutoAlignAimWithMovementChanged(object sender, EventArgs e) {
        autoAimOnMovement = SettingsManager.Instance.GetAlignAimWithMovement();
    }

    private void SettingsManager_OnAimAssistChanged(object sender, EventArgs e) {
        autoAimActive = SettingsManager.Instance.GetAimAssist();
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        isUsingGamepad = GameInput.Instance.IsUsingGamepad();
    }

    private void HandleRecoil() {

        // Recul lissé
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilDamping);

    }

    private void HandleAimGamepad(Vector2 lookInput) {

        if (lookInput.magnitude > GameInput.gamepadDeadzone) {
            aimDir = new Vector3(lookInput.x, lookInput.y, 0).normalized;
            previousGamepadAim = aimDir;
        }
        else {
            // Utiliser la direction du déplacement quand le stick est au repos *si l'option est activée*
            if (autoAimOnMovement) {
                float restingPosition = PlayerMovement.Instance.GetCurrentMoveDir();
                if(restingPosition != 0) {
                    aimDir = Vector3.Lerp(previousGamepadAim, new Vector3(restingPosition, 0f, 0f), Time.deltaTime * returnToRestSpeed);
                    previousGamepadAim = aimDir;
                } 
            }
        }

        // Auto-aim logic
        if(autoAimActive) {
            HandleAutoAim();
        }

        if (limitAimAngle) {
            ApplyAimAngleLimit();
        }

        HandleXScale();
        HandleCreaturesInAimDir();

        Vector3 rayOrigin = gunTransform.position;
        Vector3 virtualMousePosition = rayOrigin + (Vector3)(aimDir.normalized * weaponRange);

        // Position finale du réticule d’arme (autour du pointeur)
        weaponReticleWorldPos = HandleEffectiveAimPosition(virtualMousePosition);

        if (weaponReticleTransform != null) {
            weaponReticleTransform.position = weaponReticleWorldPos;
        }

        // Direction réelle (utilisable pour les tirs)
        Vector3 effectiveDir = (weaponReticleWorldPos - gunTransform.position).normalized;
        effectiveAimAngle = Mathf.Atan2(effectiveDir.y, effectiveDir.x) * Mathf.Rad2Deg;

        foreach (Transform transform in followAimDirTransformList) {
            transform.eulerAngles = new Vector3(0, 0, effectiveAimAngle);
        }

    }

    private void HandleAimMouse() {
        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 dirToMouse = mousePosition - gunTransform.position;

        if (dirToMouse.magnitude < .4f) {
            return;
        }

        aimDir = dirToMouse.normalized;


        if (limitAimAngle) {
            ApplyAimAngleLimit();
        }

        HandleXScale();
        HandleCreaturesInAimDir();

        // Position finale du réticule d’arme (autour du pointeur)
        weaponReticleWorldPos = HandleEffectiveAimPosition(mousePosition);

        if (weaponReticleTransform != null) {
            weaponReticleTransform.position = weaponReticleWorldPos;
        }

        // Direction réelle (utilisable pour les tirs)
        Vector3 effectiveDir = (weaponReticleWorldPos - gunTransform.position).normalized;
        effectiveAimAngle = Mathf.Atan2(effectiveDir.y, effectiveDir.x) * Mathf.Rad2Deg;

        foreach (Transform transform in followAimDirTransformList) {
            transform.eulerAngles = new Vector3(0, 0, effectiveAimAngle);
        }
    }

    private Vector3 HandleEffectiveAimPosition(Vector3 mousePosition) {
        HandlePlayerStationary();
        float stationaryAccuracyFactor = Mathf.Clamp01(stationaryTimer / timeToPerfectAccuracy);

        // Vérifie si on est arrivé à destination
        Vector3 pointerTarget = mousePosition;
        
        if(pointerTargetOverride != Vector3.zero) {
            pointerTarget = pointerTargetOverride;
        }

        float distance = Vector2.Distance(gunTransform.position, pointerTarget);
        //distance = Mathf.Clamp(distance, 0f, weaponRange);

        if(distance > weaponRange) {
            Vector3 rayOrigin = gunTransform.position;
            pointerTarget = rayOrigin + (Vector3)(aimDir.normalized * weaponRange);
        }

        distancePrecisionModifier = Mathf.Lerp(minAimDistancePrecisionModifier, maxAimDistancePrecisionModifier, distance / weaponRange);
        precisionModifierWithDistance = currentPrecisionModifier * distancePrecisionModifier;
        float smoothSpeedModifier = smoothSpeed * distancePrecisionModifier;

        if ((smoothedOffset - currentEffectiveOffsetTarget).sqrMagnitude < switchThreshold * switchThreshold) {
            SelectNextRandomTargetForWeaponPointer();
        }

        // Déplacement linéaire constant
        Vector2 dir = (currentEffectiveOffsetTarget - smoothedOffset).normalized;
        float dist = smoothSpeedModifier * Time.deltaTime;
        float remaining = Vector2.Distance(smoothedOffset, currentEffectiveOffsetTarget);

        if (dist >= remaining)
            smoothedOffset = currentEffectiveOffsetTarget;
        else
            smoothedOffset += dir * dist;

        // Noise
        float noiseX = (Mathf.PerlinNoise(Time.time * 2f, 0f) - 0.5f) * noiseAmount;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * 2f) - 0.5f) * noiseAmount;
        Vector2 noise = new Vector2(noiseX, noiseY);

        Vector2 reducedOffset = Vector2.Lerp(smoothedOffset, Vector2.zero, stationaryAccuracyFactor);
        Vector2 reducedNoise = Vector2.Lerp(noise, Vector2.zero, stationaryAccuracyFactor);

        offsetWithNoiseAndRecoil = reducedOffset + reducedNoise;
        offsetWithNoiseAndRecoil.y += currentRecoil;

        Vector3 effectiveAimPos = pointerTarget + (Vector3)(offsetWithNoiseAndRecoil);

        // Appliquer la contrainte de surface
        if (hasClosestHit) {
            Vector3 hitNormal = closestHit.normal;
            Vector3 toOffset = effectiveAimPos - pointerTarget;
            Vector3 projected = Vector3.ProjectOnPlane(toOffset, hitNormal);
            effectiveAimPos = pointerTarget + projected;
        }

        return effectiveAimPos;
    }

    public Vector3 GetWeaponReticleWorldPos() {
        return weaponReticleWorldPos;
    }

    private void HandlePlayerStationary() {
        playerIsStationary = !playerIsExhausted && !justTookDamage && Mathf.Abs(PlayerMovement.Instance.GetMoveSpeed()) < .2f;

        if (playerIsStationary) {
            stationaryTimer += Time.deltaTime;
        }
        else {
            stationaryTimer = 0f;
        }

    }

    private void ResetStationaryPerfectPrecision(float timeToPerfectAccuracyFractionToRemove = 0f) {
        stationaryTimer = timeToPerfectAccuracy - timeToPerfectAccuracyFractionToRemove * timeToPerfectAccuracy;
        perfectPrecisionEventSent = false;
    }

    private void HandleCreaturesInAimDir() {
        Vector2 rayOrigin = (Vector2)gunTransform.position;
        Vector2 rayDir = aimDir;
        LayerMask combinedMask = enemyLayer | groundLayer | barricadesLayer;

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, rayDir, weaponRange, combinedMask);

        float closestDistance = float.MaxValue;
        Vector2 closestHitPoint = Vector2.zero;
        hasClosestHit = false;

        bool isAimingCreature = false;
        bool isAimingCritZone = false;
        foreach (var hit in hits) {
            // Ignore les colliders de détection spécifiques inutiles
            if (hit.collider.GetComponent<CreatureDetectionCollider>() != null) continue;
            if (hit.collider.GetComponent<CreatureAutoAimCollider>() != null) continue;
            if (hit.collider.GetComponent<Barricade>() != null && hit.collider.GetComponent<Barricade>().GetBarricadeHealthNormalized() <= 0) continue;

            if(hit.collider.GetComponent<Creature>() != null) {
                isAimingCreature = true;
            }

            if (hit.collider.CompareTag("CritHitZone")) {
                isAimingCritZone = true;
            } else {
                isAimingCritZone = false;
            }

            float distance = Vector2.Distance(rayOrigin, hit.point);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestHitPoint = hit.point;
                closestHit = hit;
                hasClosestHit = true;
            }
        }
        this.isAimingCreature = isAimingCreature;
        this.isAimingCritZone = isAimingCritZone;

        if (closestDistance < float.MaxValue) {
            pointerTargetOverride = closestHitPoint;
        }
        else {
            pointerTargetOverride = Vector3.zero;
        }
    }
    private void HandleAutoAim() {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);

        Transform target = null;
        float closestAngle = float.MaxValue;

        Vector2 directionToEnemy = Vector2.zero;
        Vector2 closestEnemyAutoAimColliderPosition = Vector2.zero;

        foreach (var enemy in hitEnemies) {
            CreatureAutoAimCollider autoAimCollider = enemy.gameObject.GetComponent<CreatureAutoAimCollider>();
            if (autoAimCollider == null) continue;

            Creature creature = enemy.GetComponentInParent<Creature>();
            if (creature.GetDead()) continue;

            directionToEnemy = (autoAimCollider.GetAutoAimPosition() - transform.position).normalized;

            // Calculer l'angle entre la direction de la visée et l'ennemi
            float angleToEnemy = Vector2.Angle(aimDir, directionToEnemy);

            // Vérifier si l'ennemi se trouve dans le cône de visée
            if (angleToEnemy <= autoAimConeAngle) {
                // Sélectionner l'ennemi le plus proche dans le cône
                if (angleToEnemy < closestAngle) {
                    closestAngle = angleToEnemy;
                    target = enemy.transform;
                    closestEnemyAutoAimColliderPosition = autoAimCollider.GetAutoAimPosition();
                }
            }
        }

        // Si un ennemi a été trouvé, ajuster la visée vers cet ennemi
        if (target != null) {
            Vector2 playerPosition = gunTransform.position;
            Vector2 directionToTarget = (closestEnemyAutoAimColliderPosition - playerPosition).normalized;

            // Lissage de la direction de la visée avec Lerp
            aimDir = directionToTarget;
        }
    }
    private void SelectNextRandomTargetForWeaponPointer() {
        if (goToMouseNext) {
            currentEffectiveOffsetTarget = Vector2.zero;
        }
        else {
            float baseVerticalAngle = lastOffsetWasUp ? -90f : 90f; // alterne
            float verticalAngle = baseVerticalAngle + UnityEngine.Random.Range(-30f, 30f);
            lastOffsetWasUp = !lastOffsetWasUp; // on inverse pour la prochaine fois

            float newAngle = verticalAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * currentPrecisionRadius * precisionModifierWithDistance;
            currentEffectiveOffsetTarget = offset;
        }

        goToMouseNext = !goToMouseNext;
    }

    private void PlayerMovement_OnPlayerCrouchedEnded(object sender, EventArgs e) {
        ResetStationaryPerfectPrecision(0);
        DebuffPrecision(crouchPrecisionBuff);
    }

    private void PlayerMovement_OnPlayerCrouched(object sender, EventArgs e) {
        ResetStationaryPerfectPrecision(.7f);
        BuffPrecision(crouchPrecisionBuff);
    }

    private void PlayerMovement_OnPlayerRunStopped(object sender, EventArgs e) {
        BuffPrecision(runPrecisionDebuff);
    }

    private void PlayerMovement_OnPlayerRunStarted(object sender, EventArgs e) {
        DebuffPrecision(runPrecisionDebuff);
    }

    private void PlayerMovement_OnPlayerMoveStopped(object sender, EventArgs e) {
        BuffPrecision(movePrecisionDebuff);
    }

    private void PlayerMovement_OnPlayerMoveStarted(object sender, EventArgs e) {
        DebuffPrecision(movePrecisionDebuff);
    }

    private void PlayerMovement_OnPlayerExhaustionStopped(object sender, EventArgs e) {
        playerIsExhausted = false;
        BuffPrecision(exhaustedPrecisionDebuff);
    }

    private void PlayerMovement_OnPlayerExhaustionStarted(object sender, EventArgs e) {
        playerIsExhausted = true;
        DebuffPrecision(exhaustedPrecisionDebuff);
    }

    private void Player_OnPlayerDamaged(object sender, Player.OnPlayerChangedHealthEventArgs e) {
        DebuffPrecision(takeDamagePrecisionDebuff);
        takeDamageTimer = takeDamageTimeToRecoverPrecision;
        justTookDamage = true;
    }

    private void HandleJustTookDamagePrecisionDebuff() {
        if (justTookDamage) {
            takeDamageTimer -= Time.deltaTime;
            if (takeDamageTimer <= 0) {
                justTookDamage = false;
                BuffPrecision(takeDamagePrecisionDebuff);
            }
        }
    }

    private void BuffPrecision(float buff) {
        currentPrecisionModifier /= buff;
        smoothSpeed /= buff;
        noiseAmount /= buff;
        SelectNextRandomTargetForWeaponPointer();
        //Debug.Log("BuffPrecision currentPrecisionModifier " + currentPrecisionModifier);
    }

    private void DebuffPrecision(float debuff) {
        currentPrecisionModifier *= debuff;
        smoothSpeed *= debuff;
        noiseAmount *= debuff;
        SelectNextRandomTargetForWeaponPointer();
        //Debug.Log("DebuffPrecision currentPrecisionModifier " + currentPrecisionModifier);
    }

    public float GetCurrentPrecisionModifier() {
        return currentPrecisionModifier;
    }

    private void ApplyAimAngleLimit()
    {
        // Calculer l'angle actuel de la visée (déjà compris entre -180 et 180)
        float currentAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        // Initialiser les bornes d'angle
        float minAngle = 0;
        float maxAngle = 0;

        if (aimDir.x < 0)
        {
            // Bornes pour viser à gauche
            minAngle = -180 + maxAimAngle;
            maxAngle = 180 - maxAimAngle;

            // Limiter l'angle dans les bornes définies
            if (currentAngle > 0)
            {
                // Limiter l'angle dans les bornes définies
                if (currentAngle < maxAngle)
                {
                    currentAngle = maxAngle;
                }
            }
            if(currentAngle < 0)
            {
                // Limiter l'angle dans les bornes définies
                if (currentAngle > minAngle)
                {
                    currentAngle = minAngle;
                }
            }
        }
        else
        {
            // Bornes pour viser à droite
            minAngle = -maxAimAngle;
            maxAngle = maxAimAngle;

            // Limiter l'angle dans les bornes définies
            if (currentAngle < minAngle)
            {
                currentAngle = minAngle;
            }
            else if (currentAngle > maxAngle)
            {
                currentAngle = maxAngle;
            }
        }


        // Recalculer la direction de visée à partir de l'angle limité
        aimDir = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0).normalized;
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        isRolling = false;
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        isRolling = true;
    }

    private void PlayerShoot_OnPlayerAimedSightEnded(object sender, EventArgs e) {
        CameraManager.Instance.ResetCameraTargetToPlayer();
        OnPlayerAimSightEnded?.Invoke(this, EventArgs.Empty);
        ResetStationaryPerfectPrecision();
    }

    private void PlayerShoot_OnPlayerAimedSightStarted(object sender, EventArgs e) {
        CameraManager.Instance.ChangeCameraTarget(aimSightTransform, false);
        OnPlayerAimSightStarted?.Invoke(this, EventArgs.Empty);

        ResetStationaryPerfectPrecision(.5f);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        RefreshPrecisionVariables();
        ResetStationaryPerfectPrecision();
    }

    public void AddRecoil(float recoil, float recoilDamping) {
        ResetStationaryPerfectPrecision();
        currentRecoil += recoil;
        this.recoilDamping = recoilDamping;

        goToMouseNext = true;
    }

    public void SetGunStraight()
    {
        Vector2 aimAngleCorrectedWithAimDir = new Vector2();
        if (aimDir.x > 0)
        {
            aimAngleCorrectedWithAimDir = new Vector2(1, 0);

        }
        else
        {
            aimAngleCorrectedWithAimDir = new Vector2(-1, 0);

        }
        mouseAimAngle = Mathf.Atan2(aimAngleCorrectedWithAimDir.y, aimAngleCorrectedWithAimDir.x) * Mathf.Rad2Deg;

        foreach (Transform transform in followAimDirTransformList)
        {
            transform.eulerAngles = new Vector3(0, 0, mouseAimAngle);
        }

    }

    public void SetLimitAimAngle(bool limitAimAngle, float maxAimAngle = 0)
    {
        this.limitAimAngle = limitAimAngle;
        this.maxAimAngle = maxAimAngle;
    }

    public Vector3 GetMouseWorldPosition() {
        Vector3 vec = GetMouseWorldPosition(Input.mousePosition, Camera.main);
        vec.z = 0;
        return vec;
    }

    public Vector3 GetMouseWorldPosition(Vector3 screenPosition, Camera worldCamera) {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public float GetAimAngle() {
        return mouseAimAngle;
    }

    private void HandleXScale() {
        // Variables pour stocker les nouvelles échelles
        Vector3 localScale = transform.localScale; // Échelle du joueur
        Vector3 gunLocalScale = gunTransform.localScale; // Échelle de l'arme

        // Vérifie si le joueur change de direction
        if (aimDir.x < 0 && previousAimDir.x >= 0) {
            previousAimDir = aimDir;

            // Inverse le personnage
            localScale.x = -1;

            // Inverse les éléments liés à l'arme
            gunLocalScale = new Vector3(-1, -1, 1);
            ammoBarTransform.position = ammoBarRightPosition.position;


            // Applique les nouvelles échelles immédiatement
            foreach (Transform t in transformAffectedByXScalList) {
                t.localScale = localScale;
            }
            gunTransform.localScale = gunLocalScale;
            gunShellPSTransform.localScale = gunLocalScale;
            OnXAimDirChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (aimDir.x > 0 && previousAimDir.x <= 0) {
            previousAimDir = aimDir;

            // Retourne le personnage à l'orientation droite
            localScale.x = 1;

            // Retourne les éléments liés à l'arme
            gunLocalScale = new Vector3(1, 1, 1);
            ammoBarTransform.position = ammoBarLeftPosition.position;


            // Applique les nouvelles échelles immédiatement
            foreach (Transform t in transformAffectedByXScalList) {
                t.localScale = localScale;
            }
            gunTransform.localScale = gunLocalScale;
            gunShellPSTransform.localScale = gunLocalScale;
            OnXAimDirChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetXScale(float watchDir) {
        Vector3 localScale = transform.localScale; // Échelle du joueur
        Vector3 gunLocalScale = gunTransform.localScale; // Échelle de l'arme

        Vector3 imposedWatchDir = new Vector3(watchDir, 0, 0);
        if (watchDir >= 0) {
            previousAimDir = -imposedWatchDir;

            // Inverse le personnage
            localScale.x = -1;

            // Inverse les éléments liés à l'arme
            gunLocalScale = new Vector3(-1, -1, 1);
            ammoBarTransform.position = ammoBarRightPosition.position;

            // Applique les nouvelles échelles immédiatement
            foreach (Transform t in transformAffectedByXScalList) {
                t.localScale = localScale;
            }
            gunTransform.localScale = gunLocalScale;
            gunShellPSTransform.localScale = gunLocalScale;
            OnXAimDirChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (watchDir <= 0) {
            previousAimDir = -imposedWatchDir;

            // Retourne le personnage à l'orientation droite
            localScale.x = 1;

            // Retourne les éléments liés à l'arme
            gunLocalScale = new Vector3(1, 1, 1);
            ammoBarTransform.position = ammoBarLeftPosition.position;


            // Applique les nouvelles échelles immédiatement
            foreach (Transform t in transformAffectedByXScalList) {
                t.localScale = localScale;
            }
            gunTransform.localScale = gunLocalScale;
            gunShellPSTransform.localScale = gunLocalScale;
            OnXAimDirChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector3 GetAimDir() {
        return aimDir;
    }

    public float GetAimDirFloat() {
        if(aimDir.x >= 0) {
            return 1f;
        } else {
            return -1f;
        }
    }

    public bool GetIsAimingCritZone() {
        return isAimingCritZone;
    }
    public bool GetIsAimingCreature() {
        return isAimingCreature;
    }

    public Vector3 GetPreviousAimDir() {
        return previousAimDir;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
