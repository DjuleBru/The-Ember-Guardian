using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindParallaxTexture : MonoBehaviour
{

    [SerializeField] private SpriteRenderer windTextureSprite;
    private Material windTextureMaterial;

    private void Start() {
        windTextureMaterial = windTextureSprite.material;

        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;
        SetTextureVariables();
    }

    private void WindManager_OnWindStrengthChanged(object sender, System.EventArgs e) {
        SetTextureVariables();
    }

    private void SetTextureVariables() {

        WindManager.WindStrength currentWindStrength = WindManager.Instance.GetWindStrength();
        float windStrength = GetWindStrengthForWindTexture(currentWindStrength) * -WindManager.Instance.GetWindDir();

        windTextureMaterial.SetFloat("_TextureScrollXSpeed", windStrength);

        Color textureColor = Color.white;
        textureColor.a = GetAlphaForWindTexture(currentWindStrength);
        windTextureMaterial.SetColor("_Color", textureColor);
    }

    public float GetWindStrengthForWindTexture(WindManager.WindStrength windStrength) {

        if (windStrength == WindManager.WindStrength.soft) {
            return .1f;
        }
        if (windStrength == WindManager.WindStrength.medium) {
            return .25f;
        }
        if (windStrength == WindManager.WindStrength.strong) {
            return .4f;
        }
        if (windStrength == WindManager.WindStrength.extreme) {
            return .7f;
        }

        return 0;
    }

    public float GetAlphaForWindTexture(WindManager.WindStrength windStrength) {

        if (windStrength == WindManager.WindStrength.soft) {
            return .03f;
        }
        if (windStrength == WindManager.WindStrength.medium) {
            return .03f;
        }
        if (windStrength == WindManager.WindStrength.strong) {
            return .04f;
        }
        if (windStrength == WindManager.WindStrength.extreme) {
            return .05f;
        }

        return 0;
    }
}
