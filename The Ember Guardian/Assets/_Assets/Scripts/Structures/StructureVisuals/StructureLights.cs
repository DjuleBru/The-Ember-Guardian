using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLights : MonoBehaviour
{
    private void Awake() {
        if(transform.position.x <0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
