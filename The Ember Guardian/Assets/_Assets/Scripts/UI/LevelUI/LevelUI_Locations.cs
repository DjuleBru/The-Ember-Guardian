using Febucci.UI;
using Febucci.UI.Core;
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

    public event EventHandler OnLocationTextShown;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        locationsText.gameObject.SetActive(false);

        locationsText.font = LocalizationManager.Instance.GetCurrentFont();
    }

    public void ShowLocationText(string locationName) {
        locationsText.gameObject.SetActive(true);
        locationsText.text = "{fade d=3}" + locationName;// Relance les animations si nécessaire
        OnLocationTextShown?.Invoke(this, EventArgs.Empty);

        StartCoroutine(ShowLocationCoroutine(5f));
    }

    public void ShowFireTextForTime(float timeToShow) {
        StartCoroutine(ShowFireTextForTimeCoroutine(timeToShow));
    }

    private IEnumerator ShowFireTextForTimeCoroutine(float timeToShow) {
        CameraManager.Instance.ChangeCameraTarget(Fire.Instance.transform);

        yield return new WaitForSeconds(2f);

        locationsAnimator.ResetTrigger("Hide");
        locationsAnimator.SetTrigger("Show");

        locationsText.gameObject.SetActive(true);
        locationsText.text = "{fade d=3}" + LocalizationManager.Instance.GetLocalizedText("primordialFireLit");// Relance les animations si nécessaire
        OnLocationTextShown?.Invoke(this, EventArgs.Empty);

        StartCoroutine(ShowLocationCoroutine(timeToShow));

        yield return new WaitForSeconds(timeToShow);

        CameraManager.Instance.ResetCameraTargetToPlayer();
    }

    private IEnumerator ShowLocationCoroutine(float showDuration) {
        yield return new WaitForSeconds(showDuration);
        locationsAnimator.ResetTrigger("Show");
        locationsAnimator.SetTrigger("Hide");
    }
}
