using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGameButton_ShowDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Animator descriptionPanelAnimator;

    private void Awake() {
    }

    public void OnDeselect(BaseEventData eventData) {
        descriptionPanelAnimator.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData) {
        descriptionPanelAnimator.gameObject.SetActive(true);
        descriptionPanelAnimator.ResetTrigger("Hide");
        descriptionPanelAnimator.SetTrigger("Show");

    }

    public void OnPointerEnter(PointerEventData eventData) {
        descriptionPanelAnimator.gameObject.SetActive(true);
        descriptionPanelAnimator.ResetTrigger("Hide");
        descriptionPanelAnimator.SetTrigger("Show");

    }

    public void OnPointerExit(PointerEventData eventData) {
        descriptionPanelAnimator.gameObject.SetActive(false);
    }



}
