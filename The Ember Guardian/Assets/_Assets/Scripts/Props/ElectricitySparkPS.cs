using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricitySparkPS : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparkPS;
    [SerializeField] private float probabilityToTriggerSparks = .5f;

    public event EventHandler OnSparkTriggered;
    public void TriggerPS() {
        float randomFloat = UnityEngine.Random.Range(0f, 1f);

        if(randomFloat < probabilityToTriggerSparks) {
            int randomSparkAmount = UnityEngine.Random.Range(5, 10);
            sparkPS.Emit(randomSparkAmount);
            OnSparkTriggered?.Invoke(this, EventArgs.Empty);
        }

    }
}
