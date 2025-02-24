using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager Instance;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera UICamera;
    [SerializeField] private float initialCameraOrthographicSize = 10;
    private float zoomDuration = .5f; // Durée du zoom

    private Coroutine currentZoomCoroutine;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        virtualCamera.m_Lens.OrthographicSize = initialCameraOrthographicSize;
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

        Debug.Log("targetOrthographicSize " + targetOrthographicSize);
    }

    public void ChangeCameraTarget(Transform target, bool disablePlayerInputs = true) {
        virtualCamera.m_Follow = target;
        Player.Instance.SetCameraHasOtherTarget(disablePlayerInputs);
    }

    public void ResetCameraTargetToPlayer() {
        virtualCamera.m_Follow = Player.Instance.transform;
        Player.Instance.SetCameraHasOtherTarget(false);
    }

    private void StartZoom(float targetSize, float zoomDuration) {
        // Arrête le zoom en cours s'il y en a un
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        // Lancer une nouvelle transition
        currentZoomCoroutine = StartCoroutine(SmoothZoomCoroutine(targetSize, zoomDuration));
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

    private IEnumerator SmoothZoomCoroutine(float targetSize, float zoomDuration) {
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            // Utilise SmoothStep pour un mouvement rapide au début et lent à la fin
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Assure que la taille finale est exactement celle attendue
        virtualCamera.m_Lens.OrthographicSize = targetSize;
    }

    public void SetCameraOrthographicSize(float orthographicSize) {
        virtualCamera.m_Lens.OrthographicSize = orthographicSize;
    }

    public Camera GetUICamera() {
        return UICamera;
    }
}
