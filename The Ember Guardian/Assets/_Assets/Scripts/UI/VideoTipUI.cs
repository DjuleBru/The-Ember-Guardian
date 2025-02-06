using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class VideoTipUI : MonoBehaviour
{
    public static VideoTipUI Instance;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoTipUIMainPanel;
    [SerializeField] private GameObject replayTipButtonGO;
    [SerializeField] private GameObject resumeButtonGO;
    [SerializeField] private TextMeshProUGUI resumeButtonText;

    [SerializeField] private TextMeshProUGUI tipName;
    [SerializeField] private Transform tipTextContainer;
    [SerializeField] private Transform tipTextTemplate;
    [SerializeField] private Animator videoTipUIMainPanelAnimator;

    [SerializeField] private VideoTipSO testTipSO;

    private bool tipFinishedDisplaying;
    private TextSO tipNameTextSO;
    private List<TextSO> tipDescriptionTextSOList;
    private List<TipDescriptionTextTemplate> tipDescriptionTextTemplateList = new List<TipDescriptionTextTemplate>();
    private List<float> tipTextDelayToShowList;

    public event EventHandler OnVideoTipPanelOpened;
    public event EventHandler OnVideoTipPanelClosed;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        videoTipUIMainPanel.SetActive(false);
        replayTipButtonGO.SetActive(false);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.V)) {
            SetTipSO(testTipSO);
            OpenPanel();
            PlayTip();
        }
    }

    public void SetTipSO(VideoTipSO videoTipSO) {
        videoPlayer.clip = videoTipSO.tipClip;
        tipName.text = videoTipSO.tipName.GetTextInLanguage(TextSO.Language.English);

        tipNameTextSO = videoTipSO.tipName;
        tipDescriptionTextSOList = videoTipSO.tipTextList;
        tipTextDelayToShowList = videoTipSO.tipTextDelayToShowList;

        RefreshTipDescription();
    }

    private void RefreshTipDescription() {
        tipDescriptionTextTemplateList.Clear();
        tipTextTemplate.gameObject.SetActive(false);

        foreach (Transform child in tipTextContainer) {
            if(child == tipTextTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(TextSO textSO in tipDescriptionTextSOList) {
            TipDescriptionTextTemplate tipTemplateText = Instantiate(tipTextTemplate, tipTextContainer).GetComponent<TipDescriptionTextTemplate>();
            tipTemplateText.SetTipDescription(textSO.GetTextInLanguage(TextSO.Language.English));
            tipDescriptionTextTemplateList.Add(tipTemplateText);
        }
    }

    public void PlayTip() {
        videoPlayer.Play();
        StartCoroutine(ShowTipTextList());
    }

    private void OpenPanel() {
        videoTipUIMainPanel.SetActive(true);
        videoTipUIMainPanelAnimator.SetTrigger("Show");
        EventSystem.current.SetSelectedGameObject(resumeButtonGO);

        Player.Instance.DisableControlInputs();

        if(DayNightManager.Instance != null) {
            DayNightManager.Instance.SetCyclePaused(true);
        }

        OnVideoTipPanelOpened?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator ShowTipTextList() {
        int i = 0;

        resumeButtonText.text = "Skip";

        foreach (TipDescriptionTextTemplate textTemplate in tipDescriptionTextTemplateList) {

            float delayToWait = tipTextDelayToShowList[i];
            if (i != 0) {
                delayToWait -= tipTextDelayToShowList[i - 1];
            }

            yield return new WaitForSeconds(delayToWait);
            textTemplate.ShowTipText();

            i++;
        }
        yield return new WaitForSeconds(1f);

        replayTipButtonGO.SetActive(true);
        tipFinishedDisplaying = true;
        resumeButtonText.text = "Resume";
    }

    private IEnumerator ActivatePanelAfterDelay(bool show, float delay) {
        yield return new WaitForSeconds(delay);
        videoTipUIMainPanel.SetActive(show);
    }

    public void ClosePanel() {
        videoTipUIMainPanelAnimator.SetTrigger("Hide");
        ActivatePanelAfterDelay(false, .5f);

        Player.Instance.EnableControlInputs();

        if (DayNightManager.Instance != null) {
            DayNightManager.Instance.SetCyclePaused(false);
        }

        OnVideoTipPanelClosed?.Invoke(this, EventArgs.Empty);
    }

    #region BUTTONS

    public void SkipTipOrResumeButton() {
        if(!tipFinishedDisplaying) {
            StopCoroutine(ShowTipTextList());

            foreach (TipDescriptionTextTemplate textTemplate in tipDescriptionTextTemplateList) {
                textTemplate.ShowTipText();
            }
            videoPlayer.Play();

            tipFinishedDisplaying = true;
        } else {
            ClosePanel();
        }
    }

    #endregion
}
