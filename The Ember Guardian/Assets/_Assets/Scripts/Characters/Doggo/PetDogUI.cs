using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDogUI : MonoBehaviour
{
    [SerializeField] private PetDog petCollider;
    [SerializeField] private Animator petDogUIAnimator;
    [SerializeField] private Animator spamPressAnimator;
    [SerializeField] private GameObject petDogTxt;
    [SerializeField] private GameObject petDogIcon;
    [SerializeField] private GameObject petDogSpamPressIcon;

    private Coroutine petDogLoopCoroutine;

    private void Awake() {
        petCollider.OnPlayerTriggeredIn += PetCollider_OnPlayerTriggeredIn;
        petCollider.OnPlayerTriggeredOut += PetCollider_OnPlayerTriggeredOut;
        petCollider.OnPlayerCantPetDog += PetCollider_OnPlayerCantPetDog;
        petCollider.OnPlayerStartedPettingDog += PetCollider_OnPlayerStartedPettingDog;
        petCollider.OnPlayerStoppedPettingDog += PetCollider_OnPlayerStoppedPettingDog;
    }

    private void Start() {
        PetDog.Instance.OnPlayerRefreshedPettingDog += PetDog_OnPlayerRefreshedPettingDog;
        petDogSpamPressIcon.SetActive(false);
    }

    private void PetDog_OnPlayerRefreshedPettingDog(object sender, System.EventArgs e) {
        petDogSpamPressIcon.gameObject.SetActive(false);

        if(petDogLoopCoroutine != null) {
            StopCoroutine(petDogLoopCoroutine);
        }
        petDogLoopCoroutine = StartCoroutine(ShowSpamPressButtonAfterDelay(.5f));
    }

    private void PetCollider_OnPlayerStoppedPettingDog(object sender, System.EventArgs e) {
        StartCoroutine(StoppedPettingDogCoroutine());
    }

    private IEnumerator StoppedPettingDogCoroutine() {

        petDogSpamPressIcon.gameObject.SetActive(false);
        petDogUIAnimator.ResetTrigger("Show");
        petDogUIAnimator.SetTrigger("Hide");
        spamPressAnimator.ResetTrigger("SpamPress");
        spamPressAnimator.SetTrigger("Idle");

        yield return new WaitForSeconds(.3f);

        petDogTxt.gameObject.SetActive(true);
        petDogIcon.gameObject.SetActive(true);

    }

    private void PetCollider_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        //petDogUIAnimator.ResetTrigger("Show");
        //petDogUIAnimator.SetTrigger("Hide");
        petDogSpamPressIcon.gameObject.SetActive(false);
        petDogTxt.gameObject.SetActive(false);
        petDogIcon.gameObject.SetActive(false);

        StartCoroutine(ShowSpamPressButtonAfterDelay(1f));
    }

    private IEnumerator ShowSpamPressButtonAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        petDogSpamPressIcon.gameObject.SetActive(true);
        spamPressAnimator.ResetTrigger("Idle");
        spamPressAnimator.SetTrigger("SpamPress");
    }

    private void PetCollider_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        petDogUIAnimator.ResetTrigger("Show");
        petDogUIAnimator.SetTrigger("Hide");
    }
    private void PetCollider_OnPlayerCantPetDog(object sender, System.EventArgs e) {
        petDogUIAnimator.ResetTrigger("Show");
        petDogUIAnimator.SetTrigger("Hide");
    }

    private void PetCollider_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        petDogUIAnimator.ResetTrigger("Hide");
        petDogUIAnimator.SetTrigger("Show");
    }
}
