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

    protected bool DEBUGActivateMerchant;

    [SerializeField] protected GameObject hubMerchantItemParent;
    protected List<HubMerchantItem> hubMerchantItems = new List<HubMerchantItem>();

    protected bool merchantUnlocked;
    protected bool playerInTriggerArea;
    protected bool playerInteractingWithMerchant;
    protected bool merchantJustArrivedInHub;
    protected bool merchantHasTalkLinesToShow;
    protected bool merchantHasNewItems;

    public event EventHandler OnPlayerTriggeredIn;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnPlayerOpenedHubMerchantShop;
    public event EventHandler OnPlayerStoppedInteractingWithHubMerchant;
    public event EventHandler OnPlayerStartedTalkingWithHubMerchant;
    public event EventHandler OnMerchantHasNewTalkLines;
    public event EventHandler OnMerchantHideExclamationMark;
    public static event EventHandler OnPlayerStartedTalkingWithAnyHubMerchant;
    public static event EventHandler OnPlayerOpenedAnyHubMerchantShop;
    public static event EventHandler OnPlayerStoppedInteractingWithAnyHubMerchant;

    [SerializeField] protected bool isHubMerchant;
    [SerializeField] protected bool isLevelNPC;

    [SerializeField] protected bool isFunctionalDemoHubMerchant;
    [SerializeField] protected bool isDecorationalDemoHubMerchant;

    protected bool hubMerchantLoaded;

    protected void Start() {
        DEBUGActivateMerchant = DebugManager.Instance.GetDebugMode_HUBMerchants();

        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;

        if (isHubMerchant) {
            // HUB behavior

            if(isFunctionalDemoHubMerchant || isDecorationalDemoHubMerchant) {
                // Demo merchant
                InitializeDemoHubMerchant();
            } else {
                InitializeHubMerchantInHub();
            }
        }

        if (isLevelNPC) {
            // Level behavior : idle, talk to player, disappear
            merchantHasTalkLinesToShow = true;
        }
    }

    protected void InitializeHubMerchantInHub() {

        merchantUnlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(hubMerchantType) || hubMerchantUnlockedAtStart || DEBUGActivateMerchant;

        if (!merchantUnlocked) {
            activeGameObject.SetActive(false);
            inactiveGameObject.SetActive(true);
            return;
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

        hubMerchantLoaded = true;
    }
    protected void InitializeDemoHubMerchant() {

        activeGameObject.SetActive(true);
        inactiveGameObject.SetActive(false);
        InitializeHubMerchantItems();

        if(hubMerchantType == HubMerchantType.GemMerchant) {
            merchantHasTalkLinesToShow = true;
            merchantUnlocked = true;
        } else {
            merchantHasTalkLinesToShow = false;
        }

        hubMerchantLoaded = true;
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
        if (isHubMerchant && !merchantUnlocked) return;
        if (merchantHasTalkLinesToShow) return;

        if (playerInteractingWithMerchant) {
            StartCoroutine(StopInteractingWithMerchant());
        }
    }

    private void TryStartInteractingWithMerchant() {
        if (!playerInTriggerArea) return;
        if (isHubMerchant && !merchantUnlocked) return;
        if (isDecorationalDemoHubMerchant) return;
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
            OnPlayerStartedTalkingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnPlayerOpenedHubMerchantShop?.Invoke(this, EventArgs.Empty);
            OnPlayerOpenedAnyHubMerchantShop?.Invoke(this, EventArgs.Empty);
        }

        playerInteractingWithMerchant = true;
    }

    private IEnumerator StopInteractingWithMerchant() {

        Player.Instance.StopInteractingWithMerchant();
        CameraManager.Instance.ResetCameraTargetToPlayer();
        PauseMenuUI.Instance.SetCanOpenPauseMenuAfterFrame(true);

        OnPlayerStoppedInteractingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        OnPlayerStoppedInteractingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

        if (merchantHasNewItems) {
            merchantHasNewItems = false;
        }

        if (merchantJustArrivedInHub) {
            merchantJustArrivedInHub = false;
        }

        yield return new WaitForEndOfFrame();
        playerInteractingWithMerchant = false;
    }

    public void StartTalkingWithMerchant() {
        Player.Instance.StartInteractingWithMerchant();

        merchantHasTalkLinesToShow = true;
        playerInteractingWithMerchant = true;

        OnPlayerStartedTalkingWithHubMerchant?.Invoke(this, EventArgs.Empty);
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (isHubMerchant && !merchantUnlocked) return;

        if(collision.GetComponent<Player>() != null) {
            playerInTriggerArea = true;
            Player.Instance.SetInMerchantTriggerArea(true);
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (isHubMerchant && !merchantUnlocked) return;

        if (collision.GetComponent<Player>() != null) {
            playerInTriggerArea = false;
            Player.Instance.SetInMerchantTriggerArea(false);
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        }
    }

    #region SET PARAMETERS

    
    public void SetPlayerFinishedTalkingWithMerchant(bool openShopPanel) {
        merchantHasTalkLinesToShow = false;

        if (openShopPanel && isHubMerchant) {
            StartInteractingWithMerchant();
        }
        else {
            StartCoroutine(StopInteractingWithMerchant());
        }
    }

    public void SetHasTalkLinesToShow(bool hasTalkLinesToShow, bool showExclamationMark = true) {
        merchantHasTalkLinesToShow = hasTalkLinesToShow;

        if (hasTalkLinesToShow && showExclamationMark) {
            OnMerchantHasNewTalkLines?.Invoke(this, EventArgs.Empty);
        } else {
            OnMerchantHideExclamationMark?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetDemoMerchantUnlocked() {
        merchantUnlocked = true;
    }

    public void SetDemoMerchantFunctional() {
        isFunctionalDemoHubMerchant = true;
        isDecorationalDemoHubMerchant = false;
    }

    #endregion

    #region GET PARAMETERS

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
    public bool GetMerchantHasNewTalkLinkes() {
        return merchantHasTalkLinesToShow;
    }

    public bool GetMerchantIsLevelNPC() {
        return isLevelNPC;
    }
    public bool GetMerchantUnlocked() {
        return merchantUnlocked;
    }

    public bool GetMerchantIsFunctionalDemoMerchant() {
        return isFunctionalDemoHubMerchant;
    }
    public bool GetMerchantIsDecorationalDemoMerchant() {
        return isDecorationalDemoHubMerchant;
    }
    #endregion

    public void SaveMerchant() {
        if (!hubMerchantLoaded) return;
        if (!merchantUnlocked) return;

        MetaProgressionManager.Instance.SetMerchantHasTalkLinesToShow(hubMerchantType, merchantHasTalkLinesToShow);
        MetaProgressionManager.Instance.SetMerchantJustArrivedInHub(hubMerchantType, merchantJustArrivedInHub);

        foreach(HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.SaveItemStatus();
        }
    }



}
