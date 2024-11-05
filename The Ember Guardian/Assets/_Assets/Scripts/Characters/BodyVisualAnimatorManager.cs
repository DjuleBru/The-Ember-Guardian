using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyVisualAnimatorManager : MonoBehaviour
{
    protected Animator animator;
    [SerializeField] protected Mob mob;

    protected virtual void Awake() {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start() {
        mob.OnMobDamageTaken += Mob_OnMobDamageTaken;
    }

    protected void Mob_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        animator.SetTrigger("Hit");
    }
}
