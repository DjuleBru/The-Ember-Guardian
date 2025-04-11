using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeBreached : MonoBehaviour
{
   [SerializeField] private Barricade barricade;
   [SerializeField] private GameObject breachedVisual;
   [SerializeField] private BarricadeSound barricadeSound;
   private float edgeOffset = 1.0f; // Distance par rapport au bord de l'écran

    private Animator animator;
    private Camera mainCamera;
    private bool isNight;
    private bool barricadeBreached;
    private bool isInPlayerScreen;

    private Vector3 initialLocalPosition;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        mainCamera = Camera.main;
        initialLocalPosition = transform.localPosition;

        breachedVisual.gameObject.SetActive(false);
        animator.enabled = false;

        barricade.OnBarricadeDestroyed += Barricade_OnBarricadeDestroyed;
        barricade.OnBarricadeRepaired += Barricade_OnBarricadeRepaired;
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
    }


    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        isNight = true;
        if(barricadeBreached) {
            breachedVisual.gameObject.SetActive(true);
            animator.enabled = true;
        }
    }

    private void Barricade_OnBarricadeRepaired(object sender, System.EventArgs e) {
        barricadeBreached = false;
        breachedVisual.gameObject.SetActive(false);
        animator.enabled = false;
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        isNight = false; 
        breachedVisual.gameObject.SetActive(false);
        animator.enabled = false;
    }

    private void Barricade_OnBarricadeDestroyed(object sender, System.EventArgs e) {
        barricadeBreached = true;
        breachedVisual.gameObject.SetActive(true);
        animator.enabled = true;
    }

    private void Update() {
        if (!isNight) return;
        if (!barricadeBreached) return;

        Vector3 barricadeWorldPos = barricade.transform.position;
        Vector3 barricadeViewportPos = mainCamera.WorldToViewportPoint(barricadeWorldPos);

        // Vérifie si la barricade est dans l'écran
        isInPlayerScreen = barricadeViewportPos.x > 0 && barricadeViewportPos.x < 1 &&
                                  barricadeViewportPos.y > 0 && barricadeViewportPos.y < 1 &&
                                  barricadeViewportPos.z > 0;

        if (isInPlayerScreen) {
            transform.localPosition = initialLocalPosition;
        }
        else {
            // Obtenir les limites du monde aux bords de l'écran à la profondeur de la barricade
            Vector3 leftEdge = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, barricadeViewportPos.z));
            Vector3 rightEdge = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2, barricadeViewportPos.z));
            Vector3 topEdge = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, barricadeViewportPos.z));
            Vector3 bottomEdge = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, barricadeViewportPos.z));

            float xPos = barricadeWorldPos.x < leftEdge.x ? leftEdge.x + edgeOffset : (barricadeWorldPos.x > rightEdge.x ? rightEdge.x - edgeOffset : barricadeWorldPos.x);
            float yPos = barricadeWorldPos.y < bottomEdge.y ? bottomEdge.y + edgeOffset : (barricadeWorldPos.y > topEdge.y ? topEdge.y - edgeOffset : barricadeWorldPos.y);

            transform.position = new Vector3(xPos, yPos + initialLocalPosition.y, 0);
        }
    }

    public void TriggerBarricadeBreachedWarningDing() {
        if (isInPlayerScreen) return;
        barricadeSound.TriggerBarricadeBreachedWarningDing();
    }
}
