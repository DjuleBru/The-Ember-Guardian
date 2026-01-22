using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HordeModeMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject lockedTextGO;
    [SerializeField] private Animator descriptionPanelAnimator;

    private Button hordeModeButton;
    private bool hordeModeUnlocked;

    private void Start() {
        hordeModeButton = GetComponent<Button>();
        hordeModeButton.onClick.AddListener(() => {
            descriptionPanelAnimator.ResetTrigger("Show");
            descriptionPanelAnimator.SetTrigger("Hide");
        });

        lockedTextGO.SetActive(false);
        descriptionPanelAnimator.gameObject.SetActive(false);

        if (!MetaProgressionManager.Instance.GetHordeModeUnlocked()) {
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
