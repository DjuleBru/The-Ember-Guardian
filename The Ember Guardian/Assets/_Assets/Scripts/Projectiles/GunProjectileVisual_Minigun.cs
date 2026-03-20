using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileVisual_Minigun : GunProjectileVisual {
    [SerializeField] protected AnimatorOverrideController explosiveAnimator;

    private void Start() {
        if (PlayerShoot.Instance.GetMinigunExplosiveBulletsActive()) {
            animator.runtimeAnimatorController = explosiveAnimator;
        }
    }
}
