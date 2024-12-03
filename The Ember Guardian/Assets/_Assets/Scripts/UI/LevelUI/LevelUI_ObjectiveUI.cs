using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUI_ObjectiveUI : MonoBehaviour
{
    public static LevelUI_ObjectiveUI Instance;

    [SerializeField] private GameObject objectiveGameObject;
    [SerializeField] private TextMeshProUGUI objectiveText;

    public event EventHandler OnObjectiveUIShown;

    private void Awake() {
        Instance = this;
        objectiveGameObject.SetActive(false);
        GetComponent<Animator>().enabled = false;
    }

    public void ShowObjectiveUI(string text) {
        objectiveText.text = text;
        GetComponent<Animator>().enabled = false;
        GetComponent<Animator>().enabled = true;
        objectiveGameObject.SetActive(true);
        OnObjectiveUIShown?.Invoke(this, EventArgs.Empty);
    }
}
