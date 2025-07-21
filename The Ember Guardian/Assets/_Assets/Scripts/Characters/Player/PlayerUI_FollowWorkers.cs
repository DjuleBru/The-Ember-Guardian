using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_FollowWorkers : MonoBehaviour
{
    [SerializeField] private GameObject followWorkersGO;
    [SerializeField] private Image followWorkersFillImage;

    private bool commandWorkersHeldDown;

    private void Start() {
        GameInput.Instance.OnCommandWorkerHeldDownStarted += GameInput_OnCommandWorkerHeldDownStarted;
        GameInput.Instance.OnCommandWorkerHeldDown += GameInput_OnCommandWorkerHeldDown;
        followWorkersGO.SetActive(false);
    }

    private void Update() {
        if (!commandWorkersHeldDown) return;
        followWorkersFillImage.fillAmount = GameInput.Instance.GetCommandWorkersHoldDownTimerNormalized();
    }

    private void GameInput_OnCommandWorkerHeldDownStarted(object sender, System.EventArgs e) {
        if (WorkerFollowPlayerHandler.Instance.GetFollowingWorkers().Count == 0) return;

        followWorkersGO.SetActive(true);
        commandWorkersHeldDown = true;
    }

    private void GameInput_OnCommandWorkerHeldDown(object sender, System.EventArgs e) {
        followWorkersGO.SetActive(false);
        commandWorkersHeldDown = false;
    }
}
