using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreachUI : MonoBehaviour
{
    [SerializeField] private GameObject leftBreachGameObject;
    [SerializeField] private GameObject rightBreachGameObject;

    private bool leftSideBreached;
    private bool leftSideBreachedShown;
    private bool rightSideBreached;
    private bool rightSideBreachedShown;

    private void Awake() {
        leftBreachGameObject.SetActive(false);
        rightBreachGameObject.SetActive(false);
    }

    private void Start() {
        Barricade.OnAnyBarricadeDestroyed += Barricade_OnAnyBarricadeDestroyed;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
    }

    private void Update() {
        //if(leftSideBreached) {
        //    if(leftSideBreachedShown) {
        //        if(Player.Instance.transform.position)
        //    }
        //}
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        RefreshSidesBreached();
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        leftBreachGameObject.SetActive(false);
        rightBreachGameObject.SetActive(false);
        leftSideBreached = false;
        rightSideBreached = false;
    }

    private void Barricade_OnAnyBarricadeDestroyed(object sender, System.EventArgs e) {
        RefreshSidesBreached();
    }

    private void RefreshSidesBreached() {
        if (CampZoneManager.Instance.GetLeftBreach()) {
            leftBreachGameObject.SetActive(true);
            leftSideBreached = true;
        }
        else {
            leftBreachGameObject.SetActive(false);
            leftSideBreached = false;
        }

        if (CampZoneManager.Instance.GetRightBreach()) {
            rightBreachGameObject.SetActive(true);
            rightSideBreached = true;
        }
        else {
            rightBreachGameObject.SetActive(false);
            rightSideBreached = false;
        }
    }

    private void OnDestroy() {
        Barricade.OnAnyBarricadeDestroyed -= Barricade_OnAnyBarricadeDestroyed;
    }
}
