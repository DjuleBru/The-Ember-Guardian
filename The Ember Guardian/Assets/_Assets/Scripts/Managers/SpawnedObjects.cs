using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObjects : MonoBehaviour
{

    public static SpawnedObjects Instance;
    public Transform creaturesContainer;
    public Transform workersContainer;
    public Transform AnimalsContainer;

    private void Awake() {
        Instance = this;
    }
}
