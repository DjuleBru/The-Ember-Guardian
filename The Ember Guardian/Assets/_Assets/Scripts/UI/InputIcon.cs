using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIcon : MonoBehaviour
{
    [SerializeField] private Image controllerInputSpriteRenderer;
    [SerializeField] private Image keyboardInputSpriteRenderer;

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        RefreshInput();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshInput();
    }

    private void RefreshInput() {

        if (GameInput.Instance.IsUsingGamepad()) {
            keyboardInputSpriteRenderer.gameObject.SetActive(false);
            controllerInputSpriteRenderer.gameObject.SetActive(true);
        }
        else {
            keyboardInputSpriteRenderer.gameObject.SetActive(true);
            controllerInputSpriteRenderer.gameObject.SetActive(false);
        }
    }
}
