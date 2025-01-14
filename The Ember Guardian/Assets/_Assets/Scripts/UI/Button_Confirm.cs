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
    public void OnPointerExit(PointerEventData eventData) {
        if (!buttonPressedOnce) return;

        buttonPressedOnce = false;
        YourFunction();
    }

    void Update() {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected != gameObject && buttonPressedOnce) // Vérifie si ce bouton est sélectionné
        {
            buttonPressedOnce = false;
            YourFunction();
        }
    }

    private void YourFunction() {
        OnButtonDeselected?.Invoke(this, EventArgs.Empty);
        // Ton action spécifique
    }

    public void PressButton() {
        if(!buttonPressedOnce) {
            buttonPressedOnce = true;
        }
    }
}