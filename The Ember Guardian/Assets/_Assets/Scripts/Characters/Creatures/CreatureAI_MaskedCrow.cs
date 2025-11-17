using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_MaskedCrow : CreatureAI_RangedFlee
{
    [SerializeField] private CreatureAttackSO projectileAttackSO;
    [SerializeField] private CreatureAttackSO jumpAttackSO;
    [SerializeField] private CreatureAnimatorManager animatorManager;

    [SerializeField] private float damageTresholdToSwitchAttack = 0.33f;
    private bool switchedAttackSO;

    protected override void Start() {
        base.Start();
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
        animatorManager.OnCustomEventTrigger += AnimatorManager_OnCustomEventTrigger;
    }

    private void AnimatorManager_OnCustomEventTrigger(object sender, System.EventArgs e) {
        transform.position = (creatureAttack.GetAttackTarget() as MonoBehaviour).transform.position;
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        if (creatureAttack.GetAttackTarget() == null) return;
        if ((creatureAttack.GetAttackTarget() as MonoBehaviour).transform != Player.Instance.transform) return;

        if((float)creature.GetHealth() / (float)creature.GetCreatureMaxHealth() < damageTresholdToSwitchAttack && !switchedAttackSO) {
            switchedAttackSO = true;
            creatureAttack.SetAttackSO(jumpAttackSO);
        }
    }

    protected override void CheckAttackChange() {
    }
}
