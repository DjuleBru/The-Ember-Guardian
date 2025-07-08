using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureDamageCollider : MonoBehaviour
{
    [SerializeField] private MobAttack mobAttack;
    [SerializeField] private CreatureDamageColliderParent creatureDamageColliderParent;

    [SerializeField] private int maxImpactAmountInSingleAnimation = 1;
    private int impactAmountOnPlayerInSingleAnimation;


    private void Start() {
        mobAttack.OnMobAttack += MobAttack_OnMobAttack;
    }

    

    private void MobAttack_OnMobAttack(object sender, System.EventArgs e) {
        impactAmountOnPlayerInSingleAnimation = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        IDamageable iDamageable = collision.GetComponent<IDamageable>();

        if (iDamageable != null) {
            Creature creature = (iDamageable as MonoBehaviour).GetComponent<Creature>();

            //Check if attack hits another creature
            if (creature != null) return;

            Player player = (iDamageable as MonoBehaviour).GetComponent<Player>();

            if(player != null) {
                if (creatureDamageColliderParent.PlayerJustExitedCollider()) return;
                // Attack hit player
                if (impactAmountOnPlayerInSingleAnimation >= maxImpactAmountInSingleAnimation) return;

                if (impactAmountOnPlayerInSingleAnimation != 0) {
                    DealDamageToUntargetedIDamageable(iDamageable, true);
                } else {
                    DealDamageToUntargetedIDamageable(iDamageable, false);
                }

                impactAmountOnPlayerInSingleAnimation++;

            } else {
                // Attack hit worker, barricade or fire

                mobAttack.DealDamage();
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.GetComponent<Player>() != null) {
            creatureDamageColliderParent.SetPlayerJustExitedCollider();
        }
    }

    private void DealDamageToUntargetedIDamageable(IDamageable iDamageable, bool ignoreTemporaryInvincibility) {
        // Function required because mob attack doesn't deal damage if no target set.
        int attackDamage = mobAttack.GetAttackDamage();
        iDamageable.TakeDamage(attackDamage, transform, false, ignoreTemporaryInvincibility);
        mobAttack.InvokeAttackHit();

        Barricade barricade = iDamageable as Barricade;
        if(barricade != null) {
            if (!barricade.GetBarricadeSpiked()) return;
            mobAttack.GetComponent<Mob>().TakeDamage(barricade.GetSpikeDamage(), barricade.transform);
        }
    }
}
