using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    [SerializeField] private bool isFirstCreatureBlockingCollider;
    [SerializeField] private bool isFirstSpotlightCollider;
    [SerializeField] private bool isStopMusicCollider;
    [SerializeField] private bool isLightTipCollider;
    [SerializeField] private bool isRollTipCollider;
    [SerializeField] private bool isEndLevelAreaCollider;
    [SerializeField] private bool isEndLevelAreaBlockingCollider;
    [SerializeField] private bool isExtractEmberBlockingCollider;
    [SerializeField] private bool isFirstEnterHubBlockingCollider;
    [SerializeField] private bool isWorkerCampCollider;
    [SerializeField] private bool isWorkerCampCollider_Demo;
    [SerializeField] private bool isDogTipCollider;
    [SerializeField] private bool isRunTipCollider;

    private Tutorial tutorial;
    private Collider2D tutorialCollider;

    private bool playerCollided;

    public static event EventHandler OnRollTipCollided;
    public static event EventHandler OnRecruitWorkerTipCollided;

    private void Awake() {
        tutorial = GetComponentInParent<Tutorial>();
        tutorialCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        if (isDogTipCollider && !playerCollided) {
            playerCollided = true;
            tutorial.ShowDogTip();
        }

        if (isFirstSpotlightCollider && !playerCollided) {
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

        if (isRollTipCollider && !playerCollided) {
            playerCollided = true;
            OnRollTipCollided?.Invoke(this, EventArgs.Empty);
        }

        if (isEndLevelAreaCollider && !playerCollided) {
            playerCollided = true;
            StartCoroutine(FoundNestCoroutine());
        }

        if(isFirstEnterHubBlockingCollider && !playerCollided) {
            playerCollided = true;
            HUBManager.Instance.PlayerEnteredHubFirstTime();
        }

        if (isWorkerCampCollider && !playerCollided) {
            playerCollided = true;
            OnRecruitWorkerTipCollided?.Invoke(this, EventArgs.Empty);
        }

        if (isWorkerCampCollider_Demo && !playerCollided) {
            playerCollided = true;
            DemoMainLevelManager.Instance.TryShowRecruitWorkerTooltip();
        }

        if (isRunTipCollider && !playerCollided) {
            playerCollided = true;
            DemoLevelIntroManager.Instance.ShowRunTooltip();
        }
    }

    public void SetColliderTrigger() {
        tutorialCollider.isTrigger = true;
    }

    public void SetColliderSolid() {
        tutorialCollider.isTrigger = false;
    }

    private IEnumerator FoundNestCoroutine() {
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.FindNest);
        
        yield return new WaitForSeconds(4f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.DestroyNest);
        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveTypes = new List<LevelUI_ObjectiveUI.SubObjectiveType> { LevelUI_ObjectiveUI.SubObjectiveType.ClearNest };
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveTypes);
    }
}
