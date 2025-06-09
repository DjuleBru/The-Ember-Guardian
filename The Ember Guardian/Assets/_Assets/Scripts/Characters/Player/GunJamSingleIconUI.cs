using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunJamSingleIconUI : MonoBehaviour
{

    [SerializeField] private Image inputImage;
    [SerializeField] private Image inputImageBackground;
    [SerializeField] private Animator animator;
    [SerializeField] private Color validColor;
    [SerializeField] private Color resetColor;
    [SerializeField] private Color failedColor;
    private GameInput.Binding binding;
    private int inputIndex;
    private bool iconShown;

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        GunJamHandler.OnAnyJamSequenceProgressed += GunJamHandler_OnAnyJamSequenceProgressed;
        GunJamHandler.OnAnyCorrectJamSequenceInput += GunJamHandler_OnAnyCorrectJamSequenceInput;
        GunJamHandler.OnAnyJamSequenceFailed += GunJamHandler_OnAnyJamSequenceFailed;
        RefreshInput();
    }

    private void GunJamHandler_OnAnyCorrectJamSequenceInput(object sender, System.EventArgs e) {
        if (iconShown) {
            animator.ResetTrigger("Reset");
            animator.SetTrigger("Valid");
        }
    }

    private void GunJamHandler_OnAnyJamSequenceFailed(object sender, System.EventArgs e) {
        StartCoroutine(FailedCoroutine());
    }

    private IEnumerator FailedCoroutine() {
        inputImage.color = failedColor;
        inputImageBackground.color = failedColor;

        yield return new WaitForSeconds(.3f);

        RefreshShowIcon();
        animator.ResetTrigger("Valid");
        animator.SetTrigger("Reset");
        inputImage.color = Color.white;
        inputImageBackground.color = resetColor;
    }

    private void GunJamHandler_OnAnyJamSequenceProgressed(object sender, GunJamHandler.OnAnyJamSequenceProgressedEventArgs e) {
        if (e.currentIndex == inputIndex) {
            ShowIcon(false);
            inputImageBackground.color = validColor;
        }
        if(e.currentIndex + 1 == inputIndex) {
            ShowIcon(true);
        }
    }

    private void RefreshShowIcon() {
        if (inputIndex == 0) {
            ShowIcon(true);
            animator.ResetTrigger("Valid");
            animator.SetTrigger("Reset");
            inputImageBackground.color = resetColor;
        }
        else {
            ShowIcon(false);
        }
    } 

    public void ShowIcon(bool show) {
        iconShown = show;
        if (show) {
            inputImage.gameObject.SetActive(true);
        } else {
            inputImage.gameObject.SetActive(false);
        }

    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshInput();
    }

    private void RefreshInput() {
        Sprite sprite = InputControlIcons.Instance.GetSingleControlIconSprite(binding);
        inputImage.sprite = sprite;
        inputImageBackground.sprite = sprite;
    }

    public void SetBinding(GameInput.Binding binding) {
        this.binding = binding;
        RefreshInput();
    }

    public void SetIndex(int index) {
        inputIndex = index;
        RefreshShowIcon();
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
        GunJamHandler.OnAnyJamSequenceProgressed -= GunJamHandler_OnAnyJamSequenceProgressed;
        GunJamHandler.OnAnyCorrectJamSequenceInput -= GunJamHandler_OnAnyCorrectJamSequenceInput;
        GunJamHandler.OnAnyJamSequenceFailed -= GunJamHandler_OnAnyJamSequenceFailed;
    }
}
