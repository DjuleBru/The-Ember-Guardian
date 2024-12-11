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

        // Vérifie le bouton sélectionné par la manette ou le clavier
        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected != null && selected.GetComponent<Button>() != null) {
            hoveredButtonUI = selected.GetComponent<RectTransform>();
            previousSelectedButtonUI = hoveredButtonUI;
        }

        //// Vérifie si la souris survole un bouton
        //PointerEventData pointerData = new PointerEventData(EventSystem.current) {
        //    position = Input.mousePosition
        //};

        //var results = new System.Collections.Generic.List<RaycastResult>();
        //EventSystem.current.RaycastAll(pointerData, results);

        //foreach (var result in results) {
        //    Button hoveredButton = result.gameObject.GetComponentInParent<Button>();

        //    Debug.Log(hoveredButton);
        //    if (hoveredButton != null) {
        //        hoveredButtonUI = hoveredButton.GetComponent<RectTransform>();
        //        previousSelectedButtonUI = hoveredButtonUI;
        //        break;
        //    }
        //}

        // Ajuster la position du ScrollRect en fonction du bouton actuel
        if (previousSelectedButtonUI != null) {
            AdjustScrollPosition();
        }
    }

    private void AdjustScrollPosition() {
        // Récupérer la largeur du Content et de la Vue
        float contentWidth = scrollRect.content.rect.width;
        float viewportWidth = scrollRect.viewport.rect.width;

        // Récupérer la position globale du bouton dans le content
        float buttonLocalPositionX = previousSelectedButtonUI.localPosition.x;

        // Ajuster cette position pour la recentrer dans le viewport
        float centeredPositionX = buttonLocalPositionX - viewportWidth / 2f;

        // Calculer la position normalisée, ajustée pour les limites du scroll
        float normalizedPosition = Mathf.Clamp01((centeredPositionX + contentWidth / 2f) / (contentWidth - viewportWidth));

        // Appliquer la position au ScrollRect
        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            normalizedPosition,
            Time.deltaTime * smoothSpeed);
    }

}
