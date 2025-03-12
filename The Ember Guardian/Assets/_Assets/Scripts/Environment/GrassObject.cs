using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassObject : MonoBehaviour
{

    [SerializeField] private SpriteRenderer grassSprite;
    private Material grassMaterial;

    public static int playerTriggerCount;
    public static bool playerIsInAnyGrassArea;

    [SerializeField] private FireFlies fireflies;
    [SerializeField] private float probabilityToTriggerFireFlies;

    private void Start() {
        grassMaterial = grassSprite.material;

        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;
        SetMaterialVariables();
    }

    private void WindManager_OnWindStrengthChanged(object sender, System.EventArgs e) {
        SetMaterialVariables();
    }

    private void SetMaterialVariables() {
        WindManager.WindStrength currentWindStrength = WindManager.Instance.GetWindStrength();
        float windStrength = GetWindStrengthForGrass(currentWindStrength) * -WindManager.Instance.GetWindDir();

        grassMaterial.SetFloat("Vector1_2d61041f8dfd46289cb8aafd27290417", windStrength);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        playerTriggerCount++;

        HandleFireflySpawn();

        if (playerTriggerCount > 0 && !playerIsInAnyGrassArea) {
            playerIsInAnyGrassArea = true;
        }
    }

    private void HandleFireflySpawn() {

        float randomFloat = UnityEngine.Random.Range(0f, 1f);

        if (randomFloat < probabilityToTriggerFireFlies) {
            if (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Dawn) return;
            fireflies.SpawnFireflies();
        }

    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        playerTriggerCount--;

        if (playerTriggerCount == 0 && playerIsInAnyGrassArea) {
            playerIsInAnyGrassArea = false;
        }
    }


    public float GetWindStrengthForGrass(WindManager.WindStrength windStrength) {

        if (windStrength == WindManager.WindStrength.soft) {
            return 1f;
        }
        if (windStrength == WindManager.WindStrength.medium) {
            return 2f;
        }
        if (windStrength == WindManager.WindStrength.strong) {
            return 4f;
        }
        if (windStrength == WindManager.WindStrength.extreme) {
            return 6f;
        }

        return 0;
    }

}
