using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraWideDetector : MonoBehaviour
{
    public event EventHandler OnUltraWideScreenDetected;
    void Start() {
        float aspectRatio = (float)Screen.width / Screen.height;

        if (aspectRatio >= 2.33f) {
            OnUltraWideScreenDetected?.Invoke(this, EventArgs.Empty);
        }
        else {
            Debug.Log("Écran standard ou large (16:9, 16:10)");
        }
    }
}
