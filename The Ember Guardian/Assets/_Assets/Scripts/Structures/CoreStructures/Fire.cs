using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Structure
{
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
        CheckFireStateDowngrade();
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

        OnFireChangedState?.Invoke(this, new OnFireChangedStateEventArgs {
            previousState = state,
            newState = newState
        });

        state = newState;

        CheckFireFeedable();
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
