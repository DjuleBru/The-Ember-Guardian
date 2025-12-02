using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightVisualsManager : MonoBehaviour
{

    public static DayNightVisualsManager Instance;

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
    [SerializeField] private SpriteRenderer skySpriteRenderer2;
    [SerializeField] private Light2D globalLight2D;
    [SerializeField] private Light2D sunLight2D;
    [SerializeField] private Light2D moonLight2D;

    [SerializeField] private AnimationCurve sunAnimationCurve;
    [SerializeField] private float sunArcRadius = 5f;
    [SerializeField] private bool dontHandleMoonMovement;
    [SerializeField] private bool dontHandleSunMovement;
    [SerializeField] private bool dontHandleMoonLight;

    [SerializeField] private bool showIncomingWaveDifficultyOnSun;
    [SerializeField] private Color sunEasyIncomingWaveColor;
    [SerializeField] private Color sunHardIncomingWaveColor;
    private Color sunNormalColor;
    [SerializeField] private AnimationCurve sunColorEasyToHardAnimationCurve;

    private float nightDawnTransitionAnimationCurveFraction = .05f;
    [SerializeField] private float dawnAnimationCurveFraction = .1f;

    private float dawnDayAnimationCurveFraction = .05f;
    [SerializeField] private float dayAnimationCurveFraction = .6f;

    private float dayDuskAnimationCurveFraction = .05f;
    [SerializeField] private float duskAnimationCurveFraction = .1f;
    private float duskNightAnimationCurveFraction = .05f;

    private float nightAnimationCurveFraction = .9f;

    private float totalAnimationCurveFractionProgress = 0f;

    private float transitionProgress;
    private float moonPositionXNormalized;
    private bool isMoonMoving;
    private float moonTransitionSpeed = .15f;
    private float nightMoonTransitionSpeed = .15f;
    private float cycleTransitionMoonTransitionSpeed = .5f;
    private float currentMoonPositionXNormalized;
    private float targetMoonPositionXNormalized;

    [SerializeField] private Color caveLightColor;
    [SerializeField] private float caveLightIntensity;
    private Color currentLightColor;
    private float currentLightIntensity;

    private bool inCave;
    private bool caveEnterTransitionStarted;
    private bool caveExitTransitionStarted;
    private bool dawnStarted;
    private bool dayStarted;
    private bool nightStarted;
    private bool duskStarted;

    private bool peacefulWave;
    private bool extremeWave;

    #region SET PARAMETERS

    public void SetDawnSkyColor(Color color) {
        dawnSkyColor = color;
    }
    public void SetDaySkyColor(Color color) {
        daySkytColor = color;
    }
    public void SetDuskSkyColor(Color color) {
        duskSkyColor = color;
    }
    public void SetNightSkyColor(Color color) {
        nightSkyColor = color;
    }
    public void SetDawnLightColor(Color color) {
        dawnLightColor = color;
    }
    public void SetDayLightColor(Color color) {
        dayLightColor = color;
    }
    public void SetDuskLightColor(Color color) {
        duskLightColor = color;
    }
    public void SetNightLightColor(Color color) {
        nightLightColor = color;
    }
    public void SetDawnLightIntensity(float intensity) {
        dawnLightIntensity = intensity;
    }
    public void SetDayLightIntensity(float intensity) {
        dayLightIntensity = intensity;
    }
    public void SetDuskLightIntensity(float intensity) {
        duskLightIntensity = intensity;
    }
    public void SetNightLightIntensity(float intensity) {
        nightLightIntensity = intensity;
    }

    #endregion

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;

        CreaturesSpawnManager.Instance.OnRemainingNightCreaturesChanged += CreaturesSpawnManager_OnRemainingNightCreaturesChanged;

        if(CreaturesSpawnManager.Instance != null ) {
            if(CreaturesSpawnManager.Instance is CreaturesSpawnManager_HordeMode) {
                CreaturesSpawnManager_HordeMode hordeModeCreaturesSpawnManager = CreaturesSpawnManager.Instance as CreaturesSpawnManager_HordeMode;
                hordeModeCreaturesSpawnManager.OnExtremeWavePrepared += HodeModeCreaturesSpawnManager_OnExtremeWavePrepared;
                hordeModeCreaturesSpawnManager.OnPeacefulWavePrepared += HordeModeCreaturesSpawnManager_OnPeacefulWavePrepared;
            }
        }
        sunNormalColor = sunLight2D.color;

        globalLight2D.color = dawnLightColor;
        skySpriteRenderer.color = dawnSkyColor;

        currentLightColor = dawnLightColor;
        currentLightIntensity = dawnLightIntensity;

        if (skySpriteRenderer2 != null) {
            skySpriteRenderer2.color = dawnSkyColor;
        }
        globalLight2D.intensity = dawnLightIntensity;
        moonLight2D.intensity = 0;
    }

    private void Update() {
        HandleCycleTransitions();
    }

    private void LateUpdate() {
        if(!dontHandleSunMovement) {
            HandleSunPosition();
        }

        if (!dontHandleMoonMovement) {
            HandleMoonPosition();
            SetMoonPosition(moonPositionXNormalized);
        };
    }

    private void HandleCycleTransitions() {
        if(inCave) {
            if(caveEnterTransitionStarted) {
                transitionProgress += Time.deltaTime / transitionDuration;

                if (transitionProgress < 1) {
                    globalLight2D.color = ColorTransition(currentLightColor, caveLightColor);
                    globalLight2D.intensity = LightIntensityTransition(currentLightIntensity, caveLightIntensity);
                }
                else {
                    caveEnterTransitionStarted = false;
                    transitionProgress = 0;
                }
            }
        }

        if (caveExitTransitionStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(caveLightColor, currentLightColor);
                globalLight2D.intensity = LightIntensityTransition(caveLightIntensity, currentLightIntensity);
            }
            else {
                caveExitTransitionStarted = false;
                transitionProgress = 0;
            }
        }

        if (dawnStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;
            if (inCave) return;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(nightLightColor, dawnLightColor);
                skySpriteRenderer.color = ColorTransition(nightSkyColor, dawnSkyColor);
                globalLight2D.intensity = LightIntensityTransition(nightLightIntensity, dawnLightIntensity);
                sunLight2D.intensity = LightIntensityTransition(0, sunLightIntensity);
                moonLight2D.intensity = LightIntensityTransition(moonLightIntensity, 0);

                if (skySpriteRenderer2 != null) {
                    skySpriteRenderer2.color = ColorTransition(nightSkyColor, dawnSkyColor);
                }

                moonTransitionSpeed = cycleTransitionMoonTransitionSpeed;
            }
            else {
                moonTransitionSpeed = nightMoonTransitionSpeed;
                transitionProgress = 0;
                totalAnimationCurveFractionProgress += nightDawnTransitionAnimationCurveFraction;
                dawnStarted = false;
            }
        }

        if (dayStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;
            if (inCave) return;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(dawnLightColor, dayLightColor);
                skySpriteRenderer.color = ColorTransition(dawnSkyColor, daySkytColor);
                globalLight2D.intensity = LightIntensityTransition(dawnLightIntensity, dayLightIntensity);

                if (skySpriteRenderer2 != null) {
                    skySpriteRenderer2.color = ColorTransition(dawnSkyColor, daySkytColor);
                }
            }
            else {
                transitionProgress = 0;
                totalAnimationCurveFractionProgress += dawnDayAnimationCurveFraction;
                dayStarted = false;
            }
        }

        if (duskStarted) {
            transitionProgress += Time.deltaTime / transitionDuration;
            if (inCave) return;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(dayLightColor, duskLightColor);
                skySpriteRenderer.color = ColorTransition(daySkytColor, duskSkyColor);

                if (skySpriteRenderer2 != null) {
                    skySpriteRenderer2.color = ColorTransition(daySkytColor, duskSkyColor);
                }

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
            if (inCave) return;

            if (transitionProgress < 1) {
                globalLight2D.color = ColorTransition(duskLightColor, nightLightColor);
                skySpriteRenderer.color = ColorTransition(duskSkyColor, nightSkyColor);
                globalLight2D.intensity = LightIntensityTransition(duskLightIntensity, nightLightIntensity);
                sunLight2D.intensity = LightIntensityTransition(sunLightIntensity, 0);
                moonLight2D.intensity = LightIntensityTransition(0, moonLightIntensity);

                if (skySpriteRenderer2 != null) {
                    skySpriteRenderer2.color = ColorTransition(duskSkyColor, nightSkyColor);
                }

                moonPositionXNormalized = transitionProgress * duskNightAnimationCurveFraction;
                currentMoonPositionXNormalized = moonPositionXNormalized;
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

    private void CreaturesSpawnManager_OnRemainingNightCreaturesChanged(object sender, CreaturesSpawnManager.OnRemainingNightCreaturesChangedEventArgs e) {
        float newTargetMoonPositionXNormalized = (1 - e.remainingNightCreaturesNormalized) + nightDawnTransitionAnimationCurveFraction;

        if(newTargetMoonPositionXNormalized > targetMoonPositionXNormalized) {
            targetMoonPositionXNormalized = newTargetMoonPositionXNormalized;
            isMoonMoving = true; // Activer le mouvement
        }

    }

    private void HandleMoonPosition() {
        if (isMoonMoving) {

            currentMoonPositionXNormalized = Mathf.Lerp(currentMoonPositionXNormalized, targetMoonPositionXNormalized, Time.deltaTime * moonTransitionSpeed);
            moonPositionXNormalized = currentMoonPositionXNormalized;

            // Vérifier si la lune est proche de la position cible
            if (Mathf.Abs(currentMoonPositionXNormalized - targetMoonPositionXNormalized) < .01f) {
                isMoonMoving = false; // Arrêter le mouvement si proche de la cible
            }
            return;
        }

        if (nightStarted || dawnStarted) {
            // Transitioning states
        }
        else {
            // Inside state
            if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night) {
                //Day
                moonPositionXNormalized = 0f;
            }
        }

    }

    private void SetMoonPosition(float moonPositionXNormalized) {
        if (moonPositionXNormalized < 1f) {
            float moonPositionY = sunAnimationCurve.Evaluate(moonPositionXNormalized) * sunArcRadius;
            float moonPositionX = moonPositionXNormalized * sunArcRadius - sunArcRadius / 2;

            moonPositionX += Camera.main.transform.position.x;
            moonPositionY += Camera.main.transform.position.y ;

            moonLight2D.transform.position = new Vector3(moonPositionX, moonPositionY);
        }
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress = 0;

        if (DayNightManager.Instance.GetManualInitialCycleSet()) {
            transitionProgress = 1;
            globalLight2D.color = ColorTransition(nightLightColor, dawnLightColor);
            skySpriteRenderer.color = ColorTransition(nightSkyColor, dawnSkyColor);
            globalLight2D.intensity = LightIntensityTransition(nightLightIntensity, dawnLightIntensity);
            sunLight2D.intensity = LightIntensityTransition(0, sunLightIntensity);

            if(!dontHandleMoonLight) {
                moonLight2D.intensity = LightIntensityTransition(moonLightIntensity, 0);
            }
            return;
        }

        dawnStarted = true;
        dayStarted = false;
        duskStarted = false;
        nightStarted = false;
        transitionProgress = 0;
        currentLightColor = dawnLightColor;
        currentLightIntensity = dawnLightIntensity;

        if (showIncomingWaveDifficultyOnSun) {
            RefreshSunColorBasedOnDifficulty(.1f);
            RefreshMoonColorBasedOnDifficulty(3f);
        } else {
            if(!peacefulWave && !extremeWave) {
                sunLight2D.color = sunNormalColor;
                sunLight2D.intensity /= 2;
            }
            if(peacefulWave) {
                peacefulWave = false;
            }
            if (extremeWave) {
                extremeWave = false;
            }
        }
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {

        totalAnimationCurveFractionProgress += dawnAnimationCurveFraction;

        if (DayNightManager.Instance.GetManualInitialCycleSet()) {
            transitionProgress = 1; 
            globalLight2D.color = ColorTransition(dawnLightColor, dayLightColor);
            skySpriteRenderer.color = ColorTransition(dawnSkyColor, daySkytColor);
            globalLight2D.intensity = LightIntensityTransition(dawnLightIntensity, dayLightIntensity);

            DayNightManager.Instance.SetManualInitialCycleSet(false);
            return;
        }

        dayStarted = true; 
        dawnStarted = false;
        duskStarted = false;
        nightStarted = false;
        transitionProgress = 0;
        currentLightColor = dayLightColor;
        currentLightIntensity = dayLightIntensity;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress += dayAnimationCurveFraction;
        duskStarted = true;
        dawnStarted = false;
        dayStarted = false;
        nightStarted = false; 
        transitionProgress = 0;

        currentLightColor = duskLightColor;
        currentLightIntensity = duskLightIntensity;
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        totalAnimationCurveFractionProgress += duskAnimationCurveFraction;
        targetMoonPositionXNormalized = 0f;
        nightStarted = true;
        dawnStarted = false;
        duskStarted = false;
        dayStarted = false;
        transitionProgress = 0;

        currentLightColor = nightLightColor;
        currentLightIntensity = nightLightIntensity;
    }

    private Color ColorTransition(Color initialColor, Color finalColor) {
        return Color.Lerp(initialColor, finalColor, transitionProgress);
    }

    private float LightIntensityTransition(float initialIntensity, float finalIntensity) {
        return Mathf.Lerp(initialIntensity, finalIntensity, transitionProgress);
    }

    public void RefreshSunColorBasedOnDifficulty(float delay) {
        StartCoroutine(RefreshSunColorBasedOnDifficultyAfterDelay(delay));
    }
    public void RefreshMoonColorBasedOnDifficulty(float delay) {
        StartCoroutine(RefreshMoonColorBasedOnDifficultyAfterDelay(delay));
    }


    public IEnumerator RefreshSunColorBasedOnDifficultyAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        float currentWaveDifficulty = CreaturesSpawnManager.Instance.GetRawCurrentWaveDifficulty();
        float referenceWaveDifficulty = CreaturesSpawnManager.Instance.GetReferenceWaveDifficulty();

        float dangerRatio = currentWaveDifficulty / referenceWaveDifficulty;
        dangerRatio = Mathf.Clamp01((dangerRatio - 1f) / 0.5f);

        float animationCurveRatio = sunColorEasyToHardAnimationCurve.Evaluate(dangerRatio);
        Debug.Log("dangerRatio " + dangerRatio);
        Debug.Log("animationCurveRation " + animationCurveRatio);

        Color sunColor =  Color.Lerp(sunEasyIncomingWaveColor, sunHardIncomingWaveColor, animationCurveRatio);

        sunLight2D.color = sunColor;
    }

    public IEnumerator RefreshMoonColorBasedOnDifficultyAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        float currentWaveDifficulty = CreaturesSpawnManager.Instance.GetRawCurrentWaveDifficulty();
        float referenceWaveDifficulty = CreaturesSpawnManager.Instance.GetReferenceWaveDifficulty();

        float dangerRatio = currentWaveDifficulty / referenceWaveDifficulty;
        dangerRatio = Mathf.Clamp01((dangerRatio - 1f) / 0.5f);

        float curvedRatio = Mathf.Pow(dangerRatio, 0.5f); // sqrt = tire plus vite vers le rouge
        Debug.Log("dangerRatio " + dangerRatio);
        Debug.Log("curvedRatio " + curvedRatio);

        Color sunColor = Color.Lerp(sunEasyIncomingWaveColor, sunHardIncomingWaveColor, curvedRatio);

        moonLight2D.color = sunColor;
    }


    private void HordeModeCreaturesSpawnManager_OnPeacefulWavePrepared(object sender, System.EventArgs e) {
        Debug.Log("HordeModeCreaturesSpawnManager_OnPeacefulWavePrepared");
        Color sunColor = sunEasyIncomingWaveColor;
        sunLight2D.color = sunColor;
        sunLight2D.intensity *= 2;
        peacefulWave = true;
    }

    private void HodeModeCreaturesSpawnManager_OnExtremeWavePrepared(object sender, System.EventArgs e) {
        Debug.Log("HodeModeCreaturesSpawnManager_OnExtremeWavePrepared");
        Color sunColor = sunHardIncomingWaveColor;
        sunLight2D.color = sunColor;
        sunLight2D.intensity *= 2;
        extremeWave = true;
    }

    public void SetInCave(bool inCave) {
        if (!this.inCave && inCave) {
            caveEnterTransitionStarted = true;
        }
        if (this.inCave && !inCave) {
            caveExitTransitionStarted = true;
        }
        this.inCave = inCave;

    } 
}
