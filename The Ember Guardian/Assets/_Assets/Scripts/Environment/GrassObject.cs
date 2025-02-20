using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassObject : MonoBehaviour
{

    public static int playerTriggerCount;
    public static bool playerIsInAnyGrassArea;

    [SerializeField] private FireFlies fireflies;
    [SerializeField] private float probabilityToTriggerFireFlies;

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
            if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Day) return;
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

}
