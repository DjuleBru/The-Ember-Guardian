using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileVisual : MonoBehaviour
{
    private GunProjectile gunProjectile;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
        gunProjectile = GetComponentInParent<GunProjectile>();
        gunProjectile.OnProjectileExploded += GunProjectile_OnProjectileExploded;
    }

    private void GunProjectile_OnProjectileExploded(object sender, System.EventArgs e) {
        animator.SetTrigger("Explode");
    }
}
