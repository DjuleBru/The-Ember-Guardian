using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIcon_New : MonoBehaviour
{

    [SerializeField] private Image inputImage1;
    [SerializeField] private Image inputImage2;
    [SerializeField] private SpriteRenderer inputSpriteRenderer1;
    [SerializeField] private SpriteRenderer inputSpriteRenderer2;

    [SerializeField] private InputControlIcons.Control control;

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        RefreshInput();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshInput();
    }

    private void RefreshInput() {
        if(inputImage1 != null) {
            // Image UI
            if (inputImage2 == null) {
                inputImage1.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[0];
            }
            else {
                inputImage1.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[0];
                inputImage2.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[1];
            }
        }
        if (inputSpriteRenderer1 != null) {
            // SpriteRenderer
            if (inputSpriteRenderer2 == null) {
                inputSpriteRenderer1.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[0];
            }
            else {
                inputSpriteRenderer1.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[0];
                inputSpriteRenderer2.sprite = InputControlIcons.Instance.GetControlIconSprite(control)[1];
            }
        }

        LayoutGroup layoutGroup = GetComponentInParent<LayoutGroup>();
        if (layoutGroup != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
