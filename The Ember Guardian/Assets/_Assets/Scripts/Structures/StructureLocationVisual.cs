using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocationVisual : MonoBehaviour
{

    [SerializeField] private GameObject slotVisual;
    [SerializeField] private SpriteRenderer structureVisual_Build;

    private StructureLocation structureLocation;

    private void Awake() {
        structureLocation = GetComponentInParent<StructureLocation>();
        structureVisual_Build.gameObject.SetActive(false);
        slotVisual.SetActive(false);

        structureLocation.OnPlayerTriggeredIn += StructureLocation_OnPlayerTriggeredIn;
        structureLocation.OnPlayerTriggeredOut += StructureLocation_OnPlayerTriggeredOut;
        structureLocation.OnStructureLocationUnlocked += StructureLocation_OnStructureLocationUnlocked;
    }

    private void Start() {
        SetXAxisScale();
        HideVisuals();
    }

    private void SetXAxisScale() {
        if(structureLocation.transform.position.x < 0) {
            Vector3 localScale = new Vector3(-1, 1, 1);
            transform.localScale = localScale;
        }
    }

    private void StructureLocation_OnStructureLocationUnlocked(object sender, System.EventArgs e) {
        slotVisual.SetActive(true);
    }

    private void StructureLocation_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        HideVisuals();
    }

    private void StructureLocation_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        ShowAllVisuals();
    }

    private void HideVisuals() {
        slotVisual.GetComponent<Animator>().enabled = false;

        structureVisual_Build.GetComponent<Animator>().SetTrigger("Hide");
    }

    private void ShowAllVisuals() {
        slotVisual.GetComponent<Animator>().enabled = true;

        structureVisual_Build.gameObject.SetActive(true);
        structureVisual_Build.GetComponent<Animator>().SetTrigger("Show");
    }

}
