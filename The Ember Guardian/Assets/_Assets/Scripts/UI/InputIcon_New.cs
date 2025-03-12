using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIcon_New : MonoBehaviour
{

    [SerializeField] private Image inputImage1;
    [SerializeField] private Image inputImage2;

    [SerializeField] private InputControlIcons.Control control;

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        RefreshInput();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshInput();
    }

    private void RefreshInput() {
        if(inputImage2 == null) {
            inputImage1.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[0];
        } else {
            inputImage1.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[0];
            inputImage2.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[1];

        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
