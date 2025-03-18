using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCreatureSpawnerGroup : MonoBehaviour
{
    [SerializeField] Transform dayCreatureSpawnerTemplate;
    [SerializeField] private float radiusToRoamAround;

    private void Awake() {
        dayCreatureSpawnerTemplate.gameObject.SetActive(false);
    }

    public void InitializeSpawnerGroup(List<CreatureSO> creatureSOList, List<int> creatureAmountList, List<int> creatureEliteAmountList) {
        int i = 0;

        float radiusToRoamAroundRandomized = UnityEngine.Random.Range(radiusToRoamAround - radiusToRoamAround/1.5f, radiusToRoamAround + radiusToRoamAround/1.5f);


        // Securite pour les spawners proches de la base : 
        if (Mathf.Abs(transform.position.x) < radiusToRoamAroundRandomized * 4) {
            radiusToRoamAroundRandomized /= 4;
        }  

        foreach (CreatureSO creatureSO in creatureSOList) {
            DayCreatureSpawner creatureSpawner = Instantiate(dayCreatureSpawnerTemplate, transform.position, Quaternion.identity, transform).GetComponent<DayCreatureSpawner>();
            creatureSpawner.gameObject.SetActive(true);

            UnityEngine.Random.Range(creatureSpawner.GetRadiusToRoamAround() / 2, creatureSpawner.GetRadiusToRoamAround() * 2);

            creatureSpawner.InitializeDayCreatureSpawner(creatureSO, creatureAmountList[i], creatureEliteAmountList[i], radiusToRoamAroundRandomized);

            i++;
        }
    }
}
