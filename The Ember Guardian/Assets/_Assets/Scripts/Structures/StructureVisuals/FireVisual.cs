using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireVisual : StructureVisual
{
    private Fire fire;
    private Animator fireAnimator;

    [SerializeField] private Light2D AOEFireLight;
    [SerializeField] private Light2D fireLimitLight;
    [SerializeField] private Light2D fireAtmosphericLight1;
    [SerializeField] private Light2D fireAtmosphericLight2;

    [SerializeField] private ParticleSystem AOEFirePS;
    [SerializeField] private ParticleSystem atmosphericPS;
    [SerializeField] private ParticleSystem continuousPS;
    [SerializeField] private ParticleSystem fuelledPS;
    [SerializeField] private ParticleSystem playerRespawnPS;

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

    [SerializeField] private Light2D fireLimitLightSpriteRenderer;
    [SerializeField] private Light2D fireAmbienLightSpriteRenderer;
    [SerializeField] private Sprite fireLimitLightSprite1;
    [SerializeField] private Sprite fireLimitLightSprite2;
    [SerializeField] private Sprite fireLimitLightSprite3;
    [SerializeField] private Sprite fireLimitLightSprite4;

    private float calmLightRadius = 1.8f;
    private float mildLightRadius = 4.2f;
    private float wildLightRadius = 6.5f;
    private float insaneLightRadius = 11.17f;

    private float AOEFireLightIntensity = .65f;

    private float initialFireAOEValue;
    private float finalFireAOEValue;
    private float initialFireLightLimitValue;
    private float finalFireLightLimiValue;
    private float initialFireLightIntensityValue;
    private float finalFireLightIntensityValue;
    private float initialFirePSEmissionRateValue;
    private float finalFirePSEmissionRateValue;

    private bool lerping;
    private bool AOEFireLightsEnabled;
    private float lerpDuration = 1f;
    private float lerpTimer = 0;

    protected override void Awake() {
        base.Awake();
        fire = GetComponentInParent<Fire>();
        fireAnimator = GetComponent<Animator>();

        fire.OnFireChangedState += Fire_OnFireChangedState;
        fire.OnFireFuelled += Fire_OnFireFuelled;
        fire.OnFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;
        fire.OnFireEmberExtractionStopped += Fire_OnFireEmberExtractionStopped;
    }
    
    protected override void Start() {
        base.Start();

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

            Player.Instance.OnPlayerBackToTentToRespawn += Player_OnPlayerBackToTentToRespawn;
        }

        StartCoroutine(LerpFireLightIntensity(0, AOEFireLightIntensity, 1f));
    }

    private void Player_OnPlayerBackToTentToRespawn(object sender, System.EventArgs e) {
        playerRespawnPS.Play();
    }

    protected void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        StartCoroutine(LerpFireLightIntensity(0, AOEFireLightIntensity, 1f));
    }
    protected void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        StartCoroutine(LerpFireLightIntensity(AOEFireLightIntensity, 0,1f));
    }

    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {
        fuelledPS.Play();
    }

    private void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {

        // PREVIOUS STATE
        if (e.previousState == Fire.State.calm) {
            initialFireAOEValue = fire.GetCalmFireRadius();
            initialFireLightIntensityValue = calmLightIntensityValue;
            initialFirePSEmissionRateValue = calmPSEmissionRateValue;
            initialFireLightLimitValue = calmLightRadius;
        }

        if (e.previousState == Fire.State.mild) {
            initialFireAOEValue = fire.GetMildFireRadius();
            initialFireLightIntensityValue = mildLightIntensityValue;
            initialFirePSEmissionRateValue = mildPSEmissionRateValue;
            initialFireLightLimitValue = mildLightRadius;
        }

        if (e.previousState == Fire.State.insane) {
            initialFireAOEValue = fire.GetInsaneFireRadius();
            initialFireLightIntensityValue = insaneLightIntensityValue;
            initialFirePSEmissionRateValue = insanePSEmissionRateValue;
            initialFireLightLimitValue = insaneLightRadius;
        }

        if (e.previousState == Fire.State.wild) {
            initialFireAOEValue = fire.GetWildFireRadius();
            initialFireLightIntensityValue = wildLightIntensityValue;
            initialFirePSEmissionRateValue = wildPSEmissionRateValue;
            initialFireLightLimitValue = wildLightRadius;
        }

        if (e.previousState == Fire.State.extinguished) {
            initialFireAOEValue = 0;
            initialFireLightIntensityValue = 0;
            initialFirePSEmissionRateValue = 0;
            initialFireLightLimitValue = 0;
        }

        // NEW STATE
        if (e.newState == Fire.State.extinguished) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Extinguished");
            finalFireLightLimiValue = 0;
            finalFireAOEValue = 0;
            finalFireLightIntensityValue = 0;
            finalFirePSEmissionRateValue = 0;
            ChangeContinuousPSEmissionRate(0);
        }

        if (e.newState == Fire.State.wild) {
            fireAnimator.SetTrigger("Wild");

            finalFireAOEValue = fire.GetWildFireRadius();
            finalFireLightLimiValue = wildLightRadius;
            finalFireLightIntensityValue = wildLightIntensityValue;
            finalFirePSEmissionRateValue = wildPSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite3;

            ChangeContinuousPSEmissionRate(continuousPSWildEmissionRate);
        }

        if (e.newState == Fire.State.mild) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Mild");

            finalFireAOEValue = fire.GetMildFireRadius();
            finalFireLightLimiValue = mildLightRadius;
            finalFireLightIntensityValue = mildLightIntensityValue;
            finalFirePSEmissionRateValue = mildPSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite2;

            ChangeContinuousPSEmissionRate(continuousPSMildEmissionRate);
        }

        if (e.newState == Fire.State.insane) {
            fireAnimator.SetTrigger("Insane");

            finalFireAOEValue = fire.GetInsaneFireRadius();
            finalFireLightLimiValue = insaneLightRadius;
            finalFireLightIntensityValue = insaneLightIntensityValue;
            finalFirePSEmissionRateValue = insanePSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite4;

            ChangeContinuousPSEmissionRate(continuousPSInsaneEmissionRate);
        }

        if (e.newState == Fire.State.calm) {
            fireAnimator.SetTrigger("Calm");

            finalFireAOEValue = fire.GetCalmFireRadius();
            finalFireLightLimiValue = calmLightRadius;
            finalFireLightIntensityValue = calmLightIntensityValue;
            finalFirePSEmissionRateValue = calmPSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite1;
        }

        lerping = true;
        lerpTimer = 0;

        if (fire.GetIsHubFire()) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Mild");
        }
    }

    private void Fire_OnFireEmberExtractionStopped(object sender, System.EventArgs e) {
        continuousPS.Play();
        atmosphericPS.Play();
    }

    private void Fire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        continuousPS.Stop();
        atmosphericPS.Stop();
    }

    private void Update() {
        if(lerping) {

            lerpTimer += Time.deltaTime;
            float normalizedTime = lerpTimer / fire.GetLerpDuration();

            if (normalizedTime >= 1) {
                normalizedTime = 1;
                lerping = false;
            }

            float lightLimitValue = Mathf.Lerp(initialFireLightLimitValue, finalFireLightLimiValue, normalizedTime);
            float currentFireAOEValue = Mathf.Lerp(initialFireAOEValue, finalFireAOEValue, normalizedTime);
            float currentLightValue = Mathf.Lerp(initialFireLightIntensityValue, finalFireLightIntensityValue, normalizedTime);
            float currentPSEmissionRateValue = Mathf.Lerp(initialFirePSEmissionRateValue, finalFirePSEmissionRateValue, normalizedTime);

            ChangeFireLightScale(lightLimitValue);
            ChangeFireVisualsRadius(currentFireAOEValue);
            //ChangeFireVisualsLightIntensity(currentLightValue * 3);
            ChangeFirePSEmissionRate(currentPSEmissionRateValue);
        }
    }

    private void ChangeFireLightScale(float fireRadius) {
        fireLimitLight.transform.localScale = new Vector3(fireRadius, fireRadius, 1);
        fireAmbienLightSpriteRenderer.transform.localScale = new Vector3(fireRadius/3, fireRadius/3, 1);
        AOEFireLight.transform.localScale = new Vector3(fireRadius, fireRadius, 1);
    }

    private void ChangeFireVisualsRadius(float fireRadius) {
        if(fireRadius > 0) {
            if(AOEFireLightsEnabled) {
                AOEFireLight.enabled = true;
            }

            fireAtmosphericLight1.enabled = true;
            fireAtmosphericLight2.enabled = true;
            AOEFirePS.gameObject.SetActive(true);
        } else {

            AOEFireLight.enabled = false;
            fireAtmosphericLight1.enabled = false;
            fireAtmosphericLight2.enabled = false;
            AOEFirePS.gameObject.SetActive(false);
        }

        AOEFireLight.pointLightOuterRadius = fireRadius + fireRadius / 20;
        AOEFireLight.pointLightInnerRadius = fireRadius - fireRadius / 20;

        fireAtmosphericLight1.pointLightOuterRadius = fireRadius*3/4;
        fireAtmosphericLight1.pointLightInnerRadius = fireRadius/2 - fireRadius/10;

        fireAtmosphericLight2.pointLightOuterRadius = fireRadius/3;
        fireAtmosphericLight2.pointLightInnerRadius = fireRadius/5;

        var shape = AOEFirePS.shape;
        shape.radius = fireRadius;

        var shape2 = atmosphericPS.shape;
        shape2.radius = fireRadius;
    }

    private void ChangeFireVisualsLightIntensity(float lightIntensity) {
        fireAtmosphericLight1.intensity = lightIntensity;
        fireAtmosphericLight2.intensity = lightIntensity;
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

    private IEnumerator LerpFireLightIntensity(float initialIntensity, float destinationIntensity, float lerpDuration) {
        AOEFireLight.intensity = initialIntensity;
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration) {
            elapsedTime += Time.deltaTime;
            AOEFireLight.intensity = Mathf.Lerp(initialIntensity, destinationIntensity, elapsedTime / lerpDuration);
            yield return null;
        }

        AOEFireLight.intensity = destinationIntensity; // S'assurer que la valeur finale est bien atteinte
    }


}
