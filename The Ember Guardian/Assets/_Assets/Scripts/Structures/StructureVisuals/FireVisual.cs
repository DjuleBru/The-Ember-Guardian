using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireVisual : StructureVisual
{
    private Fire fire;
    private Animator fireAnimator;

    [SerializeField] private RuntimeAnimatorController primordialFireRedAnimator;
    [SerializeField] private RuntimeAnimatorController primordialFireGreenAnimator;
    [SerializeField] private RuntimeAnimatorController primordialFirePurpleAnimator;
    [SerializeField] private RuntimeAnimatorController primordialFireBlueAnimator;

    [SerializeField] private Light2D AOEFireLight;
    [SerializeField] private Light2D fireLimitLight;
    [SerializeField] private Light2D fireAtmosphericLight1;
    [SerializeField] private Light2D fireAtmosphericLight2;

    [SerializeField] private ParticleSystem AOEFirePS;
    [SerializeField] private ParticleSystem atmosphericPS;
    [SerializeField] private ParticleSystem continuousPS;
    [SerializeField] private ParticleSystem fuelledPS;
    [SerializeField] private ParticleSystem playerRespawnPS;
    [SerializeField] private ParticleSystem orbInsertedPS;
    [SerializeField] private ParticleSystem extractingEmberPS;
    [SerializeField] private ParticleSystem primordialFireLitPS;

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

    [SerializeField] private Color blueLimitLightColor;
    [SerializeField] private Color blueAmbientLightColor;
    [SerializeField] private Color blueAOELightColor;
    [SerializeField] private ParticleSystem.MinMaxGradient blueFuelledPSMinMaxGradient;
    [SerializeField] private Gradient blueContinuousPSColorGradient;
    [SerializeField] private Gradient blueOtherPSColorGradient;

    [SerializeField] private Color redLimitLightColor;
    [SerializeField] private Color redAmbientLightColor;
    [SerializeField] private Color redAOELightColor;
    [SerializeField] private ParticleSystem.MinMaxGradient redFuelledPSMinMaxGradient;
    [SerializeField] private Gradient redContinuousPSColorGradient;
    [SerializeField] private Gradient redOtherPSColorGradient;

    [SerializeField] private Color greenLimitLightColor;
    [SerializeField] private Color greenAmbientLightColor;
    [SerializeField] private Color greenAOELightColor;
    [SerializeField] private ParticleSystem.MinMaxGradient greenFuelledPSMinMaxGradient;
    [SerializeField] private Gradient greenContinuousPSColorGradient;
    [SerializeField] private Gradient greenOtherPSColorGradient;

    [SerializeField] private Color purpleLimitLightColor;
    [SerializeField] private Color purpleAmbientLightColor;
    [SerializeField] private Color purpleAOELightColor;
    [SerializeField] private ParticleSystem.MinMaxGradient purpleFuelledPSMinMaxGradient;
    [SerializeField] private Gradient purpleContinuousPSColorGradient;
    [SerializeField] private Gradient purpleOtherPSColorGradient;

    private float calmLightRadius = 2.12f;
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

        if(fire.GetIsPrimordialFire()) {
            SetPrimordialFireColors(fire.GetPrimordialFireColor());
            fire.OnPrimordialFireLit += Fire_OnPrimordialFireLit;
        }

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;

            Player.Instance.OnPlayerBackToTentToRespawn += Player_OnPlayerBackToTentToRespawn;
        }

        if(fire.GetIsSecondaryFire()) {
            if(DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Day) {
                StartCoroutine(LerpFireLightIntensity(0, AOEFireLightIntensity, 1f));
            } else {
                StartCoroutine(LerpFireLightIntensity(AOEFireLightIntensity, 0, 1f));
            }
        } else {
            StartCoroutine(LerpFireLightIntensity(0, AOEFireLightIntensity, 1f));
        }

    }

    private void Fire_OnPrimordialFireLit(object sender, System.EventArgs e) {
        StartLightPrimordialFireVisuals();
    }

    private void SetPrimordialFireColors(Fire.PrimordialFireColor fireColor) {
        Color limitLightColor = blueLimitLightColor;
        Color ambientLightColor = blueAmbientLightColor;
        Color AOELightColor = blueAOELightColor;
        Gradient continuousPSColorGradient = blueContinuousPSColorGradient;
        Gradient otherPSColorGradient = blueOtherPSColorGradient;
        ParticleSystem.MinMaxGradient fuelledPSMinMaxGradient = blueFuelledPSMinMaxGradient;
        ParticleSystem.MinMaxGradient primordialPSMinMaxGradient = blueFuelledPSMinMaxGradient;

        RuntimeAnimatorController animatorController = primordialFireBlueAnimator;

        if(fireColor == Fire.PrimordialFireColor.Red) {
            limitLightColor = redLimitLightColor;
            AOELightColor = redAOELightColor;
            ambientLightColor = redAmbientLightColor;
            continuousPSColorGradient = redContinuousPSColorGradient;
            otherPSColorGradient = redOtherPSColorGradient;
            fuelledPSMinMaxGradient = redFuelledPSMinMaxGradient;
            primordialPSMinMaxGradient = redFuelledPSMinMaxGradient;

            animatorController = primordialFireRedAnimator;
        }

        if (fireColor == Fire.PrimordialFireColor.Green) {
            limitLightColor = greenLimitLightColor;
            ambientLightColor = greenAmbientLightColor;
            AOELightColor = greenAOELightColor;
            continuousPSColorGradient = greenContinuousPSColorGradient;
            otherPSColorGradient = greenOtherPSColorGradient;
            fuelledPSMinMaxGradient = greenFuelledPSMinMaxGradient;
            primordialPSMinMaxGradient = greenFuelledPSMinMaxGradient;

            animatorController = primordialFireGreenAnimator;
        }

        if (fireColor == Fire.PrimordialFireColor.Purple) {
            limitLightColor = purpleLimitLightColor;
            ambientLightColor = purpleAmbientLightColor;
            AOELightColor = purpleAOELightColor;
            continuousPSColorGradient = purpleContinuousPSColorGradient;
            otherPSColorGradient = purpleOtherPSColorGradient;
            fuelledPSMinMaxGradient = purpleFuelledPSMinMaxGradient;
            primordialPSMinMaxGradient = purpleFuelledPSMinMaxGradient;

            animatorController = primordialFirePurpleAnimator;
        }

        if (fireColor == Fire.PrimordialFireColor.Orange) return;

        AOEFireLight.color = AOELightColor;
        fireLimitLight.color = limitLightColor;
        fireLimitLightSpriteRenderer.color = limitLightColor;

        fireAtmosphericLight1.color = ambientLightColor;
        fireAtmosphericLight1.color = ambientLightColor;
        fireAmbienLightSpriteRenderer.color = ambientLightColor;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = AOEFirePS.colorOverLifetime;
        colorOverLifetime.color = otherPSColorGradient;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime2 = atmosphericPS.colorOverLifetime;
        colorOverLifetime2.color = otherPSColorGradient;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime3 = playerRespawnPS.colorOverLifetime;
        colorOverLifetime3.color = otherPSColorGradient;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime4 = continuousPS.colorOverLifetime;
        colorOverLifetime4.color = continuousPSColorGradient;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime5 = extractingEmberPS.colorOverLifetime;
        colorOverLifetime5.color = continuousPSColorGradient;

        ParticleSystem.MainModule mainModule = fuelledPS.main;
        mainModule.startColor = fuelledPSMinMaxGradient;

        ParticleSystem.MainModule mainModule2 = orbInsertedPS.main;
        mainModule2.startColor = fuelledPSMinMaxGradient;

        ParticleSystem.MainModule mainModule3 = primordialFireLitPS.main;
        mainModule3.startColor = primordialPSMinMaxGradient;

        fireAnimator.runtimeAnimatorController = animatorController;
    }

    private void Player_OnPlayerBackToTentToRespawn(object sender, System.EventArgs e) {
        if (!fire.GetIsMainFire()) return;
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
            initialFireAOEValue = fire.GetLevel1FireRadius();
            initialFireLightIntensityValue = calmLightIntensityValue;
            initialFirePSEmissionRateValue = calmPSEmissionRateValue;
            initialFireLightLimitValue = calmLightRadius;
        }

        if (e.previousState == Fire.State.mild) {
            initialFireAOEValue = fire.GetLevel1FireRadius();
            initialFireLightIntensityValue = mildLightIntensityValue;
            initialFirePSEmissionRateValue = mildPSEmissionRateValue;
            initialFireLightLimitValue = calmLightRadius;
        }

        if (e.previousState == Fire.State.insane) {
            initialFireAOEValue = fire.GetLevel2FireRadius();
            initialFireLightIntensityValue = insaneLightIntensityValue;
            initialFirePSEmissionRateValue = insanePSEmissionRateValue;
            initialFireLightLimitValue = mildLightRadius;
        }

        if (e.previousState == Fire.State.wild) {
            initialFireAOEValue = fire.GetLevel2FireRadius();
            initialFireLightIntensityValue = wildLightIntensityValue;
            initialFirePSEmissionRateValue = wildPSEmissionRateValue;
            initialFireLightLimitValue = mildLightRadius;
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

        if (e.newState == Fire.State.calm) {
            fireAnimator.SetTrigger("Calm");

            finalFireAOEValue = fire.GetLevel1FireRadius();
            finalFireLightLimiValue = calmLightRadius;
            finalFireLightIntensityValue = calmLightIntensityValue;
            finalFirePSEmissionRateValue = calmPSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite1;
        }

        if (e.newState == Fire.State.mild) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Mild");

            finalFireAOEValue = fire.GetLevel1FireRadius();
            finalFireLightLimiValue = calmLightRadius;
            finalFireLightIntensityValue = mildLightIntensityValue;
            finalFirePSEmissionRateValue = mildPSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite1;

            ChangeContinuousPSEmissionRate(continuousPSMildEmissionRate);
        }

        if (e.newState == Fire.State.wild) {
            fireAnimator.SetTrigger("Wild");

            finalFireAOEValue = fire.GetLevel2FireRadius();
            finalFireLightLimiValue = mildLightRadius;
            finalFireLightIntensityValue = wildLightIntensityValue;
            finalFirePSEmissionRateValue = wildPSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite2;

            ChangeContinuousPSEmissionRate(continuousPSWildEmissionRate);
        }

        if (e.newState == Fire.State.insane) {
            fireAnimator.SetTrigger("Insane");

            finalFireAOEValue = fire.GetLevel2FireRadius();
            finalFireLightLimiValue = mildLightRadius;
            finalFireLightIntensityValue = insaneLightIntensityValue;
            finalFirePSEmissionRateValue = insanePSEmissionRateValue;
            fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite2;

            ChangeContinuousPSEmissionRate(continuousPSInsaneEmissionRate);
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


    public void StartLightPrimordialFireVisuals() {
        StartCoroutine(LightPrimordialFireCoroutine());
    }

    private IEnumerator LightPrimordialFireCoroutine() {

        CameraManager.Instance.ChangeCameraTarget(Fire.Instance.transform);

        yield return new WaitForSeconds(2f);

        Fire.Instance.SetLerpDuration(2f);
        lerping = true;
        lerpTimer = 0;

        Fire.State state = Fire.Instance.GetState();
        SetFireInitialLerpValues(state);

        fireAnimator.SetTrigger("Calm");
        finalFireLightLimiValue = .25f;
        finalFireAOEValue = .25f;
        finalFireLightIntensityValue = 0;
        finalFirePSEmissionRateValue = 0;
        ChangeContinuousPSEmissionRate(0);

        yield return new WaitForSeconds(3f);

        LevelUI_Locations.Instance.ShowFireTextForTime(5f);
        Fire.Instance.SetLerpDuration(2f);
        lerping = true;
        lerpTimer = 0;

        fireAnimator.SetTrigger("Insane");
        SetFireInitialLerpValues(Fire.State.extinguished);
        finalFireAOEValue = fire.GetLevel2FireRadius()*2;
        finalFireLightLimiValue = mildLightRadius*2;
        finalFireLightIntensityValue = insaneLightIntensityValue*2;
        finalFirePSEmissionRateValue = insanePSEmissionRateValue*2;
        fireLimitLightSpriteRenderer.lightCookieSprite = fireLimitLightSprite2;

        ChangeContinuousPSEmissionRate(continuousPSInsaneEmissionRate*2);

        yield return new WaitForSeconds(4f);

        CameraManager.Instance.ResetCameraTargetToPlayer();
        //LevelManager.Instance.EnableEndLevelPortal(2f);
    }

    private void SetFireInitialLerpValues(Fire.State state) {
        if (state == Fire.State.calm) {
            initialFireAOEValue = fire.GetLevel1FireRadius();
            initialFireLightIntensityValue = calmLightIntensityValue;
            initialFirePSEmissionRateValue = calmPSEmissionRateValue;
            initialFireLightLimitValue = calmLightRadius;
        }

        if (state == Fire.State.mild) {
            initialFireAOEValue = fire.GetLevel1FireRadius();
            initialFireLightIntensityValue = mildLightIntensityValue;
            initialFirePSEmissionRateValue = mildPSEmissionRateValue;
            initialFireLightLimitValue = calmLightRadius;
        }

        if (state == Fire.State.insane) {
            initialFireAOEValue = fire.GetLevel2FireRadius();
            initialFireLightIntensityValue = insaneLightIntensityValue;
            initialFirePSEmissionRateValue = insanePSEmissionRateValue;
            initialFireLightLimitValue = mildLightRadius;
        }

        if (state == Fire.State.wild) {
            initialFireAOEValue = fire.GetLevel2FireRadius();
            initialFireLightIntensityValue = wildLightIntensityValue;
            initialFirePSEmissionRateValue = wildPSEmissionRateValue;
            initialFireLightLimitValue = mildLightRadius;
        }
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

    private void OnDestroy() {
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            DayNightManager.Instance.OnDayStart -= DayNightManager_OnDayStart;
            DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;

            Player.Instance.OnPlayerBackToTentToRespawn -= Player_OnPlayerBackToTentToRespawn;
        }
    }

}
