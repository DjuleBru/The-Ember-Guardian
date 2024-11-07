using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Structure {

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

    private bool lerping;
    private float lerpTimer;
    private float lerpDuration = 1f;
    private float initialFireAOEValue;
    private float finalFireAOEValue;

    protected override void Awake() {
        base.Awake();
        fireOrbCollider = GetComponentInChildren<FireOrbCollider>();
    }

    protected override void Start() {
        base.Start();

        fireOrbCollider.OnOrbFellInFire += FireOrbCollider_OnOrbFellInFire;

        fuelLevel = orbFuelValue;
        ChangeState(State.calm);
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
        CheckFireStateUpgrade();

        OnFireFuelled?.Invoke(this, EventArgs.Empty);
    }

    private void CheckFireFeedable() {

        if(fuelLevel >= maxFuelTreshold) {
            SetStructureFunctionUnlocked(false);
        }

        if(fuelLevel + orbFuelValue <= maxFuelTreshold) {
            SetStructureFunctionUnlocked(true);
        }

        if (state == State.extinguished) {
            SetStructureFunctionUnlocked(false);
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

    }

    private void CheckFireStateUpgrade() {
        if (fuelLevel >= mildFuelTreshold && state == State.calm) {
            ChangeState(State.mild);
        }

        if(fuelLevel >= wildFuelTreshold && state == State.mild) {
            ChangeState(State.wild);
        }

        if(fuelLevel >= insaneFuelTreshold && state == State.wild) {
            ChangeState(State.insane);
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

        CheckFireFeedable();
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
}
