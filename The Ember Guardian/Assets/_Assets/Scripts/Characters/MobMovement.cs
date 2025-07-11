using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovement : MonoBehaviour
{
    [SerializeField] protected float initialMobSpeed;

    [SerializeField] protected float acceleration;
    [SerializeField] protected float deceleration;
    [SerializeField] protected float velPower;

    [SerializeField] private bool hadSpeedVariations;
    [SerializeField] private float minSpeedMultiplier = 0.8f; // Multiplicateur minimal pour la variation de vitesse
    [SerializeField] private float maxSpeedMultiplier = 1.2f; // Multiplicateur maximal pour la variation de vitesse
    [SerializeField] private float speedLerpDuration = 2f; // Durée pour interpoler entre les vitesses
    [SerializeField] private float speedChangeInterval = 5f; // Intervalle entre les variations de vitesse


    protected Rigidbody2D rb;
    protected Vector3 targetDestination;
    protected Vector3 moveDirection;
    protected float moveDirFloat;
    protected float lastMoveDir;
    protected float moveSpeed;
    protected float moveSpeedBuff = 1f;
    protected float speedVariationMultiplier = 1f;
    protected float targetSpeed;
    protected float movementForce;

    protected bool destinationReached;
    protected bool moving;
    protected bool canMove = true;
    protected bool readyToMoveAnimator = true;
    public event EventHandler OnDestinationReached;
    public event EventHandler OnDestinationSet;
    public event EventHandler OnMovementStarted;
    public event EventHandler OnMovementStopped;

    protected bool showDestinationGyzmos;

    public event EventHandler<OnMoveSpeedBuffedEventArgs> OnMoveSpeedBuffChanged;

    public class OnMoveSpeedBuffedEventArgs : EventArgs {
        public float moveSpeedBuff;
        public bool changeAnimatorSpeed;
    }

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();

        targetDestination = transform.position;

        moveSpeed = initialMobSpeed;
    }

    protected virtual void Start() {
        // Démarrer les variations de vitesse
        if(hadSpeedVariations) {
            StartCoroutine(AdjustSpeedOverTime());
        }

        showDestinationGyzmos = DebugManager.Instance.GetShowMobDestinationGyzmos();
    }

    protected virtual void FixedUpdate() {

        if(moving && rb.velocity.magnitude < .2f) {
            moving = false;
            OnMovementStopped?.Invoke(this, EventArgs.Empty);
        }
        if (!moving && rb.velocity.magnitude > .2f) {
            moving = true;
            OnMovementStarted?.Invoke(this, EventArgs.Empty);
        }

        if (showDestinationGyzmos) {
            Vector3 debugLineOrigin = new Vector3(transform.position.x, transform.position.y + .5f, 0);
            Vector3 debugLineDestination = new Vector3(targetDestination.x, targetDestination.y + .5f, 0);
            Debug.DrawLine(debugLineOrigin, debugLineDestination, Color.yellow);
        }

        if (Mathf.Abs(targetDestination.x - transform.position.x) < .1f) {
            moveDirFloat = 0;

            if (!destinationReached) {
                OnDestinationReached?.Invoke(this, EventArgs.Empty);
                rb.velocity = Vector3.zero;
                destinationReached = true;
            }
        }
        else {
            if (!canMove) return;
            HandleMovementForces();
        }
    }

    protected virtual void HandleMovementForces() {
        if (!readyToMoveAnimator) return;

        moveDirection = targetDestination - transform.position;

        if(moveDirection.x <0) {
            moveDirFloat = -1;
            lastMoveDir = -1;
        } else {
            moveDirFloat = 1;
            lastMoveDir = 1;
        }

        targetSpeed = moveDirFloat * moveSpeed * speedVariationMultiplier;

        if (moveDirFloat * WindManager.Instance.GetWindDir() > 0) {
            // Mob is moving in the same dir as wind
            targetSpeed *= WindManager.Instance.GetWindStrengthImpactOnSpeed();
        }
        else {
            // Mob is moving in the opposite dir as wind
            targetSpeed /= WindManager.Instance.GetWindStrengthImpactOnSpeed();
        }


        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        movementForce = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movementForce * rb.mass * Vector2.right);
    }

    private IEnumerator AdjustSpeedOverTime() {
        while (true) {
            // Calculer une nouvelle vitesse cible
            float newSpeedMultiplier = UnityEngine.Random.Range(minSpeedMultiplier, maxSpeedMultiplier);

            float elapsedTime = UnityEngine.Random.Range(0f, speedLerpDuration);
            float startSpeedMultiplier = speedVariationMultiplier;

            // Interpolation vers la nouvelle vitesse
            while (elapsedTime < speedLerpDuration) {
                speedVariationMultiplier = Mathf.Lerp(startSpeedMultiplier, newSpeedMultiplier, elapsedTime / speedLerpDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Attendre avant de changer à nouveau la vitesse
            yield return new WaitForSeconds(speedChangeInterval);
        }
    }

    public virtual void SetMoveSpeed(float moveSpeed) {
        this.moveSpeed = moveSpeed;
    }

    public void BuffMoveSpeed(float moveSpeedBuff, bool changeAnimatorSpeed = true) {

        this.moveSpeedBuff *= moveSpeedBuff;
        moveSpeed = initialMobSpeed * this.moveSpeedBuff;

        OnMoveSpeedBuffChanged?.Invoke(this, new OnMoveSpeedBuffedEventArgs {
            moveSpeedBuff = this.moveSpeedBuff,
            changeAnimatorSpeed = changeAnimatorSpeed
        });
    }

    public void DebuffMoveSpeed(float moveSpeedBuff, bool changeAnimatorSpeed = true) {
        this.moveSpeedBuff /= moveSpeedBuff;
        moveSpeed = initialMobSpeed * this.moveSpeedBuff;

        OnMoveSpeedBuffChanged?.Invoke(this, new OnMoveSpeedBuffedEventArgs {
            moveSpeedBuff = this.moveSpeedBuff,
            changeAnimatorSpeed = changeAnimatorSpeed
        });
    }

    public void ResetTempMoveSpeedBuffs() {

        moveSpeedBuff = 1f;
        moveSpeed = initialMobSpeed;

        OnMoveSpeedBuffChanged?.Invoke(this, new OnMoveSpeedBuffedEventArgs {
            moveSpeedBuff = moveSpeedBuff,
            changeAnimatorSpeed = true
        });;
    }

    public virtual void SetMoveTarget(Vector3 moveTarget) {
        OnDestinationSet?.Invoke(this, EventArgs.Empty);
        this.targetDestination = moveTarget;
        destinationReached = false;
    }

    public void SetReadyToMoveAnimator(bool ready) {
        readyToMoveAnimator = ready;
    }

    public void SetUnReadyToMoveAnimatorAndStopMoving() {
        readyToMoveAnimator = false;
        rb.velocity = Vector2.zero;
    }

    public void SetCanMove(bool canMove) {
        if(!canMove) {
            rb.velocity = Vector2.zero;
        }
        this.canMove = canMove;
    }

    public void InvokeOnDestinationReached() {
        OnDestinationReached?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetTargetDestination() {
        return targetDestination;
    }

    public float GetMoveDirFloat() {
        return moveDirFloat;
    }
    public float GetLastMoveDirFloat() {
        return lastMoveDir;
    }

    protected void OnDisable() {
        rb.velocity = Vector2.zero;
    }

}
