using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeVisual : StructureVisual
{
    [SerializeField] private List<GameObject> level1BarricadeSprites;
    [SerializeField] private List<GameObject> level2BarricadeSprites;
    [SerializeField] private List<GameObject> level3BarricadeSprites;
    [SerializeField] private List<GameObject> level4BarricadeSprites;

    protected override void Awake() {
        base.Awake();
        DeactivateAllSprites();
        ActivateSprites(level1BarricadeSprites);
    }

    protected override void Structure_OnStructureUpgraded(object sender, System.EventArgs e) {
        DeactivateAllSprites();

        int structureLevel = structure.GetStructureLevel();

        if(structureLevel == 2) {
            ActivateSprites(level2BarricadeSprites);
        }
        if (structureLevel == 3) {
            ActivateSprites(level3BarricadeSprites);
        }
        if (structureLevel == 4) {
            ActivateSprites(level4BarricadeSprites);
        }

    }

    private void ActivateSprites(List<GameObject> gameObjectList) {
        foreach(GameObject gameObject in gameObjectList) {
            gameObject.SetActive(true);
        }
    }

    private void DeactivateAllSprites() {
        foreach (GameObject gameObject in level1BarricadeSprites) {
            gameObject.SetActive(false);
        }
        foreach (GameObject gameObject in level2BarricadeSprites) {
            gameObject.SetActive(false);
        }
        foreach (GameObject gameObject in level3BarricadeSprites) {
            gameObject.SetActive(false);
        }
        foreach (GameObject gameObject in level4BarricadeSprites) {
            gameObject.SetActive(false);
        }
    }
}
