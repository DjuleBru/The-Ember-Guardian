using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapAnimatorTriggerEvents : MonoBehaviour
{
    public event EventHandler OnTrapTriggerSFX;

    public void TriggerTrapSFX() {
        OnTrapTriggerSFX?.Invoke(this, EventArgs.Empty);
    }
}
