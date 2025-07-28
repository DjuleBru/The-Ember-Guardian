using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_LandMine : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private CreatureAI_LandMine landMine;

    private void Awake() {
        animator = GetComponent<Animator>();
        landMine.OnMineDig += LandMine_OnMineDig;
    }

    private void LandMine_OnMineDig(object sender, System.EventArgs e) {
        animator.SetTrigger("Dig");
    }
}
