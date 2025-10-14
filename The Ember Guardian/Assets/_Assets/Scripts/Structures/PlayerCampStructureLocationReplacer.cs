using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCampStructureLocationReplacer : MonoBehaviour
{
    [SerializeField] private StructureSO.StructureType structureTypeToReplace;
    [SerializeField] private StructureSO.StructureType structureTypeToReplaceWith;
    [SerializeField] private int structureAmountToReplace;

    private void Start() {
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        if (SavingManager_Level.Instance.GetLoadingSavedLevel()) return;
        StartCoroutine(ReplaceClosestStructuresAfterDelay(.1f));
    }

    public IEnumerator ReplaceClosestStructuresAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        float campCenterX = PlayerCamp.Instance.LayoutToWorldPosition(56, new StructureSO { widthInCells = 0 });

        List<StructureLocation> matchingLocations = new List<StructureLocation>();

        foreach (StructureLocation location in PlayerCamp.Instance.GetAllStructureLocations()) {
            if (location == null) continue;

            StructureSO so = location.GetStructureSOToBuild();
            if (so == null) continue;

            if (so.structureType != structureTypeToReplace) continue;

            matchingLocations.Add(location);
        }

        matchingLocations.Sort((a, b) => {
            float distA = Mathf.Abs(a.transform.position.x - campCenterX);
            float distB = Mathf.Abs(b.transform.position.x - campCenterX);
            return distA.CompareTo(distB);
        });

        int replacementsDone = 0;

        foreach (StructureLocation location in matchingLocations) {
            if (replacementsDone >= structureAmountToReplace) break;

            StructureSO newSO = StructuresManager.Instance.GetStructureSO(structureTypeToReplaceWith);
            if (newSO == null || newSO.structureLocationPrefab == null) {
                Debug.LogWarning($"StructureSO for type {structureTypeToReplaceWith} is null or has no prefab.");
                continue;
            }

            // Remplacer visuellement le prefab
            Vector3 position = location.transform.position;
            Transform parent = location.transform.parent;

            location.gameObject.SetActive(false);

            StructureLocation newLocation = GameObject.Instantiate(newSO.structureLocationPrefab, position, Quaternion.identity, parent).GetComponent<StructureLocation>();

            // Wait for location to initialize
            yield return new WaitForSeconds(.05f);

            newLocation.gameObject.SetActive(true);
            newLocation.UnlockStructureLocation();

            // Mise à jour de PlayerCamp (important si on veut que les listes restent correctes)
            PlayerCamp.Instance.ReplaceStructureLocation(location, newLocation);

            replacementsDone++;
        }

        Debug.Log($"Replaced {replacementsDone} structure(s) of type {structureTypeToReplace} with {structureTypeToReplaceWith}");
    }

}
