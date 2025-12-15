using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HordeModeMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject lockedTextGO;
    [SerializeField] private Animator descriptionPanelAnimator;

    private bool hordeModeUnlocked;
    private void Start() {
        lockedTextGO.SetActive(false);
        descriptionPanelAnimator.gameObject.SetActive(false);
        if (!MetaProgressionManager.Instance.GetTutorialCompletedOrSkipped()) {
            hordeModeUnlocked = false;
        } else {
            hordeModeUnlocked = true;
        }
    }

    public void OnDeselect(BaseEventData eventData) {
        if (hordeModeUnlocked) {
            descriptionPanelAnimator.gameObject.SetActive(false);
            return;
        };
        lockedTextGO.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (hordeModeUnlocked) {
            descriptionPanelAnimator.gameObject.SetActive(true);
            descriptionPanelAnimator.ResetTrigger("Hide");
            descriptionPanelAnimator.SetTrigger("Show");
            return;
        };
        lockedTextGO.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData) {
        if (hordeModeUnlocked) {
            descriptionPanelAnimator.gameObject.SetActive(false);
            return;
        };
        lockedTextGO.SetActive(false);

    }

    public void OnSelect(BaseEventData eventData) {
        if (hordeModeUnlocked) {
            descriptionPanelAnimator.gameObject.SetActive(true);
            descriptionPanelAnimator.ResetTrigger("Hide");
            descriptionPanelAnimator.SetTrigger("Show");
            return;
        };
        lockedTextGO.SetActive(true);

    }
}
