using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScavengableObstacleVisual : MonoBehaviour
{
    [SerializeField] private List<ScavengableObstacleSubElement> subElements;
    [SerializeField] private float fallForce = 2f;
    [SerializeField] private float torqueForce = 2f;
    [SerializeField] private float fallRandomness = 1f;
    private int elementsFallen = 0;

    [SerializeField] private ScavengableObstacle scavengableObstacle;

    public event EventHandler OnPieceFell;

    private void Start() {
        scavengableObstacle.OnDamageTaken += ScavengableObstacle_OnDamageTaken;
        scavengableObstacle.OnHealthLoaded += ScavengableObstacle_OnHealthLoaded;
    }

    private void ScavengableObstacle_OnHealthLoaded(object sender, EventArgs e) {
        SyncVisualWithHealth();
    }

    private void ScavengableObstacle_OnDamageTaken(object sender, System.EventArgs e) {
        float destructionProgress =scavengableObstacle.GetHealthNormalized();
        int targetFallCount = Mathf.FloorToInt(subElements.Count * destructionProgress);

        if (elementsFallen < subElements.Count) {
            subElements[elementsFallen].TakeDamage();
        }

        while (elementsFallen < targetFallCount && elementsFallen < subElements.Count) {
            subElements[elementsFallen].Fall(fallForce, torqueForce, fallRandomness);
            elementsFallen++;
            OnPieceFell?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SyncVisualWithHealth() {
        float destructionProgress = scavengableObstacle.GetHealthNormalized();
        int targetFallCount = Mathf.FloorToInt(subElements.Count * destructionProgress);

        // Reset
        elementsFallen = 0;

        // Appliquer la destruction déjà subie
        while (elementsFallen < targetFallCount && elementsFallen < subElements.Count) {
            subElements[elementsFallen].Deactivate();
            elementsFallen++;
        }
    }
}
