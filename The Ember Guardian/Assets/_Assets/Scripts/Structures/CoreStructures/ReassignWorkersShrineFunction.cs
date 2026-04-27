using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReassignWorkersShrineFunction : MonoBehaviour
{
    [SerializeField] private Structure hunterShrine;
    [SerializeField] private bool isHunter;
    [SerializeField] private bool isGuard;


    private void Start() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            gameObject.SetActive(false);
            return;
        }

        GameInput.Instance.OnPlayerRightSwitchPerformed += Instance_OnPlayerRightSwitchPerformed;
        GameInput.Instance.OnPlayerLeftSwitchPerformed += Instance_OnPlayerLeftSwitchPerformed;
    }

    private void Instance_OnPlayerLeftSwitchPerformed(object sender, System.EventArgs e) {
        if(hunterShrine.GetPlayerInTriggerArea()) {
            if(isHunter) {
                WorkerManager.Instance.ReassignHunterToASide(CampZoneManager.CampSide.left);
            }
            if(isGuard) {
                WorkerManager.Instance.ReassignGuardToASide(CampZoneManager.CampSide.left);
            }
        }

    }

    private void Instance_OnPlayerRightSwitchPerformed(object sender, System.EventArgs e) {
        if (hunterShrine.GetPlayerInTriggerArea()) {
            if(isHunter) {
                WorkerManager.Instance.ReassignHunterToASide(CampZoneManager.CampSide.right);
            }

            if(isGuard) {
                WorkerManager.Instance.ReassignGuardToASide(CampZoneManager.CampSide.right);
            }

        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerRightSwitchPerformed -= Instance_OnPlayerRightSwitchPerformed;
        GameInput.Instance.OnPlayerLeftSwitchPerformed -= Instance_OnPlayerLeftSwitchPerformed;
    }
}
