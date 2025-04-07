using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicSettingsUI : MonoBehaviour
{

    [SerializeField] private GameObject adjustGammaGO;
    private void Start() {
        RectTransform rt = GetComponent<RectTransform>();
        
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            rt.sizeDelta = new Vector2(220, 100);
            adjustGammaGO.SetActive(true);
        } else {
            rt.sizeDelta = new Vector2(220, 50);
            adjustGammaGO.SetActive(false);
        }
    }
}
