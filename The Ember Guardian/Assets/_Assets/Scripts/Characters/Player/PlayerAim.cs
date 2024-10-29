using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public static PlayerAim Instance;

    [SerializeField] private Transform aimTransform;
    [SerializeField] private int minAngle;
    [SerializeField] private int maxAngle;

    private float aimAngle;
    private float aimHeight;
    private float lastMousePositionY;
    private float mousePositionY;
    private float mouseYDeltaTreshold = .1f;

    private float recoilDamping;
    private float currentRecoil;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        float angle = Mathf.Atan2(1, 0) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0, 0, angle);


    }

    private void Update() {

        HandleAimMouse2();
        HandleRecoil();
    }

    private void HandleRecoil() {

    }

    private void HandleAimMouse2() {
        Vector3 mousePosition = GetMouseWorldPosition();
        mousePositionY = mousePosition.y;

        if (Mathf.Abs(mousePositionY - lastMousePositionY) > mouseYDeltaTreshold) {
            float mouseDeltaY = mousePositionY - lastMousePositionY;
            aimHeight += mouseDeltaY;

            lastMousePositionY  = mousePositionY;
        }


        Vector3 aimDir = new Vector3(8 * PlayerMovement.Instance.GetLastMoveDir(), aimHeight + currentRecoil, 0);
        aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        Vector2 localScale = new Vector2(1, 1);
        if (PlayerMovement.Instance.GetLastMoveDir() < 0) {
            localScale.x = -1;
            localScale.y = -1;
        }

        aimAngle = ClampAimAngle(aimAngle, aimDir);

        aimTransform.localScale = localScale;
        aimTransform.eulerAngles = new Vector3(0, 0, aimAngle);

        // Smooth recoil back to zero
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilDamping);
    }

    private float ClampAimAngle(float inputAngle, Vector3 aimDir) {
        float outputAngle = inputAngle;

        if (PlayerMovement.Instance.GetLastMoveDir() >= 0) {

            if (aimDir.x < 0) {
                aimDir.x = -aimDir.x;
                outputAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            }

            if (inputAngle < minAngle && inputAngle > (-90 + minAngle)) {
                outputAngle = minAngle;
            }

            if (inputAngle > maxAngle) {
                outputAngle = maxAngle;
            }

        }

        else {
            if (aimDir.x > 0) {
                aimDir.x = -aimDir.x;
                outputAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            }
            if (inputAngle < 0 && inputAngle > (-90 + minAngle)) {
                outputAngle = -90 + minAngle;
            }

            if (inputAngle > 0 && inputAngle < (90 + maxAngle)) {
                outputAngle = 90 + maxAngle;
            }
        }


        return outputAngle;
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

}
