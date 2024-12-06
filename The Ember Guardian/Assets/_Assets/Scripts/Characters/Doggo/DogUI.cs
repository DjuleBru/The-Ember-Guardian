using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DogUI : MonoBehaviour
{
    private Dog dog;
    private Animator dogUIAnimator;
    [SerializeField] private TextMeshProUGUI dogUIStatText;
    private void Awake() {
        dog = GetComponentInParent<Dog>();
        dogUIAnimator = GetComponentInParent<Animator>();
        dog.OnIdleStateChanged += Dog_OnIdleStateChanged;
    }

    private void Dog_OnIdleStateChanged(object sender, System.EventArgs e) {
        dogUIAnimator.SetTrigger("Show");

        if(dog.GetIdleState() == DogAI.State.stay) {
            dogUIStatText.text = "Stay";
        }

        if (dog.GetIdleState() == DogAI.State.walkWithPlayer) {
            dogUIStatText.text = "Follow";
        }
    }
}
