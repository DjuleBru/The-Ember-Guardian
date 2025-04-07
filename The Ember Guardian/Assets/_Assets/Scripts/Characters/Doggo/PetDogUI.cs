using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDogUI : MonoBehaviour
{
    [SerializeField] private PetDog petCollider;
    [SerializeField] private Animator petDogUIAnimator;

    private void Awake() {
        petCollider.OnPlayerTriggeredIn += PetCollider_OnPlayerTriggeredIn;
        petCollider.OnPlayerTriggeredOut += PetCollider_OnPlayerTriggeredOut;
        petCollider.OnPlayerStartedPettingDog += PetCollider_OnPlayerStartedPettingDog;
    }

    private void PetCollider_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        petDogUIAnimator.ResetTrigger("Show");
        petDogUIAnimator.SetTrigger("Hide");
    }

    private void PetCollider_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        petDogUIAnimator.ResetTrigger("Show");
        petDogUIAnimator.SetTrigger("Hide");
    }

    private void PetCollider_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        petDogUIAnimator.ResetTrigger("Hide");
        petDogUIAnimator.SetTrigger("Show");
    }
}
