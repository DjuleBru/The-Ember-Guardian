using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorManager_BoneReaper : CreatureAnimatorManager
{
    private CreatureAI_BoneReaper boneReaper;

    protected override void Awake() {
        base.Awake();
        boneReaper = creature.GetComponent<CreatureAI_BoneReaper>();
    }

    protected override void Start() {
        base.Start();
        boneReaper.OnBoneReaperEnraged += BoneReaper_OnBoneReaperEnraged;
        boneReaper.OnBoneReaperSpawned += BoneReaper_OnBoneReaperSpawned;
    }

    private void BoneReaper_OnBoneReaperSpawned(object sender, System.EventArgs e) {
        animator.SetTrigger("Enrage");
    }

    private void BoneReaper_OnBoneReaperEnraged(object sender, System.EventArgs e) {
        animator.SetTrigger("Enrage");
        baseMovementAnimationSpeed *= 1.7f;
        animator.SetFloat("AnimationSpeedMultiplier", 1.7f);
    }

    protected override void HandleXScale() {
        // No X Scale swap
    }
    protected override void HandleScaleChange(float watchDir) {
        // No X Scale swap
    }

    protected override void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        float dirToTarget = (creatureAttack.GetAttackTarget() as MonoBehaviour).transform.position.x - transform.position.x;

        if (boneReaper.GetIsProjectileAttack()) {
            animator.SetTrigger("Projectiles");
        }

        if (boneReaper.GetIsHandsAttack()) {
            if (boneReaper.GetEnraged()) {
                animator.SetTrigger("AttackBoth");
                return;
            }
            if (dirToTarget > 0) {
                animator.SetTrigger("AttackRight");
            }
            else {
                animator.SetTrigger("AttackLeft");
            }
        }

        if (boneReaper.GetIsLaserAttack()) {
            if (boneReaper.GetEnraged()) {
                animator.SetTrigger("Attack_SecondaryBoth");
                return;
            }
            if (dirToTarget > 0) {
                animator.SetTrigger("Attack_SecondaryRight");
            }
            else {
                animator.SetTrigger("Attack_SecondaryLeft");
            }
        }

        if (boneReaper.GetIsSpecialAttack()) {
            animator.SetTrigger("Special");
        }
        
    }

}
