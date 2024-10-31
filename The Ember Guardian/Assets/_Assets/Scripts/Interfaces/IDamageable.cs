using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    void TakeDamage(int damage, Vector3 damageSourcePosition);
    void Die();

    Transform GetProjectileTarget();
    Transform GetMeleeAttackPosition();
}
