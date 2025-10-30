using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectsPS : MonoBehaviour
{
    private ParticleSystem ps;
    private void Start() {
        ps = GetComponent<ParticleSystem>();    
        if (WindManager.Instance != null) {
            WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;
            RefreshPSEmission(LevelManager.Instance.GetLevelSO().initialWindStrength);
        }
    }

    private void WindManager_OnWindStrengthChanged(object sender, System.EventArgs e) {
        RefreshPSEmission(WindManager.Instance.GetWindStrength());
    }

    private void RefreshPSEmission(WindManager.WindStrength windStrength) {
        ParticleSystem.EmissionModule emissionModule = ps.emission;

        if (windStrength == WindManager.WindStrength.medium || windStrength == WindManager.WindStrength.strong || windStrength == WindManager.WindStrength.extreme) {
            emissionModule.rateOverTime = 0;
        }
        else {
            emissionModule.rateOverTime = 8;
        }
    }
}
