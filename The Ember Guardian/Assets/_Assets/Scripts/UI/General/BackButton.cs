using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    [SerializeField] private Image backShortcut;

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        backShortcut.sprite = InputControlIcons.Instance.GetControlIconSprite(InputControlIcons.Control.Back)[0];
    }
}
