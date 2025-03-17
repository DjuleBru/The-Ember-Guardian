using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : ButtonUI
{
    private Slider slider;

    private void Awake() {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener((value) => {
            TriggerSFXController();
        });
    }

    private void TriggerSFXController() {
        if(GameInput.Instance.IsUsingGamepad()) {
            InvokeOnAnyButtonPressed();
        }
    }
}
