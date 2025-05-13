using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerStatusEffectVisuals : MonoBehaviour
{
    [SerializeField] private WorkerAttack workerAttack;
    [SerializeField] private ParticleSystem attackSpeedBuffPS;
    [SerializeField] private ParticleSystem attackSpeedTriggerBuffPS;

    private void Start() {
        workerAttack.OnAttackSpeedModified += WorkerAttack_OnAttackSpeedModified;
    }

    private void WorkerAttack_OnAttackSpeedModified(object sender, System.EventArgs e) {
        float attackSpeedBuff = workerAttack.GetAttackSpeedBuff();
        if(attackSpeedBuff > 1) {
            attackSpeedBuffPS.Play();
            attackSpeedTriggerBuffPS.Play();
        } else {
            attackSpeedBuffPS.Stop();
        }
    }
}
