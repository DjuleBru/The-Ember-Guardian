using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarmupShaders : MonoBehaviour
{
    private void Awake() {
        Shader myShader = Shader.Find("Packages/Pixel Graphics/Foliage/foliage_pixelart_shader");
        if (myShader != null) {
            Debug.Log("material found");
            Material mat = new Material(myShader);
            mat.EnableKeyword("_ALWAYSON"); // Force Unity à le garder
        } else {
            Debug.Log("material not found");
        }
        //Shader.WarmupAllShaders();
    }
}
