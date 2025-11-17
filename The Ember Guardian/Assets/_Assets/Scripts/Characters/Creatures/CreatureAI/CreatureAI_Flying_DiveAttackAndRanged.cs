using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAI_Flying_DiveAttackAndRanged : CreatureAI_Flying
{
    [SerializeField] private CreatureAttackSO diveAttackSO;
    [SerializeField] private CreatureAttackSO projectileAttackSO;

    private int projectileAttacksPool = 2;
    private int projectileAttacksRemaining = 2;
    private float projectileAttackRefillTime = 15f;
    private float projectileAttackRefillTimer;

    protected override void Start() {
        base.Start();
        creatureAttack.SetAttackSO(projectileAttackSO);

        projectileAttacksRemaining = UnityEngine.Random.Range(1, projectileAttacksPool);
    }

    protected override void Update() {
        base.Update();

        if(projectileAttacksRemaining < projectileAttacksPool) {
            projectileAttackRefillTimer += Time.deltaTime;

            if(projectileAttackRefillTimer > projectileAttackRefillTime) {

                projectileAttackRefillTimer = 0;
                projectileAttacksRemaining++;

                creatureAttack.SetAttackSO(projectileAttackSO);
            }
        }
    }

    public bool GetIsDiveAttack() {
        return creatureAttack.GetCurrentCreatureAttackSO() == diveAttackSO;
    }

    protected override void CreatureAttack_OnMobAttack(object sender, EventArgs e) {
        base.CreatureAttack_OnMobAttack(sender, e);

        if (!GetIsDiveAttack()) {
            projectileAttacksRemaining--;
            if(projectileAttacksRemaining == 0) {
                StartCoroutine(SetDiveAttackAfterDelay());
            }
        }
    }

    private IEnumerator SetDiveAttackAfterDelay() {
        yield return new WaitForSeconds(1f);
        creatureAttack.SetAttackSO(diveAttackSO);

    }

}
