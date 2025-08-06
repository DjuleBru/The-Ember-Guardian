using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileVisual : MonoBehaviour
{
    private GunProjectile gunProjectile;
    private Animator animator;
    [SerializeField] private int explodeVariantCount = 1;

    private void Awake() {
        animator = GetComponent<Animator>();
        gunProjectile = GetComponentInParent<GunProjectile>();
        gunProjectile.OnProjectileExploded += GunProjectile_OnProjectileExploded;
    }

    private void GunProjectile_OnProjectileExploded(object sender, System.EventArgs e) {
        int randomIndex = Random.Range(0, explodeVariantCount);
        animator.SetInteger("ExplodeIndex", randomIndex);
        animator.SetTrigger("Explode");
    }
}
