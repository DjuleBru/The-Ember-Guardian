using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchant : MonoBehaviour
{

    public enum HubMerchantType {
        GemMerchant,
        StructuresMerchant,
        HeroMerchant,
        WorkerMerchant,
        MushroomMerchant,
        GunMerchant,
        DogTamer,
        Codex
    }

    [SerializeField] protected HubMerchantType hubMerchantType;
    [SerializeField] protected Transform hubMerchantCameraFocusPosition;
    [SerializeField] protected string hubMerchantName;
    [SerializeField] protected bool hubMerchantUnlockedAtStart;

    [SerializeField] protected GameObject activeGameObject;
    [SerializeField] protected GameObject inactiveGameObject;

    [SerializeField] protected bool DEBUGActivateMerchant;

    [SerializeField] protected GameObject hubMerchantItemParent;
    protected List<HubMerchantItem> hubMerchantItems = new List<HubMerchantItem>();

    protected bool playerInTriggerArea;
    protected bool playerInteractingWithMerchant;
    protected bool merchantJustArrivedInHub;
    protected bool merchantHasTalkLinesToShow;
    protected bool merchantHasNewItems;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnPlayerOpenedHubMerchantShop;
    public event EventHandler OnPlayerStoppedInteractingWithHubMerchant;
    public event EventHandler OnPlayerStartedTalkingWithHubMerchant;
    public static event EventHandler OnPlayerOpenedAnyHubMerchantShop;
    public static event EventHandler OnPlayerStoppedInteractingWithAnyHubMerchant;

    protected bool isHubMerchant;
    protected bool isLevelNPC;

    protected void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            isHubMerchant = true;
            isLevelNPC = false;
        }
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            isHubMerchant = false;
            isLevelNPC = true;
        }

        if (isHubMerchant) {
            // HUB behavior
            InitializeHubMerchantInHub();
        }

        if (isLevelNPC) {
            // Level behavior : idle, talk to player, disappear
            merchantHasTalkLinesToShow = true;
        }
    }

    protected void InitializeHubMerchantInHub() {
        if ((!MetaProgressionManager.Instance.GetMerchantUnlocked(hubMerchantType) && !hubMerchantUnlockedAtStart) && !DEBUGActivateMerchant) {
            activeGameObject.SetActive(false);
            inactiveGameObject.SetActive(true);
        }
        else {
            activeGameObject.SetActive(true);
            inactiveGameObject.SetActive(false);
            InitializeHubMerchantItems();
        }

        merchantJustArrivedInHub = MetaProgressionManager.Instance.GetMerchantJustArrivedInHub(hubMerchantType);
        if (merchantJustArrivedInHub) {
            merchantHasTalkLinesToShow = true;
        }
        else {
            merchantHasTalkLinesToShow = MetaProgressionManager.Instance.GetMerchantHasTalkLinesToShow(hubMerchantType);
        }

    }

    protected void InitializeHubMerchantItems() {

        foreach (HubMerchantItem hubMerchantItem in hubMerchantItemParent.GetComponentsInChildren<HubMerchantItem>()) {
            hubMerchantItems.Add(hubMerchantItem);
        }
    }

    private void GameInput_OnPlayerPausePerformed(object sender, EventArgs e) {
        TryStopInteractingWithMerchant();
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        TryStopInteractingWithMerchant();
    }

    protected void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        TryStartInteractingWithMerchant();
    }

    private void TryStopInteractingWithMerchant() {
        if (!playerInTriggerArea) return;
        if (merchantHasTalkLinesToShow) return;

        if (playerInteractingWithMerchant) {
            StopInteractingWithMerchant();
        }
    }
    private void TryStartInteractingWithMerchant() {
        if (!playerInTriggerArea) return;
        if (playerInteractingWithMerchant) return;

        if (isHubMerchant) {
            StartInteractingWithMerchant();
        }

        if (isLevelNPC && merchantHasTalkLinesToShow) {
            StartInteractingWithMerchant();
        }
    }

    private void StartInteractingWithMerchant() {
        Player.Instance.StartInteractingWithMerchant();
        PauseMenuUI.Instance.SetCanOpenPauseMenu(false);

        if (merchantHasTalkLinesToShow) {
            OnPlayerStartedTalkingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnPlayerOpenedHubMerchantShop?.Invoke(this, EventArgs.Empty);
            OnPlayerOpenedAnyHubMerchantShop?.Invoke(this, EventArgs.Empty);
        }

        playerInteractingWithMerchant = true;
    }

    private void StopInteractingWithMerchant() {
        Player.Instance.StopInteractingWithMerchant();
        CameraManager.Instance.ResetCameraTargetToPlayer();
        PauseMenuUI.Instance.SetCanOpenPauseMenu(true);

        OnPlayerStoppedInteractingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        OnPlayerStoppedInteractingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

        if (merchantHasNewItems) {
            merchantHasNewItems = false;
        }

        if (merchantJustArrivedInHub) {
            merchantJustArrivedInHub = false;
        }

        playerInteractingWithMerchant = false;
    }

    public void StartTalkingWithMerchant() {
        Player.Instance.StartInteractingWithMerchant();

        merchantHasTalkLinesToShow = true;
        playerInteractingWithMerchant = true;

        OnPlayerStartedTalkingWithHubMerchant?.Invoke(this, EventArgs.Empty);
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<Player>() != null) {
            playerInTriggerArea = true;
            Player.Instance.SetCanDropOrbOnTheFloor(true);
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() != null) {
            playerInTriggerArea = false;
            Player.Instance.SetCanDropOrbOnTheFloor(false);
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetPlayerFinishedTalkingWithMerchant(bool openShopPanel) {
        merchantHasTalkLinesToShow = false;

        if(openShopPanel && isHubMerchant) {
            StartInteractingWithMerchant();
        } else {
            StopInteractingWithMerchant();
        }
    }

    public HubMerchantType GetHubMerchantType() {
        return hubMerchantType;
    }

    public Transform GetCameraFocusTransform() {
        return hubMerchantCameraFocusPosition;
    }

    public string GetHubMerchantName() {
        return hubMerchantName;
    }

    public bool GetMerchantHasNewItems() {
        return merchantHasNewItems;
    }

    public bool GetMerchantJustArrivedInHub() {
        return merchantJustArrivedInHub;
    }

    public bool GetMerchantIsLevelNPC() {
        return isLevelNPC;
    }

    public void SaveMerchant() {
        MetaProgressionManager.Instance.SetMerchantHasTalkLinesToShow(hubMerchantType, merchantHasTalkLinesToShow);
        MetaProgressionManager.Instance.SetMerchantJustArrivedInHub(hubMerchantType, merchantJustArrivedInHub);

        foreach(HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.SaveItemStatus();
        }
    }

}
