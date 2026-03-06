using System;
using UnityEngine;

public class HuntingFlag_PlayerDefined : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer pickUpSpriteRenderer;
    [SerializeField] private GameObject exclamationMark;

    private HuntingFlag huntingFlag;
    private bool playerInTriggerArea;
    private bool playerCarryingFlag;
    private bool tooFarForHunters;
    private float maxSecureDistance;

    public static event EventHandler OnAnyHuntingFlagPickedUp;
    public static event EventHandler OnAnyHuntingFlagNewPositionSet;
    public static event EventHandler OnAnyPlayerTriggeredIn;
    public static event EventHandler OnHuntingFlagTooFarCarriedByPlayer;
    public static event EventHandler OnHuntingFlagBackToSafetyCarriedByPlayer;

    private void Awake() {
        huntingFlag = GetComponentInParent<HuntingFlag>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickUpSpriteRenderer.enabled = false;
        exclamationMark.SetActive(false);

        FastTravelTP.OnAnyFastTravelTPBuilt += FastTravelTP_OnAnyFastTravelTPBuilt;
    }


    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        CampZoneManager.Instance.OnCampZoneLimitsChanged += CampZoneManager_OnCampZoneLimitsChanged;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        FastTravelTP.OnAnyPlayerPositionedOnTP += FastTravelTP_OnAnyPlayerPositionedOnTP;
        FastTravelTP.OnAnyPlayerWarped += FastTravelTP_OnAnyPlayerWarped;

        RefreshMaxSecureDistance();
        huntingFlag.OnPlayerResetManualHuntingLimit += HuntingFlag_OnPlayerResetManualHuntingLimit;
    }

    private void FastTravelTP_OnAnyPlayerWarped(object sender, EventArgs e) {
        FastTravelTP fastTravelTP = (FastTravelTP)sender;

        if(fastTravelTP.GetReceiverFastTravelTP().GetIsTentTP() || CampZoneManager.Instance.IsWithinCampZoneLimits(fastTravelTP.GetReceiverFastTravelTP().transform.position)) {

            if (huntingFlag.GetPlayerCarryingFlag()) {
                Vector3 currentPosition = new Vector3(Player.Instance.transform.position.x - 2f, 0f, 0f);
                SetNewFlagPosition(currentPosition);
            }
        }
    }

    private void FastTravelTP_OnAnyPlayerPositionedOnTP(object sender, EventArgs e) {


    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if(huntingFlag.GetPlayerCarryingFlag()) {
            Vector3 currentPosition = new Vector3(Player.Instance.transform.position.x, 0f, 0f);
            SetNewFlagPosition(currentPosition);
        }
    }

    private void Update() {
        if (!huntingFlag.GetPlayerCarryingFlag()) return;
        if(CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) {
            Vector3 currentPosition = new Vector3(Player.Instance.transform.position.x, 0f, 0f);

            if(currentPosition.x > 0) {
                currentPosition.x += 3f;
            } else {
                currentPosition.x -= 3f;
            }

            SetNewFlagPosition(currentPosition);
        }

        CheckSecureDistance();

    }

    private void FastTravelTP_OnAnyFastTravelTPBuilt(object sender, EventArgs e) {
        Invoke("CheckSecureDistance", .1f);
    }

    private void CampZoneManager_OnCampZoneLimitsChanged(object sender, EventArgs e) {
        RefreshMaxSecureDistance();
    }

    private void RefreshMaxSecureDistance() {
        float hunterRunBackToCampSpeed = (1 + WorkerStats.Instance.GetHunterMoveSpeedBuff()) * WorkerStats.Instance.GetHeadToCampMoveSpeed();
        maxSecureDistance = hunterRunBackToCampSpeed * DayNightManager.Instance.GetDuskDuration() + CampZoneManager.Instance.GetMaxZoneLimit();
        CheckSecureDistance();
    }
    private void CheckSecureDistance() {
        if (!tooFarForHunters) {
            if(EvaluateOptimizedPath() > maxSecureDistance) {
                tooFarForHunters = true;
                exclamationMark.SetActive(true);

                if(playerCarryingFlag) {
                    OnHuntingFlagTooFarCarriedByPlayer?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        if (tooFarForHunters) {
            if (EvaluateOptimizedPath() < maxSecureDistance) {
                tooFarForHunters = false;
                exclamationMark.SetActive(false);
                OnHuntingFlagBackToSafetyCarriedByPlayer?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    private float EvaluateOptimizedPath() {
        Vector2 targetPos = Fire.Instance.transform.position;
        Vector3 currentPos = transform.position;
        float directDist = Vector3.Distance(currentPos, targetPos);
        float bestTotalDist = directDist;

        foreach (var tpFrom in PlayerCamp.Instance.GetAllFastTravelTPsBuilt()) {
            float distToTP = Vector3.Distance(currentPos, tpFrom.transform.position);

            foreach (var tpTo in PlayerCamp.Instance.GetAllFastTravelTPsBuilt()) {
                if (tpFrom == tpTo) continue;

                float distFromTP = Vector3.Distance(tpTo.transform.position, targetPos);
                float totalDistance = distToTP + distFromTP;

                if (totalDistance < bestTotalDist) {
                    bestTotalDist = totalDistance;
                }
            }
        }

        return bestTotalDist;
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {

        if (playerCarryingFlag) {
            if (!Player.Instance.GetInNoOtherObjectTriggerArea()) return;
            Vector3 currentPosition = new Vector3(Player.Instance.transform.position.x, 0f, 0f);
            SetNewFlagPosition(currentPosition);

        } else {

            if (!playerInTriggerArea) return;
            StartCarryingFlag();

        }
    }

    private void StartCarryingFlag() {
        playerCarryingFlag = true;

        Vector3 newScale = Vector3.one;
        newScale.x = -1f;

        transform.position = Player.Instance.GetCarryingFlagPosition().position;
        transform.SetParent(Player.Instance.GetCarryingFlagPosition());

        huntingFlag.SetPlayerCarryingFlag(true);
        OnAnyHuntingFlagPickedUp?.Invoke(this, EventArgs.Empty);

        spriteRenderer.transform.localScale = newScale;

        pickUpSpriteRenderer.enabled = false;
    }

    private void HuntingFlag_OnPlayerResetManualHuntingLimit(object sender, System.EventArgs e) {
        ResetFlagPosition();
    }

    private void SetNewFlagPosition(Vector3 position) {
        playerCarryingFlag = false;
        huntingFlag.SetPlayerDefinedHuntingLimit(true);
        huntingFlag.SetPlayerCarryingFlag(false);
        transform.SetParent(huntingFlag.transform);
        transform.position = position;

        OnAnyHuntingFlagNewPositionSet?.Invoke(this, EventArgs.Empty);
    }

    private void ResetFlagPosition() {
        transform.localScale = new Vector3(-1, 1, 1);
        huntingFlag.SetPlayerDefinedHuntingLimit(false);
        playerCarryingFlag = false;
        Player.Instance.SetCarryinhOtherObject(false);
        transform.SetParent(huntingFlag.transform);
        transform.position = huntingFlag.GetCampDefinedHuntingFlagPosition();
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if(collision.gameObject.GetComponent<Player>() != null) {
            OnAnyPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);

            if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

            playerInTriggerArea = true;
            Player.Instance.SetCarryinhOtherObject(true);
            spriteRenderer.material.SetFloat("_Glow", .2f);
            pickUpSpriteRenderer.enabled = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!PlayerSave.Instance.GetPlayerUnlockedFlagCarry()) return;

        if (collision.gameObject.GetComponent<Player>() != null) {

            playerInTriggerArea = false;
            spriteRenderer.material.SetFloat("_Glow", 0f);

            if (huntingFlag.GetPlayerCarryingFlag()) return;
            Player.Instance.SetCarryinhOtherObject(false);
            pickUpSpriteRenderer.enabled = false;
        }
    }

    public bool GetPlayerIsCarryingFlag() {
        return playerCarryingFlag;
    }

    private void OnDestroy() {
        FastTravelTP.OnAnyFastTravelTPBuilt -= FastTravelTP_OnAnyFastTravelTPBuilt;
        FastTravelTP.OnAnyPlayerPositionedOnTP -= FastTravelTP_OnAnyPlayerPositionedOnTP;
        FastTravelTP.OnAnyPlayerWarped -= FastTravelTP_OnAnyPlayerWarped;
    }
}
