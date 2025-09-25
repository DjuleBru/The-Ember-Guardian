using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VideoTipUI_TipTemplate : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [SerializeField] private TextMeshProUGUI tipNameText;

    private Button button;
    private VideoTipSO videoTipSO;

    bool tipUnlocked;
    bool tipNewlyUnlocked;
    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            PlayVideoTipSO();
        });
    }

    public void SetVideoTipSO(VideoTipSO videoTipSO) {
        this.videoTipSO = videoTipSO;
        tipUnlocked = MetaProgressionManager.Instance.GetTipUnlocked(videoTipSO);
        tipNewlyUnlocked = MetaProgressionManager.Instance.GetTipNewlyUnlocked(videoTipSO);

        tipNameText.text = "";

        if (tipNewlyUnlocked) {
            tipNameText.text += "! ";
        }
        tipNameText.text += LocalizationManager.Instance.GetLocalizedText(videoTipSO.tipNameLocalizationKey);

        RefreshFontMaterial();
    }

    private void PlayVideoTipSO() {
        if (!tipUnlocked) return;
        VideoTipUI.Instance.PlayTipSO(videoTipSO, 0, false, true, false);
    }

    public void OnSelect(BaseEventData eventData) {
        VideoTipUI_ManualPanel.Instance.SetLastSelectedTipButton(GetComponent<Button>());
        RefreshNewlyUnlocked();
    }

    private void RefreshNewlyUnlocked() {

        if (tipNewlyUnlocked) {
            MetaProgressionManager.Instance.SetTipNewlyUnlocked(videoTipSO, false);
            tipNameText.text = LocalizationManager.Instance.GetLocalizedText(videoTipSO.tipNameLocalizationKey);
            tipNewlyUnlocked = false;
        }

        RefreshFontMaterial();

    }

    private void RefreshFontMaterial() {
        if (tipNewlyUnlocked) {
            tipNameText.fontMaterial = LocalizationManager.Instance.GetBlueGlowMaterial();
        }
        else {
            tipNameText.fontMaterial = LocalizationManager.Instance.GetStandardMaterial();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        RefreshNewlyUnlocked();
    }
}
