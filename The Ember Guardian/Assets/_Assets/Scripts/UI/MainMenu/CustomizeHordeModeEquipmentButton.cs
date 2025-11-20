using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomizeHordeModeEquipmentButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material customizingMaterial;
    [SerializeField] private List<SpriteRenderer> customizableSpriteRendererList;

    private bool customizable;
    private bool selected;

    public void SetCustomizable(bool customizable) {
        this.customizable = customizable;
        if(!customizable) {
            gameObject.SetActive(false);
        }
    }

    public void SetCustomizing(bool customizing) {
        if (!customizable) return;

        if (customizing) {
            SetRendererMaterial(customizingMaterial);
            SetMaterialFloatAlpha(.5f);
        }
        else {
            SetRendererMaterial(emptyMaterial);
        }
    }

    public void SetSelected(bool selected) {
        if (!customizable) return;

        this.selected = selected;

        if(selected) {
            SetMaterialFloatAlpha(1f);

        } else {
            SetMaterialFloatAlpha(0);

        }
    }

    public void OnDeselect(BaseEventData eventData) {
        if (!customizable) return;
        if (selected) return;
        SetMaterialFloatAlpha(.5f);
    }

    public void OnSelect(BaseEventData eventData) {
        if (!customizable) return;
        SetMaterialFloatAlpha(1f);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!customizable) return;
        SetMaterialFloatAlpha(1f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!customizable) return;
        if (selected) return;
        SetMaterialFloatAlpha(.5f);
    }

    private void SetMaterialFloatAlpha(float alpha) {
        foreach(SpriteRenderer renderer in  customizableSpriteRendererList) {
            renderer.material.SetFloat("_OutlineAlpha", alpha);
        }
    }
    private void SetRendererMaterial(Material mat) {
        foreach (SpriteRenderer renderer in customizableSpriteRendererList) {
            renderer.material = mat;
        }
    }
}
