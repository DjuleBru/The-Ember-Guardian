using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMouse : MonoBehaviour
{
    public static HideMouse Instance;

    private void Awake() {
        Instance = this;
    }
    void Start()
    {
        ShowMouse(false);
    }

    public void ShowMouse(bool show) {
        Cursor.visible = show;
    }
    
}
