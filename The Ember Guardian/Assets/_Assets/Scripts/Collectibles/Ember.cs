using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ember : Collectible
{
    [SerializeField] private GameObject emphasizePositionGameObject;

    protected override void Awake() {
        base.Awake();
    }
    public override void SetMovingForPayment(bool moving, float smoothTime = 1f, Transform destination = null) {
        base.SetMovingForPayment(moving, smoothTime, destination);

        if(moving) {
            emphasizePositionGameObject.SetActive(false);
        } else {
            emphasizePositionGameObject.SetActive(true);
        }
    }
}
