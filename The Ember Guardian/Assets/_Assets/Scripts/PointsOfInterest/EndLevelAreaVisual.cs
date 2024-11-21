using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelAreaVisual : MonoBehaviour
{
    private EndLevelArea endLevelArea;
    [SerializeField] private EndLevelAreaProp endLevelAreaMainProp;
    [SerializeField] private ParticleSystem endLevelAreaAmbientPS;

    private void Awake() {
        endLevelArea = GetComponentInParent<EndLevelArea>();
    }

    private void Start() {
        endLevelAreaMainProp.SetGlow(.5f);
        endLevelArea.OnEndLevelAreaCleared += EndLevelArea_OnEndLevelAreaCleared;
    }

    private void EndLevelArea_OnEndLevelAreaCleared(object sender, System.EventArgs e) {
        endLevelAreaAmbientPS.Stop();
        endLevelAreaMainProp.BurnProp();
    }
}
