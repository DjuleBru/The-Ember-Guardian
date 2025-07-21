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

    protected bool hubScene;
    protected bool playerInTriggerArea;
    protected bool obstacleBuilt;

    public event EventHandler OnObstacleBuilt;
    public static event EventHandler OnAnyObstacleInitialized;
    public static event EventHandler OnAnyObstacleBuilt;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredIn;
    public static event EventHandler OnAnyPlayerTriggeredOut;
    public event EventHandler OnPlayerTriggeredOut;

    protected virtual void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();

        obstacleSolidCollider.enabled = false;
    }

    protected virtual void Start() {
        hubScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;

        payCurrencyUI.OnCurrencyPaymentSuccess += PayOrbsUI_OnOrbPaymentSuccess;
        payCurrencyUI.SetOrbTemplateUIList(buildStructureOrbTemplates);

        OnAnyObstacleInitialized?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {
        BuildObstacle();
    }

    public virtual void BuildObstacle() {
        foreach(Collider2D collider in blockingColliders) {
              collider.enabled = false;
        }

        obstacleSolidCollider.enabled = true;
        obstacleBuilt = true;
        InvokeObstacleBuiltEvents();
        SetTriggerExit();
    }

    public void InvokeObstacleBuiltEvents() {
        OnObstacleBuilt?.Invoke(this, EventArgs.Empty);
        OnAnyObstacleBuilt?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        //if (!Player.Instance.GetCanInteractWithStructureLocation()) return;

        payCurrencyUI.SetPlayerInteracting(true);
    }

    protected virtual void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (hubScene) return;
        if (!playerInTriggerArea) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (obstacleBuilt) return;
        SetTriggerEnter();
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        SetTriggerExit();
    }

    protected void SetTriggerEnter() {
        Player.Instance.SetInPayCurrencyArea(true);
        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        playerInTriggerArea = true;
    }

    protected void SetTriggerExit() {
        Player.Instance.SetInPayCurrencyArea(false);
        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
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
