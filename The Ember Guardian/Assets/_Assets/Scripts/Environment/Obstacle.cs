using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Obstacle : MonoBehaviour {

    [SerializeField] private string obstacleID;
    public string GetObstacleID() => obstacleID;

#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrEmpty(obstacleID) && gameObject.scene.IsValid()) {
            obstacleID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    public void ForceNewID() {
        obstacleID = System.Guid.NewGuid().ToString();
    }
#endif

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

    public event EventHandler<OnObstacleBuiltEventArgs> OnObstacleBuilt;
    public static event EventHandler OnAnyObstacleInitialized;
    public static event EventHandler<OnObstacleBuiltEventArgs> OnAnyObstacleBuilt;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredIn;
    public static event EventHandler OnAnyPlayerTriggeredOut;
    public event EventHandler OnPlayerTriggeredOut;

    public class OnObstacleBuiltEventArgs:EventArgs {
        public bool builtFromGame;
    }

    protected virtual void Awake() {
        payCurrencyUI = GetComponent<PayCurrencyUI>();
        InitializeOrbTemplateList();

        if (obstacleSolidCollider != null) {
            obstacleSolidCollider.enabled = false;
        }
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

    public virtual void BuildObstacle(bool triggerSFX = true) {

        foreach(Collider2D collider in blockingColliders) {
              collider.enabled = false;
        }

        if(obstacleSolidCollider != null) {
            obstacleSolidCollider.enabled = true;
        }

        obstacleBuilt = true;
        InvokeObstacleBuiltEvents(triggerSFX);
        SetTriggerExit();
    }


    public void InvokeObstacleBuiltEvents(bool builtFromGame = true) {
        StartCoroutine(InvokeObstacleBuiltEventsAfterFrame(builtFromGame));
    }

    private IEnumerator InvokeObstacleBuiltEventsAfterFrame(bool builtFromGame = true) {
        yield return new WaitForEndOfFrame();

        OnObstacleBuilt?.Invoke(this, new OnObstacleBuiltEventArgs {
            builtFromGame = builtFromGame
        });
        OnAnyObstacleBuilt?.Invoke(this, new OnObstacleBuiltEventArgs {
            builtFromGame = builtFromGame
        });
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
