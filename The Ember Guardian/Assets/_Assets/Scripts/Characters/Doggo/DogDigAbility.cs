using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogDigAbility : MonoBehaviour
{
    [SerializeField] private DogAnimatorManager dogAnimator;
    [SerializeField] private Transform spawnPosition;

    public static DogDigAbility Instance;

    private bool digAbilityUnlocked;
    private bool sniffing;

    private float digTimer;
    private float digCooldown;
    private float digProbability;
    private float digDoubleProbability;

    public event EventHandler OnSniffStart;
    public static event EventHandler OnAnyResourceDug;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        dogAnimator.OnDogSniffedEnd += DogAnimator_OnDogSniffedEnd;

        digAbilityUnlocked = DogStats.Instance.GetGermanShepherdDigResourceAbilityUnlocked();

        digCooldown = DogStats.Instance.GetGermanShepherdDigResourceCooldown();
        digProbability = DogStats.Instance.GetGermanShepherdDigResourceProbility()/100f;
        digDoubleProbability = DogStats.Instance.GetGermanShepherdDigResourceDoubleProbability()/100f;

        digTimer = digCooldown;
    }

    private void DogAnimator_OnDogSniffedEnd(object sender, EventArgs e) {
        sniffing = false;
        if (!digAbilityUnlocked) return;
        TryDiggingOutStuff();
    }

    private void Update() {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;

        if (!digAbilityUnlocked) return;
        if (CampZoneManager.Instance.IsWithinCampZoneLimits(transform.position)) return;


        if(!sniffing) {
            HandleSniffStart();
        }

    }

    private void HandleSniffStart() {
        digTimer -= Time.deltaTime;

        if (digTimer < 0) {
            sniffing = true;
            OnSniffStart?.Invoke(this, EventArgs.Empty);
            digTimer = digCooldown;
            return;
        }

    }

    private void TryDiggingOutStuff() {
        List<PlayerCurrencies.CurrencyType> currencyTypesDiggedOut = new List<PlayerCurrencies.CurrencyType>();
        List<int> currencyAmountDiggedOut = new List<int>();

        float randomNumber = UnityEngine.Random.Range(0f, 1f);

        int bigBlueOrbDiggedOut = UnityEngine.Random.Range(0, 2);
        int smallBlueOrbDiggedOut = UnityEngine.Random.Range(0, 2);
        if (randomNumber < digProbability) {
            // Dig successful ! 

            float randomNumber2 = UnityEngine.Random.Range(0f, 1f);

            if (randomNumber2 < digDoubleProbability) {
                // Double dig successful ! 

                bigBlueOrbDiggedOut *= 2;
                smallBlueOrbDiggedOut *= 2;

            }

            if (bigBlueOrbDiggedOut != 0) {
                currencyTypesDiggedOut.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);
                currencyAmountDiggedOut.Add(bigBlueOrbDiggedOut);
            }
            if (smallBlueOrbDiggedOut != 0) {
                currencyTypesDiggedOut.Add(PlayerCurrencies.CurrencyType.smallBlueOrb);
                currencyAmountDiggedOut.Add(smallBlueOrbDiggedOut);
            }

            if (bigBlueOrbDiggedOut == 0 && smallBlueOrbDiggedOut == 0) {
                smallBlueOrbDiggedOut = 1;
                currencyTypesDiggedOut.Add(PlayerCurrencies.CurrencyType.smallBlueOrb);
                currencyAmountDiggedOut.Add(smallBlueOrbDiggedOut);
            }

            StartCoroutine(DigOutStuff(currencyTypesDiggedOut, currencyAmountDiggedOut));

        }
    }

    private IEnumerator DigOutStuff(List<PlayerCurrencies.CurrencyType> currencyTypeToRewardList, List<int> rewardAmountList) {
        int j = 0;

        foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
            int rewardAmount = rewardAmountList[j];

            for (int i = 0; i < rewardAmount; i++) {
                Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), spawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

                collectible.ApplyRandomUpwardsForce(5, 8);
                collectible.SetCollectibleUnInteractable(.75f);
                OnAnyResourceDug?.Invoke(this, EventArgs.Empty);

                yield return new WaitForSeconds(.2f);
            }
            j++;
        }
    }

    public bool GetSniffing() {
        return sniffing;
    }

}
