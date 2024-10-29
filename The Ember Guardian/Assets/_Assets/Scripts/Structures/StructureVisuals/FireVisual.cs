using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireVisual : StructureVisual
{
    private Fire fire;
    private Animator fireAnimator;

    [SerializeField] private Light2D AOEFireLight;
    [SerializeField] private Light2D fireAtmosphericLight1;
    [SerializeField] private Light2D fireAtmosphericLight2;

    [SerializeField] private ParticleSystem AOEFirePS;
    [SerializeField] private ParticleSystem atmosphericPS;
    [SerializeField] private ParticleSystem continuousPS;
    [SerializeField] private ParticleSystem fuelledPS;

    [SerializeField] private float calmLightIntensityValue;
    [SerializeField] private float mildLightIntensityValue;
    [SerializeField] private float wildLightIntensityValue;
    [SerializeField] private float insaneLightIntensityValue;
    [SerializeField] private float calmPSEmissionRateValue;
    [SerializeField] private float mildPSEmissionRateValue;
    [SerializeField] private float wildPSEmissionRateValue;
    [SerializeField] private float insanePSEmissionRateValue;

    [SerializeField] private int continuousPSCalmEmissionRate;
    [SerializeField] private int continuousPSMildEmissionRate;
    [SerializeField] private int continuousPSWildEmissionRate;
    [SerializeField] private int continuousPSInsaneEmissionRate;

    private float initialFireAOEValue;
    private float finalFireAOEValue;
    private float initialFireLightIntensityValue;
    private float finalFireLightIntensityValue;
    private float initialFirePSEmissionRateValue;
    private float finalFirePSEmissionRateValue;

    private bool lerping;
    private float lerpDuration = 1f;
    private float lerpTimer = 0;

    protected override void Awake() {
        base.Awake();
        fire = GetComponentInParent<Fire>();
        fireAnimator = GetComponent<Animator>();

        fire.OnFireChangedState += Fire_OnFireChangedState;
        fire.OnFireFuelled += Fire_OnFireFuelled;
    }

    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {
        fuelledPS.Play();
    }

    private void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {
        Debug.Log(e.newState);
        if (e.newState == Fire.State.extinguished) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Extinguished");

            finalFireAOEValue = 0;
            finalFireLightIntensityValue = 0;
            finalFirePSEmissionRateValue = 0;
            ChangeContinuousPSEmissionRate(0);

            if (e.previousState == Fire.State.calm) {
                initialFireAOEValue = fire.GetCalmFireRadius();
                initialFireLightIntensityValue = calmLightIntensityValue;
                initialFirePSEmissionRateValue = calmPSEmissionRateValue;
            }
        }

        if (e.newState == Fire.State.wild) {
            fireAnimator.SetTrigger("Wild");

            finalFireAOEValue = fire.GetWildFireRadius();
            finalFireLightIntensityValue = wildLightIntensityValue;
            finalFirePSEmissionRateValue = wildPSEmissionRateValue;

            ChangeContinuousPSEmissionRate(continuousPSWildEmissionRate);

            if (e.previousState == Fire.State.mild) {
                initialFireAOEValue = fire.GetMildFireRadius();
                initialFireLightIntensityValue = mildLightIntensityValue;
                initialFirePSEmissionRateValue = mildPSEmissionRateValue;
            }
            if(e.previousState == Fire.State.insane) {
                initialFireAOEValue = fire.GetInsaneFireRadius();
                initialFireLightIntensityValue = insaneLightIntensityValue;
                initialFirePSEmissionRateValue = insanePSEmissionRateValue;
            }
        }

        if (e.newState == Fire.State.mild) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Mild");

            finalFireAOEValue = fire.GetMildFireRadius();
            finalFireLightIntensityValue = mildLightIntensityValue;
            finalFirePSEmissionRateValue = mildPSEmissionRateValue;

            ChangeContinuousPSEmissionRate(continuousPSMildEmissionRate);

            if (e.previousState == Fire.State.calm) {
                initialFireAOEValue = fire.GetCalmFireRadius();
                initialFireLightIntensityValue = calmLightIntensityValue;
                initialFirePSEmissionRateValue = calmPSEmissionRateValue;
            }
            if (e.previousState == Fire.State.wild) {
                initialFireAOEValue = fire.GetWildFireRadius();
                initialFireLightIntensityValue = wildLightIntensityValue;
                initialFirePSEmissionRateValue = wildPSEmissionRateValue;
            }
        }

        if (e.newState == Fire.State.insane) {
            fireAnimator.SetTrigger("Insane");

            finalFireAOEValue = fire.GetInsaneFireRadius();
            finalFireLightIntensityValue = insaneLightIntensityValue;
            finalFirePSEmissionRateValue = insanePSEmissionRateValue;

            ChangeContinuousPSEmissionRate(continuousPSInsaneEmissionRate);

            if (e.previousState == Fire.State.wild) {
                initialFireAOEValue = fire.GetWildFireRadius();
                initialFireLightIntensityValue = wildLightIntensityValue;
                initialFirePSEmissionRateValue = wildPSEmissionRateValue;
            }
        }

        if (e.newState == Fire.State.calm) {
            fireAnimator.SetTrigger("Calm");

            finalFireAOEValue = fire.GetCalmFireRadius();
            finalFireLightIntensityValue = calmLightIntensityValue;
            finalFirePSEmissionRateValue = calmPSEmissionRateValue;

            if (e.previousState == Fire.State.mild) {
                initialFireAOEValue = fire.GetMildFireRadius();
                initialFireLightIntensityValue = mildLightIntensityValue;
                initialFirePSEmissionRateValue = mildPSEmissionRateValue;
            }

            if (e.previousState == Fire.State.extinguished) {
                initialFireAOEValue = 0;
                initialFireLightIntensityValue = 0;
                initialFirePSEmissionRateValue = 0;
            }
        }

        lerping = true;
        lerpTimer = 0;
    }

    private void Update() {
        if(lerping) {

            lerpTimer += Time.deltaTime;
            float normalizedTime = lerpTimer / lerpDuration;

            if (normalizedTime >= 1) {
                normalizedTime = 1;
                lerping = false;
            }

            float currentFireAOEValue = Mathf.Lerp(initialFireAOEValue, finalFireAOEValue, normalizedTime);
            float currentLightValue = Mathf.Lerp(initialFireLightIntensityValue, finalFireLightIntensityValue, normalizedTime);
            float currentPSEmissionRateValue = Mathf.Lerp(initialFirePSEmissionRateValue, finalFirePSEmissionRateValue, normalizedTime);

            ChangeFireVisualsRadius(currentFireAOEValue);
            ChangeFireVisualsLightIntensity(currentLightValue);
            ChangeFirePSEmissionRate(currentPSEmissionRateValue);
        }
    }

    private void ChangeFireVisualsRadius(float fireRadius) {
        if(fireRadius > 0) {
            AOEFireLight.enabled = true;
            fireAtmosphericLight1.enabled = true;
            fireAtmosphericLight2.enabled = true;
            AOEFirePS.gameObject.SetActive(true);
        } else {
            AOEFireLight.enabled = false;
            fireAtmosphericLight1.enabled = false;
            fireAtmosphericLight2.enabled = false;
            AOEFirePS.gameObject.SetActive(false);
        }

        AOEFireLight.pointLightOuterRadius = fireRadius;
        AOEFireLight.pointLightInnerRadius = fireRadius - fireRadius / 10;

        fireAtmosphericLight1.pointLightOuterRadius = fireRadius*3/4;
        fireAtmosphericLight1.pointLightInnerRadius = fireRadius/2 - fireRadius/10;

        fireAtmosphericLight2.pointLightOuterRadius = fireRadius/3;
        fireAtmosphericLight2.pointLightInnerRadius = fireRadius/5;

        var shape = AOEFirePS.shape;
        shape.radius = fireRadius - fireRadius/10;

        var shape2 = atmosphericPS.shape;
        shape2.radius = fireRadius - fireRadius / 10;
    }

    private void ChangeFireVisualsLightIntensity(float lightIntensity) {

        AOEFireLight.intensity = lightIntensity;
        fireAtmosphericLight1.intensity = lightIntensity * 2f;
        fireAtmosphericLight2.intensity = lightIntensity * 2f;

    }

    private void ChangeFirePSEmissionRate(float rate) {
        var emission = AOEFirePS.emission;
        emission.rateOverTime = rate;

        var emission2 = atmosphericPS.emission;
        emission2.rateOverTime = rate/20;
    }

    private void ChangeContinuousPSEmissionRate(float rate) {
        var emission = continuousPS.emission;
        emission.rateOverTime = rate;
    }

}
