using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{
    private Projectile projectile;
    [SerializeField] private ParticleSystem projectileTrailPS;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem projectileHitPS;
    [SerializeField] private Animator projectileAnimator;

    private bool projectileHasHit;

    private void Awake() {
        projectile = GetComponentInParent<Projectile>();
    }

    private void Start() {
        projectile.OnProjectileHit += Projectile_OnProjectileHit;
        projectile.OnProjectileReset += Projectile_OnProjectileReset;

        float direction = projectile.GetTrajectoryEndPoint().x - transform.position.x;
        if ((direction) < 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Projectile_OnProjectileReset(object sender, System.EventArgs e) {
        projectileHasHit = false;

        if (projectileTrailPS != null) {
            projectileTrailPS.Play();
        }

        if (projectileHitPS != null) {
            projectileHitPS.Stop();
        }

        if (projectileAnimator != null) {
            projectileAnimator.SetTrigger("Reset");
            spriteRenderer.enabled = true;
        }
        else {
            spriteRenderer.enabled = true;
        }
    }

    private void Projectile_OnProjectileHit(object sender, System.EventArgs e) {

        projectileHasHit = true;

        if(projectileTrailPS != null) {
            projectileTrailPS.Stop();
        }

        if (projectileHitPS != null) {
            projectileHitPS.Play();
        }

        if(projectileAnimator != null) {
            projectileAnimator.SetTrigger("Hit");
        } else {
            spriteRenderer.enabled = false;
        }
    }

    private void Update() {
        if (projectileHasHit) return;
        UpdateProjectileRotation();
    }

    private void UpdateProjectileRotation() {
        Vector3 projectileDir = projectile.GetProjectileMoveDir();

        transform.rotation = LookAtTarget(projectileDir);
    }

    private Quaternion LookAtTarget(Vector2 moveDir) {
        return Quaternion.Euler(0, 0, Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg);
    }
}
