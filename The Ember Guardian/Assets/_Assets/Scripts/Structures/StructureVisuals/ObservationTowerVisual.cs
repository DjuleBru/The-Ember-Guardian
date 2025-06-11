using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservationTowerVisual : StructureVisual
{
    [SerializeField] private Animator observationTowerAnimator;
    [SerializeField] private Animator observationTowerTopMaterialAnimator;
    [SerializeField] private ObservationTower observationTower;

    [SerializeField] private SpriteRenderer baseSpriteRenderer;
    [SerializeField] private SpriteRenderer lightsSpriteRenderer;
    [SerializeField] private Sprite level2BaseSprite;
    [SerializeField] private Sprite level3BaseSprite;
    [SerializeField] private Sprite level2LightsSprite;
    [SerializeField] private Sprite level3LightsSprite;

    [SerializeField] private Transform topTransform;
    [SerializeField] private float level2TopPosition;
    [SerializeField] private float level3TopPosition;
    [SerializeField] private RectTransform orbTemplateUI;
    [SerializeField] private float level2OrbTemplateUIPosition;
    [SerializeField] private float level3OrbTemplateUIPosition;

    private bool enemyTypesDetectionUnlocked;
    private bool enemyAmountDetectionUnlocked;

    protected override void Start() {
        base.Start();

        observationTower.OnObservationTowerActivated += ObservationTower_OnObservationTowerActivated;
        observationTower.OnObservationTowerDeActivated += ObservationTower_OnObservationTowerDeActivated;

        enemyTypesDetectionUnlocked = StructureStats.Instance.GetObservationTowerEnemyTypesDetectionUnlocked();
        enemyAmountDetectionUnlocked = StructureStats.Instance.GetObservationTowerEnemyAmountDetectionUnlocked();

        if (enemyAmountDetectionUnlocked) {
            topTransform.localPosition = new Vector3(topTransform.localPosition.x, level3TopPosition);
            orbTemplateUI.anchoredPosition = new Vector2(orbTemplateUI.anchoredPosition.x, level3OrbTemplateUIPosition);

            baseSpriteRenderer.sprite = level3BaseSprite;
            lightsSpriteRenderer.sprite = level3LightsSprite;
            return;
        }

        if (enemyTypesDetectionUnlocked) {
            topTransform.localPosition = new Vector3(topTransform.localPosition.x, level2TopPosition);
            orbTemplateUI.anchoredPosition = new Vector2(orbTemplateUI.anchoredPosition.x, level2OrbTemplateUIPosition);

            baseSpriteRenderer.sprite = level2BaseSprite;
            lightsSpriteRenderer.sprite = level2LightsSprite;
        }
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
        observationTowerAnimator.ResetTrigger("On");
        observationTowerAnimator.SetTrigger("Off");
    }

    private void ObservationTower_OnObservationTowerActivated(object sender, System.EventArgs e) {
        observationTowerAnimator.ResetTrigger("Off");
        observationTowerAnimator.SetTrigger("On");
    }
}
