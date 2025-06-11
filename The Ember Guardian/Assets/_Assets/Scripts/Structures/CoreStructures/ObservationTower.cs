using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObservationTower : Structure
{
    public event EventHandler OnObservationTowerActivated;
    public event EventHandler OnObservationTowerDeActivated;
    public static event EventHandler OnAnyObservationTowerActivated;
    public static event EventHandler OnAnyObservationTowerDeActivated;

    private bool observationTowerActive;

    protected override void Start() {
        base.Start();

        Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (observationTowerActive) {
            LevelUI_WaveInfoUI.Instance.ShowFullWaveInfoUI();
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (observationTowerActive) {
            LevelUI_WaveInfoUI.Instance.HideFullWaveInfoUI();
        }
    }

    private void Player_OnPlayerExitedCamp(object sender, EventArgs e) {
        if (!observationTowerActive) return;

        LevelUI_WaveInfoUI.Instance.HideWaveInfoUI();
    }

    private void Player_OnPlayerEnteredCamp(object sender, EventArgs e) {
        if (!observationTowerActive) return;

        LevelUI_WaveInfoUI.Instance.ShowWaveInfoUI();
    }

    protected override void PayOrbsUI_OnOrbPaymentSuccess(object sender, EventArgs e) {

        base.PayOrbsUI_OnOrbPaymentSuccess(sender, e);
        SetStructurePrimaryFunctionUnlocked(false);
        ActivateObservationTower();
    }

    protected override void DayNightManager_OnNightStart(object sender, EventArgs e) {
        base.DayNightManager_OnNightStart(sender, e);

        if (observationTowerActive) {
            SetStructurePrimaryFunctionUnlocked(true);
            DeactivateObservationTower();

        }
    }

    [Button]
    private void ActivateObservationTower() {
        observationTowerActive = true;
        OnObservationTowerActivated?.Invoke(this, EventArgs.Empty);
        OnAnyObservationTowerActivated?.Invoke(this, EventArgs.Empty);

        LevelUI_WaveInfoUI.Instance.RefreshWaveInfo();
        LevelUI_WaveInfoUI.Instance.ShowWaveInfoUI();
        LevelUI_WaveInfoUI.Instance.ShowFullWaveInfoUI();
    }

    private void DeactivateObservationTower() {
        OnObservationTowerDeActivated?.Invoke(this, EventArgs.Empty);
        OnAnyObservationTowerDeActivated?.Invoke(this, EventArgs.Empty);
        observationTowerActive = false;

        LevelUI_WaveInfoUI.Instance.HideWaveInfoUI();
    }
}
