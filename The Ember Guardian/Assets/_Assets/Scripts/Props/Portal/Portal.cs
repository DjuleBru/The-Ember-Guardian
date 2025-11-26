using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool isHubDemoPortal;
    [SerializeField] private List<LevelSO> linkedLevelSOList;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform levelSelectionCameraTarget;
    [SerializeField] private Transform dogPosition;
    [SerializeField] private Collider2D floorCollider;
    [SerializeField] private bool isEndLevelTeleporter;
    [SerializeField] private bool isStartLevelTeleporter;
    [SerializeField] private bool isHUBTeleporter;
    [SerializeField] private bool isUnlockedAtStart;
    [SerializeField] private int portalNumber;
    private bool DEBUGMODE;

    [SerializeField] private float delayToAppearTeleporterIn;
    [SerializeField] private float delayToActivateTeleporter;
    [SerializeField] private float delayToActivateTeleportAnimation;
    [SerializeField] private float delayToTeleportPlayerInAnimation;
    [SerializeField] private float delayToTeleportPlayerAnimation;
    [SerializeField] private float delayToReleasePlayerAnimation;
    [SerializeField] private float delayToStartCrossfade;

    private LevelSO linkedLevelSO;
    private bool playerInTriggerArea;
    private bool playerOpenedPortalUI;
    private bool playerIsSetOnTeleporter;
    private bool portalUnlocked;

    public  event EventHandler OnPortalUnlocked;
    public  event EventHandler OnPortalActivated;

    public static event EventHandler OnAnyPortalSetToTeleportPlayer;
    public event EventHandler OnPlayerInteractedWithPortalFromHub;
    public event EventHandler OnPlayerStartedTeleportingFromHub;
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


    public event EventHandler OnLinkedLevelSOSet;

    private void Awake() {
        if (isEndLevelTeleporter) {
            portalUnlocked = true;
        }

        if (isStartLevelTeleporter) {
            portalUnlocked = true;
        }
    }

    private void Start() {
        DEBUGMODE = DebugManager.Instance.GetDebugMode_Portals();

        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractStarted;
        MetaProgressionManager.Instance.OnLevelSOUnlocked += MetaProgressionManager_OnLevelSOUnlocked;
        floorCollider.enabled = false;

        if (isHUBTeleporter) {
            portalUnlocked = MetaProgressionManager.Instance.GetPortalUnlocked(gameObject.name);
            if(linkedLevelSOList.Count != 0 && !isHubDemoPortal) {
                SetLinkedLevelSO(linkedLevelSOList[MetaProgressionManager.Instance.GetPortalLinkedLevelSOIndex(portalNumber)]);
            }

            // DEEEEEEEEEEEEEEEEEEEBUG
            if (DEBUGMODE) {
                portalUnlocked = true;
                return;
            }


            if (isHubDemoPortal || isUnlockedAtStart) {
                portalUnlocked = true;
            }


            if (!portalUnlocked && !isHubDemoPortal) {
                gameObject.SetActive(false);
                return;
            }


            if (MetaProgressionManager.Instance.GetNextHubArrivalThroughPortal() && MetaProgressionManager.Instance.lastHUBPortalUsedByPlayer == portalNumber) {
                StartCoroutine(TeleportPlayerOutInHub());
            }
        }

        if (isEndLevelTeleporter) {
            MakePortalAppear();
        }

        if(isStartLevelTeleporter) {
            if (DEBUGMODE) return;
            if (SavingManager_Level.Instance.GetLoadingSavedLevel()) {
                gameObject.SetActive(false);
                return;
            } 

            if(LevelManager.Instance.IsHordeMode()) {
                if(!HordeModeEnvironmentManager.Instance.GetIsEnvironmentPortal(this)) {
                    gameObject.SetActive(false);
                    return;
                }
            }
            StartCoroutine(TeleportPlayerOutInLevel());
        }
    }

    private void MetaProgressionManager_OnLevelSOUnlocked(object sender, MetaProgressionManager.OnLevelSOUnlockedEventArgs e) {
        if(linkedLevelSOList.Contains(e.levelSOUnlocked)) {
            SetLinkedLevelSO(e.levelSOUnlocked);
        }
    }

    private void GameInput_OnPlayerInteractStarted(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        if (playerIsSetOnTeleporter) return;
        if (playerOpenedPortalUI) return;

        if (isHUBTeleporter && !PlayerCurrencies.Instance.GetCarryingEmber() && !DEBUGMODE) {
            PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_carryEmber"), 2f);
            return;
        }

        if(isHubDemoPortal || isEndLevelTeleporter) {
            StartCoroutine(TeleportPlayerIn());
            return;
        }

        if(isHUBTeleporter && (MetaProgressionManager.Instance.GetLevelUnlocked(linkedLevelSOList[0]) || DEBUGMODE)) {
            OnPlayerInteractedWithPortalFromHub?.Invoke(this, EventArgs.Empty);
            return;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!portalUnlocked) return;
        if (playerIsSetOnTeleporter) return;
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB && CreditsManager.Instance != null && CreditsManager.Instance.GetCreditsPlaying()) return;

        if (collision.gameObject.GetComponent<Player>() != null) {
            playerInTriggerArea = true;

            if (isHUBTeleporter && !GetPortalHasUnlockedUnfinishedLevels()) return;

            Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
            Player.Instance.SetInPortalTriggerArea(true);
            OnPlayerEnteredTriggerArea?.Invoke(this, EventArgs.Empty);
        }

    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;

        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        Player.Instance.SetInPortalTriggerArea(false);
        playerInTriggerArea = false;

        if (playerIsSetOnTeleporter) {
            OnPlayerExitedTriggerArea?.Invoke(this, EventArgs.Empty);
            floorCollider.enabled = false;
            playerIsSetOnTeleporter = false;
        }

        if (!gameObject.activeSelf) return;

        if (isStartLevelTeleporter) {
            StartCoroutine(RemoveTeleporter());

        } else {

            if (!portalUnlocked) return; 

            if (isHUBTeleporter && !GetPortalHasUnlockedUnfinishedLevels()) return;

            OnPlayerExitedTriggerArea?.Invoke(this, EventArgs.Empty);
        }

        floorCollider.enabled = false;

    }

    private IEnumerator RemoveTeleporter() {
        playerInTriggerArea = false;
        floorCollider.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(1f);
        OnPortalDisappeared?.Invoke(this, EventArgs.Empty);
        OnAnyPortalDisappeared?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    public void TeleportPlayerFromHub() {
        StartCoroutine(TeleportPlayerIn());
    }

    private IEnumerator TeleportPlayerIn() {
        playerIsSetOnTeleporter = true;

        floorCollider.enabled = true;

        Player.Instance.MoveOnTeleporter(playerPosition);
        Dog.Instance.MoveOnTeleporter(dogPosition);
        OnPlayerStartedTeleportingFromHub?.Invoke(this, EventArgs.Empty);

        if (isHUBTeleporter) {
            HUBManager.Instance.SaveHub();
            MetaProgressionManager.Instance.DeleteLevelSaveFile();
        }

        yield return new WaitForEndOfFrame();

        OnPlayerMovedOnTeleporter?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerMovedOnTeleporter?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToActivateTeleporter);
        OnTeleporterActivated?.Invoke(this, EventArgs.Empty);
        OnAnyTeleporterActivated?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToTeleportPlayerAnimation);
        OnAnyPlayerTeleported?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToStartCrossfade);

        if(isEndLevelTeleporter) {
            LevelManager.Instance.SaveLevelCompletedProgression();
            SceneLoader.Instance.LoadHub(2f);
            MetaProgressionManager.Instance.SetNextHubArrivalThroughPortal(true);

        } else {
            
            MetaProgressionManager.Instance.SetAsLastPortalUsedByPlayer(portalNumber);
            SceneLoader.Instance.LoadLevel(linkedLevelSO, 2f);
        }
    }

    private IEnumerator TeleportPlayerOutInLevel() {
        playerIsSetOnTeleporter = true;
        Player.Instance.MoveOnTeleporter(playerPosition);
        Dog.Instance.MoveOnTeleporter(dogPosition);

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

        if(LevelManager.Instance.GetLevelSO().showDirectionAtStart) {
            DirectionIndicator.Instance.ShowDirection(LevelManager.Instance.GetLevelSO().directionToShow);
        }

    }

    public void TeleportPlayerOutInHubManually() {
        StartCoroutine(TeleportPlayerOutInHub());
    }

    private IEnumerator TeleportPlayerOutInHub() {
        playerIsSetOnTeleporter = true;
        Player.Instance.MoveOnTeleporter(playerPosition);
        Dog.Instance.MoveOnTeleporter(dogPosition);

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

    public void SetPlayerOpenedPortalUI(bool isOpen) {
        playerOpenedPortalUI = isOpen;
    }

    public void SetPortalUnlockedInSave(string savePath = "SaveFile.es3") {
        if (portalUnlocked) {
            MetaProgressionManager.Instance.SetPortalUnlocked(gameObject.name, savePath);
        }
    }

    public void UnlockOrActivatePortal() {
        if (!portalUnlocked) {
            // UnlockPortal
            gameObject.SetActive(true);
            MakePortalAppear();
            OnPortalUnlocked?.Invoke(this, EventArgs.Empty);
            portalUnlocked = true;

        } else {
            // Activate portal
            OnPortalActivated?.Invoke(this, EventArgs.Empty);
        };

    }

    public bool GetLevelSOIsInPortal(LevelSO levelSO) {
        return linkedLevelSOList.Contains(levelSO);
    }

    public void SetLinkedLevelSO(LevelSO levelSO) {
        linkedLevelSO = levelSO;
        OnLinkedLevelSOSet?.Invoke(this, EventArgs.Empty);
    }

    public int GetLinkedLevelSOIndex() {
        if (linkedLevelSO == null) return 0;
        return linkedLevelSOList.IndexOf(linkedLevelSO);
    }
    public LevelSO GetLinkedLevelSO() {
        return linkedLevelSO;
    }
    public List<LevelSO> GetLinkedLevelSOList() {
        return linkedLevelSOList;
    }

    public Transform GetLevelSelectionCameraTarget() {
        return levelSelectionCameraTarget;
    }

    public bool GetPortalUnlocked() {
        return portalUnlocked;
    }

    public bool GetPortalHasUnlockedUnfinishedLevels() {
        bool hasUnlockedAndUnfinishedLevels = false;

        if(DEBUGMODE) {
            return true;
        }

        foreach(LevelSO levelSO in linkedLevelSOList) {
            if((!MetaProgressionManager.Instance.GetLevelCompleted(levelSO) || levelSO.isReplayableLevel )) {
                // Level is not completed OR level is replayable 

                if(MetaProgressionManager.Instance.GetLevelUnlocked(levelSO)) {
                    // Level is Unlocked

                    hasUnlockedAndUnfinishedLevels = true;
                }
            }
        }

        return hasUnlockedAndUnfinishedLevels;
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
    public int GetPortalNumber() {
        return portalNumber;
    }
    private void OnDisable() {
        StopAllCoroutines();
    }
}
