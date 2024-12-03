using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Collider2D floorCollider;
    [SerializeField] private bool isEndLevelTeleporter;
    [SerializeField] private bool isStartLevelTeleporter;
    [SerializeField] private bool isHUBTeleporter;
    [SerializeField] private bool portalUnlocked;
    [SerializeField] private bool DEBUGMODE;

    [SerializeField] private float delayToAppearTeleporterIn;
    [SerializeField] private float delayToActivateTeleporter;
    [SerializeField] private float delayToActivateTeleportAnimation;
    [SerializeField] private float delayToTeleportPlayerInAnimation;
    [SerializeField] private float delayToTeleportPlayerAnimation;
    [SerializeField] private float delayToReleasePlayerAnimation;
    [SerializeField] private float delayToStartCrossfade;

    private bool playerInTriggerArea;
    private bool playerIsSetOnTeleporter;

    public static event EventHandler OnAnyPortalSetToTeleportPlayer;
    public event EventHandler OnPortalSetToTeleportPlayer;
    public event EventHandler OnPortalAppeared;
    public static event EventHandler OnAnyPortalAppeared;
    public event EventHandler OnPortalDisappeared;
    public static event EventHandler OnAnyPortalDisappeared;
    public event EventHandler OnPlayerEnteredTriggerArea;
    public event EventHandler OnPlayerExitedTriggerArea;
    public event EventHandler OnPlayerMovedOnTeleporter;
    public static event EventHandler OnAnyPlayerMovedOnTeleporter;
    public event EventHandler OnTeleporterActivated;
    public static event EventHandler OnAnyTeleporterActivated;
    public event EventHandler OnTeleporterActivatedOut;
    public static event EventHandler OnAnyTeleporterActivatedOut;
    public static event EventHandler OnAnyTeleporterTeleportedPlayerOut;
    public static event EventHandler OnAnyPlayerTeleported;

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        floorCollider.enabled = false;

        if (isHUBTeleporter) {
            StartCoroutine(TeleportPlayerOutInHub());
        }

        if (DEBUGMODE) return;
        if (isEndLevelTeleporter) {
            MakePortalAppear();
        }

        if(isStartLevelTeleporter) {
            StartCoroutine(TeleportPlayerOutInLevel());
        }
    }

    private void GameInput_OnPlayerInteractStarted(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;

        if (isHUBTeleporter && !PlayerCurrencies.Instance.GetCarryingEmber() && !DEBUGMODE) {
            PlayerUI_World.Instance.GetTooltipLeft().ShowTooltip("I must carry an ember ...", 2f);
            return;
        }

        StartCoroutine(TeleportPlayerIn());
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!portalUnlocked) return;
        if (playerIsSetOnTeleporter) return;

        if(collision.gameObject.GetComponent<Player>() != null) {
            playerInTriggerArea = true;
            OnPlayerEnteredTriggerArea?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!gameObject.activeSelf) return;

        if(isStartLevelTeleporter) {
            StartCoroutine(RemoveTeleporter());

        } else {
            if (!portalUnlocked) return;

            if (collision.gameObject.GetComponent<Player>() != null) {
                playerInTriggerArea = false;
                OnPlayerExitedTriggerArea?.Invoke(this, EventArgs.Empty);
                floorCollider.enabled = false;
            }
        }

        playerIsSetOnTeleporter = false;

    }

    private IEnumerator RemoveTeleporter() {
        playerInTriggerArea = false;
        OnPlayerExitedTriggerArea?.Invoke(this, EventArgs.Empty);
        floorCollider.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(1f);
        OnPortalDisappeared?.Invoke(this, EventArgs.Empty);
        OnAnyPortalDisappeared?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private IEnumerator TeleportPlayerIn() {
        floorCollider.enabled = true;
        Player.Instance.MoveOnTeleporter(playerPosition);
        OnPlayerMovedOnTeleporter?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerMovedOnTeleporter?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToActivateTeleporter);
        OnTeleporterActivated?.Invoke(this, EventArgs.Empty);
        OnAnyTeleporterActivated?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToTeleportPlayerAnimation);
        OnAnyPlayerTeleported?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToStartCrossfade);

        if(isEndLevelTeleporter) {
            SceneLoader.Instance.LoadHub();
        } else {
            SceneLoader.Instance.LoadTestLevel();
        }
    }

    private IEnumerator TeleportPlayerOutInLevel() {
        playerIsSetOnTeleporter = true;
        Player.Instance.MoveOnTeleporter(playerPosition);
        floorCollider.enabled = true;
        OnAnyPortalSetToTeleportPlayer?.Invoke(this, EventArgs.Empty);
        OnPortalSetToTeleportPlayer?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToAppearTeleporterIn);

        MakePortalAppear();

        yield return new WaitForSeconds(delayToActivateTeleportAnimation);
        
        OnTeleporterActivatedOut?.Invoke(this, EventArgs.Empty);
        OnAnyTeleporterActivatedOut?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToTeleportPlayerInAnimation);

        OnAnyTeleporterTeleportedPlayerOut?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToReleasePlayerAnimation);

        Player.Instance.ReleasePlayerFromTeleporter();

    }

    private IEnumerator TeleportPlayerOutInHub() {
        playerIsSetOnTeleporter = true;
        Player.Instance.MoveOnTeleporter(playerPosition);
        floorCollider.enabled = true;
        OnAnyPortalSetToTeleportPlayer?.Invoke(this, EventArgs.Empty);
        OnPortalSetToTeleportPlayer?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToActivateTeleportAnimation);

        OnTeleporterActivatedOut?.Invoke(this, EventArgs.Empty);
        OnAnyTeleporterActivatedOut?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToTeleportPlayerInAnimation);

        OnAnyTeleporterTeleportedPlayerOut?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToReleasePlayerAnimation);

        Player.Instance.ReleasePlayerFromTeleporter();
    }

    public void MakePortalAppear() {
        OnPortalAppeared?.Invoke(this, EventArgs.Empty);
        OnAnyPortalAppeared?.Invoke(this, EventArgs.Empty);
    }

    public bool GetPortalUnlocked() {
        return portalUnlocked;
    }
    public bool GetIsEndLevelTeleporter() {
        return isEndLevelTeleporter;
    }
    public bool GetIsStartLevelTeleporter() {
        return isStartLevelTeleporter;
    }
    public bool GetIsHUBTeleporter() {
        return isHUBTeleporter;
    }
}
