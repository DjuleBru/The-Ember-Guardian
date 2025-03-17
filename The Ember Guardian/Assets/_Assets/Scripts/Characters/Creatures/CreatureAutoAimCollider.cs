using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAutoAimCollider : MonoBehaviour
{
    [SerializeField] private Transform autoAimTransform;

    public Vector3 GetAutoAimPosition() {
        return autoAimTransform.position;
    }
}
