using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCampVisual : MonoBehaviour
{
    public static PlayerCampVisual Instance;

    [SerializeField] private Transform backgroundSpriteMaskLeft;
    [SerializeField] private Transform backgroundSpriteMaskRight;
    [SerializeField] private Transform backgroundLeftFenceTransform_Level1;
    [SerializeField] private Transform backgroundRightFenceTransform_Level1;
    [SerializeField] private Transform backgroundLeftFenceTransform_Level2;
    [SerializeField] private Transform backgroundRightFenceTransform_Level2;
    [SerializeField] private Transform backgroundLeftFenceTransform_Level3;
    [SerializeField] private Transform backgroundRightFenceTransform_Level3;

    [SerializeField] private Animator currentBackgroundRightFenceAnimator;
    [SerializeField] private Animator currentBackgroundLeftFenceAnimator;

    private float maxLeftLimit;
    private float maxRightLimit;
    private float scaleToWorldUnits = 2.56f;

    private bool initialFireActivated;

    private bool lerpingLeft;
    private bool lerpingRight;
    private float lerpTimer;
    private float lerpDuration = 4f;
    private float lerpDurationDistanceToTimeFactor = 4f;

    private float left_fromScale;
    private float left_toScale;
    private float right_fromScale;
    private float right_toScale;

    public event EventHandler OnCampBackgroundBuild_Start;
    public event EventHandler OnCampBackgroundBuilt;

    private bool campLimitsInitialized;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        CampZoneManager.Instance.OnCampZoneLimitsChanged += CampZoneManager_OnCampZoneLimitsChanged;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        Tent.Instance.OnStructureUpgraded += Tent_OnStructureUpgraded;
        backgroundSpriteMaskLeft.localScale = Vector3.zero;
        backgroundSpriteMaskRight.localScale = Vector3.zero;
        UpdateActiveFence();
    }

    private void Tent_OnStructureUpgraded(object sender, Structure.OnStructureUpgradedEventArgs e) {
        UpdateActiveFence();
    }

    private void UpdateActiveFence() {
        backgroundLeftFenceTransform_Level1.gameObject.SetActive(false);
        backgroundRightFenceTransform_Level1.gameObject.SetActive(false);
        backgroundLeftFenceTransform_Level2.gameObject.SetActive(false);
        backgroundRightFenceTransform_Level2.gameObject.SetActive(false);
        backgroundLeftFenceTransform_Level3.gameObject.SetActive(false);
        backgroundRightFenceTransform_Level3.gameObject.SetActive(false);

        if (Tent.Instance.GetStructureLevel() == 1) {

            backgroundLeftFenceTransform_Level1.gameObject.SetActive(true);
            backgroundRightFenceTransform_Level1.gameObject.SetActive(true);

            currentBackgroundRightFenceAnimator = backgroundRightFenceTransform_Level1.GetComponent<Animator>();
            currentBackgroundLeftFenceAnimator = backgroundLeftFenceTransform_Level1.GetComponent<Animator>();
        }
        if (Tent.Instance.GetStructureLevel() == 2) {

            backgroundLeftFenceTransform_Level2.gameObject.SetActive(true);
            backgroundRightFenceTransform_Level2.gameObject.SetActive(true);

            currentBackgroundRightFenceAnimator = backgroundRightFenceTransform_Level2.GetComponent<Animator>();
            currentBackgroundLeftFenceAnimator = backgroundLeftFenceTransform_Level2.GetComponent<Animator>();

        }
        if (Tent.Instance.GetStructureLevel() == 3) {

            backgroundLeftFenceTransform_Level3.gameObject.SetActive(true);
            backgroundRightFenceTransform_Level3.gameObject.SetActive(true);

            currentBackgroundRightFenceAnimator = backgroundRightFenceTransform_Level3.GetComponent<Animator>();
            currentBackgroundLeftFenceAnimator = backgroundLeftFenceTransform_Level3.GetComponent<Animator>();

        }

        currentBackgroundRightFenceAnimator.ResetTrigger("Build_Start");
        currentBackgroundRightFenceAnimator.SetTrigger("Build");
        currentBackgroundLeftFenceAnimator.ResetTrigger("Build_Start");
        currentBackgroundLeftFenceAnimator.SetTrigger("Build");
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        StartCoroutine(StartLerpingAfterDelay(.5f));
    }

    private void Update() {
        if (!initialFireActivated) return;
        if(lerpingLeft) {

            lerpTimer += Time.deltaTime;
            float timerNormalized = lerpTimer / lerpDuration;

            float newLeftScaleValue = Mathf.Abs(Mathf.Lerp(left_fromScale, left_toScale, timerNormalized));
            Vector3 newLeftLocalScale = newLeftScaleValue * Vector3.one;

            backgroundSpriteMaskLeft.localScale = newLeftLocalScale;

            if (timerNormalized >= 1) {
                lerpingLeft = false;
                currentBackgroundLeftFenceAnimator.ResetTrigger("Build_Start");
                currentBackgroundLeftFenceAnimator.SetTrigger("Build");

                OnCampBackgroundBuilt?.Invoke(this, EventArgs.Empty);
            }
        }

        if(lerpingRight) {

            lerpTimer += Time.deltaTime;
            float timerNormalized = lerpTimer / lerpDuration;

            float newRightScaleValue = Mathf.Lerp(right_fromScale, right_toScale, timerNormalized);
            Vector3 newRightLocalScale = newRightScaleValue * Vector3.one;

            backgroundSpriteMaskRight.localScale = newRightLocalScale;

            if(timerNormalized >= 1) {
                lerpingRight = false;
                currentBackgroundRightFenceAnimator.ResetTrigger("Build_Start");
                currentBackgroundRightFenceAnimator.SetTrigger("Build");

                OnCampBackgroundBuilt?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void CampZoneManager_OnCampZoneLimitsChanged(object sender, CampZoneManager.OnCampZoneLimitsChangedEventArgs e) {
        if(!campLimitsInitialized) {
            maxRightLimit = CampZoneManager.Instance.GetMaxZoneLimit();
            maxLeftLimit = Mathf.Abs(CampZoneManager.Instance.GetMinZoneLimit());
            campLimitsInitialized = true;
            return;
        }

        float rightLimit = CampZoneManager.Instance.GetMaxZoneLimit();
        float leftLimit = Mathf.Abs(CampZoneManager.Instance.GetMinZoneLimit());

        lerpTimer = 0;

        if (rightLimit > maxRightLimit) {
            lerpDuration =  (rightLimit - maxRightLimit)/ lerpDurationDistanceToTimeFactor;

            maxRightLimit = rightLimit;
            right_fromScale = backgroundSpriteMaskRight.localScale.x;
            right_toScale = rightLimit / scaleToWorldUnits;

            if (!e.triggerSFXAndFenceAnimation) {
                backgroundSpriteMaskRight.localScale = right_toScale * Vector3.one;
                currentBackgroundRightFenceAnimator.SetTrigger("BuiltAtStart");
                return;
            };

            lerpingRight = true;
            OnCampBackgroundBuild_Start?.Invoke(this, EventArgs.Empty);
            currentBackgroundRightFenceAnimator.SetTrigger("Build_Start");
        }

        if(leftLimit > maxLeftLimit) {
            lerpDuration = (leftLimit - maxLeftLimit)/ lerpDurationDistanceToTimeFactor;

            maxLeftLimit = leftLimit;
            left_fromScale = backgroundSpriteMaskLeft.localScale.x;
            left_toScale = leftLimit / scaleToWorldUnits;

            if (!e.triggerSFXAndFenceAnimation) {
                backgroundSpriteMaskLeft.localScale = left_toScale * Vector3.one;
                currentBackgroundLeftFenceAnimator.SetTrigger("BuiltAtStart");
                return;
            };

            lerpingLeft = true;
            OnCampBackgroundBuild_Start?.Invoke(this, EventArgs.Empty);
            currentBackgroundLeftFenceAnimator.SetTrigger("Build_Start");
        }

    }

    private IEnumerator StartLerpingAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        initialFireActivated = true;
    }
}
