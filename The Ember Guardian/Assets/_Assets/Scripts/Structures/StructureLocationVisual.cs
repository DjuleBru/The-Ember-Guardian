using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocationVisual : MonoBehaviour
{

    [SerializeField] protected GameObject slotVisual;
    [SerializeField] protected SpriteRenderer structureVisual_Build;
    [SerializeField] protected SpriteRenderer structureVisual_ProgressionLocked;
    [SerializeField] protected Sprite[] progressionLockedCampSprites;
    [SerializeField] protected Transform payCurrenciesGO;

    protected StructureLocation structureLocation;
    protected bool buildable;
    protected bool progression_locked;

    protected virtual void Awake() {
        structureLocation = GetComponentInParent<StructureLocation>();
        structureVisual_Build.gameObject.SetActive(false);
        slotVisual.SetActive(false);

        structureLocation.OnStructureLocationLoaded_Locked += StructureLocation_OnStructureLocationLoaded_Locked;
        structureLocation.OnPlayerTriggeredIn += StructureLocation_OnPlayerTriggeredIn;
        structureLocation.OnPlayerTriggeredOut += StructureLocation_OnPlayerTriggeredOut;
        structureLocation.OnStructureLocationUnlocked += StructureLocation_OnStructureLocationUnlocked;
    }


    protected virtual void Start() {
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

    protected void SetXAxisScale() {
        float scaleX = structureLocation.transform.position.x;

        if(structureLocation.GetIsWorldStructureLocation()) {
            scaleX = structureLocation.GetStructureLocationWorldScaleX();
        }

        if (scaleX < 0) {
            Vector3 localScale = new Vector3(-1, 1, 1);
            transform.localScale = localScale;

            payCurrenciesGO.localScale = new Vector3(-1, 1, 1);
        }
    }

    protected virtual void StructureLocation_OnStructureLocationLoaded_Locked(object sender, EventArgs e) {
        progression_locked = true;
        structureVisual_ProgressionLocked.sprite = progressionLockedCampSprites[UnityEngine.Random.Range(0, progressionLockedCampSprites.Length)];
    }

    protected virtual void StructureLocation_OnStructureLocationUnlocked(object sender, System.EventArgs e) {
        progression_locked = false;
        structureVisual_ProgressionLocked.gameObject.SetActive(false);

        slotVisual.SetActive(true);
    }

    protected void StructureLocation_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (progression_locked) return;
        if (!buildable) return;
        HideVisuals();
    }

    protected void StructureLocation_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (progression_locked) return;
        if (!buildable) return;
        ShowAllVisuals();
    }

    protected virtual void HideVisuals() {
        slotVisual.GetComponent<Animator>().enabled = false;

        structureVisual_Build.GetComponent<Animator>().ResetTrigger("Show");
        structureVisual_Build.GetComponent<Animator>().SetTrigger("Hide");
    }

    protected virtual void ShowAllVisuals() {
        slotVisual.GetComponent<Animator>().enabled = true;

        structureVisual_Build.gameObject.SetActive(true);
        structureVisual_Build.GetComponent<Animator>().ResetTrigger("Hide");
        structureVisual_Build.GetComponent<Animator>().SetTrigger("Show");
    }

    protected void DayNightManager_OnDawnStart(object sender, EventArgs e) {
        if (progression_locked) return;

        if (!structureLocation.GetStructureSOToBuild().buildableAtNight) {
            buildable = true;
        }
    }

    protected void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (progression_locked) return;

        if (!structureLocation.GetStructureSOToBuild().buildableAtNight) {
            HideVisuals();
            buildable = false;
        }
    }

    protected void OnDestroy() {
        DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
        DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
    }
}
