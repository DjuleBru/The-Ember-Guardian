using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    public static PlayerMeleeAttack Instance;

    private bool isMeleeAttacking;
    private float meleeAttackTimer;
    private float meleeAttackCooldown = .75f;

    public event EventHandler OnMeleeAttackStarted;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if(isMeleeAttacking) {
            meleeAttackTimer += Time.deltaTime;
            if(meleeAttackTimer >= meleeAttackCooldown) {
                isMeleeAttacking = false;
                meleeAttackTimer = 0;
            }
        }
    }

    private void Start() {
        GameInput.Instance.OnMeleeAttackPerformed += GameInput_OnMeleeAttackPerformed;
    }

    private void GameInput_OnMeleeAttackPerformed(object sender, System.EventArgs e) {
        if (isMeleeAttacking) return;
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (!PlayerShoot.Instance.GetHeldGunSO().canUseMeleeAttack) return;

        isMeleeAttacking = true;
        OnMeleeAttackStarted?.Invoke(this, EventArgs.Empty);
    }
}
