using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservationTowerVisual : StructureVisual
{
    [SerializeField] private Animator observationTowerAnimator;
    [SerializeField] private Animator observationTowerTopMaterialAnimator;
    [SerializeField] private ObservationTower observationTower;

    protected override void Start() {
        base.Start();

        observationTower.OnObservationTowerActivated += ObservationTower_OnObservationTowerActivated;
        observationTower.OnObservationTowerDeActivated += ObservationTower_OnObservationTowerDeActivated;
    }
    protected override void HandleInitialBuildAnimation() {
        base.HandleInitialBuildAnimation();

        if (!animateSpriteMaterialOnBuild) {
            observationTowerTopMaterialAnimator.SetTrigger("BuiltAtStart");

        }
        else {
            observationTowerTopMaterialAnimator.SetTrigger("Build");
        }
    }

    private void ObservationTower_OnObservationTowerDeActivated(object sender, System.EventArgs e) {
        observationTowerAnimator.SetTrigger("Off");
    }

    private void ObservationTower_OnObservationTowerActivated(object sender, System.EventArgs e) {
        observationTowerAnimator.SetTrigger("On");
    }
}
