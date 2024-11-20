using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Structure, IDamageable {

    public static Fire Instance;

    [SerializeField] private CircleCollider2D fireRadiusCollider;

    [SerializeField] private float calmFireRadius;
    [SerializeField] private float mildFireRadius;
    [SerializeField] private float wildFireRadius;
    [SerializeField] private float insaneFireRadius;

    [SerializeField] private float orbFuelValue;
    [SerializeField] private float fuelDepletionRate;

    [SerializeField] private int calmFuelTreshold;
    [SerializeField] private int mildFuelTreshold;
    [SerializeField] private int wildFuelTreshold;
    [SerializeField] private int insaneFuelTreshold;
    [SerializeField] private int maxFuelTreshold;

    [SerializeField] private float debugFuelLevel;

    private FireOrbCollider fireOrbCollider;
    private float fuelLevel;
    private float damageToFuelConversionRate = 5f;

    public enum State {
        extinguished,
        calm,
        mild,
        wild,
        insane,
    }
    private State state;

    public event EventHandler<OnFireChangedStateEventArgs> OnFireChangedState;
    public class OnFireChangedStateEventArgs {
        public State previousState;
        public State newState;
    }

    public event EventHandler OnFireFuelled;
    public event EventHandler OnFireDamageTaken;

    private bool lerping;
    private float lerpTimer;
    private float lerpDuration = 1f;
    private float initialFireAOEValue;
    private float finalFireAOEValue;

    protected override void Awake() {
        Instance = this;

        base.Awake();

        fireOrbCollider = GetComponentInChildren<FireOrbCollider>();
    }

    protected override void Start() {
        base.Start();

        fireOrbCollider.OnOrbFellInFire += FireOrbCollider_OnOrbFellInFire;

        fuelLevel = mildFuelTreshold - 1;
        ChangeState(State.calm);
        SetFireCurrentMaxFuelTreshold();
    }

    private void Update() {

        if(fuelLevel > 0) {
            fuelLevel -= Time.deltaTime * fuelDepletionRate;
            debugFuelLevel = fuelLevel;
        }

        if (lerping) {

            lerpTimer += Time.deltaTime;
            float normalizedTime = lerpTimer / lerpDuration;

            if (normalizedTime >= 1) {
                normalizedTime = 1;
                lerpTimer = 0;
                lerping = false;
            }

            float currentFireAOEValue = Mathf.Lerp(initialFireAOEValue, finalFireAOEValue, normalizedTime);

            ChangeFireRadius(currentFireAOEValue);
        }

        CheckFireStateDowngrade();
    }
    
    private void ChangeFireRadius(float fireRadius) {
        fireRadiusCollider.radius = fireRadius;
    }

    private void FireOrbCollider_OnOrbFellInFire(object sender, EventArgs e) {
        fuelLevel += orbFuelValue;

        if(fuelLevel >= maxFuelTreshold) {
            fuelLevel = maxFuelTreshold;
        }

        CheckFireStateUpgrade();

        OnFireFuelled?.Invoke(this, EventArgs.Empty);
    }

    private void CheckFireFeedable() {

        if(fuelLevel + orbFuelValue <= maxFuelTreshold) {
            SetStructurePrimaryFunctionUnlocked(true);
        } else {
            SetStructurePrimaryFunctionUnlocked(false);
        }

        if (state == State.extinguished) {
            SetStructurePrimaryFunctionUnlocked(false);
        }
    } 

    private void CheckFireStateDowngrade() {

        if (fuelLevel < 0 && state == State.calm) {
            ChangeState(State.extinguished);
        }

        if(fuelLevel <= mildFuelTreshold && state == State.mild) {
            ChangeState(State.calm);
        }

        if (fuelLevel <= wildFuelTreshold && state == State.wild) {
            ChangeState(State.mild);
        }

        if (fuelLevel <= insaneFuelTreshold && state == State.insane) {
            ChangeState(State.wild);
        }

        CheckFireSecondaryFunctionInteractable();
        CheckFireFeedable();
    }

    private void CheckFireStateUpgrade() {
        State maxState = LevelManager.Instance.GetLevelSO().maxFireState;

        if (state == State.calm && fuelLevel >= mildFuelTreshold) {
            ChangeState(State.mild);
        }

        if (state == State.mild && fuelLevel >= wildFuelTreshold) {
            ChangeState(State.wild);
        }

        if(state == State.wild && fuelLevel >= insaneFuelTreshold) {
            ChangeState(State.insane);
        }
    }

    private void CheckFireSecondaryFunctionInteractable() {
        State maxState = LevelManager.Instance.GetLevelSO().maxFireState;

        if(state == maxState) {
            SetStructureSecondaryFunctionUnlocked(true);
        } else {
            SetStructureSecondaryFunctionUnlocked(false);
        }
    }

    private void ChangeState(State newState) {
        SetFireAOEValues(newState);

        State previousState = state;
        state = newState;

        OnFireChangedState?.Invoke(this, new OnFireChangedStateEventArgs {
            previousState = previousState,
            newState = newState
        });
    }

    private void SetFireCurrentMaxFuelTreshold() {
        Debug.Log("SetFireCurrentMaxFuelTreshold");
        State state = LevelManager.Instance.GetLevelSO().maxFireState;
        if (state == State.calm) {
            maxFuelTreshold = mildFuelTreshold;
        }
        if (state == State.mild) {
            maxFuelTreshold = wildFuelTreshold;
        }
        if (state == State.wild) {
            maxFuelTreshold = insaneFuelTreshold;
        }
    }

    private void SetFireAOEValues(State newState) {

        if (state == State.extinguished) {
            initialFireAOEValue = 0;
        }

        if (state == State.mild) {
            initialFireAOEValue = mildFireRadius;
        }
        if (state == State.calm) {
            initialFireAOEValue = calmFireRadius;
        }
        if (state == State.wild) {
            initialFireAOEValue = wildFireRadius;
        }
        if (state == State.insane) {
            initialFireAOEValue = insaneFireRadius;
        }

        if (newState == State.extinguished) {
            finalFireAOEValue = 0;
        }
        if (newState == State.mild) {
            finalFireAOEValue = mildFireRadius;
        }
        if (newState == State.calm) {
            finalFireAOEValue = calmFireRadius;
        }
        if (newState == State.wild) {
            finalFireAOEValue = wildFireRadius;
        }
        if (newState == State.insane) {
            finalFireAOEValue = insaneFireRadius;
        }

        lerping = true;
    }

    public State GetState() {
        return state;
    }

    public float GetCalmFireRadius() {
        return calmFireRadius;
    }
    public float GetMildFireRadius() {
        return mildFireRadius;
    }
    public float GetWildFireRadius() {
        return wildFireRadius;
    }
    public float GetInsaneFireRadius() {
        return insaneFireRadius;
    }

    public float GetCalmFireTreshold() {
        return calmFuelTreshold;
    }

    public float GetMildFireTreshold() {
        return mildFuelTreshold;
    }

    public float GetWildFireTreshold() {
        return wildFuelTreshold;
    }

    public float GetInsaneFireTreshold() {
        return insaneFuelTreshold;
    }

    public float GetMaxFireTreshold() {
        return maxFuelTreshold;
    }

    public float GetOrbFuelValue() {
        return orbFuelValue;
    }

    public float GetCurrentFuelLevel() {
        return fuelLevel;
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition) {
        fuelLevel -= (damage * damageToFuelConversionRate);
        CheckFireStateDowngrade();
        OnFireDamageTaken?.Invoke(this, EventArgs.Empty);
    }

    public void Die() {

    }

    public Transform GetProjectileTarget() {
        return transform;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }
}
