using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Obstacle : MonoBehaviour {

    protected PayCurrencyUI payCurrencyUI;
    [SerializeField] protected List<Collider2D> blockingColliders;
    [SerializeField] protected TilemapCollider2D obstacleSolidCollider;
    [SerializeField] protected Collider2D detectionCollicer;
    [SerializeField] protected AudioClip obstacleBuiltAudioClip;

    [SerializeField] protected Transform orbTemplateWorldUIParent;

    protected List<PayCurrencyTemplateWorldUI> buildStructureOrbTemplates = new List<PayCurrencyTemplateWorldUI>();

    private bool hubScene;
    private bool playerInTriggerArea;
    private bool obstacleBuilt;

    public event EventHandler OnObstacleBuilt;
    public static event EventHandler OnAnyObstacleBuilt;
    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;

    protected void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();

        obstacleSolidCollider.enabled = false;
    }

    protected void Start() {
        hubScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;

        payCurrencyUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payCurrencyUI.SetOrbTemplateUIList(buildStructureOrbTemplates);
    }

    protected void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        BuildObstacle();
    }

    private void BuildObstacle() {
      foreach(Collider2D collider in blockingColliders) {
            collider.enabled = false;
      }

      obstacleSolidCollider.enabled = true;
      obstacleBuilt = true;
      OnObstacleBuilt?.Invoke(this, EventArgs.Empty);
      OnAnyObstacleBuilt?.Invoke(this, EventArgs.Empty);
    }

    protected void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (hubScene) return;
        if (!playerInTriggerArea) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetInPayCurrencyArea(true);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

        Player.Instance.SetInPayCurrencyArea(false);
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        payCurrencyUI.SetPlayerInteracting(false);
        playerInTriggerArea = false;
    }

    protected void InitializeOrbTemplateList() {
        PayCurrencyTemplateWorldUI[] orbTemplates = orbTemplateWorldUIParent.GetComponentsInChildren<PayCurrencyTemplateWorldUI>();

        foreach (PayCurrencyTemplateWorldUI orbTemplateWorldUI in orbTemplates) {
            buildStructureOrbTemplates.Add(orbTemplateWorldUI);
        }
    }

    public bool GetBuilt() {
        return obstacleBuilt;
    }

    public AudioClip GetObstacleBuiltAudioClip() {
        return obstacleBuiltAudioClip;
    }

}
