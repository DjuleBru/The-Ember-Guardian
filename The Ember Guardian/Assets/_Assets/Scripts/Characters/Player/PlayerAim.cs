using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public static PlayerAim Instance;

    [SerializeField] private List<Transform> followAimDirTransformList;
    [SerializeField] private Transform gunTransform;

    private bool isUsingGamepad;

    private float aimAngle;
    private float aimHeight;
    private float lastMousePositionY;
    private float mousePositionY;
    private float mouseYDeltaTreshold = .1f;

    private float recoilDamping;
    private float currentRecoil;

    private Vector3 aimDir;
    private Vector3 previousGamepadAim = new Vector3(1,0,0);
    private Vector3 previousAimDir = new Vector3(1,0,0);

    public event EventHandler OnXAimDirChanged;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        float angle = Mathf.Atan2(1, 0) * Mathf.Rad2Deg;

        foreach(Transform transform in followAimDirTransformList) {
            transform.eulerAngles = new Vector3(0, 0, angle);
        }

        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        isUsingGamepad = GameInput.Instance.IsUsingGamepad();
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        isUsingGamepad = GameInput.Instance.IsUsingGamepad();
    }

    private void Update() {

        if(isUsingGamepad) {
            HandleAimGamepad(GameInput.Instance.GetAimInput());
        } else {
            HandleAimMouse();
        }

        HandleXScale();
        HandleRecoil();
    }

    private void HandleRecoil() {

    }

    private void HandleAimGamepad(Vector2 lookInput) {

        if(lookInput.magnitude > GameInput.gamepadDeadzone) {
            aimDir = new Vector3(lookInput.x, lookInput.y, 0).normalized;
            previousGamepadAim = aimDir;
        } else {
            aimDir = previousGamepadAim;
        }

        aimDir.y += currentRecoil;

        aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        foreach (Transform transform in followAimDirTransformList) {
            transform.eulerAngles = new Vector3(0, 0, aimAngle);
        }

        // Smooth recoil back to zero
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilDamping);
    }

    private void HandleAimMouse() {
        Vector3 mousePosition = GetMouseWorldPosition();

        aimDir = (mousePosition - gunTransform.position).normalized;

        aimDir.y += currentRecoil;

        aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        foreach (Transform transform in followAimDirTransformList) {
            transform.eulerAngles = new Vector3(0, 0, aimAngle);
        }

        // Smooth recoil back to zero
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilDamping);
    }

    public void AddRecoil(float recoil, float recoilDamping) {
        currentRecoil = recoil;
        this.recoilDamping = recoilDamping;
    }

    public Vector3 GetMouseWorldPosition() {
        Vector3 vec = GetMouseWorldPosition(Input.mousePosition, Camera.main);
        vec.z = 0;
        return vec;
    }

    public Vector3 GetMouseWorldPosition(Vector3 screenPosition, Camera worldCamera) {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public float GetAimAngle() {
        return aimAngle;
    }

    private void HandleXScale() {

        if (aimDir.x < 0 && previousAimDir.x > 0) {
            previousAimDir = aimDir;
            Vector3 newScale = new Vector3(-1, 1, 1);
            OnXAimDirChanged?.Invoke(this, EventArgs.Empty);
        }

        if (aimDir.x > 0 && previousAimDir.x < 0) {
            previousAimDir = aimDir;
            Vector3 newScale = new Vector3(1, 1, 1);
            OnXAimDirChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector3 GetAimDir() {
        return aimDir;
    }
    public Vector3 GetPreviousAimDir() {
        return previousAimDir;
    }

}
