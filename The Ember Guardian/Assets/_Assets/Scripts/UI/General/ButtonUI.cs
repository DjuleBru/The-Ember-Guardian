using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler {

    public static event EventHandler OnAnyButtonSelected;
    public static event EventHandler OnAnyButtonHovered;
    public static event EventHandler OnAnyButtonUnhovered;

    protected bool buttonSelected;
    protected bool buttonHovered;

    protected virtual void Start() {
        OnAnyButtonHovered += ButtonUI_OnAnyButtonHovered;
        OnAnyButtonSelected += ButtonUI_OnAnyButtonSelected;
    }

    protected virtual void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            buttonSelected = true;
            transform.SetAsLastSibling(); // Amène la carte au-dessus
        }

        if (this != buttonUI && buttonSelected) {
            buttonSelected = false;
        }
    }

    protected virtual void ButtonUI_OnAnyButtonHovered(object sender, EventArgs e) {
        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            buttonHovered = true;
        }

        if (this != buttonUI && buttonHovered) {
            buttonHovered = false;
        }
    }

    #region NAVIGATION

    public void OnSelect(BaseEventData eventData) {
        buttonSelected = true;
        OnAnyButtonSelected?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        OnAnyButtonHovered?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        buttonHovered = false;
    }

    public void OnDeselect(BaseEventData eventData) {
        buttonSelected = false;
    }
    #endregion

    protected virtual void OnDestroy() {
        OnAnyButtonHovered -= ButtonUI_OnAnyButtonHovered;
        OnAnyButtonSelected -= ButtonUI_OnAnyButtonSelected;

    }
}
