using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ember : Collectible
{
    [SerializeField] private GameObject emphasizePositionGameObject;

    protected override void Awake() {
        base.Awake();
        emphasizePositionGameObject.SetActive(false);
    }

    public void EmphasizePosition() {
        emphasizePositionGameObject.SetActive(true);
    }
}
