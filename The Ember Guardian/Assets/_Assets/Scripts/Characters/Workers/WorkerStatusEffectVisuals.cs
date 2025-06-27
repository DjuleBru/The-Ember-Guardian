using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerStatusEffectVisuals : MonoBehaviour
{
    [SerializeField] private WorkerAttack workerAttack;
    [SerializeField] private ParticleSystem attackSpeedBuffPS;
    [SerializeField] private ParticleSystem attackSpeedTriggerBuffPS;

    private int minPSRate = 0;
    private int maxPSRate = 25;
    private float maxAttackSpeedRate = 1f;

    private void Start() {
        workerAttack.OnAttackSpeedModified += WorkerAttack_OnAttackSpeedModified;
    }

    private void WorkerAttack_OnAttackSpeedModified(object sender, System.EventArgs e) {
        float attackSpeedBuff = workerAttack.GetAttackSpeedBuff();

        if(attackSpeedBuff > 1) {
            float t = Mathf.Clamp01((attackSpeedBuff - 1) / maxAttackSpeedRate);
            float rate = Mathf.Lerp(minPSRate, maxPSRate, t);

            Debug.Log("buff amount " + attackSpeedBuff);
            Debug.Log("t " + t);
            Debug.Log("rate " + rate);

            ParticleSystem.EmissionModule emissionModule = attackSpeedBuffPS.emission;
            emissionModule.rateOverTime = rate;
            attackSpeedBuffPS.Play();
            attackSpeedTriggerBuffPS.Play();

        } else {

            attackSpeedBuffPS.Stop();

        }
    }
}
