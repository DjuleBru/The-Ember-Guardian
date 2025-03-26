using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCreatureSpawnerManager : MonoBehaviour
{
    private List<DayCreatureSpawnerGroup> spawnerGroupsInLevel;
    private List<CreatureSO> dayCreaturesInLevel;
    private int totalDayCreatureDifficulty;

    private void Awake() {
        DayCreatureSpawnerGroup[] spawners = GetComponentsInChildren<DayCreatureSpawnerGroup>();
        spawnerGroupsInLevel = new List<DayCreatureSpawnerGroup>();
        foreach (DayCreatureSpawnerGroup group in spawners) {
            spawnerGroupsInLevel.Add(group);
        }
    }

    private void Start() {
        dayCreaturesInLevel = LevelManager.Instance.GetLevelSO().dayCreatureTypes;
        totalDayCreatureDifficulty = LevelManager.Instance.GetLevelSO().totalDayCreatureDifficulty;

        InitializeSpawnerGroups();
    }


    private void InitializeSpawnerGroups() {
        // Calculer la difficulté totale restante pour répartir
        int remainingDifficulty = totalDayCreatureDifficulty;
        int totalGroups = spawnerGroupsInLevel.Count;

        // Définir un nombre maximum de créatures par groupe
        int minCreatureTypesPerGroup = 1;
        int maxCreatureTypesPerGroup = 3;

        // Calculer les limites du niveau
        float levelMax = LevelManager.Instance.GetMaxLevelLimitAbsolute();  // Supposons que vous utilisez une valeur 2D, x est suffisant
        float levelMin = LevelManager.Instance.GetMinLevelLimitAbsolute();  // Idem pour le min

        // Répartir uniformément la difficulté entre tous les groupes
        int baseDifficultyPerGroup = Mathf.FloorToInt(remainingDifficulty / totalGroups);

        // Calculer la distance maximale entre le centre et les bords du niveau
        float levelMaxSizeXWidth = Mathf.Max(Mathf.Abs(levelMax), Mathf.Abs(levelMin));

        // Parcourir chaque groupe de spawners
        foreach (DayCreatureSpawnerGroup spawnerGroup in spawnerGroupsInLevel) {
            // Calculer la distance du centre
            float distanceToCenter = Vector2.Distance(spawnerGroup.transform.position, Vector2.zero);

            // Courbe d'ajustement pour éviter une chute trop brutale
            float difficultyFactor = Mathf.Pow(distanceToCenter / levelMaxSizeXWidth, 0.6f); // Exponent < 1 pour lisser la montée

            // Normaliser la distance par rapport à la largeur du niveau pour obtenir un facteur de difficulté
            float normalizedDistance = Mathf.InverseLerp(0, levelMaxSizeXWidth, Mathf.Abs(spawnerGroup.transform.position.x));

            // Ajuster la difficulté de base par un facteur lié à la distance du centre
            int groupDifficulty =  Mathf.FloorToInt(difficultyFactor * baseDifficultyPerGroup);

            //Debug.Log("distanceToCenter " + distanceToCenter + " groupDifficulty " + groupDifficulty);

            // Sélectionner les créatures à spawner pour ce groupe (maximum 3 types)
            List<CreatureSO> selectedCreatures = new List<CreatureSO>();  // Liste des créatures à spawner
            List<int> selectedAmounts = new List<int>();  // Liste des quantités de chaque créature
            List<int> selectedEliteAmounts = new List<int>();  // Liste des quantités de créatures élites

            // Randomiser le nombre de types de créatures entre 1 et 3
            int numCreatureTypes = UnityEngine.Random.Range(minCreatureTypesPerGroup, maxCreatureTypesPerGroup + 1);

            // Mélanger la liste des créatures disponibles pour éviter toujours le même ordre de sélection
            List<CreatureSO> availableCreatures = new List<CreatureSO>(dayCreaturesInLevel);
            Shuffle(availableCreatures);

            // Sélectionner aléatoirement jusqu'à `numCreatureTypes` créatures parmi les créatures disponibles
            for (int i = 0; i < numCreatureTypes; i++) {
                CreatureSO creature = availableCreatures[i]; // Sélectionner la créature aléatoirement
                selectedCreatures.Add(creature);
            }

            // Maintenant, répartir le nombre total de créatures entre les types choisis
            int remainingGroupDifficulty = groupDifficulty;

            //Debug.Log(" numCreatureTypes " + numCreatureTypes);

            // Répartir les créatures sélectionnées de manière aléatoire
            foreach (CreatureSO creature in selectedCreatures) {
                // Attribuer une quantité aléatoire de créatures (au moins 1)
                int minCreatureTypeDifficulty = 1; // Minimum 1 créature
                int maxCreatureTypeDifficulty = remainingGroupDifficulty; // Diviser la difficulté entre les types

                int creatureTypeDifficultyRandomized = UnityEngine.Random.Range(minCreatureTypeDifficulty, maxCreatureTypeDifficulty + 1);

                if(creature == selectedCreatures[selectedCreatures.Count - 1]) {
                    creatureTypeDifficultyRandomized = remainingGroupDifficulty;
                }

                remainingGroupDifficulty -= creatureTypeDifficultyRandomized;

                int creatureDifficulty = creature.difficulty;
                int spawnAmount = Mathf.FloorToInt(creatureTypeDifficultyRandomized / creatureDifficulty);

                // Assigner un nombre aléatoire entre 1 et `maxCreaturesForThisType`
                if(spawnAmount <=1) {
                    spawnAmount = 1;
                }

                // Assigner un nombre d'élites (facultatif)
                int eliteAmount = Mathf.FloorToInt(spawnAmount * 0.1f);  // 10% d'élites

                // Ajouter à la liste des quantités
                selectedAmounts.Add(spawnAmount);
                selectedEliteAmounts.Add(eliteAmount);
            }

            // Initialiser le DayCreatureSpawnerGroup avec les créatures sélectionnées
            spawnerGroup.InitializeSpawnerGroup(selectedCreatures, selectedAmounts, selectedEliteAmounts);
        }

        
    }


    // Fonction pour mélanger une liste de créatures
    private void Shuffle<T>(List<T> list) {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
