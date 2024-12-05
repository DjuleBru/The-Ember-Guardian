using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyVisualAnimatorManager : MonoBehaviour
{
    protected Animator animator;
    [SerializeField] protected Mob mob;

    private bool critHit;

    protected virtual void Awake() {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start() {
        mob.OnMobDamageTaken += Mob_OnMobDamageTaken;
        mob.OnMobCritDamageTaken += Mob_OnMobCritDamageTaken;
    }

    private void Mob_OnMobCritDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        animator.SetTrigger("CritHit");
        critHit = true;
    }

    protected void Mob_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        if (critHit) {
            critHit = false;
            return;
        };
        animator.SetTrigger("Hit");
    }
}
