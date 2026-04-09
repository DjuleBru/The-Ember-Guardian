using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HuntersAssignedUI : MonoBehaviour
{
    
    [SerializeField] private ReassignWorkersShrineFunction reassignHunters;

    [SerializeField] private GameObject leftHuntersAssignedAmountGO;
    [SerializeField] private GameObject rightHuntersAssignedAmountGO;
    [SerializeField] private GameObject rightHuntersAssignedAmountControlGO;
    [SerializeField] private GameObject leftHuntersAssignedAmountControlGO;
    [SerializeField] private TextMeshProUGUI leftHuntersAssignedAmountText;
    [SerializeField] private TextMeshProUGUI rightHuntersAssignedAmountText;

    [SerializeField] private bool isHunter;
    [SerializeField] private bool isGuard;

    private void Start() {
        WorkerManager.Instance.OnHunterAssignedSide += Instance_OnHunterAssignedSide;
        WorkerManager.Instance.OnGuardAssignedSide += Instance_OnGuardAssignedSide;
        RefreshHunterAmounts();
        RefreshEnabledControlsGO();
    }

    private void Instance_OnGuardAssignedSide(object sender, System.EventArgs e) {
        if(isGuard) {
            RefreshHunterAmounts();
            RefreshEnabledControlsGO();
        }
    }

    private void Instance_OnHunterAssignedSide(object sender, System.EventArgs e) {
        if(isHunter) {
            RefreshHunterAmounts();
            RefreshEnabledControlsGO();
        }
    }

    private void RefreshHunterAmounts() {
        if(isHunter) {
            leftHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetLeftHuntersAssignedAmount().ToString();
            rightHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetRightHuntersAssignedAmount().ToString();
        }
        if(isGuard) {
            leftHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetLeftGuardsAssignedAmount().ToString();
            rightHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetRightGuardsAssignedAmount().ToString();
        }

    }

    private void RefreshEnabledControlsGO() {
        int leftAssignedAmount = WorkerManager.Instance.GetLeftHuntersAssignedAmount();
        if(isGuard) {
            leftAssignedAmount = WorkerManager.Instance.GetLeftGuardsAssignedAmount();
        }

        int rightAssignedAmount = WorkerManager.Instance.GetRightHuntersAssignedAmount();
        if (isGuard) {
            rightAssignedAmount = WorkerManager.Instance.GetRightGuardsAssignedAmount();
        }

        if (leftAssignedAmount == 0) {
            rightHuntersAssignedAmountControlGO.SetActive(false);
        }
        else {
            rightHuntersAssignedAmountControlGO.SetActive(true);
        }

        if (rightAssignedAmount == 0) {
            leftHuntersAssignedAmountControlGO.SetActive(false);
        }
        else {
            leftHuntersAssignedAmountControlGO.SetActive(true);
        }
    }
}
