using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileVisual_Pistol : GunProjectileVisual {

    [SerializeField] protected AnimatorOverrideController explosiveAnimator;
    [SerializeField] protected AnimatorOverrideController standardAnimator;

    private void Start() {
        if(PlayerShoot.Instance.GetPistolExplosiveBulletsActive()) {
            animator.runtimeAnimatorController = explosiveAnimator;
        }
    }
}
