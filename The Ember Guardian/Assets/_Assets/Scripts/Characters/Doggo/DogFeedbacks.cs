using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogFeedbacks : MonoBehaviour
{
    [SerializeField] private DogAI_DarkCompanion darkCompanion;
    [SerializeField] private MMF_Player stompFeedbacks;

    private void Start() {
        darkCompanion.OnStompAbilityStarted += DarkCompanion_OnStompAbilityStarted;
    }

    private void DarkCompanion_OnStompAbilityStarted(object sender, System.EventArgs e) {
        StartCoroutine(PlayStompFeedbacksAfterDelay());
    }

    private IEnumerator PlayStompFeedbacksAfterDelay() {
        yield return new WaitForSeconds(.7f);
        stompFeedbacks.PlayFeedbacks();
    }
}
