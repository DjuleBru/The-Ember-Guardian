using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    void TakeDamage(int damage, Transform damageSource, bool crit = false, bool ignoreTemporaryInvincibility = false, bool weakSpotHit = false);

    void Die();

    Transform GetProjectileTarget();
    Transform GetMeleeAttackPosition();
}
