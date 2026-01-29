using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Button_Confirm : MonoBehaviour, IPointerExitHandler {

    private bool buttonPressedOnce;
    private Button button;

    public event EventHandler OnButtonDeselected;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            PressButton();
        });
    }

    void Update() {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected != null && currentSelected != gameObject && buttonPressedOnce) // Vérifie si ce bouton est sélectionné
        {
            buttonPressedOnce = false;
            DeselectButton();
        }
    }

    private void DeselectButton() {
        OnButtonDeselected?.Invoke(this, EventArgs.Empty);
        // Ton action spécifique
    }

    public void PressButton() {
        if(!buttonPressedOnce) {
            buttonPressedOnce = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnButtonDeselected?.Invoke(this, EventArgs.Empty);
    }
}