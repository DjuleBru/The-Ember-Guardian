using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponReticleSprite : MonoBehaviour
{
    [SerializeField] private Image hitReticleSpriteRenderer;
    [SerializeField] private Animator hitCursorAnimator;
    [SerializeField] private Color aimingCritZoneColor;
    [SerializeField] private Color aimingEnemyColor;

    private float showHitReticleTimer;
    private float showHitReticleTime = .2f;
    private bool showingHitReticle;

    private void Start() {
        ParticleCollision.OnAnyPlayerBulletHitEnemy += ParticleCollision_OnAnyPlayerBulletHitEnemy;
        ParticleCollision.OnAnyPlayerBulletHitEnemyCrit += ParticleCollision_OnAnyBulletHitEnemyCrit;
    }

    private void ParticleCollision_OnAnyBulletHitEnemyCrit(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        if (!gameObject.activeInHierarchy) return;

        StartCoroutine(ResetHitReticleColorAfterDelay(.3f));
        hitReticleSpriteRenderer.color = aimingCritZoneColor;
        ShowHitReticle();
    }

    private void ParticleCollision_OnAnyPlayerBulletHitEnemy(object sender, ParticleCollision.OnBulletHitEventArgs e) {
        if (!gameObject.activeInHierarchy) return;

        StartCoroutine(ResetHitReticleColorAfterDelay(.01f));
        ShowHitReticle();
    }


    private IEnumerator ResetHitReticleColorAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        hitReticleSpriteRenderer.color = Color.white;
    }

    private void Update() {
        if(showingHitReticle) {
            showHitReticleTimer -= Time.deltaTime;
            if(showHitReticleTimer < 0) {
                HideHitReticle();
            }
        }

        //if (PlayerAim.Instance.GetIsAimingCritZone()) {
        //    spriteRenderer.color = aimingCritZoneColor;
        //    return;
        //}

        //if (PlayerAim.Instance.GetIsAimingCreature()) {
        //    spriteRenderer.color = aimingEnemyColor;
        //    return;
        //}

        //spriteRenderer.color = Color.white;
    }

    private void ShowHitReticle() {
        hitCursorAnimator.ResetTrigger("Hide");
        hitCursorAnimator.SetTrigger("Show");
        showingHitReticle = true;
        showHitReticleTimer = showHitReticleTime;

    }
    private void HideHitReticle() {
        hitCursorAnimator.ResetTrigger("Show");
        hitCursorAnimator.SetTrigger("Hide");
        showingHitReticle = false;
    }

    private void OnDestroy() {
        ParticleCollision.OnAnyPlayerBulletHitEnemy -= ParticleCollision_OnAnyPlayerBulletHitEnemy;
        ParticleCollision.OnAnyPlayerBulletHitEnemyCrit -= ParticleCollision_OnAnyBulletHitEnemyCrit;
    }

}
