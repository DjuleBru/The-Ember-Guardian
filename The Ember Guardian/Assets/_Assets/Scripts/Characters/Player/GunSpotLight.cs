using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GunSpotLight : MonoBehaviour
{
    [SerializeField] private Transform gunSpotLightTransform;
    [SerializeField] private Light2D gunLaserLight;
    [SerializeField] private Transform gunVisualTransform;
    [SerializeField] private Light2D gunShootLight;
    private Light2D gunSpotLight;
    private Gun gun;

    private float gunSpotLightRange;
    private float gunSpotLightIntensity = 1.5f;
    private float noFogVolumetricAmount = .2f;
    private float fogVolumetricAmount = .1f;
    private bool autoSwitchWithDay;
    private bool laserLightActiveWithInput;
    private bool laserLightActive = true;
    public static bool lightActive = true;
    private bool rolling;
    private bool reloading;
    private bool shotCoolingDown;
    private bool swappingGun;
    private bool canSwitchLight = true;
    private bool dead;
    private bool aiming = false;
    private bool aimingPreviousFrame;
    private static bool playerJustTeleportedStatic;
    private bool playerJustTeleported;
    private bool playerTeleporting;
    private bool playerSteppedOnPortal;
    private float playerJustTeleportedTimerStatic;
    private float playerJustTeleportedTimer;
    public static event EventHandler OnAnyLightSwitched;

    private Coroutine laserLerpCoroutine;
    private Coroutine shotCoolingDownCoroutine;
    private Coroutine swappingGunCoroutine;
    private float laserLerpDuration_StateChangeEnd = 0.2f;
    private float laserLerpDuration_StateChangeStart = 0.05f;
    private float laserBaseIntensity = 1.5f;

    private void Awake() {
        gun = GetComponent<Gun>();
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyPlayerWarped += FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut += FastTravelTP_OnAnyPlayerWarpedOut;
        FastTravelTP.OnAnyPlayerPositionedOnTP += FastTravelTP_OnAnyPlayerPositionedOnTP;
        FastTravelTP.OnAnyPlayerCanceledTP += FastTravelTP_OnAnyPlayerCanceledTP;

        gunSpotLight = gunSpotLightTransform.GetComponent<Light2D>();
        gunShootLight.pointLightOuterAngle = 360;
        gunShootLight.pointLightInnerAngle = 360;

    }

    private void Start() {

        if (SceneLoader.Instance != null && SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;

            float fogAmount = LevelManager.Instance.GetLevelSO().fogFrontAlpha;
            float volumetricAmount = Mathf.Lerp(noFogVolumetricAmount, fogVolumetricAmount, fogAmount);
            gunSpotLight.volumeIntensity = volumetricAmount;
        } 

        lightActive = false;
        gunSpotLight.intensity = gunSpotLightIntensity;
        gunSpotLight.enabled = false;

        GameInput.Instance.OnPlayerGunLightSwitch += GameInput_OnPlayerGunLightSwitch;
        SettingsManager.Instance.OnAutoSwitchLightGunChanged += SettingsManager_OnAutoSwitchLightGunChanged;
        autoSwitchWithDay = SettingsManager.Instance.GetAutoSwitchLight();

        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerReloadInterruptedEnded += PlayerShooot_OnPlayerReloadInterruptedEnded;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerAim.Instance.OnXAimDirChanged += PlayerAim_OnXAimDirChanged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawnEnded += Player_OnPlayerRespawnEnded;
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnCooldownEnded += PlayerShoot_OnCooldownEnded;
        PlayerShoot.Instance.OnPlayerSwappedGunStarted += PlayerShoot_OnPlayerSwappedGunStarted;
        PlayerShoot.Instance.OnPlayerSwappedGunEnded += PlayerShoot_OnPlayerSwappedGunEnded;


        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        }

        PlayerStats.Instance.OnFlashlightRangeChanged += PlayerStats_OnFlashlightRangeChanged;

        RefreshLaserLightActiveWithInput();
        StartCoroutine(RefreshLaserRangeAfterFrame());
    }

    private void LateUpdate() {
        aiming = PlayerAim.Instance.GetAimInputGamepad() != Vector2.zero;

        if(aiming && !aimingPreviousFrame) {
            RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
        }

        if(!aiming && aimingPreviousFrame) {
            RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
        }
        aimingPreviousFrame = aiming;


        if (playerJustTeleportedStatic) {
            playerJustTeleportedTimerStatic += Time.deltaTime;
            if (playerJustTeleportedTimerStatic > 1f) {
                playerJustTeleportedStatic = false;
                SwitchLight();
                RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
            }
        }

        if (rolling) return;
        RefreshLightRotation();
    }

    private IEnumerator RefreshLaserRangeAfterFrame() {
        yield return new WaitForEndOfFrame();
        RefreshLaserRange();
    }

    private void PlayerShoot_OnPlayerSwappedGunEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        swappingGun = false;

        // Stoppe la coroutine précédente
        if (swappingGunCoroutine != null)
            StopCoroutine(swappingGunCoroutine);

        // Lance le délai avant de rallumer le laser
        swappingGunCoroutine = StartCoroutine(SwappingGunCoroutine());
    }
    private IEnumerator SwappingGunCoroutine() {
        yield return new WaitForSeconds(0.35f); // duree du cooldown visuel

        swappingGun = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void PlayerShoot_OnPlayerSwappedGunStarted(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        swappingGun = true;

        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }

    private void PlayerShoot_OnCooldownEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        // Stoppe la coroutine précédente
        if (shotCoolingDownCoroutine != null)
            StopCoroutine(shotCoolingDownCoroutine);

        // Lance le délai avant de rallumer le laser
        shotCoolingDownCoroutine = StartCoroutine(ShotCoolingDownCoroutine());
    }

    private void PlayerShoot_OnPlayerShot(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        shotCoolingDown = true;
        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);

        // On arrete la coroutine éventuelle pour éviter le rallumage trop tôt
        if (shotCoolingDownCoroutine != null)
            StopCoroutine(shotCoolingDownCoroutine);
    }

    private IEnumerator ShotCoolingDownCoroutine() {
        yield return new WaitForSeconds(0.1f); // duree du cooldown visuel

        shotCoolingDown = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        RefreshLaserLightActiveWithInput();
    }

    private void RefreshLaserLightActiveWithInput() {
        if (GameInput.Instance.IsUsingGamepad()) {
            laserLightActiveWithInput = true;
        }
        else {
            laserLightActiveWithInput = false;
            gunLaserLight.intensity = 0;
        }

        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }

    private void RefreshLaserLightActive(float lerpDuration) {
        if (gunLaserLight == null) return;

        bool laserShouldBeActive = true;
        if (!aiming) {
            laserShouldBeActive = false;
        }

        if (!laserLightActiveWithInput && !DebugManager.Instance.GetDebugShowLaser()) {
            laserShouldBeActive = false;
        }

        if (dead) {
            laserShouldBeActive = false;
        }

        if (reloading) {
            laserShouldBeActive = false;
        }

        if(rolling) {
            laserShouldBeActive = false;
        }

        if (shotCoolingDown) {
            laserShouldBeActive = false;
        }

        if(swappingGun) {
            laserShouldBeActive = false;
        }

        if (playerJustTeleportedStatic || playerTeleporting || playerSteppedOnPortal) {
            laserShouldBeActive = false;
        }

        float targetIntensity = (laserShouldBeActive && laserLightActive) ? laserBaseIntensity : 0f;
        LerpLaserLightIntensity(targetIntensity, lerpDuration);
    }

    private void LerpLaserLightIntensity(float targetIntensity, float duration) {
        if (gunLaserLight == null) return;
        if (laserLerpCoroutine != null)
            StopCoroutine(laserLerpCoroutine);

        laserLerpCoroutine = StartCoroutine(LerpLaserLightIntensityCoroutine(targetIntensity, duration));
    }

    private IEnumerator LerpLaserLightIntensityCoroutine(float targetIntensity, float duration) {
        float startIntensity = gunLaserLight.intensity;
        float time = 0f;

        // On s'assure que la lumière reste active pendant le fade-out
        if (!gunLaserLight.enabled)
            gunLaserLight.enabled = true;

        while (time < duration) {
            time += Time.deltaTime;
            gunLaserLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, time / duration);
            yield return null;
        }

        gunLaserLight.intensity = targetIntensity;

        // Si elle est éteinte, on la disable pour éviter un drawcall inutile
        if (Mathf.Approximately(targetIntensity, 0f))
            gunLaserLight.enabled = false;

        laserLerpCoroutine = null;
    }

    private void PlayerAim_OnXAimDirChanged(object sender, EventArgs e) {
        RefreshLightRotation();
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;

        playerJustTeleportedStatic = true;

        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        playerJustTeleported = true;
    }

    private void FastTravelTP_OnAnyPlayerPositionedOnTP(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        playerSteppedOnPortal = true;
        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }
    private void FastTravelTP_OnAnyPlayerCanceledTP(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        playerSteppedOnPortal = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);

    }

    private void FastTravelTP_OnAnyPlayerWarpedOut(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        playerJustTeleportedTimerStatic = 0;
        playerJustTeleportedTimer = 0;
        playerJustTeleportedStatic = true;
        playerJustTeleported = true;
        playerTeleporting = false;
        playerSteppedOnPortal = false;

        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void FastTravelTP_OnAnyPlayerWarped(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        if (lightActive) {
            SwitchLight();
        }

        playerTeleporting = true;
    }


    private void RefreshLightRotation() {
        Vector3 dir = gunVisualTransform.right;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gunSpotLightTransform.eulerAngles = new Vector3(0, 0, angle - 90);
    }
    private void SettingsManager_OnAutoSwitchLightGunChanged(object sender, EventArgs e) {
        autoSwitchWithDay = SettingsManager.Instance.GetAutoSwitchLight();
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        rolling = false;

        if(lightActive) {
            gunSpotLight.enabled = true;
        } else {
            gunSpotLight.enabled = false;
        }

        if (gunLaserLight == null) return;
        if (laserLightActive) {
            gunLaserLight.enabled = true;
        }
        else {
            gunLaserLight.enabled = false;
        }

        RefreshLaserRange();
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        dead = true;
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        lightActive = false;
        gunSpotLight.enabled = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }

    private void Player_OnPlayerRespawnEnded(object sender, EventArgs e) {
        dead = false;

        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void PlayerStats_OnFlashlightRangeChanged(object sender, EventArgs e) {
        RefreshFlashlightRange();
    }

    private void RefreshFlashlightRange() {
        gunSpotLightRange = PlayerStats.Instance.GetFlashlightRange();
        gunSpotLight.pointLightOuterRadius = gunSpotLightRange;
    }

    private void RefreshLaserRange() {
        if (gunLaserLight == null) return;
        float weaponRange = PlayerShoot.Instance.GetHeldGun().GetRange();

        // Interpolation linéaire entre 10m, 0.36 et 15m, 0.54
        float a = ((0.54f - 0.36f) / (15f - 10f));
        float b = 0.36f - a * 10;
        float laserLightYScale = a * weaponRange + b;

        GunSO.GunType gunType = PlayerShoot.Instance.GetHeldGunSO().gunType;
        if (gunType == GunSO.GunType.RocketLauncher) {
            laserLightYScale *= 2f;
        }
        if (gunType == GunSO.GunType.AAGun) {
            laserLightYScale = .45f;
        }

        gunLaserLight.transform.localScale = new Vector3(1, laserLightYScale, 1);
    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        canSwitchLight = false;
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        canSwitchLight = true;
    }

    private void PlayerShoot_OnPlayerReloadEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        reloading = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void PlayerShoot_OnPlayerReloadInterrupted(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void PlayerShooot_OnPlayerReloadInterruptedEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = true;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void PlayerShoot_OnPlayerReload(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = true;
        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }


    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        rolling = false;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        rolling = true;
        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {

        lightActive = false;
        gunSpotLight.enabled = false;
        canSwitchLight = false;
        playerSteppedOnPortal = true;

        RefreshLaserLightActive(laserLerpDuration_StateChangeStart);
    }

    private void GameInput_OnPlayerGunLightSwitch(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammedAndNextInputSequence(GameInput.Binding.torchOnOff)) return;

        if (!canSwitchLight) return;
        if (Player.Instance.GetInCurrencyStorageArea()) return;
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;

        SwitchLight();
    }

    private void SwitchLight() {
        lightActive = !lightActive;

        if (lightActive) {
            gunSpotLight.enabled = true;
        }
        else {
            gunSpotLight.enabled = false;
        }
        OnAnyLightSwitched?.Invoke(this, EventArgs.Empty);
    }
    private void SwitchLaserLight() {
        laserLightActive = !laserLightActive;
        RefreshLaserLightActive(laserLerpDuration_StateChangeEnd);
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        if (!autoSwitchWithDay) return;
        if (lightActive) {
            lightActive = false;
            gunSpotLight.enabled = false;
            OnAnyLightSwitched?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        if (!autoSwitchWithDay) return;
        if (!lightActive) {
            lightActive = true;
            gunSpotLight.enabled = true;
            OnAnyLightSwitched?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerGunLightSwitch -= GameInput_OnPlayerGunLightSwitch;
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyPlayerWarped -= FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut -= FastTravelTP_OnAnyPlayerWarpedOut;
        FastTravelTP.OnAnyPlayerPositionedOnTP -= FastTravelTP_OnAnyPlayerPositionedOnTP;
        FastTravelTP.OnAnyPlayerCanceledTP -= FastTravelTP_OnAnyPlayerCanceledTP;
        PlayerShoot.Instance.OnPlayerReloadInterruptedEnded -= PlayerShooot_OnPlayerReloadInterruptedEnded;
    }
}
