using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DayNightManager : MonoBehaviour
{

    public static DayNightManager Instance;

    [SerializeField] private float dawnDuration;
    [SerializeField] private float dayDuration;
    [SerializeField] private float duskDuration;
    [SerializeField] private float nightDuration;

    [SerializeField] private bool manualInitialCycleSet;
    [SerializeField] private bool cyclePaused;
    [SerializeField] private State debugState;

    private int currentDay;

    private float cycleTimer;
    private float totalDayTimer;
    private float totalNightTimer;

    public enum State { 
    Dawn,
    Day,
    Dusk,
    Night,
    }

    private State state;
    

    public event EventHandler OnDawnStart;
    public event EventHandler OnDayStart;
    public event EventHandler OnDuskStart;
    public event EventHandler OnNightStart;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        CreaturesManager.Instance.OnAllCreaturesAtNightKilled += CreaturesManager_OnAllCreaturesAtNightKilled;
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        Fire.Instance.OnFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;
        Fire.Instance.OnFireEmberExtractionStopped += Fire_OnFireEmberExtractionStopped;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;

        if (manualInitialCycleSet) {
            ChangeState(debugState);
            return;
        }

        state = State.Dawn;
        OnDawnStart?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        HandleDebugNextState();
        if (cyclePaused) return;

        cycleTimer += Time.deltaTime;

        switch (state) {

            case State.Dawn:
                totalDayTimer += Time.deltaTime;
                if (cycleTimer > dawnDuration) {
                    ChangeState(State.Day);
                    cycleTimer = 0;
                }
                break;

            case State.Day:
                totalDayTimer += Time.deltaTime;
                if (cycleTimer > dayDuration) {
                    ChangeState(State.Dusk);
                    cycleTimer = 0;
                }
                break;

            case State.Dusk:
                totalDayTimer += Time.deltaTime;

                if (cycleTimer > duskDuration) {
                    ChangeState(State.Night);
                    cycleTimer = 0;
                    totalDayTimer = 0;
                }
                break;

            case State.Night:
                totalNightTimer += Time.deltaTime;
                break;
        }
    }

    private void CreaturesManager_OnAllCreaturesAtNightKilled(object sender, EventArgs e) {
        ChangeState(State.Dawn);
        cycleTimer = 0;
        totalNightTimer = 0;
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        SetCyclePaused(false);
    }

    private void Fire_OnFireEmberExtractionStopped(object sender, EventArgs e) {
        SetCyclePaused(false);
    }

    private void Fire_OnFireEmberExtractionStarted(object sender, EventArgs e) {
        SetCyclePaused(true);
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, EventArgs e) {
        SetCyclePaused(true);
    }

    private void HandleDebugNextState() {
        if(Input.GetKeyDown(KeyCode.N)) {
            switch (state) {

                case State.Dawn:
                        ChangeState(State.Day);
                    break;

                case State.Day:
                        ChangeState(State.Dusk);
                    break;

                case State.Dusk:
                        ChangeState(State.Night);
                    break;

                case State.Night:
                        ChangeState(State.Dawn);
                    break;
            }
            cycleTimer = 0;
            totalNightTimer = 0; totalDayTimer = 0;
        }
    }

    public void ChangeState(State newState) {
        state = newState;

        if(newState == State.Day) {
            OnDayStart?.Invoke(this, EventArgs.Empty);
        }
        if(newState == State.Dawn) {
            currentDay++;
            OnDawnStart?.Invoke(this, EventArgs.Empty);
        }
        if(newState == State.Night) {
            OnNightStart?.Invoke(this, EventArgs.Empty);
        }
        if(newState == State.Dusk) {
            OnDuskStart?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetCyclePaused(bool paused) {
        cyclePaused = paused;
    }

    public float GetCycleTimer() {
        return cycleTimer;
    }

    public float GetDuskDuration() {
        return duskDuration;
    }
    public float GetDayDuration() {
        return dayDuration;
    }
    public float GetDawnDuration() {
        return dawnDuration;
    }

    public float GetNightDuration() {
        return nightDuration;
    }

    public void SetNightDuration(float nightDuration) {
        this.nightDuration = nightDuration;
    }

    public float GetTotalDayDuration() {
        return duskDuration + dayDuration + dawnDuration;
    }

    public float GetTotalDayDurationNormalized() {
        return totalDayTimer/GetTotalDayDuration();
    }

    public float GetDayDurationNormalized() {
        return cycleTimer / dayDuration;
    }

    public float GetDuskDurationNormalized() {
        return cycleTimer / duskDuration;
    }

    public float GetDawnDurationNormalized() {
        return cycleTimer /dawnDuration;
    }

    public float GetNightDurationNormalized() {
        return totalNightTimer / nightDuration;
    }

    public State GetDayNightCycleState() {
        return state;
    }

    public int GetCurrentDay() {
        return currentDay;
    }
}
