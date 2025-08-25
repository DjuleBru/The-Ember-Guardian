using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public void OnPointerEnter(PointerEventData eventData) {
        if (GameInput.Instance.IsUsingGamepad()) return;
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (GameInput.Instance.IsUsingGamepad()) return;
        EventSystem.current.SetSelectedGameObject(null);
    }

}
