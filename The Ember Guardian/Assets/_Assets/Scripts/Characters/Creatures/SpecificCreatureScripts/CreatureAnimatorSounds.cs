using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorSounds : MonoBehaviour
{
    public event EventHandler OnAttackStartedCharging; 
    public event EventHandler OnChargedAttackReleased; 

    public void AttackStartedCharging() {
        OnAttackStartedCharging?.Invoke(this, EventArgs.Empty);
    }
    public void ChargedAttackReleased() {
        OnChargedAttackReleased?.Invoke(this, EventArgs.Empty);
    }
}
