using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresManager : MonoBehaviour
{
    public static StructuresManager Instance;

    private void Awake() {
        Instance = this;
    }

}
