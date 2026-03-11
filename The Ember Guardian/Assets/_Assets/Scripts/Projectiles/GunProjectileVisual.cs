using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileVisual : MonoBehaviour
{
    private GunProjectile gunProjectile;
    private Animator animator;
    [SerializeField] private int explodeVariantCount = 1;
    [SerializeField] private GunProjectile_Bullet bullet;

    private void Awake() {
        animator = GetComponent<Animator>();
        gunProjectile = GetComponentInParent<GunProjectile>();
        gunProjectile.OnProjectileExploded += GunProjectile_OnProjectileExploded;

        if(bullet != null) {
            bullet.OnProjectileFadedOut += Bullet_OnProjectileFadedOut;
        }
    }

    private void Bullet_OnProjectileFadedOut(object sender, System.EventArgs e) {
        animator.SetTrigger("Explode");
    }

    private void GunProjectile_OnProjectileExploded(object sender, System.EventArgs e) {
        int randomIndex = Random.Range(0, explodeVariantCount);
        animator.SetInteger("ExplodeIndex", randomIndex);
        animator.SetTrigger("Explode");
    }
}
