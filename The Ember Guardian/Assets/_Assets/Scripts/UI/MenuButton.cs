using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler {
    public static event EventHandler OnAnyMenuButtonHovered;
    public static event EventHandler OnAnyMenuButtonPressed;

    protected Button button;
    protected bool buttonHoverable = true;
    protected bool isSelected;

    protected void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            OnAnyMenuButtonPressed?.Invoke(this, EventArgs.Empty);
            isSelected = true;
        });
    }

    protected void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;

        if (button != null && isSelected) {
            isSelected = false;
            button.OnPointerExit(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (GameInput.Instance.IsUsingGamepad()) {
            // force sortie immédiate
            button.OnPointerExit(eventData);
            return;
        }

        if (!button.interactable) return;
        if (!buttonHoverable) return;

        OnAnyMenuButtonHovered?.Invoke(this, EventArgs.Empty);
    }

    public void OnPointerExit(PointerEventData eventData) {
        // Désélectionner le bouton si le pointeur sort et qu'il n'est pas activement utilisé par le gamepad
        if (button != null && EventSystem.current != null && !isSelected) {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnSelect(BaseEventData eventData) {
        if (!button.interactable || !buttonHoverable) return;

        // Déclencher l'événement lorsque le bouton est sélectionné via gamepad
        OnAnyMenuButtonHovered?.Invoke(this, EventArgs.Empty);
        isSelected = true;
    }


    public void OnDeselect(BaseEventData eventData) {
        isSelected = false;
    }


    protected void OnDestroy() {
        if(GameInput.Instance != null) {
            GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
        }

    }
}
