using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GunSpotLight : MonoBehaviour
{
    [SerializeField] private Transform gunSpotLightTransform;
    private Light2D gunSpotLight;

    private bool lightActive = true;

    private void Awake() {
        gunSpotLight = gunSpotLightTransform.GetComponent<Light2D>();
    }

    private void Start() {
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;

        GameInput.Instance.OnPlayerGunLightSwitch += GameInput_OnPlayerGunLightSwitch;

        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
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
    }

    private void Update() {
        gunSpotLightTransform.eulerAngles = new Vector3(0, 0, PlayerAim.Instance.GetAimAngle()-90); 
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        if (lightActive) {
            lightActive = false;
            gunSpotLight.enabled = false;
        }
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!lightActive) {
            lightActive = true;
            gunSpotLight.enabled = true;
        }
    }
}
