using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonUI_ChildTreeShowHide : MonoBehaviour
{
    [SerializeField] private List<Image> inputLinkList; 
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private Image treeRaycastImage;
    [SerializeField] private Image treeRaycastImage2;
    private CanvasGroup canvasGroup;

    private Coroutine activeCoroutine;

    private bool treeShown;

    private void Start() {
       StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay() {
        yield return new WaitForSeconds(.1f);
        canvasGroup = GetComponent<CanvasGroup>();

        treeRaycastImage.raycastTarget = true;
        if(treeRaycastImage2 != null) {
            treeRaycastImage2.raycastTarget = true;
        }

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        foreach (var image in inputLinkList) {
            Color color = image.color;
            color.a = 0;
            image.color = color;
        }
    }

    public void ShowTree() {
        if (treeShown) return;
        if (activeCoroutine != null) {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(ShowTreeCoroutine());

        treeShown = true;
    }

    public void HideTree() {
        if (!gameObject.activeInHierarchy) return;
        if (!treeShown) return;
        if (activeCoroutine != null) {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(HideTreeCoroutine());

        treeShown = false;
    }

    private IEnumerator ShowTreeCoroutine() {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration) {
            float alpha = elapsedTime / fadeDuration;
            canvasGroup.alpha = alpha;
            foreach (var image in inputLinkList) {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        treeRaycastImage.raycastTarget = false;
        if(treeRaycastImage2 != null) {
            treeRaycastImage2.raycastTarget = false;
        }
        foreach (var image in inputLinkList) {
            Color color = image.color;
            color.a = 1f;
            image.color = color;
        }
    }

    private IEnumerator HideTreeCoroutine() {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration) {
            float alpha = 1f - (elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;
            foreach (var image in inputLinkList) {
                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        treeRaycastImage.raycastTarget = true;
        foreach (var image in inputLinkList) {
            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }
    }
}
