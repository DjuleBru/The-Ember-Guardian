using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    [SerializeField] private bool isFirstCreatureBlockingCollider;
    [SerializeField] private bool isStopMusicCollider;
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
        }

        if(isStopMusicCollider && !playerCollided) {
            playerCollided = true;
            MusicManager.Instance.FadeOutMusic();
        }
    }

    public void SetColliderTrigger() {
        tutorialCollider.isTrigger = true;
    }
}
