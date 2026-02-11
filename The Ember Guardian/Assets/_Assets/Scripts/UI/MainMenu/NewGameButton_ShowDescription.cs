using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGameButton_ShowDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Animator descriptionPanelAnimator;
    private bool savedOnce;
    private bool saveFileDeleted;

    private void Awake() {
        VersioningManager.Instance.OnSaveFileDeleted += VersioningManager_OnSaveFileDeleted;
    }

    private void VersioningManager_OnSaveFileDeleted(object sender, System.EventArgs e) {
        saveFileDeleted = true;
    }

    private void Start() {
        if (saveFileDeleted) return;
        if (MetaProgressionManager.Instance.GetTutorialCompletedOrSkipped()) {
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
