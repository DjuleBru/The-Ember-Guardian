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
    [SerializeField] private bool isMiner;
    [SerializeField] private bool isEngineer;

    private void Start() {
        WorkerManager.Instance.OnHunterAssignedSide += Instance_OnHunterAssignedSide;
        WorkerManager.Instance.OnGuardAssignedSide += Instance_OnGuardAssignedSide;
        WorkerAI.OnAnyWorkerAssignedJob += WorkerAI_OnAnyWorkerAssignedJob;
        Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;

        RefreshDisplayedAmounts();

        if(isHunter || isGuard) {
            RefreshEnabledControlsGO();
        }

    }

    private void WorkerAI_OnAnyWorkerAssignedJob(object sender, System.EventArgs e) {
        if (isMiner || isEngineer) {
            RefreshDisplayedAmounts();
        }
    }

    private void Worker_OnAnyWorkerDied(object sender, System.EventArgs e) {
        RefreshDisplayedAmounts();
    }

    private void Instance_OnGuardAssignedSide(object sender, System.EventArgs e) {
        if(isGuard) {
            RefreshDisplayedAmounts();
            RefreshEnabledControlsGO();
        }
    }

    private void Instance_OnHunterAssignedSide(object sender, System.EventArgs e) {
        if(isHunter) {
            RefreshDisplayedAmounts();
            RefreshEnabledControlsGO();
        }
    }

    private void RefreshDisplayedAmounts() {
        if(isHunter) {
            leftHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetLeftHuntersAssignedAmount().ToString();
            rightHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetRightHuntersAssignedAmount().ToString();
        }

        if(isGuard) {
            leftHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetLeftGuardsAssignedAmount().ToString();
            rightHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetRightGuardsAssignedAmount().ToString();
        }

        if (isEngineer) {
            leftHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetEngineersAmount().ToString();
        }

        if (isMiner) {
            leftHuntersAssignedAmountText.text = "x " + WorkerManager.Instance.GetMinersAmount().ToString();
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

    private void OnDestroy() {
        Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;
        WorkerAI.OnAnyWorkerAssignedJob -= WorkerAI_OnAnyWorkerAssignedJob;
    }
}
