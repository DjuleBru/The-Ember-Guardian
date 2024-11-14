using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCurrencies : MonoBehaviour
{
    public static PlayerCurrencies Instance;

    [SerializeField] private int initialOrbAmountDebug;
    [SerializeField] private Transform blueOrbPrefab;
    [SerializeField] private Transform blueOrbDropPoint;

    public enum CurrencyType {
        blueOrb,
        redOrb,
        greenGem,
        redGem,
        ember,
        ammo,
    }

    private int blueOrbAmount;
    private int redOrbAmount;
    private int greenGemOrbAmount;
    private int redGemOrbAmount;
    private int emberAmount;
    private int ammoAmount;

    public event EventHandler<OnCurrencyChangedEventArgs> OnBlueOrbChanged;
    public event EventHandler<OnBlueOrbDroppedOnTheFloorEventArgs> OnBlueOrbDroppedOnTheFloor;
    public class OnCurrencyChangedEventArgs : EventArgs {
        public int previousAmount;
        public int newAmount;
    }
    public class OnBlueOrbDroppedOnTheFloorEventArgs : EventArgs {
        public Collectible blueOrbDropped;
    }
    private Collectible lastBlueOrbDroppedOnTheFloor;


    private void Awake() {
        Instance = this;
        blueOrbAmount = initialOrbAmountDebug;
    }

    private void Start() {
        GameInput.Instance.OnPlayerInteractStarted += GameInput_OnPlayerInteractStarted;
    }

    private void GameInput_OnPlayerInteractStarted(object sender, EventArgs e) {

        if(Player.Instance.GetCanDropOrbOnTheFloor()) {
            if(blueOrbAmount >= 1) {

                lastBlueOrbDroppedOnTheFloor = Instantiate(blueOrbPrefab, blueOrbDropPoint.transform.position, Quaternion.identity).GetComponent<Collectible>();

                float aimDirX = PlayerAim.Instance.GetAimDir().x;
                lastBlueOrbDroppedOnTheFloor.ApplyRandomForce(1f * aimDirX, 2f * aimDirX, 6f, 8f);
                lastBlueOrbDroppedOnTheFloor.SetCollectibleUnInteractable(3f);
                lastBlueOrbDroppedOnTheFloor.SetDroppedByPlayer();

                OnBlueOrbDroppedOnTheFloor?.Invoke(this, new OnBlueOrbDroppedOnTheFloorEventArgs {
                    blueOrbDropped = lastBlueOrbDroppedOnTheFloor
                });
            }
        }
    }

    public void ChangeCurrencyAmount(CurrencyType currencyType, int amount) {
        if(currencyType == CurrencyType.blueOrb) {
            blueOrbAmount += amount;

            OnBlueOrbChanged?.Invoke(this, new OnCurrencyChangedEventArgs() {
                previousAmount = blueOrbAmount - amount,
                newAmount = blueOrbAmount
            });
        }

        if(currencyType == CurrencyType.redOrb) {
            redOrbAmount += amount;
        }

        if (currencyType == CurrencyType.ammo) {
            ammoAmount += amount;
        }
    }

    public int GetCurrencyAmount(CurrencyType currencyType) {
        if(currencyType == CurrencyType.blueOrb) {
            return blueOrbAmount;
        }
        return 0;
    }

}
