using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropFadeOut : MonoBehaviour
{
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void FadeOut() {
        animator.enabled = true;
    }
}
