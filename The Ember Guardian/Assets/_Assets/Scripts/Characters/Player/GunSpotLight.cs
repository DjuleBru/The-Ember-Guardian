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
    private bool canSwitchLight = true;
    private static bool playerJustTeleported;
    private float playerJustTeleportedTimer;
    public static event EventHandler OnAnyLightSwitched;

    private Coroutine laserLerpCoroutine;
    private float laserLerpDuration_StateChange = 0.2f;
    private float laserLerpDuration_ShootCooldown = 0.05f;
    private float laserBaseIntensity = 1f;

    private void Awake() {
        gun = GetComponent<Gun>();
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyPlayerWarped += FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut += FastTravelTP_OnAnyPlayerWarpedOut;

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
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnCooldownEnded += PlayerShoot_OnCooldownEnded;


        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        }

        PlayerStats.Instance.OnFlashlightRangeChanged += PlayerStats_OnFlashlightRangeChanged;

        RefreshLaserLightActiveWithInput();
    }

    private void PlayerShoot_OnCooldownEnded(object sender, EventArgs e) {

    }

    private void PlayerShoot_OnPlayerShot(object sender, EventArgs e) {

    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        RefreshLaserLightActiveWithInput();
    }


    private void LateUpdate() {
        if (playerJustTeleported) {
            playerJustTeleportedTimer += Time.deltaTime;
            if (playerJustTeleportedTimer > .5f) {
                playerJustTeleported = false;
                SwitchLight();
                SwitchLaserLight();
            }
        }

        if (rolling) return;
        RefreshLightRotation();
    }

    private void RefreshLaserLightActiveWithInput() {
        if (GameInput.Instance.IsUsingGamepad()) {
            laserLightActiveWithInput = true;
        }
        else {
            laserLightActiveWithInput = false;
        }

        RefreshLaserLightActive();
    }

    private void RefreshLaserLightActive() {
        bool laserShouldBeActive = true;

        if(!laserLightActiveWithInput) {
            laserShouldBeActive = false;
        }

        if (Player.Instance.GetDead()) {
            laserShouldBeActive = false;
        }

        if (reloading) {
            laserShouldBeActive = false;
        }

        if(rolling) {
            laserShouldBeActive = false;
        }

        float targetIntensity = (laserShouldBeActive && laserLightActive) ? laserBaseIntensity : 0f;

        if (laserLerpCoroutine != null)
            StopCoroutine(laserLerpCoroutine);

        laserLerpCoroutine = StartCoroutine(LerpLaserLightIntensity(targetIntensity, laserLerpDuration_StateChange));
    }

    private IEnumerator LerpLaserLightIntensity(float targetIntensity, float duration) {
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
        playerJustTeleported = true;
    }

    private void FastTravelTP_OnAnyPlayerWarpedOut(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        playerJustTeleportedTimer = 0;
        playerJustTeleported = true;
    }

    private void FastTravelTP_OnAnyPlayerWarped(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        if (lightActive) {
            SwitchLight();
        }
        if(laserLightActive) {
            SwitchLaserLight();
        }
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

        if (laserLightActive) {
            gunLaserLight.enabled = true;
        }
        else {
            gunLaserLight.enabled = false;
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        lightActive = false;
        gunSpotLight.enabled = false;
        RefreshLaserLightActive();
    }

    private void Player_OnPlayerRespawned(object sender, EventArgs e) {
        RefreshLaserLightActive();
    }
    private void PlayerStats_OnFlashlightRangeChanged(object sender, EventArgs e) {
        RefreshFlashlightRange();
    }

    private void RefreshFlashlightRange() {
        gunSpotLightRange = PlayerStats.Instance.GetFlashlightRange();
        gunSpotLight.pointLightOuterRadius = gunSpotLightRange;
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
        RefreshLaserLightActive();
    }

    private void PlayerShoot_OnPlayerReloadInterrupted(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = false;
        RefreshLaserLightActive();
    }

    private void PlayerShooot_OnPlayerReloadInterruptedEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = true;
        RefreshLaserLightActive();
    }

    private void PlayerShoot_OnPlayerReload(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = true;
        RefreshLaserLightActive();
    }


    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        rolling = false;
        RefreshLaserLightActive();
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        rolling = true;
        RefreshLaserLightActive();
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {

        lightActive = false;
        gunSpotLight.enabled = false;
        canSwitchLight = false;
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
        RefreshLaserLightActive();
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
        PlayerShoot.Instance.OnPlayerReloadInterruptedEnded -= PlayerShooot_OnPlayerReloadInterruptedEnded;
    }
}
