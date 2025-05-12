using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile_TrapAnimator : MonoBehaviour
{

    private StaticProjectile staticProjectile;
    [SerializeField] private Animator staticProjectileTrapAnimator;

    private void Awake() {
        staticProjectile = GetComponent<StaticProjectile>();
        staticProjectile.OnTrapTriggered += StaticProjectile_OnTrapTriggered;
    }

    private void StaticProjectile_OnTrapTriggered(object sender, System.EventArgs e) {
        staticProjectileTrapAnimator.SetTrigger("Triggered");
    }
}
