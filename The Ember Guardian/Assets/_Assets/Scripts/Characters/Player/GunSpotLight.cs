using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GunSpotLight : MonoBehaviour
{
    [SerializeField] private Transform gunSpotLightTransform;
    private Light2D gunSpotLight;

    private float noFogVolumetricAmount = .2f;
    private float fogVolumetricAmount = .1f;
    private bool autoSwitchWithDay;
    private bool lightActive = true;
    private bool rolling;
    public static event EventHandler OnAnyLightSwitched;

    private void Awake() {
        gunSpotLight = gunSpotLightTransform.GetComponent<Light2D>();
    }

    private void Start() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        }

        float fogAmount = LevelManager.Instance.GetLevelSO().fogAlpha;
        float volumetricAmount = Mathf.Lerp(noFogVolumetricAmount, fogVolumetricAmount, fogAmount);
        gunSpotLight.volumeIntensity = volumetricAmount;

        GameInput.Instance.OnPlayerGunLightSwitch += GameInput_OnPlayerGunLightSwitch;
        lightActive = false;
        gunSpotLight.enabled = false;

        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, EventArgs e) {
        rolling = false;
    }

    private void PlayerMovement_OnPlayerRoll(object sender, EventArgs e) {
        rolling = true;
    }
    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        lightActive = false;
        gunSpotLight.enabled = false;
    }

    private void GameInput_OnPlayerGunLightSwitch(object sender, System.EventArgs e) {
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

    private void Update() {
        if (rolling) return;
        gunSpotLightTransform.eulerAngles = new Vector3(0, 0, PlayerAim.Instance.GetAimAngle() - 90); 
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        if (!autoSwitchWithDay) return;
        if (lightActive) {
            lightActive = false;
            gunSpotLight.enabled = false;
            OnAnyLightSwitched?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!autoSwitchWithDay) return;
        if (!lightActive) {
            lightActive = true;
            gunSpotLight.enabled = true;
            OnAnyLightSwitched?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerGunLightSwitch -= GameInput_OnPlayerGunLightSwitch;
    }
}
