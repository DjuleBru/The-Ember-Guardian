using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoliageObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer foliageSprite;
    [SerializeField] private float softWindVelocityValue;
    [SerializeField] private float mediumWindVelocityValue;
    [SerializeField] private float strongWindVelocityValue;
    [SerializeField] private float extremeWindVelocityValue;

    private Material foliageMaterial;
    private Vector2 currentWindSpeed;

    private void Start() {
        foliageMaterial = foliageSprite.material;

        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;
        SetMaterialVariables();
    }

    private void WindManager_OnWindStrengthChanged(object sender, System.EventArgs e) {
        SetMaterialVariables();
    }

    private void SetMaterialVariables() {

        WindManager.WindStrength currentWindStrength = WindManager.Instance.GetWindStrength();
        float windStrength = GetWindStrengthForFoliage(currentWindStrength) * -WindManager.Instance.GetWindDir();
        SetWindVelocityValue(currentWindStrength);

        if (currentWindSpeed.x == 0) {
            foliageMaterial.SetFloat("Vector1_2d61041f8dfd46289cb8aafd27290417", 0);
        }
        else {
            float windStrengthRandomized = UnityEngine.Random.Range(windStrength - windStrength / 2, windStrength + windStrength / 2);
            foliageMaterial.SetFloat("Vector1_2d61041f8dfd46289cb8aafd27290417", windStrengthRandomized);
        }
    }

    private void SetWindVelocityValue(WindManager.WindStrength windStrength) {

        Vector2 windSpeed = new Vector2(0, 0);

        if (windStrength == WindManager.WindStrength.soft) {
            windSpeed = new Vector2(softWindVelocityValue, softWindVelocityValue);
        }

        if (windStrength == WindManager.WindStrength.medium) {
            windSpeed = new Vector2(mediumWindVelocityValue, mediumWindVelocityValue);
        }

        if (windStrength == WindManager.WindStrength.strong) {
            windSpeed = new Vector2(strongWindVelocityValue, strongWindVelocityValue);
        }

        if (windStrength == WindManager.WindStrength.extreme) {
            windSpeed = new Vector2(extremeWindVelocityValue, extremeWindVelocityValue);
        }

        currentWindSpeed = windSpeed;
        foliageMaterial.SetVector("Vector2_2ad9dffd23234809bbb6d55338af2214", windSpeed);
    }

    public float GetWindStrengthForFoliage(WindManager.WindStrength windStrength) {

        if (windStrength == WindManager.WindStrength.soft) {
            return 1f;
        }
        if (windStrength == WindManager.WindStrength.medium) {
            return 2f;
        }
        if (windStrength == WindManager.WindStrength.strong) {
            return 3f;
        }
        if (windStrength == WindManager.WindStrength.extreme) {
            return 4f;
        }

        return 0;
    }
}
