using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocationVisual : MonoBehaviour
{

    [SerializeField] private GameObject slotVisual;
    [SerializeField] private SpriteRenderer structureVisual_Build;

    private StructureLocation structureLocation;
    private bool buildable;

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


        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;

        if(structureLocation.GetStructureSOToBuild().buildableAtNight) {
            buildable = true;
        } else {
            buildable = (DayNightManager.Instance.GetDayNightCycleState() != DayNightManager.State.Night);
        }
        
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
        if (!buildable) return;
        HideVisuals();
    }

    private void StructureLocation_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (!buildable) return;
        ShowAllVisuals();
    }

    private void HideVisuals() {
        slotVisual.GetComponent<Animator>().enabled = false;

        structureVisual_Build.GetComponent<Animator>().ResetTrigger("Show");
        structureVisual_Build.GetComponent<Animator>().SetTrigger("Hide");
    }

    private void ShowAllVisuals() {
        slotVisual.GetComponent<Animator>().enabled = true;

        structureVisual_Build.gameObject.SetActive(true);
        structureVisual_Build.GetComponent<Animator>().ResetTrigger("Hide");
        structureVisual_Build.GetComponent<Animator>().SetTrigger("Show");
    }

    private void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (!structureLocation.GetStructureSOToBuild().buildableAtNight) {
            buildable = true;
        }
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (!structureLocation.GetStructureSOToBuild().buildableAtNight) {
            HideVisuals();
            buildable = false;
        }
    }

    private void OnDestroy() {
        DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
    }
}
