using Febucci.UI;
using Febucci.UI.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUI_Locations : MonoBehaviour
{
    public static LevelUI_Locations Instance;

    [SerializeField] private GameObject locationsGameObject;
    [SerializeField] private Animator locationsAnimator;
    [SerializeField] private TextMeshProUGUI locationsText;
    [SerializeField] private TypewriterCore typeWriter;

    [SerializeField] private TypewriterByCharacter locationsTypeWriter;
    [SerializeField] private ParticleSystem locationsPS;
    [SerializeField] private RectTransform locationsPSRT;

    private bool showingLocation;
    private int totalChars;
    private int shownChars;
    private float psFollowSpeed = 9f; // ajustable dans l’inspector
    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 targetPos;

    public event EventHandler OnLocationTextShown;

    private void Awake() {
        Instance = this;

        locationsTypeWriter.onTypewriterStart.AddListener(OnTypewriterStart);
        locationsTypeWriter.onCharacterVisible.AddListener(OnCharacterShown);
    }

    private void Start() {
        locationsText.gameObject.SetActive(false);

        locationsText.font = LocalizationManager.Instance.GetCurrentFont();

    }

    private void Update() {
        if(locationsPS == null) return;
        if (!showingLocation) return;
        // Lerp fluide vers la position cible
        locationsPSRT.anchoredPosition = Vector2.Lerp(locationsPSRT.anchoredPosition,targetPos,Time.deltaTime * psFollowSpeed);

    }

    private void OnTypewriterStart() {
        shownChars = 0;
        showingLocation = true;
    }

    public void OnCharacterShown(char c) {
        if (locationsPSRT == null) return;

        shownChars++;

        float progress = Mathf.Clamp01((float)shownChars / Mathf.Max(totalChars, 1));
        float posX = Mathf.Lerp(startPos.x, endPos.x, progress);

        // On définit une position cible fluide au lieu de téléporter directement
        targetPos = new Vector2(posX, locationsPSRT.anchoredPosition.y);

        if (shownChars == totalChars) {
            StartCoroutine(StopPSAfterDelay());
            showingLocation = false;
        }
    }

    public void InitBoxForNewText(string text) {
        shownChars = 0;

        //locationsTypeWriter.ShowText(text);
        //locationsText.ForceMeshUpdate();
        //var bounds = locationsText.textBounds;

        float textWidth = locationsText.GetPreferredValues().x;

        // Définir les bornes gauche et droite (centrées sur le TMP)
        startPos = new Vector2(-textWidth / 2 + 150f, locationsPSRT.anchoredPosition.y);
        endPos = new Vector2(textWidth / 2 - 150, locationsPSRT.anchoredPosition.y);

        targetPos = startPos;
        locationsPSRT.anchoredPosition = startPos;
        locationsPS.Play();

        totalChars = text.Length;
    }

    private IEnumerator StopPSAfterDelay() {
        yield return new WaitForSeconds(.1f);
        locationsPS.Stop();
    }

    [Button]
    public void ShowLocationText(string locationName) {
        locationsText.gameObject.SetActive(true);
        locationsText.text = "{fade d=3}" + locationName;// Relance les animations si nécessaire
        OnLocationTextShown?.Invoke(this, EventArgs.Empty);

        InitBoxForNewText(locationName);

        StartCoroutine(ShowLocationCoroutine(5f));
    }

    public void ShowFireTextForTime(float timeToShow) {
        locationsAnimator.ResetTrigger("Hide");
        locationsAnimator.SetTrigger("Show");

        locationsText.gameObject.SetActive(true);
        locationsText.text = "{fade d=3}" + LocalizationManager.Instance.GetLocalizedText("primordialFireLit");// Relance les animations si nécessaire
        OnLocationTextShown?.Invoke(this, EventArgs.Empty);

        StartCoroutine(ShowLocationCoroutine(timeToShow));
    }

    private IEnumerator ShowFireTextForTimeCoroutine(float timeToShow) {
        yield return new WaitForSeconds(2f);

    }

    private IEnumerator ShowLocationCoroutine(float showDuration) {
        yield return new WaitForSeconds(showDuration);
        locationsAnimator.ResetTrigger("Show");
        locationsAnimator.SetTrigger("Hide");
    }
    void OnDestroy() {
        // Bonne pratique : se désabonner pour éviter les leaks
        locationsTypeWriter.onTypewriterStart.RemoveListener(OnTypewriterStart);
    }
}
