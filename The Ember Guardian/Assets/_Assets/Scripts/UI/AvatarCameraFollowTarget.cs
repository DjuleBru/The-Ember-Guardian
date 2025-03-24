using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCameraFollowTarget : MonoBehaviour
{

    [SerializeField] private Transform followTargetTransform;
    [SerializeField] private float xDelta;
    [SerializeField] private float yPosition;


    private Camera avatarCamera;

    private void Awake() {
        avatarCamera = GetComponent<Camera>();
        avatarCamera.enabled = false;
    }

    private void Start() {
        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        avatarCamera.enabled = true;
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, System.EventArgs e) {
        avatarCamera.enabled = false;
    }

    void Update()
    {
        Vector3 newPosition = new Vector3(xDelta + followTargetTransform.position.x, yPosition, - 10f);
        transform.position = newPosition;
    }

    public void EnableCamera(bool enabled) {
        avatarCamera.enabled = enabled;
    }

}
