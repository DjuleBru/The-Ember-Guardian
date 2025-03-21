using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCameraFollowTarget : MonoBehaviour
{

    [SerializeField] private Transform followTargetTransform;
    [SerializeField] private float xDelta;
    [SerializeField] private float yPosition;

    void Update()
    {
        Vector3 newPosition = new Vector3(xDelta + followTargetTransform.position.x, yPosition, - 10f);
        transform.position = newPosition;
    }

}
