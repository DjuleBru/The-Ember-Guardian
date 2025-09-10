using Cinemachine;
using System;
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

    private bool isMainMenu;
    private bool cameraCenteredOnPlayer;
    public event EventHandler OnCameraCenteredOnPlayer;

    [SerializeField] private float maxZoomDurationBeforeReset = 15f; // Temps max avant retour auto
    private float zoomTimer = 0f;
    private bool cameraLockedByTransition;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        virtualCamera.m_Lens.OrthographicSize = initialCameraOrthographicSize;

        isMainMenu = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;
    }

    private void Update() {
        if (isMainMenu) return;

        // Check centrage caméra/player
        if (!cameraCenteredOnPlayer) {
            float distanceFromCameraToPlayer = Mathf.Abs(Camera.main.transform.position.x - Player.Instance.transform.position.x);
            if (distanceFromCameraToPlayer < 2f) {
                cameraCenteredOnPlayer = true;
                OnCameraCenteredOnPlayer?.Invoke(this, EventArgs.Empty);
            }
        }

        // --- SÉCURITÉ ZOOM ---
        if (cameraLockedByTransition) return;
        if (Player.Instance.GetInteractingWithMerchant()) return;

        if (Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - initialCameraOrthographicSize) > 0.01f) {
            zoomTimer += Time.deltaTime;
            if (zoomTimer >= maxZoomDurationBeforeReset) {
                StartZoom(initialCameraOrthographicSize, 1f);
                zoomTimer = 0f;
            }
        }
        else {
            zoomTimer = 0f;
        }
        
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

    public void ChangeCameraTarget(Transform target, bool disablePlayerInputs = true) {
        virtualCamera.m_Follow = target;
        Player.Instance.SetCameraHasOtherTarget(disablePlayerInputs);
    }

    public void ResetCameraTargetToPlayer() {
        cameraCenteredOnPlayer = false;
        virtualCamera.m_Follow = Player.Instance.transform;
        Player.Instance.SetCameraHasOtherTarget(false);
    }

    public void SetCameraNotCenteredOnPlayer() {
        cameraCenteredOnPlayer = false;
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
    public void ResetCameraOrthographicSize() {
        virtualCamera.m_Lens.OrthographicSize = initialCameraOrthographicSize;
    }
    public void SetCameraLockedByTransition(bool locked) {
        cameraLockedByTransition = locked;
    }

    public Camera GetUICamera() {
        return UICamera;
    }

    public bool GetCameraCenteredOnPlayer() {
        return cameraCenteredOnPlayer;
    }
}
