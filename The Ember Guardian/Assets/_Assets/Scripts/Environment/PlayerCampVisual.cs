using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCampVisual : MonoBehaviour
{
    [SerializeField] private Transform backgroundLeftFenceTransform;
    [SerializeField] private Transform backgroundRightFenceTransform;
    [SerializeField] private Animator backgroundRightFenceAnimator;
    [SerializeField] private Animator backgroundLeftFenceAnimator;

    private float maxLeftLimit;
    private float maxRightLimit;
    private float scaleToWorldUnits = 2.56f;

    private bool initialFireActivated;

    private bool lerpingLeft;
    private bool lerpingRight;
    private float lerpTimer;
    private float lerpDuration = 4f;

    private float left_fromScale;
    private float left_toScale;
    private float right_fromScale;
    private float right_toScale;

    private void Start() {
        CampZoneManager.Instance.OnCampZoneLimitsChanged += CampZoneManager_OnCampZoneLimitsChanged;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        backgroundLeftFenceTransform.localScale = Vector3.zero;
        backgroundRightFenceTransform.localScale = Vector3.zero;
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        StartCoroutine(StartLerpingAfterDelay(1f));
    }

    private void Update() {
        if (!initialFireActivated) return;
        if(lerpingLeft) {

            lerpTimer += Time.deltaTime;
            float timerNormalized = lerpTimer / lerpDuration;

            float newLeftScaleValue = Mathf.Abs(Mathf.Lerp(left_fromScale, left_toScale, timerNormalized));
            Vector3 newLeftLocalScale = newLeftScaleValue * Vector3.one;

            backgroundLeftFenceTransform.localScale = newLeftLocalScale;

            if (timerNormalized >= 1) {
                lerpingLeft = false;
                backgroundLeftFenceAnimator.SetTrigger("Build");
            }
        }

        if(lerpingRight) {

            lerpTimer += Time.deltaTime;
            float timerNormalized = lerpTimer / lerpDuration;

            float newRightScaleValue = Mathf.Lerp(right_fromScale, right_toScale, timerNormalized);
            Vector3 newRightLocalScale = newRightScaleValue * Vector3.one;

            backgroundRightFenceTransform.localScale = newRightLocalScale;

            if(timerNormalized >= 1) {
                lerpingRight = false;
                backgroundRightFenceAnimator.SetTrigger("Build");
            }
        }
    }

    private void CampZoneManager_OnCampZoneLimitsChanged(object sender, System.EventArgs e) {
        float rightLimit = CampZoneManager.Instance.GetMaxZoneLimit();
        float leftLimit = Mathf.Abs(CampZoneManager.Instance.GetMinZoneLimit());

        Debug.Log("rightLimit " + rightLimit);
        Debug.Log("leftLimit " + leftLimit);

        if(rightLimit > maxRightLimit) {
            maxRightLimit = rightLimit;
            right_fromScale = backgroundRightFenceTransform.localScale.x;
            right_toScale = rightLimit / scaleToWorldUnits;

            lerpingRight = true;
        }

        if(leftLimit > maxLeftLimit) {
            maxLeftLimit = leftLimit;
            left_fromScale = backgroundLeftFenceTransform.localScale.x;
            left_toScale = leftLimit / scaleToWorldUnits;

            lerpingLeft = true;
        }
    }

    private IEnumerator StartLerpingAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        initialFireActivated = true;
    }
}
