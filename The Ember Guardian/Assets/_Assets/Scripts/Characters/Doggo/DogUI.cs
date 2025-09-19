using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DogUI : MonoBehaviour
{
    private Animator dogUIAnimator;
    [SerializeField] private TextMeshProUGUI dogUIStatText;

    private void Awake() {
        dogUIAnimator = GetComponent<Animator>();
    }

    private void Start() {
        if (Dog.Instance == null) return;

        Dog.Instance.OnIdleStateChanged += Instance_OnIdleStateChanged;
        dogUIStatText.font = LocalizationManager.Instance.GetCurrentFont();
    }

    private void Instance_OnIdleStateChanged(object sender, System.EventArgs e) {
        dogUIAnimator.SetTrigger("Show");

        if (Dog.Instance.GetIdleState() == DogAI.State.stay) {
            dogUIStatText.text = LocalizationManager.Instance.GetLocalizedText("dog_follow");
        }

        if (Dog.Instance.GetIdleState() == DogAI.State.walkWithPlayer) {
            dogUIStatText.text = LocalizationManager.Instance.GetLocalizedText("dog_stay");
        }
    }

}
