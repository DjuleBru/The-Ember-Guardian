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
    [SerializeField] protected string hubMerchantName;
    [SerializeField] protected bool hubMerchantUnlockedAtStart;

    protected bool playerInTriggerArea;
    protected bool playerInteractingWithMerchant;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler OnPlayerInteractedWithHubMerchant;
    public event EventHandler OnPlayerStoppedInteractingWithHubMerchant;
    public static event EventHandler OnPlayerInteractedWithAnyHubMerchant;
    public static event EventHandler OnPlayerStoppedInteractingWithAnyHubMerchant;

    protected void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            // HUB behavior

            if(!MetaProgressionManager.Instance.GetMerchantUnlocked(gameObject.name) && !hubMerchantUnlockedAtStart) {
                gameObject.SetActive(false);
            };

        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            // Level behavior : idle, talk to player, disappear
        }
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        if (playerInteractingWithMerchant) {
            Player.Instance.StopInteractingWithMerchant();

            OnPlayerStoppedInteractingWithHubMerchant?.Invoke(this, EventArgs.Empty);
            OnPlayerStoppedInteractingWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

            playerInteractingWithMerchant = false;
        }
    }

    protected void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;

        if(!playerInteractingWithMerchant) {
            Player.Instance.StartInteractingWithMerchant();

            OnPlayerInteractedWithHubMerchant?.Invoke(this, EventArgs.Empty);
            OnPlayerInteractedWithAnyHubMerchant?.Invoke(this, EventArgs.Empty);

            playerInteractingWithMerchant = true;
        }
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

    public string GetHubMerchantName() {
        return hubMerchantName;
    }
}
