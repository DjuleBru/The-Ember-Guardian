using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private string chestID;
    public string GetChestID() => chestID;

#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrEmpty(chestID) && gameObject.scene.IsValid()) {
            chestID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
    public void ForceNewID() {
        chestID = System.Guid.NewGuid().ToString();
    }
#endif
    public enum ChestType {
        orbChest,
        gemChest,
        ammoChest,
        initialChest,
        hugeChest,
        weaponChest,
        skillChest,
        trapChest,
    }

    [SerializeField] protected ChestType chestType;
    [SerializeField] protected Transform orbSpawnPosition;
    [SerializeField] protected List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList;
    [SerializeField] protected List<int> rewardAmountList;
    [SerializeField] protected bool chestDisappearsAutomaticallyAfterOpened;
    [SerializeField] protected bool payToOpenChest;
    [SerializeField] protected PayCurrencyUI payCurrencyUI;
    [SerializeField] protected List<PayCurrencyTemplateWorldUI> payCurrencyTemplates;

    protected float delayToChestUnlockAnimation;
    protected float delayToSpawnCollectibles;

    protected bool chestLocked = false;
    protected bool chestOpened;
    protected bool chestOpenedAnimationStarted;
    protected bool chestOpenedAnimationOver;
    protected bool playerInTriggerArea;
    protected bool playerPayingCurrencies;
    protected bool chestPricePaid;
    protected bool playerIsInTriggerAreaButChestLocked;
    protected bool rewardOfferedToPlayer;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;
    public event EventHandler<OnChestUnlockedEventArgs> OnChestUnlocked;
    public event EventHandler OnChestOpenable;
    public event EventHandler<OnChestUnlockedEventArgs> OnChestOpened;
    public event EventHandler OnChestOpenedAnimationOver;
    public event EventHandler OnChestDisappear;
    public event EventHandler OnChestPricePaid;
    public event EventHandler OnChestPricePaidLoaded;
    public event EventHandler OnChestTypeSet;

    public class OnChestUnlockedEventArgs:EventArgs {
        public bool triggerSFX;
    }


    public static event EventHandler<OnAnyChestSpawnedCollectibleEventArgs> OnAnyChestSpawnedCollectible;
    public class OnAnyChestSpawnedCollectibleEventArgs : EventArgs {
        public PlayerCurrencies.CurrencyType currencyType;
    }

    protected void Awake() {
        SetChestAnimationTimes();
    }

    protected virtual void Start() {
        if(payToOpenChest) {
            payCurrencyUI.OnCurrencyPaymentSuccess += PayCurrencyUI_OnCurrencyPaymentSuccess;
            payCurrencyUI.SetOrbTemplateUIList(payCurrencyTemplates);
        }

        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        int totalGemAmount = 0;
        int i = 0;

        foreach(PlayerCurrencies.CurrencyType type in currencyTypeToRewardList) {

            if(CurrenciesManager.Instance.GetCurrencyCategory(type) == PlayerCurrencies.CurrencyCategory.gem) {
                totalGemAmount += rewardAmountList[i];
            }
            i++;
        }

        GemDropManager.Instance.RecordChestGems(totalGemAmount);
        LevelManager.Instance.AddChest(this);
    }

    private void SetChestAnimationTimes() {
        if (chestType == ChestType.orbChest) {
            delayToChestUnlockAnimation = 2.5f;
            delayToSpawnCollectibles = 3.4f;
        }
        if (chestType == ChestType.gemChest) {
            delayToChestUnlockAnimation = 2.1f;
            delayToSpawnCollectibles = 2.8f;
        }
        if (chestType == ChestType.ammoChest) {
            delayToChestUnlockAnimation = 3.8f;
            delayToSpawnCollectibles = 4.8f;
        }
        if (chestType == ChestType.initialChest) {
            delayToChestUnlockAnimation = 1.9f;
            delayToSpawnCollectibles = 2.6f;
        }
        if (chestType == ChestType.hugeChest) {
            delayToChestUnlockAnimation = 2.2f;
            delayToSpawnCollectibles = 3.2f;
        }
        if (chestType == ChestType.weaponChest) {
            delayToChestUnlockAnimation = 3.8f;
            delayToSpawnCollectibles = 4.8f;
        }
        if (chestType == ChestType.skillChest) {
            delayToChestUnlockAnimation = 2.5f;
            delayToSpawnCollectibles = 0;
        }
        if (chestType == ChestType.trapChest) {
            delayToChestUnlockAnimation = 2.5f;
            delayToSpawnCollectibles = 3.3f;
        }
    }


    protected void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if(payToOpenChest) {
            if(!chestPricePaid) {
                payCurrencyUI.SetPlayerInteracting(true);
                playerPayingCurrencies = true;
            }
        }
    }

    protected void PayCurrencyUI_OnCurrencyPaymentSuccess(object sender, EventArgs e) {
        if (chestOpened) return;
        if (!playerInTriggerArea) return;

        OnChestPricePaid?.Invoke(this, EventArgs.Empty);
        chestPricePaid = true;
        OpenChest(chestDisappearsAutomaticallyAfterOpened);
    }

    protected virtual void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (chestOpened) return;

        if(playerPayingCurrencies) {
            payCurrencyUI.SetPlayerInteracting(false);
            return;
        }

        if (payToOpenChest) return;

        OpenChest(chestDisappearsAutomaticallyAfterOpened);
    }

    protected void OpenChest(bool spawnCollectibles) {
        chestOpened = true;
        StartCoroutine(OpenChestCoroutine(spawnCollectibles));
        OnChestOpened?.Invoke(this, new OnChestUnlockedEventArgs {
            triggerSFX = true
        });
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;
        if (chestLocked) return;

        playerInTriggerArea = false;
        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        if (chestDisappearsAutomaticallyAfterOpened && chestOpened) return;

        playerIsInTriggerAreaButChestLocked = true;
        if (chestLocked) return;

        playerInTriggerArea = true;
        Player.Instance.SetInOtherInteractableObjectTriggerArea(true);

        OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;
        playerInTriggerArea = false;
        playerIsInTriggerAreaButChestLocked = false;

        if (chestDisappearsAutomaticallyAfterOpened && chestOpened) return;
        if (chestLocked) return;

        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

        OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
    }

    protected IEnumerator OpenChestCoroutine(bool spawnCollectibles, bool triggerSFX = true) {
        yield return new WaitForSeconds(.1f);

        if(chestDisappearsAutomaticallyAfterOpened) {
            Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        }

        yield return new WaitForSeconds(delayToChestUnlockAnimation - .1f);

        playerPayingCurrencies = false;
        OnChestUnlocked?.Invoke(this, new OnChestUnlockedEventArgs {
            triggerSFX = triggerSFX,
        });

        yield return new WaitForSeconds(delayToSpawnCollectibles - delayToChestUnlockAnimation - .1f);

        if(spawnCollectibles) {
            int j = 0;

            foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
                int rewardAmount = rewardAmountList[j];

                PlayerCurrencies.CurrencyType currencyTypeToReward = currencyType;

                if (currencyType == PlayerCurrencies.CurrencyType.ammo) {
                    bool secondaryGunUnlocked = PlayerShoot.Instance.GetSecondaryGunSO() != null;

                    if (PlayerShoot.Instance.GetPrimaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special || (secondaryGunUnlocked && PlayerShoot.Instance.GetSecondaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special)) {
                        // Player has at least 1 special ammo weapon:
                        float randomFloat = UnityEngine.Random.value;
                        if(randomFloat < 0.5f) {
                            currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                            rewardAmount /= 2;
                        } else {
                            if (PlayerShoot.Instance.GetPrimaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special && (secondaryGunUnlocked && PlayerShoot.Instance.GetSecondaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special)) {
                                // Player has at 2 special ammo weapons:
                                currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                                rewardAmount /= 2;
                            }
                        }
                    }
                }

                for (int i = 0; i < rewardAmount; i++) {
                    
                    Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeToReward), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

                    OnAnyChestSpawnedCollectible?.Invoke(this, new OnAnyChestSpawnedCollectibleEventArgs {
                        currencyType = currencyType,
                    });

                    collectible.ApplyRandomUpwardsForce(5, 8);
                    collectible.SetCollectibleUnInteractable(.75f);
                    yield return new WaitForSeconds(.2f);
                }
                j++;
            }
        }

        chestOpenedAnimationOver = true;
        OnChestOpenedAnimationOver?.Invoke(this, EventArgs.Empty);

        if (!chestDisappearsAutomaticallyAfterOpened) yield break;

        yield return new WaitForSeconds(2f);

        StartCoroutine(MakeChestDisappear(3f));
    }

    protected IEnumerator MakeChestDisappear(float delayToDisappear) {

        OnChestDisappear?.Invoke(this, EventArgs.Empty);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);

        yield return new WaitForSeconds(delayToDisappear);
        gameObject.SetActive(false);
    }

    public ChestType GetChestType() {
        return chestType;
    }

    public void InvokeOnChestOpened(bool triggerSFX = true) {
        OnChestOpened?.Invoke(this, new OnChestUnlockedEventArgs {
            triggerSFX = triggerSFX
        });
    }

    public void InvokeOnAnyChestSpawnedCollectibles(PlayerCurrencies.CurrencyType currencyType) {
        OnAnyChestSpawnedCollectible?.Invoke(this, new OnAnyChestSpawnedCollectibleEventArgs {
            currencyType = currencyType,
        });
    }

    public bool GetChestDisappearsAutomatically() {
        return chestDisappearsAutomaticallyAfterOpened;
    } 

    public bool GetPayToOpenChest() {
        return payToOpenChest;
    }
    public bool GetChestOpened() {
        return chestOpened;
    }
    public bool GetChestPricePaid() {
        return chestPricePaid;
    }
    public bool GetRewardOfferedToPlayer() {
        return rewardOfferedToPlayer;
    }

    public void SetRewardOfferedToPlayer(bool rewardOfferedToPlayer) {
        this.rewardOfferedToPlayer = rewardOfferedToPlayer;

    }

    public void SetChestLocked(bool locked) {
        chestLocked = locked;

        if(!locked) {
            if(playerIsInTriggerAreaButChestLocked) {
                playerInTriggerArea = true;
                Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
                OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
                OnChestOpenable?.Invoke(this, EventArgs.Empty);
            }
        }

    }

    public void SetChestOpened(bool opened) {
        chestOpened = opened;

        if(chestOpened && chestDisappearsAutomaticallyAfterOpened) {
            gameObject.SetActive(false);
        }
    }

    public virtual void SetChestPaid(bool paid) {
        
        chestPricePaid = paid;


        if (payToOpenChest) {
            payToOpenChest = !paid;
        }

    }

    public bool GetChestLocked() {
        return chestLocked;
    }

    public void SetChestParameters(ChestType chestType, List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList, List<int> rewardAmountList) {
        this.chestType = chestType;
        this.currencyTypeToRewardList = currencyTypeToRewardList;
        this.rewardAmountList = rewardAmountList;

        OnChestTypeSet?.Invoke(this, EventArgs.Empty);
        SetChestAnimationTimes();
    }

    private void OnDestroy() {

        GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractPerformed;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
    }
}
