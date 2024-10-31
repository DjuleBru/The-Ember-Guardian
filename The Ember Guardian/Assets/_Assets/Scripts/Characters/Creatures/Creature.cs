using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] private CreatureSO creatureSO;

    private void OnEnable() {
        CreaturesManager.Instance.AddCreatureSpawned(this);
        health = creatureSO.maxHealth;
    }

    public override void Die() {
        base.Die();

        StartCoroutine(DisableGameObjectAfterDelay());

        CreaturesManager.Instance.RemoveCreatureSpawned(this);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    private IEnumerator DisableGameObjectAfterDelay() {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    public CreatureSO GetCreatureSO() {
        return creatureSO;
    }
}
