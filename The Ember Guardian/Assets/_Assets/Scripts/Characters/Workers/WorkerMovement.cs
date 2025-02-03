using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerMovement : MobMovement
{

    private WorkerAI workerAI;

    protected override void Awake() {
        base.Awake();
        workerAI = GetComponent<WorkerAI>();    
    }

    protected override void Start() {
        base.Start();

        workerAI.OnWorkerFollowPlayerChanged += WorkerAI_OnWorkerFollowPlayerChanged;
        PlayerMovement.Instance.OnPlayerMovespeedChanged += PlayerMovement_OnPlayerMovespeedChanged;
    }

    private void WorkerAI_OnWorkerFollowPlayerChanged(object sender, System.EventArgs e) {
        if(workerAI.GetFollowingPlayer()) {
            StartCoroutine(SetTargetMoveSpeedAfterDelay(.5f));
        }
    }

    private void PlayerMovement_OnPlayerMovespeedChanged(object sender, System.EventArgs e) {
        if (!GetComponent<WorkerAI>().GetFollowingPlayer()) return;
        StartCoroutine(SetTargetMoveSpeedAfterDelay(.5f));
    }

    private IEnumerator SetTargetMoveSpeedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        float moveSpeedRandomizer = UnityEngine.Random.Range(PlayerMovement.Instance.GetTargetMoveSpeed()/15, PlayerMovement.Instance.GetTargetMoveSpeed()/10);
        SetMoveSpeed(PlayerMovement.Instance.GetTargetMoveSpeed() - moveSpeedRandomizer);

    }

    private void OnDestroy() {
        PlayerMovement.Instance.OnPlayerMovespeedChanged -= PlayerMovement_OnPlayerMovespeedChanged;
    }
}
