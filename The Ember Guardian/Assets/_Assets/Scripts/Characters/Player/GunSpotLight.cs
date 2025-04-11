using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GunSpotLight : MonoBehaviour
{
    [SerializeField] private Transform gunSpotLightTransform;
    [SerializeField] private Transform gunVisualTransform;
    [SerializeField] private Light2D gunShootLight;
    private Light2D gunSpotLight;
    private Gun gun;

    private float gunSpotLightRange;
    private float gunSpotLightIntensity = 1.5f;
    private float noFogVolumetricAmount = .2f;
    private float fogVolumetricAmount = .1f;
    private bool autoSwitchWithDay;
    public static bool lightActive = true;
    private bool rolling;
    private bool reloading;
    private bool canSwitchLight = true;
    private bool playerJustTeleported;
    private float playerJustTeleportedTimer;
    public static event EventHandler OnAnyLightSwitched;

    private void Awake() {
        gun = GetComponent<Gun>();
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;

        gunSpotLight = gunSpotLightTransform.GetComponent<Light2D>();
        gunShootLight.pointLightOuterAngle = 360;
        gunShootLight.pointLightInnerAngle = 360;
    }

    private void Start() {
        if (SceneLoader.Instance != null && SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;

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


        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerReloadInterrupted += PlayerShoot_OnPlayerReloadInterrupted;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        }

        PlayerStats.Instance.OnFlashlightRangeChanged += PlayerStats_OnFlashlightRangeChanged;
    }

    private void Update() {
        if (playerJustTeleported) {
            playerJustTeleportedTimer += Time.deltaTime;
            if (playerJustTeleportedTimer > .5f) {
                playerJustTeleported = false;
                SwitchLight();
            }
        }

        if (rolling) return;

        float angle = gunVisualTransform.rotation.eulerAngles.z;
        gunSpotLightTransform.eulerAngles = new Vector3(0, 0, angle - 90);

    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;
        playerJustTeleported = true;
    }

    private void SettingsManager_OnAutoSwitchLightGunChanged(object sender, EventArgs e) {
        autoSwitchWithDay = SettingsManager.Instance.GetAutoSwitchLight();
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        if(lightActive) {
            gunSpotLight.enabled = true;
        } else {
            gunSpotLight.enabled = false;
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        lightActive = false;
        gunSpotLight.enabled = false;
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
    }

    private void PlayerShoot_OnPlayerReloadInterrupted(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = false;
    }

    private void PlayerShoot_OnPlayerReload(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        reloading = true;
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        rolling = false;
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        rolling = true;
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        lightActive = false;
        gunSpotLight.enabled = false;
    }

    private void GameInput_OnPlayerGunLightSwitch(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;

        if (!canSwitchLight) return;
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
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
    }
}
