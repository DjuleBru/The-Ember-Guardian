using Sirenix.OdinInspector;
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
        Codex,
        ArchitectTable
    }

    [SerializeField] protected HubMerchantType hubMerchantType;
    [SerializeField] protected Transform hubMerchantCameraFocusPosition;
    [SerializeField] protected string hubMerchantNameLocalizationKey;
    [SerializeField] protected bool hubMerchantUnlockedAtStart;
    [SerializeField] protected bool unlockHubMerchantWhenPlayerStopsTalking;
    [SerializeField] protected MerchantTextLinesSO backToHubMerchantTextLines;

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
    public event EventHandler OnPlayerInterruptedInteractingWithHubMerchant;
    public event EventHandler OnPlayerStartedTalkingWithHubMerchant;
    public event EventHandler OnMerchantHasNewInteraction;
    public event EventHandler OnMerchantHideExclamationMark;
    public event EventHandler OnMerchantFadeOutStarted;
    public static event EventHandler OnPlayerStartedTalkingWithAnyHubMerchant;
    public static event EventHandler OnPlayerOpenedAnyHubMerchantShop;
    public static event EventHandler OnPlayerStoppedInteractingWithAnyHubMerchant;

    [SerializeField] protected bool isHubMerchant;
    [SerializeField] protected bool isLevelNPC;
    [SerializeField] protected bool isHordeModeMerchant;
    [SerializeField] protected bool fadeOutAfterTalk;
    [SerializeField] protected float delayToFadeOutAfterTalk;
    protected float fadeOutDuration = 1.5f;

    [SerializeField] protected bool isFunctionalDemoHubMerchant;
    [SerializeField] protected bool isDecorationalDemoHubMerchant;

    protected bool hubMerchantLoaded;
    protected bool playerCanInteractWithMerchant = true;

    protected int redGemCosts;
    protected int blueGemCosts;
    protected int greenGemCosts;
    protected int yellowGemCosts;
    protected int purpleGemCosts;
    protected int cyanGemCosts;

    protected void Awake() {
        if (isLevelNPC) {
            // Level behavior : idle, talk to player, disappear
            merchantHasTalkLinesToShow = true;
        }

        if(isHubMerchant || isHordeModeMerchant) {
            InitializeHubMerchantItems();
            InitializeItemButtonUIs();
        }
    }

    protected void Start() {
        DEBUGActivateMerchant = DebugManager.Instance.GetDebugMode_HUBMerchants();

        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;

        if (isHubMerchant) {
            // HUB behavior

            if (isFunctionalDemoHubMerchant || isDecorationalDemoHubMerchant) {
                // Demo merchant
                InitializeDemoHubMerchant();
            } else {
                InitializeHubMerchantInHub();
            }
        }

        if(isHordeModeMerchant) {
            InitializeHordeModeMerchant();
            
        }

        SetHubMerchantParentInItems();
    }

    private void Player_OnPlayerDamaged(object sender, Player.OnPlayerChangedHealthEventArgs e) {
        if(playerInteractingWithMerchant) {
            StartCoroutine(InterruptInteractingWithMerchant());
        }
    }

    protected void InitializeItemButtonUIs() {
        foreach (HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.GetComponent<ItemButtonUI>().InitializeItemButtonUI();
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
        }

        merchantJustArrivedInHub = MetaProgressionManager.Instance.GetMerchantJustArrivedInHub(hubMerchantType);
        if (merchantJustArrivedInHub && !DEBUGActivateMerchant) {
            merchantHasTalkLinesToShow = true;
            if(hubMerchantType == HubMerchantType.ArchitectTable) {
               StructureStats.Instance.UnlockArchitectTable();
            }
        }
        else {
            if(!DEBUGActivateMerchant && !DebugManager.Instance.GetDebugMode_Credits()) {
                merchantHasTalkLinesToShow = MetaProgressionManager.Instance.GetMerchantHasTalkLinesToShow(hubMerchantType);
            }
        }

        if(!merchantHasNewItems) {
            merchantHasNewItems = MetaProgressionManager.Instance.GetHubMerchantNewItemsToSale(hubMerchantType);
        }

        foreach (HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.LoadItemStatus_Batch();
        }

        hubMerchantLoaded = true;
    }

    protected void InitializeHordeModeMerchant() {
        merchantUnlocked = HordeModeProgressionManager.Instance.GetMerchantUnlocked(hubMerchantType);

        if (!merchantUnlocked) {
            activeGameObject.SetActive(false);
            inactiveGameObject.SetActive(true);
            return;
        }
        else {
            activeGameObject.SetActive(true);
            inactiveGameObject.SetActive(false);
        }

        if(SavingManager_Level.Instance.GetLoadingSavedLevel()) {
            foreach (HubMerchantItem merchantItem in hubMerchantItems) {
                merchantItem.LoadItemStatus_HodeMode();
            }
        } else {
            foreach (HubMerchantItem merchantItem in hubMerchantItems) {
                merchantItem.ResetHordeItemStatus_Batch();
                merchantItem.LoadItemStatus_HodeMode();
            }
        }


        hubMerchantLoaded = true;
    }

    protected void InitializeDemoHubMerchant() {

        activeGameObject.SetActive(true);
        inactiveGameObject.SetActive(false);

        if(hubMerchantType == HubMerchantType.GemMerchant) {
            merchantHasTalkLinesToShow = true;
            merchantUnlocked = true;
        } else {
            merchantHasTalkLinesToShow = false;
        }

        foreach (HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.LoadItemStatus_Batch();
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
            StartCoroutine(StopInteractingWithMerchant(false));
        }
    }

    private void TryStartInteractingWithMerchant() {
        if (!playerInTriggerArea) return;
        if (isHubMerchant && !merchantUnlocked) return;
        if (isDecorationalDemoHubMerchant) return;
        if (playerInteractingWithMerchant) return;
        if (!playerCanInteractWithMerchant) return;
        if (Player.Instance.GetCameraHasOtherTarget()) return;
        if (!Player.Instance.GetAllMenusClosed()) return;

        if (isHubMerchant || isHordeModeMerchant) {
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

            OnPlayerStartedTalkingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

            if (HUBManager.Instance != null && hubMerchantType == HubMerchantType.GemMerchant) {
                if(HUBManager.Instance.GetIsEndGameSequence()) {
                    playerInteractingWithMerchant = true;
                    return;
                }
            }

            OnPlayerStartedTalkingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnPlayerOpenedHubMerchantShop?.Invoke(this, EventArgs.Empty);
            OnPlayerOpenedAnyHubMerchantShop?.Invoke(this, EventArgs.Empty);
        }

        playerInteractingWithMerchant = true;
    }

    private IEnumerator InterruptInteractingWithMerchant() {
        Player.Instance.StopInteractingWithMerchant();
        CameraManager.Instance.ResetCameraTargetToPlayer();
        PauseMenuUI.Instance.SetCanOpenPauseMenuAfterFrame(true);

        OnPlayerStoppedInteractingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);
        OnPlayerInterruptedInteractingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        yield return new WaitForEndOfFrame();
        playerInteractingWithMerchant = false;

    }

    private IEnumerator StopInteractingWithMerchant(bool stopFromTalking) {

        Player.Instance.StopInteractingWithMerchant();
        CameraManager.Instance.ResetCameraTargetToPlayer();
        PauseMenuUI.Instance.SetCanOpenPauseMenuAfterFrame(true);

        OnPlayerStoppedInteractingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        OnPlayerStoppedInteractingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

        if(!stopFromTalking) {
            if (merchantHasNewItems) {
                MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(hubMerchantType, false);
                merchantHasNewItems = false;
            }
        }

        if (merchantJustArrivedInHub) {
            merchantJustArrivedInHub = false;
        }

        if(isLevelNPC) {
            if (unlockHubMerchantWhenPlayerStopsTalking) {
                MetaProgressionManager.Instance.SetMerchantUnlocked(hubMerchantType);
                MetaProgressionManager.Instance.SetNextMerchantTalkLines(hubMerchantType, backToHubMerchantTextLines);
            }
        }

        yield return new WaitForEndOfFrame();
        playerInteractingWithMerchant = false;

        if(fadeOutAfterTalk) {
            StartCoroutine(FadeOutCoroutine());
        }
    }

    private IEnumerator FadeOutCoroutine() {
        yield return new WaitForSeconds(delayToFadeOutAfterTalk);
        OnMerchantFadeOutStarted?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(fadeOutDuration);

        gameObject.SetActive(false);
    }

    public void StartTalkingWithMerchant() {
        Player.Instance.StartInteractingWithMerchant();

        merchantHasTalkLinesToShow = true;
        playerInteractingWithMerchant = true;

        OnPlayerStartedTalkingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        OnPlayerStartedTalkingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);
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
        Debug.Log(this + " SetPlayerFinishedTalkingWithMerchant ");
        merchantHasTalkLinesToShow = false;

        if (openShopPanel && isHubMerchant) {
            StartInteractingWithMerchant();
        }
        else {
            StartCoroutine(StopInteractingWithMerchant(true));
        }
    }

    public void SetHasTalkLinesToShow(bool hasTalkLinesToShow, bool showExclamationMark = true, bool loadingFromLevelSave = false) {
        Debug.Log(this + " SetHasTalkLinesToShow " + hasTalkLinesToShow);
        merchantHasTalkLinesToShow = hasTalkLinesToShow;

        if (hasTalkLinesToShow && showExclamationMark) {
            OnMerchantHasNewInteraction?.Invoke(this, EventArgs.Empty);
        } else {
            OnMerchantHideExclamationMark?.Invoke(this, EventArgs.Empty);

            if(loadingFromLevelSave && fadeOutAfterTalk) {
                gameObject.SetActive(false);
            }
        }
    }

    public void SetHasTalkLinesToShowAfterDelay(bool hasTalkLinesToShow, bool showExclamationMark = true, float delay = 0f) {
        Debug.Log(this + " SetHasTalkLinesToShowAfterDelay " + hasTalkLinesToShow);
        StartCoroutine(SetHasTalkLinesToShowAfterDelayCoroutine(hasTalkLinesToShow, showExclamationMark, delay));   
        
    }

    private IEnumerator SetHasTalkLinesToShowAfterDelayCoroutine(bool hasTalkLinesToShow, bool showExclamationMark = true, float delay = 0f) {
        yield return new WaitForSeconds(delay);

        merchantHasTalkLinesToShow = hasTalkLinesToShow;

        if (hasTalkLinesToShow && showExclamationMark) {
            OnMerchantHasNewInteraction?.Invoke(this, EventArgs.Empty);
        }
        else {
            OnMerchantHideExclamationMark?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetMerchantUnlocked() {
        merchantUnlocked = true;
    }

    public void SetDemoMerchantFunctional() {
        isFunctionalDemoHubMerchant = true;
        isDecorationalDemoHubMerchant = false;
    }

    public void SetDemoMerchantDecorational() {
        isDecorationalDemoHubMerchant = true;
        isFunctionalDemoHubMerchant = false;
    }

    public void SetPlayerCanInteractWithMerchant(bool canInteract) {
        playerCanInteractWithMerchant = canInteract;
    }


    #endregion

    #region GET PARAMETERS

    public bool GetPlayerCanInteractWithMerchant() {
        if (isHubMerchant && !merchantUnlocked) return false;
        if (isDecorationalDemoHubMerchant) return false;
        if (playerInteractingWithMerchant) return false;
        if (!playerCanInteractWithMerchant) return false;
        return true;
    }

    public HubMerchantType GetHubMerchantType() {
        return hubMerchantType;
    }

    public Transform GetCameraFocusTransform() {
        return hubMerchantCameraFocusPosition;
    }

    public string GetHubMerchantName() {
        return hubMerchantNameLocalizationKey;
    }

    public bool GetMerchantHasNewItems() {
        return merchantHasNewItems;
    }

    public void SetMerchantHasNewItems() {
        merchantHasNewItems = true;
        OnMerchantHasNewInteraction?.Invoke(this, EventArgs.Empty);
        //Debug.Log(this + " SetMerchantHasNewItems " + merchantHasNewItems);
    }

    public bool GetMerchantJustArrivedInHub() {
        return merchantJustArrivedInHub;
    }
    public bool GetMerchantHasNewTalkLinkes() {
        return merchantHasTalkLinesToShow;
    }

    public bool GetPlayerInteractingWithMerchant() {
        return playerInteractingWithMerchant;
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

    public bool GetIsHordeModeNPC() {
        return isHordeModeMerchant;
    }

    #endregion

    public void SaveMerchant() {
        if (!hubMerchantLoaded) return;
        if (!merchantUnlocked) return;

        MetaProgressionManager.Instance.SetMerchantHasTalkLinesToShow(hubMerchantType, merchantHasTalkLinesToShow);
        MetaProgressionManager.Instance.SetMerchantJustArrivedInHub(hubMerchantType, merchantJustArrivedInHub);

        foreach(HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.SaveItemStatus_Batch();
        }
    }

    public void SaveHordeMerchant() {
        foreach (HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.SaveItemStatus_Batch();
        }
    }


    [Button]
    public int CountAllGemCosts(PlayerCurrencies.CurrencyType gemType) {
        int gemCosts = 0;
        foreach (HubMerchantItem merchantItem in hubMerchantItems) {
            gemCosts += merchantItem.GetTotalGemCosts(gemType);
        }
        return gemCosts;
    }
    public void SetHubMerchantParentInItems() {
        foreach (HubMerchantItem merchantItem in hubMerchantItems) {
            merchantItem.GetComponent<ItemButtonUI>().SetParentHubMerchant(this);
        }
    }

}
