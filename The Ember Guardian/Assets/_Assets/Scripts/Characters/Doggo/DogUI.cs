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
    }

    private void Instance_OnIdleStateChanged(object sender, System.EventArgs e) {
        dogUIAnimator.SetTrigger("Show");
        if (Dog.Instance.GetIdleState() == DogAI.State.stay) {
            dogUIStatText.text = "Stay";
        }

        if (Dog.Instance.GetIdleState() == DogAI.State.walkWithPlayer) {
            dogUIStatText.text = "Follow";
        }
    }

}
