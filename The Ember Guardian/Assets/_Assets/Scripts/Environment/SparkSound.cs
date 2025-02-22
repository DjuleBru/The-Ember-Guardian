using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkSound : SoundObject {

    [SerializeField] private AudioClip[] audioClips;
    private ElectricitySparkPS electricitySpark;

    private void Awake() {
        electricitySpark = GetComponent<ElectricitySparkPS>();
        electricitySpark.OnSparkTriggered += ElectricitySpark_OnSparkTriggered;
    }

    private void ElectricitySpark_OnSparkTriggered(object sender, System.EventArgs e) {
        PlaySound2D(audioClips);
    }
}
