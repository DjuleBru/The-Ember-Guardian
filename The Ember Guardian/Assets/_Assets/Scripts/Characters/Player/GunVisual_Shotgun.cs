using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual_Shotgun : MonoBehaviour
{
    [SerializeField] private ParticleSystem loadShotgunLeftPS;
    [SerializeField] private ParticleSystem loadShotgunRightPS;
    [SerializeField] private MMF_Player loadShotgunFeedbacks;

    void Start()
    {
        PlayerShoot.Instance.OnPlayerFocusBlastStarted += PlayerSHoor_OnPlayerFocusBlastStarted;
        PlayerShoot.Instance.OnPlayerFocusBlastStopped += PlayerShoot_OnPlayerFocusBlastStopped;
    }

    private void PlayerShoot_OnPlayerFocusBlastStopped(object sender, System.EventArgs e) {
        loadShotgunLeftPS.Stop();
        loadShotgunRightPS.Stop();
        loadShotgunFeedbacks.StopFeedbacks();
    }

    private void PlayerSHoor_OnPlayerFocusBlastStarted(object sender, System.EventArgs e) {
        loadShotgunLeftPS.Play();
        loadShotgunRightPS.Play();
        loadShotgunFeedbacks.PlayFeedbacks();
    }
}
