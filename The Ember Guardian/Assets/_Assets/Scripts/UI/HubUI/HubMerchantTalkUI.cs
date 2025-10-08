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
    [SerializeField] private List<string> merchantTalkLinesLocalizationKeys;

    [SerializeField] protected bool DEBUGShowTextLines;

    private Coroutine startTalkingCoroutine;

    public static event EventHandler OnAnyMerchantShowNewTalkLine;
    public static event EventHandler OnAnyMerchantEndTalk;
    public event EventHandler OnMerchantEndTalk;

    private bool merchantHasTalkLinesToShow;
    private bool playerIsTalkingToMerchant;
    private bool currentDialogLineShown;
    private bool showShopAfterDialog = true;
    private int talkLinesIndex;

    private void Awake() {
        talkPanelUIGameObject.SetActive(false);
        continueGameObject.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;

        hubMerchant.OnPlayerStartedTalkingWithHubMerchant += HubMerchant_OnPlayerStartedTalkingWithHubMerchant;
        hubMerchant.OnPlayerTriggeredIn += HubMerchant_OnPlayerTriggeredIn;
        hubMerchant.OnPlayerTriggeredOut += HubMerchant_OnPlayerTriggeredOut;
        hubMerchant.OnPlayerInterruptedInteractingWithHubMerchant += HubMerchant_OnPlayerInterruptedInteractingWithHubMerchant;

        continueInputImage.sprite = InputControlIcons.Instance.GetControlIconSprite(InputControlIcons.Control.Interact)[0];

        TMP_FontAsset font = LocalizationManager.Instance.GetCurrentFont();
        talkText.font = font;

        LoadTalkData();
    }


    private void LoadTalkData() {
        merchantHasTalkLinesToShow = MetaProgressionManager.Instance.GetMerchantHasTalkLinesToShow(hubMerchant.GetHubMerchantType());

        if (merchantHasTalkLinesToShow && !hubMerchant.GetMerchantIsLevelNPC()) {
            showShopAfterDialog = MetaProgressionManager.Instance.GetNextMerchantTextLinesShowShopAfterDialog(hubMerchant.GetHubMerchantType());
            merchantTalkLinesLocalizationKeys = MetaProgressionManager.Instance.GetNextMerchantTextLines(hubMerchant.GetHubMerchantType());
        }

        if (hubMerchant.GetMerchantIsDecorationalDemoMerchant() || hubMerchant.GetMerchantIsFunctionalDemoMerchant() || DEBUGShowTextLines || hubMerchant.GetMerchantIsLevelNPC()) {
            merchantTalkLinesLocalizationKeys = textLinesSO.merchantTextLinesLocalizationKeys;
            showShopAfterDialog = textLinesSO.showShopAfterDialog;
        }
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        continueInputImage.sprite = InputControlIcons.Instance.GetControlIconSprite(InputControlIcons.Control.Interact)[0];
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerIsTalkingToMerchant) return;

        if(currentDialogLineShown) {

            talkLinesIndex++;

            if (talkLinesIndex == merchantTalkLinesLocalizationKeys.Count) {
                EndTalkUI();
                hubMerchant.SetPlayerFinishedTalkingWithMerchant(showShopAfterDialog);
                CameraManager.Instance.ZoomOut(true, 1f);
                OnAnyMerchantEndTalk?.Invoke(this, EventArgs.Empty);
                OnMerchantEndTalk?.Invoke(this, EventArgs.Empty);
            }
            else {
                OnAnyMerchantShowNewTalkLine?.Invoke(this, EventArgs.Empty);
                talkText.text = LocalizationManager.Instance.GetLocalizedText(merchantTalkLinesLocalizationKeys[talkLinesIndex]);
                continueGameObject.SetActive(false);
                currentDialogLineShown = false;
            }

        } else {
            typeWriter.SkipTypewriter();
            continueGameObject.SetActive(true);
        }

    }

    private void HubMerchant_OnPlayerInterruptedInteractingWithHubMerchant(object sender, EventArgs e) {
        if(startTalkingCoroutine != null) {
            StopCoroutine(startTalkingCoroutine);
        }
        EndTalkUI();
        talkLinesIndex = 0;
        currentDialogLineShown = false;
        CameraManager.Instance.ZoomOut(true, 1f);
    }

    private void EndTalkUI() {
        talkText.text = "";
        playerIsTalkingToMerchant = false;
        talkPanelUIGameObject.SetActive(false);
    }

    private void HubMerchant_OnPlayerStartedTalkingWithHubMerchant(object sender, System.EventArgs e) {
        startTalkingCoroutine = StartCoroutine(StartTalkingToMerchantCoroutine());
    }

    private void HubMerchant_OnPlayerTriggeredOut(object sender, EventArgs e) {
        if (hubMerchant.GetMerchantIsDecorationalDemoMerchant() && !hubMerchant.GetMerchantIsFunctionalDemoMerchant()) {
            talkPanelUIGameObject.SetActive(false);
            talkText.text = "";
        }
    }

    private void HubMerchant_OnPlayerTriggeredIn(object sender, EventArgs e) {
        if (hubMerchant.GetMerchantIsDecorationalDemoMerchant() && !hubMerchant.GetMerchantIsFunctionalDemoMerchant()) {
            talkPanelUIGameObject.SetActive(true);
            talkText.text = LocalizationManager.Instance.GetLocalizedText(merchantTalkLinesLocalizationKeys[0]);
            OnAnyMerchantShowNewTalkLine?.Invoke(this, EventArgs.Empty);
            continueGameObject.SetActive(false);
        }
    }

    public void SetTalkingWithMerchant(MerchantTextLinesSO textLinesSO) {
        showShopAfterDialog = textLinesSO.showShopAfterDialog;
        merchantTalkLinesLocalizationKeys = textLinesSO.merchantTextLinesLocalizationKeys;
        hubMerchant.StartTalkingWithMerchant();

        startTalkingCoroutine = StartCoroutine(StartTalkingToMerchantCoroutine());
    }

    public void SetTextLinesSO(MerchantTextLinesSO textLinesSO) {
        this.textLinesSO = textLinesSO;
        showShopAfterDialog = textLinesSO.showShopAfterDialog;
        merchantTalkLinesLocalizationKeys = textLinesSO.merchantTextLinesLocalizationKeys;
        merchantHasTalkLinesToShow = true;
        hubMerchant.SetHasTalkLinesToShow(true);
    }

    public MerchantTextLinesSO GetCurrentTextLineSO() {
        return textLinesSO;
    }

    private IEnumerator StartTalkingToMerchantCoroutine() {
        CameraManager.Instance.ZoomIn(false, 1.5f, 2f);
        CameraManager.Instance.ChangeCameraTarget(hubMerchant.GetCameraFocusTransform());

        yield return new WaitForSeconds(1.5f);

        talkPanelUIGameObject.SetActive(true);
        playerIsTalkingToMerchant = true;
        currentDialogLineShown = false;

        talkLinesIndex = 0;
        talkText.text = LocalizationManager.Instance.GetLocalizedText(merchantTalkLinesLocalizationKeys[talkLinesIndex]);

        OnAnyMerchantShowNewTalkLine?.Invoke(this, EventArgs.Empty);
    }

    public HubMerchant.HubMerchantType GetHubMerchantType() {
        return hubMerchant.GetHubMerchantType();
    }

    public void SetCurrentDialogLineShown() {
        if (hubMerchant.GetMerchantIsDecorationalDemoMerchant()) return;

        currentDialogLineShown = true;
        continueGameObject.SetActive(true);
    }

    public HubMerchant GetHubMerchant() {
        return hubMerchant;
    }
    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractPerformed;

        hubMerchant.OnPlayerStartedTalkingWithHubMerchant -= HubMerchant_OnPlayerStartedTalkingWithHubMerchant;
    }
}
