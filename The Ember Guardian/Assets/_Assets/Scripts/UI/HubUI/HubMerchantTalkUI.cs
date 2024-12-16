using Febucci.UI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubMerchantTalkUI : MonoBehaviour
{
    [SerializeField] private HubMerchant hubMerchant;
    [SerializeField] private GameObject talkPanelUIGameObject;
    [SerializeField] private TextMeshProUGUI talkText;
    [SerializeField] private TypewriterCore typeWriter;
    [SerializeField] private GameObject continueGameObject;
    [SerializeField] private Image continueInputImage;

    [SerializeField] private MerchantTextLinesSO textLinesSO;
    [SerializeField] private List<string> merchantTalkLines;

    public static event EventHandler OnAnyMerchantShowNewTalkLine;
    public static event EventHandler OnAnyMerchantEndTalk;

    private bool playerIsTalkingToMerchant;
    private bool currentDialogLineShown;
    private bool showShopAfterDialog = true;
    private int talkLinesIndex;

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;

        hubMerchant.OnPlayerStartedTalkingWithHubMerchant += HubMerchant_OnPlayerStartedTalkingWithHubMerchant;

        merchantTalkLines = textLinesSO.merchantTextLines;
        talkPanelUIGameObject.SetActive(false);
        continueGameObject.SetActive(false);
        continueInputImage.sprite = InputControlIcons.Instance.GetControlIconSprite(InputControlIcons.Control.Interact)[0];
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        continueInputImage.sprite = InputControlIcons.Instance.GetControlIconSprite(InputControlIcons.Control.Interact)[0];
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerIsTalkingToMerchant) return;

        if(currentDialogLineShown) {

            talkLinesIndex++;

            if (talkLinesIndex == merchantTalkLines.Count) {
                talkText.text = "";
                playerIsTalkingToMerchant = false;
                hubMerchant.SetPlayerFinishedTalkingWithMerchant(showShopAfterDialog);
                talkPanelUIGameObject.SetActive(false);
                CameraManager.Instance.ZoomOut(true, 1f);
                OnAnyMerchantEndTalk?.Invoke(this, EventArgs.Empty);
            }
            else {
                OnAnyMerchantShowNewTalkLine?.Invoke(this, EventArgs.Empty);
                talkText.text = merchantTalkLines[talkLinesIndex];
                continueGameObject.SetActive(false);
                currentDialogLineShown = false;
            }

        } else {
            typeWriter.SkipTypewriter();
            continueGameObject.SetActive(true);
        }

    }

    private void HubMerchant_OnPlayerStartedTalkingWithHubMerchant(object sender, System.EventArgs e) {
        StartCoroutine(StartTalkingToMerchantCoroutine());
    }

    public void SetTalkingWithMerchant(MerchantTextLinesSO textLinesSO, bool showShopAfterDialog) {
        this.showShopAfterDialog = showShopAfterDialog;
        merchantTalkLines = textLinesSO.merchantTextLines;
        StartCoroutine(StartTalkingToMerchantCoroutine());
    }

    private IEnumerator StartTalkingToMerchantCoroutine() {
        CameraManager.Instance.ZoomIn(false, 1.5f, 2f);
        CameraManager.Instance.ChangeCameraTarget(hubMerchant.GetCameraFocusTransform());

        yield return new WaitForSeconds(1.5f);

        talkPanelUIGameObject.SetActive(true);
        playerIsTalkingToMerchant = true;
        currentDialogLineShown = false;

        talkLinesIndex = 0;
        talkText.text = merchantTalkLines[talkLinesIndex];

        OnAnyMerchantShowNewTalkLine?.Invoke(this, EventArgs.Empty);
    }

    public HubMerchant.HubMerchantType GetHubMerchantType() {
        return hubMerchant.GetHubMerchantType();
    }

    public void SetCurrentDialogLineShown() {
        currentDialogLineShown = true;
        continueGameObject.SetActive(true);
    }
}
