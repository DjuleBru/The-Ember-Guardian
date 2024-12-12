using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMouse : MonoBehaviour
{
    public static HideMouse Instance;

    [SerializeField] private Texture2D cursorTexture;

    private Vector2 cursorHotspot;

    private void Awake() {
        Instance = this;
    }

    void Start()
    {
        cursorHotspot = new Vector2(cursorTexture.width/2, cursorTexture.height/2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);

        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        RefreshMouseHideWithGamepad();
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
        Debug.Log("ShowMouse " + show);
        Cursor.visible = show;
    }
    
}
