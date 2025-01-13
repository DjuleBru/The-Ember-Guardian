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

    private Tutorial tutorial;
    private Collider2D tutorialCollider;

    private bool playerCollided;

    private void Awake() {
        tutorial = GetComponentInParent<Tutorial>();
        tutorialCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        if(isFirstSpotlightCollider && !playerCollided) {
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
            tutorial.ShowRollTip();
        }

        if (isEndLevelAreaCollider && !playerCollided) {
            playerCollided = true;
            StartCoroutine(FoundNestCoroutine());
        }

        if(isFirstEnterHubBlockingCollider && !playerCollided) {
            playerCollided = true;
            HUBManager.Instance.PlayerEnteredHubFirstTime();
        }
    }

    public void SetColliderTrigger() {
        Debug.Log(gameObject + " " + "SetColliderTrigger");

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
