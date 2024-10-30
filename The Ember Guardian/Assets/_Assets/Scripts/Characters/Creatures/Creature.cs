using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Mob
{

    [SerializeField] private CreatureSO creatureSO;

    private void Awake() {
    }

    void Start()
    {
        
    }

    private void OnEnable() {
        CreaturesManager.Instance.AddCreatureSpawned(this);
    }

    public override void Die() {
        base.Die();
        CreaturesManager.Instance.RemoveCreatureSpawned(this);
    }

    public CreatureSO GetCreatureSO() {
        return creatureSO;
    }
}
