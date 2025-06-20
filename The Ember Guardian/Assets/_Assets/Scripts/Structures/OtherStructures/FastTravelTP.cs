using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastTravelTP : Structure
{
    [SerializeField] private Transform playerPositionOnTP;
    [SerializeField] private Collider2D floorCollider;
    [SerializeField] private bool isTentTP;

    public event EventHandler OnPlayerPositionedOnTP;
    public event EventHandler OnPlayerCanceledTP;
    public event EventHandler OnPlayerWarpStarted;
    public event EventHandler OnPlayerWarped;
    public event EventHandler OnPlayerWarpedOut;
    public event EventHandler OnPlayerWarpEnded;
    public event EventHandler OnTPSetAsReceiver;
    public event EventHandler OnReceiverFastTravelTPChanged;
    public event EventHandler OnOtherCharacterWarped;
    public static event EventHandler OnAnyDogWarped;
    public static event EventHandler OnAnyFastTravelTPBuilt;
    public static event EventHandler OnAnyPlayerWarped;
    public static event EventHandler OnAnyPlayerWarpedOut;
    public static event EventHandler OnAnyPlayerPositionedOnTP;
    public static event EventHandler OnAnyPlayerCanceledTP;

    private FastTravelTP receiverTP;
    private bool isSelectingReceiverTP;
    private bool teleportingPlayer;
    private bool teleportingOther;
    private bool isReceiverTP;
    private int currentTPIndex = 0;
    private int fastTravelTPIndexIdentifier = 0;
    private static int fastTravelTPIndex = 0;

    protected override void Awake() {
        base.Awake();
        floorCollider.enabled = false;

        OnAnyFastTravelTPBuilt?.Invoke(this, EventArgs.Empty);

        fastTravelTPIndexIdentifier = fastTravelTPIndex;
        fastTravelTPIndex++;
    }

    protected override void Start() {
        base.Start();
        GameInput.Instance.OnPlayerRightSwitchPerformed += GameInput_OnPlayerRightSwitchPerformed;
        GameInput.Instance.OnPlayerLeftSwitchPerformed += GameInput_OnPlayerLeftSwitchPerformed;
        GameInput.Instance.OnPlayerJumpPerformed += GameInput_OnPlayerJumpPerformed;
        CameraManager.Instance.OnCameraCenteredOnPlayer += CameraManager_OnCameraCenteredOnPlayer;
    }

    private void GameInput_OnPlayerJumpPerformed(object sender, EventArgs e) {
        isSelectingReceiverTP = false;
        floorCollider.enabled = false;
        OnPlayerCanceledTP?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerCanceledTP?.Invoke(this, EventArgs.Empty);
        Player.Instance.ReleasePlayerFromTeleporter();
    }

    private void GameInput_OnPlayerLeftSwitchPerformed(object sender, EventArgs e) {
        if (!isSelectingReceiverTP) return;
        SelectNextReceiverFastTravelTP(-1);
    }

    private void GameInput_OnPlayerRightSwitchPerformed(object sender, EventArgs e) {
        if (!isSelectingReceiverTP) return;
        SelectNextReceiverFastTravelTP(1);
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        base.GameInput_OnPlayerInteractCanceled(sender, e);

        bool playerWasHoldingInteract = GameInput.Instance.GetWasHoldingInteract();
        if (!playerWasHoldingInteract && playerInTriggerArea && !teleportingPlayer && !teleportingOther) {

            if(!isSelectingReceiverTP) {
                StartReceiverTPSelection();
            } else {
                StartCoroutine(TeleportPlayerFromThisTP(receiverTP));
            }

        }
    }

    private IEnumerator TeleportPlayerFromThisTP(FastTravelTP receiverTP) {
        teleportingPlayer = true;
        isSelectingReceiverTP = false;
        OnPlayerWarpStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        OnPlayerWarped?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerWarped?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        floorCollider.enabled = false;
        teleportingPlayer = false;
        OnPlayerWarpEnded?.Invoke(this, EventArgs.Empty);
        receiverTP.SetReceiverTP();
    }

    public void SetReceiverTP() {
        isReceiverTP = true;
        floorCollider.enabled = true;
        Player.Instance.SetPosition(playerPositionOnTP.position);
        CameraManager.Instance.SetCameraNotCenteredOnPlayer();
        OnTPSetAsReceiver?.Invoke(this, EventArgs.Empty);
    }

    private void CameraManager_OnCameraCenteredOnPlayer(object sender, EventArgs e) {
        if (isReceiverTP) {
            StartCoroutine(ReceiverTPCoroutine());
        }
    }

    private IEnumerator ReceiverTPCoroutine() {
        OnPlayerWarpedOut?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerWarpedOut?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(1f);

        Player.Instance.ReleasePlayerFromTeleporter();
        isReceiverTP = false;

        yield return new WaitForSeconds(.6f);

        if(Dog.Instance.GetIdleState() == DogAI.State.walkWithPlayer) {
            teleportingOther = true;

            Dog.Instance.MoveOnTeleporter(playerPositionOnTP);
            OnAnyDogWarped?.Invoke(this, EventArgs.Empty);
            OnOtherCharacterWarped?.Invoke(this, EventArgs.Empty);
            yield return new WaitForSeconds(.7f);
            Dog.Instance.MoveOutTeleporter();
        }

        List<Worker> workerFollowingPlayers = new List<Worker>();
        foreach(Worker worker in WorkerFollowPlayerHandler.Instance.GetFollowingWorkers()) {
            workerFollowingPlayers.Add(worker);
        }

        foreach(Worker worker in workerFollowingPlayers) {
            if (worker == null) continue;
            teleportingOther = true;
            worker.SetPosition(playerPositionOnTP.position);
            OnOtherCharacterWarped?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(.7f);
        }

        teleportingOther = false;
        floorCollider.enabled = false;
    }

    public FastTravelTP GetReceiverFastTravelTP() {
        return receiverTP;
    }

    private void StartReceiverTPSelection() {
        isSelectingReceiverTP = true;
        Player.Instance.MoveOnTeleporter(playerPositionOnTP);

        OnPlayerPositionedOnTP?.Invoke(this, EventArgs.Empty);
        OnAnyPlayerPositionedOnTP?.Invoke(this, EventArgs.Empty);
        floorCollider.enabled = true;

        List<FastTravelTP> allTPs = PlayerCamp.Instance.GetAllFastTravelTPsBuilt();
        allTPs.Remove(this);
        receiverTP = GetClosestTP(allTPs);
        OnReceiverFastTravelTPChanged?.Invoke(this, EventArgs.Empty);
    }

    public (bool canGoLeft, bool canGoRight) GetLeftRightAvailability() {
        List<FastTravelTP> allTPs = PlayerCamp.Instance.GetAllFastTravelTPsBuilt();
        allTPs.Remove(this);

        if (allTPs.Count == 0) return (false, false);

        allTPs.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        int index = allTPs.IndexOf(receiverTP);

        bool canGoLeft = index > 0;
        bool canGoRight = index < allTPs.Count - 1;

        return (canGoLeft, canGoRight);
    }

    private FastTravelTP GetClosestTP(List<FastTravelTP> tps) {
        FastTravelTP closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tp in tps) {
            if (tp == this) continue;
            float dist = Vector2.Distance(transform.position, tp.transform.position);
            if (dist < minDist) {
                minDist = dist;
                closest = tp;
            }
        }
        return closest;
    }

    private FastTravelTP SelectNextReceiverFastTravelTP(int direction) {
        if (!isSelectingReceiverTP) return null;

        List<FastTravelTP> allTPs = PlayerCamp.Instance.GetAllFastTravelTPsBuilt();
        allTPs.Remove(this);
        if (allTPs.Count == 0) return null;

        allTPs.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        currentTPIndex = allTPs.IndexOf(receiverTP);
        int newIndex = currentTPIndex + direction;

        if (newIndex < 0 || newIndex >= allTPs.Count) return null;

        currentTPIndex = newIndex;
        receiverTP = allTPs[currentTPIndex];

        OnReceiverFastTravelTPChanged?.Invoke(this, EventArgs.Empty);
        return receiverTP;
    }

    public int GetFastTravelTPIdentifier() {
        return fastTravelTPIndexIdentifier;
    }


    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);
        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

    }
}
