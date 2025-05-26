using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance;

    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Canvas cursorCanvas;
    [SerializeField] private RectTransform canvasCursorGO;
    [SerializeField] private SpriteRenderer weaponCursorSprite;

    private float initialMouseCursorWidth = 55f;
    private float initialMouseCursorHeight = 55f;

    private float minMouseCursorWidth = 45f;
    private float minMouseCursorHeight = 45f;

    private float precisionModifierScaleMultiplier = .5f;

    private float cursorSizeSmoothSpeed = 10f;
    private Vector2 currentCursorSize;
    private Vector2 cursorHotspot;

    private void Awake() {
        Instance = this;
    }

    void Start() {

        ShowMouse(false);
        currentCursorSize = new Vector2(initialMouseCursorWidth, initialMouseCursorHeight);

        //cursorHotspot = new Vector2(cursorTexture.width/2, cursorTexture.height/2);
        //Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);

        //GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        //RefreshMouseHideWithGamepad();
    }

    private void LateUpdate() {
        HandleMouseCursorSize();
        HandleMouseCursorPosition();
        HandleWeaponCursorPosition();
    }

    private void HandleMouseCursorSize() {
        float currentPrecisionModifier = PlayerAim.Instance.GetCurrentPrecisionModifier();

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

    private void HandleWeaponCursorPosition() {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        weaponCursorSprite.transform.position = PlayerAim.Instance.GetWeaponReticleWorldPos();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshMouseHideWithGamepad();
    }

    private void RefreshMouseHideWithGamepad() {

        if (GameInput.Instance.IsUsingGamepad()) {
            ShowMouse(false);
        }
        else {
            ShowMouse(true);
        }
    }

    public void ShowMouse(bool show) {
        Cursor.visible = show;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }

}
