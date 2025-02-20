using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFlies : MonoBehaviour {

    [SerializeField] private int minFireFlyEmitted;
    [SerializeField] private int maxFireFlyEmitted;

    [SerializeField] private ParticleSystem fireflyPS;

    public event EventHandler OnFirefliesSpawned;
    public void SpawnFireflies() {
        int randomInt = UnityEngine.Random.Range(minFireFlyEmitted, maxFireFlyEmitted);
        fireflyPS.transform.position = Player.Instance.transform.position;
        fireflyPS.Emit(randomInt);

        OnFirefliesSpawned?.Invoke(this, EventArgs.Empty);
    }


}
