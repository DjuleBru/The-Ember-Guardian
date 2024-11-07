using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeVisual : StructureVisual
{
    [SerializeField] private List<GameObject> level1BarricadeSprites;
    [SerializeField] private List<GameObject> level2BarricadeSprites;
    [SerializeField] private List<GameObject> level3BarricadeSprites;
    [SerializeField] private List<GameObject> level4BarricadeSprites;

    private List<GameObject> currentLevelBarricadeSprites;
    private List<Vector3> currentLevelBarricadeSpritesPositions = new List<Vector3>();

    private Barricade barricade;
    private int spriteIndex = 1;

    public event EventHandler OnBarricadeSpriteFell;

    protected override void Awake() {
        base.Awake();
        barricade = GetComponentInParent<Barricade>();

        DeactivateAllSprites();
        ActivateSprites(level1BarricadeSprites);
    }

    protected override void Start() {
        base.Start();
        barricade.OnBarricadeDamageTaken += Barricade_OnBarricadeDamageTaken;
        barricade.OnBarricadeRepaired += Barricade_OnBarricadeRepaired;
    }

    private void Barricade_OnBarricadeRepaired(object sender, System.EventArgs e) {
        RepairStructureVisual();
    }

    private void Barricade_OnBarricadeDamageTaken(object sender, System.EventArgs e) {
        float barricadeHealthNormalized = barricade.GetBarricadeHealthNormalized();
        float spriteIndexNormalized = 1 - ((float)spriteIndex / (float)currentLevelBarricadeSprites.Count);

        Vector2 force = new Vector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(2, 4));
        float torque = UnityEngine.Random.Range(-2, 2);

        if (spriteIndex == currentLevelBarricadeSprites.Count +1) return;

        if(barricadeHealthNormalized <= spriteIndexNormalized) {
            currentLevelBarricadeSprites[spriteIndex -1].GetComponent<Rigidbody2D>().gravityScale = 1.5f;
            currentLevelBarricadeSprites[spriteIndex -1].GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
            currentLevelBarricadeSprites[spriteIndex -1].GetComponent<Rigidbody2D>().AddTorque(torque, ForceMode2D.Impulse);
            OnBarricadeSpriteFell?.Invoke(this, EventArgs.Empty);

            StartCoroutine(DeactivateBarricadeSpriteAfterDelay(currentLevelBarricadeSprites[spriteIndex - 1]));

            spriteIndex++;
        } else {
            currentLevelBarricadeSprites[spriteIndex - 1].GetComponent<Animator>().SetTrigger("Damaged");
        }
    }

    private IEnumerator DeactivateBarricadeSpriteAfterDelay(GameObject gameObject) {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
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

        currentLevelBarricadeSprites = gameObjectList;
        currentLevelBarricadeSpritesPositions.Clear();

        foreach (GameObject gameObject in currentLevelBarricadeSprites) {
            currentLevelBarricadeSpritesPositions.Add(gameObject.transform.position);
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


    protected void RepairStructureVisual() {
        int i = 0;

        foreach(GameObject gameObject in currentLevelBarricadeSprites) {
            gameObject.SetActive(true);
            gameObject.transform.position = currentLevelBarricadeSpritesPositions[i];
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            i++;
        }
    }

    protected override void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        foreach(GameObject go in currentLevelBarricadeSprites) {
            go.GetComponent<SpriteRenderer>().material = unhoveredMaterial;
        }
    }

    protected override void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        foreach (GameObject go in currentLevelBarricadeSprites) {
            go.GetComponent<SpriteRenderer>().material = hoveredMaterial;
        }
    }
}
