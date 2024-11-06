using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] private CreatureSO creatureSO;
    private bool enteredLight;

    public event EventHandler OnCreatureEnteredLight;
    public event EventHandler OnCreatureExitedLight;

    private void OnEnable() {
        CreaturesManager.Instance.AddCreatureSpawned(this);
        health = creatureSO.maxHealth;
    }

    public override void Die() {
        CreaturesManager.Instance.RemoveCreatureSpawned(this);

        base.Die();

        StartCoroutine(DisableGameObjectAfterDelay());

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

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponentInParent<Fire>() != null) {
            if (enteredLight) return;
            OnCreatureEnteredLight?.Invoke(this, EventArgs.Empty);
            enteredLight = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponentInParent<Fire>() != null) {
            if (!enteredLight) return;
            OnCreatureExitedLight?.Invoke(this, EventArgs.Empty);
            enteredLight = false;
        }
    }
}
