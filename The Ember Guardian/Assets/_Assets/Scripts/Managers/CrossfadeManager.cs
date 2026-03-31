using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossfadeManager : MonoBehaviour
{
    public static CrossfadeManager Instance;
    [SerializeField] private Animator animator;

    private void Awake() {
        Instance = this;
    }

    public void StartCrossfade() {
        animator.ResetTrigger("End");
        animator.SetTrigger("Start");
    }
    public void EndCrossfade() {
        animator.ResetTrigger("Start");
        animator.SetTrigger("End");
    }
}
