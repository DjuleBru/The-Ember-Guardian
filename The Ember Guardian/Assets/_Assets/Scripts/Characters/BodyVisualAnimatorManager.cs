using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyVisualAnimatorManager : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Mob mob;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        mob.OnMobDamageTaken += Mob_OnMobDamageTaken;
    }

    private void Mob_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        animator.SetTrigger("Hit");
    }
}
