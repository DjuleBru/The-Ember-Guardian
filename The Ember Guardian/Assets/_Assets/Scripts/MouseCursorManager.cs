using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance;

    [SerializeField] private Texture2D invisibleCursorTexture;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Canvas cursorCanvas;
    [SerializeField] private RectTransform canvasCursorGO;
    [SerializeField] private Image weaponCursorImage;
    [SerializeField] private Image weaponCursorHitImage;
    [SerializeField] private RectTransform weaponCursorRectTransform;
    [SerializeField] private GameObject weaponCursorGameObject;
    [SerializeField] private GameObject mouseCursorGameObject;
    [SerializeField] private Image mouseCursorSpriteRenderer_NE;
    [SerializeField] private Image mouseCursorSpriteRenderer_NW;
    [SerializeField] private Image mouseCursorSpriteRenderer_SE;
    [SerializeField] private Image mouseCursorSpriteRenderer_SW;

    private bool isMenuScene;
    private bool isUsingGamepad;
    private bool pauseMenuOpen;
    private bool tabMenuOpen;
    private bool videoTipMenuOpen;

    private GunSO currentGunSO;
    private float initialMouseCursorWidth = 55f;
    private float initialMouseCursorHeight = 55f;

    private float minMouseCursorWidth = 50f;
    private float minMouseCursorHeight = 50f;

    private float weaponMaxRecoilImpactOnMouseReticle = .25f;
    private float precisionModifierScaleMultiplier = .5f;

    private float cursorSizeSmoothSpeed = 10f;
    private Vector2 currentCursorSize;
    private Vector2 cursorHotspot;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            isMenuScene = true;
            ShowMouse(true);
            return;
        };

        currentGunSO = PlayerShoot.Instance.GetHeldGunSO();
        currentCursorSize = new Vector2(initialMouseCursorWidth, initialMouseCursorHeight);

        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerSwappedGunStarted += PlayerShoot_OnPlayerSwappedGunStarted;
        PlayerShoot.Instance.OnPlayerSwappedGunEnded += PlayerShoot_OnPlayerSwappedGunEnded;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
        PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
        PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        PetDog.Instance.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
        PetDog.Instance.OnPlayerEndedPettingDog += PetDog_OnPlayerEndedPettingDog;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        Player.Instance.OnPlayerPositionSet += Player_OnPlayerPositionSet;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded += PlayerShoot_OnPlayerReloadEnded;
        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        VideoTipUI.Instance.OnVideoTipPanelOpened += VideoTipUI_OnVideoTipPanelOpened;
        Portal.OnAnyPortalSetToTeleportPlayer += Portal_OnAnyPortalSetToTeleportPlayer;
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
        FastTravelTP.OnAnyPlayerWarpedOut += FastTravelTP_OnAnyPlayerWarpedOut;
        FastTravelTP.OnAnyPlayerCanceledTP += FastTravelTP_OnAnyPlayerCanceledTP;
        FastTravelTP.OnAnyPlayerPositionedOnTP += FastTravelTP_OnAnyPlayerPositionedOnTP;

        HubMerchant.OnPlayerOpenedAnyHubMerchantShop += HubMerchant_OnPlayerOpenedAnyHubMerchantShop;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
        PortalUI.OnAnyPortalUIOpened += PortalUI_OnAnyPortalUIOpened;
        PortalUI.OnAnyPortalUIClosed += PortalUI_OnAnyPortalUIClosed;

        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        ShowMouse(false);
    }


    private void LateUpdate() {
        if (isMenuScene) return;

        HandleMouseCursorSize();
        HandleWeaponCursorPosition();

        if (!GameInput.Instance.IsUsingGamepad()) {
            HandleMouseCursorPosition();
        }
        else {
            HandleGamepadCursorPosition();
        }
    }


    private void Player_OnPlayerPositionSet(object sender, System.EventArgs e) {
        PlayerAim.Instance.SetWeaponReticleToAimPos();
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(false);
    }

    private void PetDog_OnPlayerEndedPettingDog(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void PetDog_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(false);
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void PlayerMovement_OnPlayerRoll(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(false);
    }

    private void PlayerShoot_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(false);
    }
    private void VideoTipUI_OnVideoTipPanelOpened(object sender, System.EventArgs e) {
        videoTipMenuOpen = true;
        Debug.Log("videoTipMenuOpen " + videoTipMenuOpen);
        if (!isUsingGamepad) {
            ShowMouse(true);
        }
    }

    private void Portal_OnAnyPortalSetToTeleportPlayer(object sender, System.EventArgs e) {
        ShowWeaponAndMouseCursorGO(false);
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, System.EventArgs e) {
        ShowWeaponAndMouseCursorGO(true);
    }
    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        ShowWeaponAndMouseCursorGO(false);
    }

    private void FastTravelTP_OnAnyPlayerWarpedOut(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void FastTravelTP_OnAnyPlayerPositionedOnTP(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(false);
    }

    private void FastTravelTP_OnAnyPlayerCanceledTP(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        Debug.Log("VideoTipUI_OnVideoTipPanelClosed ");
        videoTipMenuOpen = false;
        if (!isUsingGamepad) {
            ShowMouse(false);
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(false);
        } else {
            ShowWeaponAndMouseCursorGO(true);
        }
    }
    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(true);
        } else {
            ShowWeaponAndMouseCursorGO(true);
        }
    }
    private void PortalUI_OnAnyPortalUIClosed(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(false);
        }
    }

    private void PortalUI_OnAnyPortalUIOpened(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(true);
        }

    }
    private void HubMerchant_OnPlayerOpenedAnyHubMerchantShop(object sender, System.EventArgs e) {
        if(isUsingGamepad) {
            ShowWeaponAndMouseCursorGO(false);
        } else {
           ShowMouse(true);
        }        

    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, System.EventArgs e) {
        pauseMenuOpen = true;

        if(!isUsingGamepad) {
            ShowMouse(true);
        }
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, System.EventArgs e) {
        pauseMenuOpen = false;

        if (!isUsingGamepad) {
            ShowMouse(false);
        }
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, System.EventArgs e) {
        tabMenuOpen = false;

        if (!isUsingGamepad) {
            ShowMouse(false);
        }
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        tabMenuOpen = true;

        if (!isUsingGamepad) {
            ShowMouse(true);
        }
    }

    private void PlayerShoot_OnPlayerSwappedGunStarted(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(false);
    }
    private void PlayerShoot_OnPlayerSwappedGunEnded(object sender, System.EventArgs e) {
        ShowWeaponCursorGO(true);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        currentGunSO = PlayerShoot.Instance.GetHeldGunSO();
        RefreshWeaponVariables();
    }

    private void RefreshWeaponVariables() {
        weaponCursorImage.sprite = currentGunSO.weaponCursorSprite;
        weaponCursorHitImage.sprite = currentGunSO.weaponHitCursorSprite;
        mouseCursorSpriteRenderer_NE.sprite = currentGunSO.mouseCursorSprite_NE;
        mouseCursorSpriteRenderer_NW.sprite = currentGunSO.mouseCursorSprite_NW;
        mouseCursorSpriteRenderer_SE.sprite = currentGunSO.mouseCursorSprite_SE;
        mouseCursorSpriteRenderer_SW.sprite = currentGunSO.mouseCursorSprite_SW;

        initialMouseCursorWidth = currentGunSO.initialMouseCursorWidth;
        initialMouseCursorHeight = currentGunSO.initialMouseCursorHeight;
        weaponMaxRecoilImpactOnMouseReticle = currentGunSO.weaponMaxRecoilImpactOnMouseReticle;
    }

    private void HandleMouseCursorSize() {
        float currentPrecisionModifier = PlayerAim.Instance.GetCurrentPrecisionModifier() / PlayerAim.Instance.GetWeaponPrecisionModifier();
        float currentRecoilNormalized = PlayerAim.Instance.GetCurrentRecoil() / currentGunSO.gunRecoil;

        float recoilPrecisionImpact = weaponMaxRecoilImpactOnMouseReticle* currentRecoilNormalized;
        currentPrecisionModifier += recoilPrecisionImpact;

        float precisionDifference = currentPrecisionModifier - 1;
        precisionDifference *= precisionModifierScaleMultiplier;
        float scaledPrecisionDifference = 1 + precisionDifference;

        float targetWidth = initialMouseCursorWidth * scaledPrecisionDifference;
        float targetHeight = initialMouseCursorHeight * scaledPrecisionDifference;

        targetWidth = Mathf.Max(targetWidth, minMouseCursorWidth);
        targetHeight = Mathf.Max(targetHeight, minMouseCursorHeight);

        Vector2 targetSize = new Vector2(targetWidth, targetHeight);

        // Lerp vers la taille cible
        currentCursorSize = Vector2.Lerp(currentCursorSize, targetSize, Time.deltaTime * cursorSizeSmoothSpeed);

        canvasCursorGO.sizeDelta = currentCursorSize;
    }

    private void HandleMouseCursorPosition() {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorCanvas.transform as RectTransform,
            Input.mousePosition,
            cursorCanvas.worldCamera,
            out pos
        );
        canvasCursorGO.localPosition = pos;
    }

    private void HandleGamepadCursorPosition() {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(PlayerAim.Instance.GetVirtualMousePosition());
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorCanvas.transform as RectTransform,
             screenPos,
            cursorCanvas.worldCamera,
            out pos
        );
        canvasCursorGO.localPosition = pos;
    }

    private void HandleWeaponCursorPosition() {
        Vector3 worldPos = PlayerAim.Instance.GetWeaponReticleWorldPos();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        weaponCursorRectTransform.position = screenPos;
    }

    private void ShowWeaponCursorGO(bool show) {
        weaponCursorGameObject.SetActive(show);
        PlayerAim.Instance.SetWeaponReticleToAimPos();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        isUsingGamepad = GameInput.Instance.IsUsingGamepad();

        if (AllMenusClosed()) return;
        RefreshMouseHideWithGamepad();
    }

    private void RefreshMouseHideWithGamepad() {
        if (isUsingGamepad) {
            ShowMouse(false);
        }
        else {
            ShowMouse(true);
        }
    }

    public void ShowMouse(bool show) {
        if (this == null) return; // Safety check if called on destroyed object
        if (!show && !AllMenusClosed()) return;

        Cursor.visible = show;

        if(!show) {
            Cursor.SetCursor(invisibleCursorTexture, cursorHotspot, CursorMode.Auto);
            PlayerAim.Instance.SetWeaponReticleToAimPos();
        } else {
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }

        if (weaponCursorGameObject == null) {
            Debug.LogError("weaponCursorGameObject est null dans ShowMouse()");
            return;
        }
        weaponCursorGameObject.SetActive(!show);
        

        if (mouseCursorGameObject == null) {
            Debug.LogError("mouseCursorGameObject est null dans ShowMouse()");
            return;
        }
        mouseCursorGameObject.SetActive(!show);
    }

    private void ShowWeaponAndMouseCursorGO(bool show) {
        weaponCursorGameObject.SetActive(show);
        mouseCursorGameObject.SetActive(show);
        PlayerAim.Instance.SetWeaponReticleToAimPos();
    }

    private bool AllMenusClosed() {
        return !tabMenuOpen && !pauseMenuOpen && !videoTipMenuOpen;
    }

    private void OnDestroy() {
        HubMerchant.OnPlayerOpenedAnyHubMerchantShop -= HubMerchant_OnPlayerOpenedAnyHubMerchantShop;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant -= HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
        PortalUI.OnAnyPortalUIOpened -= PortalUI_OnAnyPortalUIOpened;
        PortalUI.OnAnyPortalUIClosed -= PortalUI_OnAnyPortalUIClosed;
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
        FastTravelTP.OnAnyPlayerWarpedOut -= FastTravelTP_OnAnyPlayerWarpedOut;
        FastTravelTP.OnAnyPlayerCanceledTP -= FastTravelTP_OnAnyPlayerCanceledTP;
        FastTravelTP.OnAnyPlayerPositionedOnTP -= FastTravelTP_OnAnyPlayerPositionedOnTP;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
        Portal.OnAnyPortalSetToTeleportPlayer -= Portal_OnAnyPortalSetToTeleportPlayer;
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
    }

}
