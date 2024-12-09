using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBreathAnimator : MonoBehaviour
{

    public event EventHandler OnPantTriggered;
    public void PantEvent() {
        OnPantTriggered?.Invoke(this, EventArgs.Empty);
    }
}
