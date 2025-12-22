using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGameButton_ShowDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Animator descriptionPanelAnimator;
    private bool savedOnce;

    private void Start() {
        if (MetaProgressionManager.Instance.GetSavedOnce()) {
            savedOnce = true;
        }
    }

    public void OnDeselect(BaseEventData eventData) {
        if (savedOnce) return;

        descriptionPanelAnimator.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData) {
        if (savedOnce) return;
        descriptionPanelAnimator.gameObject.SetActive(true);
        descriptionPanelAnimator.ResetTrigger("Hide");
        descriptionPanelAnimator.SetTrigger("Show");

    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (savedOnce) return;
        descriptionPanelAnimator.gameObject.SetActive(true);
        descriptionPanelAnimator.ResetTrigger("Hide");
        descriptionPanelAnimator.SetTrigger("Show");

    }

    public void OnPointerExit(PointerEventData eventData) {
        if (savedOnce) return;
        descriptionPanelAnimator.gameObject.SetActive(false);
    }



}
