using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    [SerializeField] private Transform wishlistButton;
    [SerializeField] private Animator videoTipUIMainPanelAnimator;

    [SerializeField] private VideoTipSO testTipSO;
    private VideoTipSO shownVideoTipSO;

    private bool tipFinishedDisplaying;
    private bool panelOpen;
    private bool dontShowDebugMode;
    private List<string> tipDescriptionKeyList;
    private List<TipDescriptionTextTemplate> tipDescriptionTextTemplateList = new List<TipDescriptionTextTemplate>();
    private List<float> tipTextDelayToShowList;

    private Coroutine activeCoroutine;

    public event EventHandler OnVideoTipPanelOpened;
    public event EventHandler<OnVideoTipPanelClosedEventArgs> OnVideoTipPanelClosed;

    public class OnVideoTipPanelClosedEventArgs : EventArgs {
        public VideoTipSO.VideoTipType tipTypeShown;
    }

    private void Awake() {
        Instance = this;
        wishlistButton.gameObject.SetActive(false);
    }

    private void Start() {
        dontShowDebugMode = DebugManager.Instance.GetDebugMode_DontShowVideoTips();

        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        videoTipUIMainPanel.SetActive(false);
        replayTipButtonGO.GetComponent<Button>().interactable = false;
    }

    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!panelOpen) return;
        SkipTipOrResumeButton();
    }

    private void Update() {
        //if (Input.GetKeyDown(KeyCode.V)) {
        //    PlayTipSO(testTipSO);
        //    OpenPanel();
        //    PlayTip();
        //}
    }

    public void SetEndDemoTip() {
       StartCoroutine(SetEndDemoTipCoroutine());
    }

    private IEnumerator SetEndDemoTipCoroutine() {
        RectTransform rt = videoTipUIMainPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); // Centre en X, bas en Y
        rt.anchorMax = new Vector2(0.5f, 0.5f);

        // Modifier la position X en prenant en compte les anchors
        Vector2 newAnchoredPos = rt.anchoredPosition;
        newAnchoredPos.x = 0; // Nouvelle position X
        rt.anchoredPosition = newAnchoredPos;

        yield return new WaitForSecondsRealtime(5f);

        EventSystem.current.SetSelectedGameObject(wishlistButton.gameObject);
        wishlistButton.gameObject.SetActive(true);
    }

    public void PlayTipSO(VideoTipSO videoTipSO, float delayToPlayTip = 0f) {
        shownVideoTipSO = videoTipSO;
        videoPlayer.clip = videoTipSO.tipClip;
        tipName.text = LocalizationManager.Instance.GetLocalizedText(videoTipSO.tipNameLocalizationKey);

        tipDescriptionKeyList = videoTipSO.tipTextLocalizationKeys;
        tipTextDelayToShowList = videoTipSO.tipTextDelayToShowList;

        RefreshTipDescription();

        if (delayToPlayTip == 0) {
            OpenPanel();
            PlayTip(false);
        } else {
            StartCoroutine(PlayTipAfterDelay(delayToPlayTip));
        }
    }

    private IEnumerator PlayTipAfterDelay(float delay) {
        yield return new WaitForSecondsRealtime(delay);

        OpenPanel();
        PlayTip(false);
    }

    private void RefreshTipDescription() {
        tipDescriptionTextTemplateList.Clear();
        tipTextTemplate.gameObject.SetActive(false);

        foreach (Transform child in tipTextContainer) {
            if(child == tipTextTemplate) continue;
            Destroy(child.gameObject);
        }

        int i = 0;
        foreach(string key in tipDescriptionKeyList) {
            TipDescriptionTextTemplate tipTemplateText = Instantiate(tipTextTemplate, tipTextContainer).GetComponent<TipDescriptionTextTemplate>();
            tipTemplateText.SetTipDescriptionAdvanced(LocalizationManager.Instance.GetLocalizedText(key), i == 0);
            i++;

            tipDescriptionTextTemplateList.Add(tipTemplateText);
        }
    }

    public void PlayTip(bool setReplayTipButtonInteractable = false) {

        tipFinishedDisplaying = false;

        videoPlayer.Play();
        if(activeCoroutine != null) {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(ShowTipTextList());
        replayTipButtonGO.GetComponent<Button>().interactable = setReplayTipButtonInteractable;
    }

    private void OpenPanel() {
        if (dontShowDebugMode) return;

        OnVideoTipPanelOpened?.Invoke(this, EventArgs.Empty);

        Time.timeScale = 0f;
        panelOpen = true;
        videoTipUIMainPanel.SetActive(true);
        videoTipUIMainPanelAnimator.ResetTrigger("Hide");
        videoTipUIMainPanelAnimator.SetTrigger("Show");
        EventSystem.current.SetSelectedGameObject(resumeButtonGO);
    }

    private IEnumerator ShowTipTextList() {
        int i = 0;

        resumeButtonText.text = LocalizationManager.Instance.GetLocalizedText("menu_skip");

        foreach (TipDescriptionTextTemplate textTemplate in tipDescriptionTextTemplateList) {

            float delayToWait = tipTextDelayToShowList[i];
            if (i != 0) {
                delayToWait -= tipTextDelayToShowList[i - 1];
            }

            yield return new WaitForSecondsRealtime(delayToWait);
            textTemplate.ShowTipText();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tipTextContainer.GetComponent<RectTransform>());

            i++;
        }
        yield return new WaitForSecondsRealtime(1f);

        replayTipButtonGO.GetComponent<Button>().interactable = true;
        tipFinishedDisplaying = true;
        resumeButtonText.text = LocalizationManager.Instance.GetLocalizedText("menu_resume");
        activeCoroutine = null;
    }

    private IEnumerator ActivatePanelAfterDelay(bool show, float delay) {
        yield return new WaitForSecondsRealtime(delay);
        videoTipUIMainPanel.SetActive(show);
    }

    public void ClosePanel() {
        Time.timeScale = 1f;
        panelOpen = false;
        videoTipUIMainPanelAnimator.ResetTrigger("Show");
        videoTipUIMainPanelAnimator.SetTrigger("Hide");
        StartCoroutine(ActivatePanelAfterDelay(false, .5f));

        OnVideoTipPanelClosed?.Invoke(this, new OnVideoTipPanelClosedEventArgs {
            tipTypeShown = shownVideoTipSO.tipType
        });
    }

    #region BUTTONS

    public void SkipTipOrResumeButton() {
        if(!tipFinishedDisplaying) {
            StopCoroutine(activeCoroutine);

            foreach (TipDescriptionTextTemplate textTemplate in tipDescriptionTextTemplateList) {
                textTemplate.ShowTipText();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(tipTextContainer.GetComponent<RectTransform>());
            videoPlayer.Stop();
            videoPlayer.Play();

            resumeButtonText.text = LocalizationManager.Instance.GetLocalizedText("menu_resume");
            replayTipButtonGO.GetComponent<Button>().interactable = true;
            tipFinishedDisplaying = true;
        } else {
            ClosePanel();
        }
    }

    public void ReplayTip() {
        if(activeCoroutine != null) {
            StopCoroutine(activeCoroutine);
        }
        foreach (TipDescriptionTextTemplate textTemplate in tipDescriptionTextTemplateList) {
            textTemplate.HideTipText();
        }
        videoPlayer.Stop();
        PlayTip(true);
    }

    #endregion

    private void OnDestroy() {
        GameInput.Instance.OnPlayerBackPerformed -= GameInput_OnPlayerBackPerformed;
    }
}
