using Cinemachine;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager Instance;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera UICamera;
    [SerializeField] private float initialCameraOrthographicSize = 11f;
    [SerializeField] private float tutorialInitialCameraOrthographicSize = 7f;

    private CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseInOut;
    private float blendTime = 1.5f;
    private CinemachineBrain brain;

    [SerializeField] private float referenceCameraOrthographicSize = 11f;
    [SerializeField] private float minCameraOrthographicSize = 7f;
    [SerializeField] private float maxCameraOrthographicSize = 11.5f;
    [SerializeField] private Transform scrollTarget;
    private float zoomDuration = .5f; // Durée du zoom

    private Transform initialCameraFollowTarget;
    private Coroutine currentZoomCoroutine;

    private bool isChangingOrthographicSize;
    private float isChangingOrthographicSizeTimer;
    private float isChangingOrthographicSizeTime = .05f;

    private bool isMainMenu;
    private bool isTutorial;
    private bool initialCameraOrthographicSizeForceSet;
    private bool cameraCenteredOnPlayer;
    public event EventHandler OnCameraCenteredOnPlayer;

    [SerializeField] private float maxZoomDurationBeforeReset = 15f; // Temps max avant retour auto
    private float zoomTimer = 0f;
    private bool cameraLockedByTransition;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        initialCameraFollowTarget = virtualCamera.Follow;
        RestoreBlend();

        RefreshCameraOrthographicSize(false);

        SettingsManager.Instance.OnZoomLevelChanged += SettingsManager_OnZoomLevelChanged;
        isMainMenu = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;
        isTutorial = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial;

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            SavingManager_Level.Instance.OnLoadGameEnded += SavingManager_OnLoadGameEnded;
        }

        if(isTutorial) {
            SetCameraOrthographicSize(tutorialInitialCameraOrthographicSize);
        }
    }

    private void SavingManager_OnLoadGameEnded(object sender, EventArgs e) {
        RestoreBlend();
    }

    private void RestoreBlend() {
        if (brain != null)
            brain.m_DefaultBlend = new CinemachineBlendDefinition(blendStyle, blendTime);
    }

    private void Update() {
        if (isMainMenu) return;

        if (isChangingOrthographicSize) {
            isChangingOrthographicSizeTimer += Time.unscaledDeltaTime;

            if (isChangingOrthographicSizeTimer >= isChangingOrthographicSizeTime) {
                isChangingOrthographicSize = false;
                Time.timeScale = 0f;
            }

        }

        if (initialCameraOrthographicSizeForceSet) return;

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

    private void SettingsManager_OnZoomLevelChanged(object sender, EventArgs e) {
        RefreshCameraOrthographicSize(true);
    }

    private void RefreshCameraOrthographicSize(bool pauseMenu) {
        if (pauseMenu) {
            Time.timeScale = 1f;
            isChangingOrthographicSizeTimer = 0;
            isChangingOrthographicSize = true;
        }

        float zoomLevel = SettingsManager.Instance.GetZoomLevel();
        float cameraOrthograhpicSize;
        if (zoomLevel <= 0.8f) {
            cameraOrthograhpicSize = Mathf.Lerp(minCameraOrthographicSize, referenceCameraOrthographicSize, zoomLevel / 0.8f);
        }
        else {
            cameraOrthograhpicSize = Mathf.Lerp(referenceCameraOrthographicSize, maxCameraOrthographicSize, (zoomLevel - 0.8f) / 0.2f);
        }

        initialCameraOrthographicSize = cameraOrthograhpicSize;

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
    }

    public void ChangeCameraTarget(Transform target, bool disablePlayerInputs = true) {
        virtualCamera.m_Follow = target;

        if(Player.Instance != null) {
            Player.Instance.SetCameraHasOtherTarget(disablePlayerInputs);
        }

    }

    public void ResetCameraTargetToPlayer() {
        cameraCenteredOnPlayer = false;
        virtualCamera.m_Follow = Player.Instance.transform;
        Player.Instance.SetCameraHasOtherTarget(false);
    }

    public void ResetCameraTarget() {
        cameraCenteredOnPlayer = false;
        virtualCamera.m_Follow = initialCameraFollowTarget;
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
        initialCameraOrthographicSizeForceSet = true;
        initialCameraOrthographicSize = orthographicSize;
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

    public void SetCameraPositionToPlayer() {

        Vector3 camPos = virtualCamera.transform.position;
        Vector3 playerPos = Player.Instance.transform.position;

        Vector3 delta = new Vector3(playerPos.x - camPos.x, 0f, 0f);

        virtualCamera.OnTargetObjectWarped(Player.Instance.transform,delta);


        // Optionnel : reset le blend pour être sûr
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null) {
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        }

        cameraCenteredOnPlayer = true;
        
        // AJOUT IMPORTANT
        StartCoroutine(RestoreBlendNextFrame());
    }
    private IEnumerator RestoreBlendNextFrame() {
        yield return null;
        RestoreBlend();
    }

    public bool IsChangingCameraOrthographicSize() {
        return isChangingOrthographicSize;
    }

    [Button]
    public void StartScroll() {
        cameraLockedByTransition = true;
        virtualCamera.m_Follow = scrollTarget;
        scrollTarget.position = Player.Instance.transform.position;
        ZoomOut(false, .75f, .1f);
        StartCoroutine(Scroll());
    }

    private IEnumerator Scroll() {
        float scrollDistance = 100f;
        float elapsed = 0f;
        float scrollSpeed = .025f;

        while(elapsed < scrollDistance) {
            elapsed += scrollSpeed;
            scrollTarget.position = new Vector3(scrollTarget.position.x + scrollSpeed, 0, 0);
            yield return new WaitForEndOfFrame();
        }
        
    }
}
