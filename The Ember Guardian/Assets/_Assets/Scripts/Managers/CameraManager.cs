using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager Instance;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private float zoomDuration = .5f; // Durée du zoom

    private float initialCameraOrthographicSize;
    private Coroutine currentZoomCoroutine;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        initialCameraOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
    }

    public void ZoomIn(bool toInitialValue, float targetZoomInOrthographicSizeMultiplier = 1f, float zoomDuration = 1f) {
        // Démarre le zoom vers l'intérieur
        float targetOrthographicSize = initialCameraOrthographicSize* 1 / targetZoomInOrthographicSizeMultiplier;

        if (toInitialValue) {
            targetOrthographicSize = initialCameraOrthographicSize;
        }

        StartZoom(targetOrthographicSize, zoomDuration);
    }

    public void ZoomOut(bool toInitialValue, float targetZoomOutOrthographicSizeMultiplier = 1f, float zoomDuration = 1f) {
        // Démarre le zoom vers l'extérieur
        float targetOrthographicSize = initialCameraOrthographicSize * 1 / targetZoomOutOrthographicSizeMultiplier;
        if (toInitialValue) {
            targetOrthographicSize = initialCameraOrthographicSize;
        }

        StartZoom(targetOrthographicSize, zoomDuration);
    }

    private void StartZoom(float targetSize, float zoomDuration) {
        // Arrête le zoom en cours s'il y en a un
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        // Lancer une nouvelle transition
        currentZoomCoroutine = StartCoroutine(ZoomCoroutine(targetSize, zoomDuration));
    }

    private IEnumerator ZoomCoroutine(float targetSize, float zoomDuration) {
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration) {
            elapsedTime += Time.deltaTime;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, elapsedTime / zoomDuration);
            yield return null;
        }

        // Assure que la taille finale est exactement celle attendue
        virtualCamera.m_Lens.OrthographicSize = targetSize;
    }
}
