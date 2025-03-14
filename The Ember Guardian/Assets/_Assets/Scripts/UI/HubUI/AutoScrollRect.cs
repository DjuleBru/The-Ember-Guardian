using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollRect : MonoBehaviour {

    private ScrollRect scrollRect; // Référence au ScrollRect
    private RectTransform hoveredButtonUI;
    private RectTransform previousSelectedButtonUI;
    private float smoothSpeed = 2f;

    private EventSystem eventSystem; // Référence au système d'événements

    void Start() {
        eventSystem = EventSystem.current; // Récupérer l'EventSystem actif
        scrollRect = GetComponent<ScrollRect>();
    }

    void Update() {

        if (!GameInput.Instance.IsUsingGamepad()) return;

        // Vérifie le bouton sélectionné par la manette ou le clavier
        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected != null && selected.GetComponent<Button>() != null) {
            hoveredButtonUI = selected.GetComponent<RectTransform>();
            previousSelectedButtonUI = hoveredButtonUI;
        }

        // Ajuster la position du ScrollRect en fonction du bouton actuel
        if (previousSelectedButtonUI != null) {
            AdjustScrollPosition();
        }
    }

    private void AdjustScrollPosition() {
        // Récupérer la taille du Content et de la Vue
        float contentWidth = scrollRect.content.rect.width;
        float contentHeight = scrollRect.content.rect.height;
        float viewportWidth = scrollRect.viewport.rect.width;
        float viewportHeight = scrollRect.viewport.rect.height;

        // Récupérer la position locale du bouton dans le Content
        Vector2 buttonLocalPosition = previousSelectedButtonUI.localPosition;
        ItemButtonUI itemButtonUI = previousSelectedButtonUI.GetComponent<ItemButtonUI>();
        if (itemButtonUI != null) {
            buttonLocalPosition.x = itemButtonUI.GetLocalPosition().x;
        }
        // Calculer les positions centrées pour les axes horizontal et vertical
        float centeredPositionX = buttonLocalPosition.x - viewportWidth / 2f;
        float centeredPositionY = buttonLocalPosition.y - viewportHeight / 2f;

        // Calculer les positions normalisées pour chaque axe
        float normalizedPositionX = Mathf.Clamp01((centeredPositionX + contentWidth / 2f) / (contentWidth - viewportWidth));
        float normalizedPositionY = Mathf.Clamp01((centeredPositionY + contentHeight / 2f) / (contentHeight - viewportHeight));

        // Appliquer les positions normalisées au ScrollRect
        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            normalizedPositionX,
            Time.deltaTime * smoothSpeed);

        scrollRect.verticalNormalizedPosition = Mathf.Lerp(
            scrollRect.verticalNormalizedPosition,
            normalizedPositionY,
            Time.deltaTime * smoothSpeed);
    }

}
