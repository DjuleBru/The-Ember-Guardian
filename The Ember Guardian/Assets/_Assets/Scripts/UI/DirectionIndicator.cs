using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionIndicator : MonoBehaviour
{
    public static DirectionIndicator Instance;

    private Animator showHideAnimator;

    private bool directionIsBeingShown;
    private float showTimer;
    private float showTime = 2f;
    private float direction;
    private Vector3 positionToIndicate;

    private void Awake() {
        Instance = this;
        showHideAnimator = GetComponent<Animator>();
    }

    private void Start() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            LevelManager.Instance.OnEndLevelPortalEnabled += LevelManager_OnEndLevelPortalEnabled;
        }
    }

    private void LevelManager_OnEndLevelPortalEnabled(object sender, LevelManager.OnEndLevelPortalEnabledEventArgs e) {
        Debug.Log("LevelManager_OnEndLevelPortalEnabled");
        positionToIndicate = e.endLevelPortalPosition;
        direction = (float)positionToIndicate.x - Player.Instance.transform.position.x;

        if(direction > 0) {
            direction = 1;
        } else {
            direction = -1;
        }

        ShowDirection(direction);
    }


    private void Update() {
        if(directionIsBeingShown) {
            if (GameInput.Instance.GetMovementFloatNormalized() * direction > 0) {
                showTimer += Time.deltaTime; 
                if(showTimer > showTime) {
                    HideDirection();
                    directionIsBeingShown = false;
                }
            }

        }
    }

    public void ShowDirection(float direction) {
        this.direction = direction;
        showHideAnimator.SetTrigger("Show");
        directionIsBeingShown = true;
        showTimer = 0;

        transform.localScale = new Vector3(direction, 1, 1);

    }

    private void HideDirection() {
        showHideAnimator.SetTrigger("Hide");
    }
}
