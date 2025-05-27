using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance;

    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Canvas cursorCanvas;
    [SerializeField] private RectTransform canvasCursorGO;
    [SerializeField] private SpriteRenderer weaponCursorSpriteRenderer;
    [SerializeField] private SpriteRenderer weaponCursorHitSpriteRenderer;
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

        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerAim_OnPlayerSwappedGun;
        currentGunSO = PlayerShoot.Instance.GetHeldGunSO();
        currentCursorSize = new Vector2(initialMouseCursorWidth, initialMouseCursorHeight);

        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
        PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
        PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        PetDog.Instance.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
        PetDog.Instance.OnPlayerEndedPettingDog += PetDog_OnPlayerEndedPettingDog;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        PlayerMovement.Instance.OnPlayerRoll += PlayerMovement_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += PlayerMovement_OnPlayerRollEnded;
        VideoTipUI.Instance.OnVideoTipPanelClosed += VideoTipUI_OnVideoTipPanelClosed;
        VideoTipUI.Instance.OnVideoTipPanelOpened += VideoTipUI_OnVideoTipPanelOpened;

        HubMerchant.OnPlayerOpenedAnyHubMerchantShop += HubMerchant_OnPlayerOpenedAnyHubMerchantShop;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;

        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        ShowMouse(false);
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        weaponCursorGameObject.SetActive(true);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        weaponCursorGameObject.SetActive(false);
    }

    private void PetDog_OnPlayerEndedPettingDog(object sender, System.EventArgs e) {
        weaponCursorGameObject.SetActive(true);
    }

    private void PetDog_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        weaponCursorGameObject.SetActive(false);
    }

    private void PlayerMovement_OnPlayerRollEnded(object sender, System.EventArgs e) {
        weaponCursorGameObject.SetActive(true);
    }

    private void PlayerMovement_OnPlayerRoll(object sender, System.EventArgs e) {
        weaponCursorGameObject.SetActive(false);
    }

    private void VideoTipUI_OnVideoTipPanelOpened(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(true);
        }
    }

    private void VideoTipUI_OnVideoTipPanelClosed(object sender, VideoTipUI.OnVideoTipPanelClosedEventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(false);
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(false);
        }
    }
    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (!isUsingGamepad) {
            ShowMouse(true);
        }
    }
    private void HubMerchant_OnPlayerOpenedAnyHubMerchantShop(object sender, System.EventArgs e) {
        ShowMouse(true);
    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, System.EventArgs e) {
        pauseMenuOpen = true;

        if(!isUsingGamepad) {
            ShowMouse(true);
        }
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, System.EventArgs e) {
        pauseMenuOpen = false;

        if (!isUsingGamepad && AllMenusClosed()) {
            ShowMouse(false);
        }
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, System.EventArgs e) {
        tabMenuOpen = false;

        if (!isUsingGamepad && AllMenusClosed()) {
            ShowMouse(false);
        }
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        tabMenuOpen = true;

        if (!isUsingGamepad) {
            ShowMouse(true);
        }
    }

    private void PlayerAim_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        currentGunSO = PlayerShoot.Instance.GetHeldGunSO();
        RefreshWeaponVariables();
    }

    private void RefreshWeaponVariables() {
        weaponCursorSpriteRenderer.sprite = currentGunSO.weaponCursorSprite;
        weaponCursorHitSpriteRenderer.sprite = currentGunSO.weaponHitCursorSprite;
        mouseCursorSpriteRenderer_NE.sprite = currentGunSO.mouseCursorSprite_NE;
        mouseCursorSpriteRenderer_NW.sprite = currentGunSO.mouseCursorSprite_NW;
        mouseCursorSpriteRenderer_SE.sprite = currentGunSO.mouseCursorSprite_SE;
        mouseCursorSpriteRenderer_SW.sprite = currentGunSO.mouseCursorSprite_SW;

        initialMouseCursorWidth = currentGunSO.initialMouseCursorWidth;
        initialMouseCursorHeight = currentGunSO.initialMouseCursorHeight;
        weaponMaxRecoilImpactOnMouseReticle = currentGunSO.weaponMaxRecoilImpactOnMouseReticle;
    }

    private void LateUpdate() {
        if (isMenuScene) return;

        HandleMouseCursorSize();
        HandleWeaponCursorPosition();

        if(!GameInput.Instance.IsUsingGamepad()) {
            HandleMouseCursorPosition();
        } else {
            HandleGamepadCursorPosition();
        }
    }

    private void HandleMouseCursorSize() {
        float currentPrecisionModifier = PlayerAim.Instance.GetCurrentPrecisionModifier();
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
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        weaponCursorSpriteRenderer.transform.position = PlayerAim.Instance.GetWeaponReticleWorldPos();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        isUsingGamepad = GameInput.Instance.IsUsingGamepad();

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
        Debug.Log("ShowMouse " + show);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = show;
        weaponCursorGameObject.SetActive(!show);
        mouseCursorGameObject.SetActive(!show);

    }

    private bool AllMenusClosed() {
        return !tabMenuOpen && !pauseMenuOpen;
    }

    private void OnDestroy() {
        HubMerchant.OnPlayerOpenedAnyHubMerchantShop -= HubMerchant_OnPlayerOpenedAnyHubMerchantShop;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant -= HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
    }

}
