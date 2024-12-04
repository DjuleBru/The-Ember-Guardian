using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    [SerializeField] private bool isFirstCreatureBlockingCollider;
    [SerializeField] private bool isStopMusicCollider;
    [SerializeField] private bool isLightTipCollider;
    [SerializeField] private bool isEndLevelAreaCollider;

    private Tutorial tutorial;
    private Collider2D tutorialCollider;

    private bool playerCollided;
    private void Awake() {
        tutorial = GetComponentInParent<Tutorial>();
        tutorialCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        if(isFirstCreatureBlockingCollider && !playerCollided) {
            playerCollided = true;
            tutorial.TransitionToCombatCamera();
            tutorial.ActivateCreatureSpotLight();
        }

        if(isStopMusicCollider && !playerCollided) {
            playerCollided = true;
            tutorial.StartSetupCampObjective();
            MusicManager.Instance.FadeOutMusic(5f);
        }

        if(isLightTipCollider && !playerCollided) {
            playerCollided = true;
            tutorial.ShowLightTip();
        }

        if (isEndLevelAreaCollider && !playerCollided) {
            playerCollided = true;
            LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.DestroyNest);
        }
    }

    public void SetColliderTrigger() {
        tutorialCollider.isTrigger = true;
    }
}
