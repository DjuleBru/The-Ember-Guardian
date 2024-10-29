using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightVisualsManager : MonoBehaviour
{
    [SerializeField] private Color dawnLightColor;
    [SerializeField] private Color dawnSkyColor;
    [SerializeField] private float dawnLightIntensity;

    [SerializeField] private Color dayLightColor;
    [SerializeField] private Color daySkytColor;
    [SerializeField] private float dayLightIntensity;

    [SerializeField] private Color duskLightColor;
    [SerializeField] private Color duskSkyColor;
    [SerializeField] private float duskLightIntensity;

    [SerializeField] private Color nightLightColor;
    [SerializeField] private Color nightSkyColor;
    [SerializeField] private float nightLightIntensity;

    [SerializeField] private float sunLightIntensity;
    [SerializeField] private float moonLightIntensity;

    [SerializeField] private float transitionDuration;

    [SerializeField] private SpriteRenderer skySpriteRenderer;
    [SerializeField] private Light2D globalLight2D;
    [SerializeField] private Light2D sunLight2D;
    [SerializeField] private Light2D moonLight2D;

    [SerializeField] private AnimationCurve sunAnimationCurve;
    [SerializeField] private float sunArcRadius = 5f;

    private float nightDawnTransitionAnimationCurveFraction = .05f;
    private float dawnAnimationCurveFraction = .1f;

    private float dawnDayAnimationCurveFraction = .05f;
    private float dayAnimationCurveFraction = .6f;

    private float dayDuskAnimationCurveFraction = .05f;
    private float duskAnimationCurveFraction = .1f;
    private float duskNightAnimationCurveFraction = .05f;

    private float nightAnimationCurveFraction = .9f;

    private float totalAnimationCurveFractionProgress = 0f;

    private float transitionProgress;
    private bool dawnStarted;
    private bool dayStarted;
    private bool nightStarted;
    private bool duskStarted;

    private void Start() {

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        globalLight2D.color = dawnLightColor;
        skySpriteRenderer.color = dawnSkyColor;
        globalLight2D.intensity = dawnLightIntensity;
    }

    private void Update() {
        HandleCycleTransitions();
    }

    private void FixedUpdate() {
        HandleSunPosition();
        HandleMoonPosition();
    }

    private void HandleCycleTransitions() {
        if (dawnStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(nightLightColor, dawnLightColor);
                skySpriteRenderer.color = ColorTransition(nightSkyColor, dawnSkyColor);
                globalLight2D.intensity = LightIntensityTransition(nightLightIntensity, dawnLightIntensity);
                sunLight2D.intensity = LightIntensityTransition(0, sunLightIntensity);
                moonLight2D.intensity = LightIntensityTransition(moonLightIntensity, 0);
            }
            else {
                transitionProgress = 0;
                totalAnimationCurveFractionProgress += nightDawnTransitionAnimationCurveFraction;
                dawnStarted = false;
            }
        }

        if (dayStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(dawnLightColor, dayLightColor);
                skySpriteRenderer.color = ColorTransition(dawnSkyColor, daySkytColor);
                globalLight2D.intensity = LightIntensityTransition(dawnLightIntensity, dayLightIntensity);
            }
            else {
                transitionProgress = 0;
                totalAnimationCurveFractionProgress += dawnDayAnimationCurveFraction;
                dayStarted = false;
            }
        }

        if (duskStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(dayLightColor, duskLightColor);
                skySpriteRenderer.color = ColorTransition(daySkytColor, duskSkyColor);
                globalLight2D.intensity = LightIntensityTransition(dayLightIntensity, duskLightIntensity);
            }
            else {
                transitionProgress = 0;
                totalAnimationCurveFractionProgress += dayDuskAnimationCurveFraction;
                duskStarted = false;
            }
        }

        if (nightStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(duskLightColor, nightLightColor);
                skySpriteRenderer.color = ColorTransition(duskSkyColor, nightSkyColor);
                globalLight2D.intensity = LightIntensityTransition(duskLightIntensity, nightLightIntensity);
                sunLight2D.intensity = LightIntensityTransition(sunLightIntensity, 0);
                moonLight2D.intensity = LightIntensityTransition(0, moonLightIntensity);
            }
            else {
                transitionProgress = 0;
                totalAnimationCurveFractionProgress += duskNightAnimationCurveFraction;
                nightStarted = false;
            }
        }
    }

    private void HandleSunPosition() {
        float sunPositionXNormalized = 0;

        if (dawnStarted || dayStarted || duskStarted || nightStarted) {
            // Transitioning states

            if(dawnStarted) {
                sunPositionXNormalized = transitionProgress * nightDawnTransitionAnimationCurveFraction+ totalAnimationCurveFractionProgress;
            }

            if(dayStarted) {
                sunPositionXNormalized = transitionProgress * dawnDayAnimationCurveFraction + totalAnimationCurveFractionProgress;
            }

            if(duskStarted) {
                sunPositionXNormalized = transitionProgress * dayDuskAnimationCurveFraction + totalAnimationCurveFractionProgress;
            }

            if(nightStarted) {
                sunPositionXNormalized = transitionProgress * duskNightAnimationCurveFraction + totalAnimationCurveFractionProgress;
            }

        } else {
            // Inside state

            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dawn) {
                sunPositionXNormalized = (DayNightManager.Instance.GetCycleTimer() - transitionDuration) / (DayNightManager.Instance.GetDawnDuration() - transitionDuration) * dawnAnimationCurveFraction + totalAnimationCurveFractionProgress;
            }

            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Day) {
                sunPositionXNormalized = (DayNightManager.Instance.GetCycleTimer() - transitionDuration) / (DayNightManager.Instance.GetDayDuration() - transitionDuration) * dayAnimationCurveFraction + totalAnimationCurveFractionProgress;
            }

            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk) {
                sunPositionXNormalized = (DayNightManager.Instance.GetCycleTimer() - transitionDuration) / (DayNightManager.Instance.GetDuskDuration() - transitionDuration) * duskAnimationCurveFraction + totalAnimationCurveFractionProgress;
            }

            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
                sunPositionXNormalized = 0;
            }
        }

        float sunPositionY = sunAnimationCurve.Evaluate(sunPositionXNormalized) * sunArcRadius;
        float sunPositionX = sunPositionXNormalized * sunArcRadius - sunArcRadius/2;

        sunPositionX = Camera.main.transform.position.x + sunPositionX;
        sunPositionY = Camera.main.transform.position.y + sunPositionY;

        sunLight2D.transform.position = new Vector3(sunPositionX, sunPositionY);
    }

    private void HandleMoonPosition() {
        float moonPositionXNormalized = 0;

        if (nightStarted || dawnStarted) {
            // Transitioning states

            if (nightStarted) {
                moonPositionXNormalized = transitionProgress * duskNightAnimationCurveFraction;
            }

            if (dawnStarted) {
                moonPositionXNormalized = transitionProgress * nightDawnTransitionAnimationCurveFraction + nightAnimationCurveFraction + duskNightAnimationCurveFraction;
            }

        }
        else {
            // Inside state
            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
                moonPositionXNormalized = (DayNightManager.Instance.GetCycleTimer() - transitionDuration) / (DayNightManager.Instance.GetNightDuration() - transitionDuration) * nightAnimationCurveFraction + duskNightAnimationCurveFraction;
            } else {
                //Day
                moonPositionXNormalized = 0f;
            }
        }

        float moonPositionY = sunAnimationCurve.Evaluate(moonPositionXNormalized) * sunArcRadius;
        float moonPositionX = moonPositionXNormalized * sunArcRadius - sunArcRadius / 2;

        moonPositionX = Camera.main.transform.position.x + moonPositionX;
        moonPositionY = Camera.main.transform.position.y + moonPositionY;

        moonLight2D.transform.position = new Vector3(moonPositionX, moonPositionY);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress = 0;
        dawnStarted = true;
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress += dawnAnimationCurveFraction;
        dayStarted = true;
    }
    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress += dayAnimationCurveFraction;
        duskStarted = true;
    }
    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress += duskAnimationCurveFraction;
        nightStarted = true;
    }

    private Color ColorTransition(Color initialColor, Color finalColor) {
        return Color.Lerp(initialColor, finalColor, transitionProgress);
    }

    private float LightIntensityTransition(float initialIntensity, float finalIntensity) {
        return Mathf.Lerp(initialIntensity, finalIntensity, transitionProgress);
    }

}
