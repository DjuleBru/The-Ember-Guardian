using MoreMountains.Feedbacks;
using UnityEngine;

public class CreatureFeedbacks_BoneReaper : CreatureFeedbacks
{
    private CreatureAI_BoneReaper boneReaper;

    [SerializeField] private MMF_Player enragedFeedbacks;
    [SerializeField] private MMF_Player laserAttackFeedbacks;
    [SerializeField] private MMF_Player laserAttackFeedbacks_Enraged;
    [SerializeField] private MMF_Player handAttackFeedbacks;
    [SerializeField] private MMF_Player handAttackFeedbacks_Enraged;
    [SerializeField] private MMF_Player projectileAttackFeedbacks;
    [SerializeField] private MMF_Player specialAttackFeedbacks;
    [SerializeField] private MMF_Player dieFeedbacks;


    protected override void Awake() {
        base.Awake();

        boneReaper = creature.GetComponent<CreatureAI_BoneReaper>();
        boneReaper.OnBoneReaperEnraged += BoneReaper_OnBoneReaperEnraged;
        boneReaper.OnBoneReaperSpawned += BoneReaper_OnBoneReaperEnraged;
    }


    protected override void Creature_OnCreatureDied(object sender, System.EventArgs e) {
        died = true;
        dieFeedbacks.PlayFeedbacks();
    }
    private void BoneReaper_OnBoneReaperEnraged(object sender, System.EventArgs e) {
        enragedFeedbacks.PlayFeedbacks();
    }

    protected override void CreatureAttack_OnMobAttack(object sender, System.EventArgs e) {
        if (boneReaper.GetIsProjectileAttack()) {
            projectileAttackFeedbacks.PlayFeedbacks();
        }

        if (boneReaper.GetIsHandsAttack()) {
            if (boneReaper.GetEnraged()) {
                handAttackFeedbacks.PlayFeedbacks();
            } else {
                handAttackFeedbacks_Enraged.PlayFeedbacks();
            }
        }

        if (boneReaper.GetIsLaserAttack()) {
            if (boneReaper.GetEnraged()) {
                laserAttackFeedbacks_Enraged.PlayFeedbacks();
            } else {
                laserAttackFeedbacks.PlayFeedbacks();
            }
        }

        if (boneReaper.GetIsSpecialAttack()) {
            specialAttackFeedbacks.PlayFeedbacks();
        }
    }
}
