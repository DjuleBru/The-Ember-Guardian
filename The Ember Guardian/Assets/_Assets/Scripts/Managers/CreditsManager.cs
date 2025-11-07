using Febucci.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    public static CreditsManager Instance;

    public event EventHandler OnCreditsNameShown;

    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Animator psAnimator;
    [SerializeField] private ParticleSystem creditsTitlePS;
    [SerializeField] private RectTransform creditsPSRT;

    [SerializeField] private TypewriterByCharacter creditTitleTypeWriter;
    [SerializeField] private TypewriterByCharacter creditNamesTypeWriter;
    [SerializeField] private TypewriterByCharacter creditNamesTypeWriter2;
    [SerializeField] private TypewriterByCharacter creditNamesTypeWriter3;
    [SerializeField] private TypewriterByCharacter creditNamesTypeWriter4;
    [SerializeField] private TextMeshProUGUI creditTitlesText;
    [SerializeField] private TextMeshProUGUI creditNamesText;
    [SerializeField] private TextMeshProUGUI creditNamesText2;
    [SerializeField] private TextMeshProUGUI creditNamesText3;
    [SerializeField] private TextMeshProUGUI creditNamesText4;

    [SerializeField] private Transform cameraTargetTransformDuringCredits;

    private int totalChars;
    private int shownChars;
    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 targetPos;
    private bool isShowingCredits;
    private Coroutine creditsCoroutine;
    [SerializeField] private float psFollowSpeed = 12f; // ajustable dans l’inspector

    private void Awake() {
        Instance = this;
        creditTitleTypeWriter.onTypewriterStart.AddListener(OnTypewriterStart);
        creditTitleTypeWriter.onCharacterVisible.AddListener(OnCharacterShown);

        creditsPanel.SetActive(false);
    }


    private void Update() {
        // Lerp fluide vers la position cible
        creditsPSRT.anchoredPosition = Vector2.Lerp(
            creditsPSRT.anchoredPosition,
            targetPos,
            Time.deltaTime * psFollowSpeed
        );

        if (!isShowingCredits) return;
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {

            if(Input.anyKeyDown) {
                StopCoroutine(creditsCoroutine);
                EndShowCredits();
            }
        }
    }

    private void RefreshFonts() {
        creditTitlesText.font = LocalizationManager.Instance.GetCurrentFont();

        creditTitlesText.font = LocalizationManager.Instance.GetCurrentFont();
        creditNamesText.font = LocalizationManager.Instance.GetCurrentFont();
        creditNamesText2.font = LocalizationManager.Instance.GetCurrentFont();
        creditNamesText3.font = LocalizationManager.Instance.GetCurrentFont();
        //creditNamesText4.font = LocalizationManager.Instance.GetCurrentFont();


        creditTitlesText.material = LocalizationManager.Instance.GetBlueGlowMaterial();
        creditNamesText.material = LocalizationManager.Instance.GetStandardMaterial();
        creditNamesText2.material = LocalizationManager.Instance.GetStandardMaterial();
        creditNamesText3.material = LocalizationManager.Instance.GetStandardMaterial();
    }

    [Button]
    public void StartShowCredits() {
        isShowingCredits = true;
        RefreshFonts();

        MusicManager.Instance.SetAudioVolume(.8f);
        creditsCoroutine = StartCoroutine(ShowCredits());
    }
    private void OnTypewriterStart() {
        shownChars = 0;
        OnCreditsNameShown?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator ShowCredits() {

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            HUBManager.Instance.SetHubMerchantsCreditsMode(true);
            HUBManager.Instance.SetHubFireAndChestInteractable(false);
            HubChest.Instance.SetCanOpenChest(false);
            yield return new WaitForSeconds(1f);
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            CameraManager.Instance.ChangeCameraTarget(cameraTargetTransformDuringCredits, false);
            MainMenuUI.Instance.HideMainMenuButtons();
            MusicManager.Instance.SetAudioVolume(.75f);
            yield return new WaitForSeconds(2f);
        }

        creditsPanel.SetActive(true);
        creditTitleTypeWriter.ShowText("");
        creditNamesTypeWriter.ShowText("");
        creditNamesTypeWriter2.ShowText("");
        creditNamesTypeWriter3.ShowText("");

        InitBoxForNewText(LocalizationManager.Instance.GetLocalizedText("Created by"));
        yield return new WaitForSeconds(1.5f);
        creditNamesTypeWriter.ShowText("Julien Taconet");

        yield return new WaitForSeconds(3f);
        creditTitleTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(1.5f);

        InitBoxForNewText(LocalizationManager.Instance.GetLocalizedText("Published by"));
        yield return new WaitForSeconds(1.5f);
        creditNamesTypeWriter.ShowText("Slug Disco");

        yield return new WaitForSeconds(3f);
        creditTitleTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(1.5f);

        InitBoxForNewText(LocalizationManager.Instance.GetLocalizedText("Pixel Artists"));
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter.ShowText("Penubsmic");
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.ShowText("Krishna Palacio");

        yield return new WaitForSeconds(3f);
        creditTitleTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter2.StartDisappearingText();
        yield return new WaitForSeconds(1.5f);

        InitBoxForNewText(LocalizationManager.Instance.GetLocalizedText("Concept Artists"));
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter.ShowText("Amy");
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.ShowText("Nico Square");

        yield return new WaitForSeconds(3f);
        creditTitleTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter2.StartDisappearingText();
        yield return new WaitForSeconds(1.5f);

        InitBoxForNewText(LocalizationManager.Instance.GetLocalizedText("Original Music by"));
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter.ShowText("Jonathan Meyer");
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.ShowText("Alexey Samojlenko");
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter3.ShowText("Matryoshka");

        yield return new WaitForSeconds(3f);
        creditTitleTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter2.StartDisappearingText();
        yield return new WaitForSeconds(.5f);
        creditNamesTypeWriter3.StartDisappearingText();
        yield return new WaitForSeconds(1.5f);

        creditNamesText.fontSize = 50f;
        creditNamesText.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 60f);
        creditNamesText2.fontSize = 50f;
        creditNamesText2.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 60f);
        creditNamesText3.fontSize = 50f;
        creditNamesText3.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 60f);

        InitBoxForNewText(LocalizationManager.Instance.GetLocalizedText("Special Thanks"));
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter.ShowText("Joha");
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.ShowText("Javingor");
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter3.ShowText("TheRoyalTiger");
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(1f);

        creditNamesTypeWriter.ShowText("Verneveyel");
        creditNamesTypeWriter2.StartDisappearingText();
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.ShowText("Reuhnarr");
        creditNamesTypeWriter3.StartDisappearingText();
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter3.ShowText("CodeMonkey");
        creditNamesTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.StartDisappearingText();
        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter3.StartDisappearingText();

        yield return new WaitForSeconds(1f);
        creditNamesTypeWriter2.ShowText("Romane, pour ta confiance depuis le premier jour. Merci <3");
        yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(2f);
        creditTitleTypeWriter.StartDisappearingText();
        yield return new WaitForSeconds(2f);
        creditNamesTypeWriter2.StartDisappearingText();

        yield return new WaitForSeconds(5f);
        EndShowCredits();
    }

    private void EndShowCredits() {
        creditsPanel.SetActive(false);
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            CameraManager.Instance.ResetCameraTargetToPlayer();
            HUBManager.Instance.SetHubMerchantsCreditsMode(false);
            HUBManager.Instance.SetHubFireAndChestInteractable(true);
            MusicManager.Instance.FadeOutMusic(1f);
            HubChest.Instance.SetCanOpenChest(true);
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            MainMenuUI.Instance.ShowMainMenuButtons();
            CameraManager.Instance.ResetCameraTarget();
            MusicManager.Instance.SetAudioVolume(1f);
        }

        isShowingCredits = false;
    }

    public void OnCharacterShown(char c) {
        shownChars++;

        float progress = Mathf.Clamp01((float)shownChars / Mathf.Max(totalChars, 1));
        float posX = Mathf.Lerp(startPos.x, endPos.x, progress);

        // On définit une position cible fluide au lieu de téléporter directement
        targetPos = new Vector2(posX, creditsPSRT.anchoredPosition.y);

        if (shownChars == totalChars) {
            StartCoroutine(StopPSAfterDelay());
        }
    }

    private IEnumerator StopPSAfterDelay() {
        yield return new WaitForSeconds(.1f);
        creditsTitlePS.Stop();
    }

    public void InitBoxForNewText(string text) {
        shownChars = 0;

        creditTitleTypeWriter.ShowText(text);
        creditTitlesText.ForceMeshUpdate();
        var bounds = creditTitlesText.textBounds;

        float textWidth = creditTitlesText.GetPreferredValues().x;

        // Définir les bornes gauche et droite (centrées sur le TMP)
        startPos = new Vector2(-textWidth/2 + 50f, creditsPSRT.anchoredPosition.y);
        endPos = new Vector2(textWidth/ 2, creditsPSRT.anchoredPosition.y);

        targetPos = startPos;
        creditsPSRT.anchoredPosition = startPos;
        creditsTitlePS.Play();

        totalChars = text.Length;
    }

    public bool GetCreditsPlaying() {
        return isShowingCredits;
    }

    void OnDestroy() {
        // Bonne pratique : se désabonner pour éviter les leaks
        creditTitleTypeWriter.onTypewriterStart.RemoveListener(OnTypewriterStart);
    }
}
