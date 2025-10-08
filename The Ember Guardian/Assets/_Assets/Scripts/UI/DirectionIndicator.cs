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

    private void Awake() {
        Instance = this;
        showHideAnimator = GetComponent<Animator>();
    }

    private void Update() {
        if(directionIsBeingShown) {

            if(GameInput.Instance.GetMovementFloatNormalized() * direction > 0) {
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
    }

    private void HideDirection() {
        showHideAnimator.SetTrigger("Hide");
    }
}
