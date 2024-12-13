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

    [SerializeField] protected bool DEBUGMODE;

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

    protected void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            // HUB behavior

            if(!MetaProgressionManager.Instance.GetMerchantUnlocked(hubMerchantType) && !hubMerchantUnlockedAtStart) {
                gameObject.SetActive(false);
            };

            merchantJustArrivedInHub = MetaProgressionManager.Instance.GetMerchantJustArrivedInHub(hubMerchantType);
            merchantHasTalkLinesToShow = MetaProgressionManager.Instance.GetMerchantHasTalkLinesToShow(hubMerchantType);
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            // Level behavior : idle, talk to player, disappear
        }
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        if (playerInteractingWithMerchant) {
            StopInteractingWithMerchant();
        }
    }

    protected void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;

        if(!playerInteractingWithMerchant) {
            StartInteractingWithMerchant();
        }
    }

    private void StartInteractingWithMerchant() {
        Player.Instance.StartInteractingWithMerchant();

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

        OnPlayerStoppedInteractingWithHubMerchant?.Invoke(this, EventArgs.Empty);
        OnPlayerStoppedInteractingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

        if (merchantHasNewItems) {
            merchantHasNewItems = false;
        }

        if (merchantJustArrivedInHub) {
            merchantJustArrivedInHub = false;
            MetaProgressionManager.Instance.SetMerchantJustArrivedInHub(hubMerchantType, false);
        }

        playerInteractingWithMerchant = false;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponent<Player>() != null) {
            playerInTriggerArea = true;
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() != null) {
            playerInTriggerArea = false;
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetPlayerFinishedTalkingWithMerchant(bool openShopPanel) {
        merchantHasTalkLinesToShow = false;
        MetaProgressionManager.Instance.SetMerchantHasTalkLinesToShow(hubMerchantType, false);

        if(openShopPanel) {
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
}
