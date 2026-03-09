using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    public static Dog Instance;

    public enum DogType {
        GermanShepherd,
        GoldenRetreiver,
        DarkCompanion,
    }

    public enum DogSkin {
        GermanShepherdSkin,
        GoldenRetreiverSkin,
        DarkCompanionSkin,
        Husky,
        GermanShepherdLight,
        GoldenBrownSkin,
        DarkCompanionRed,
    }

    [Serializable]
    public class DogTypeSkins {
        public Dog.DogType dogType;
        public List<DogSkinDLCLink> dogTypeDLCSkins;
    }

    [Serializable]
    public class DogSkinDLCLink {
        public DogSkin dogSkin;
        public DLCManager.DLCType linkedDLC;
    }

    public DogType dogType;
    public DogSkin dogSkin;

    public event EventHandler OnPlayerTriggeredIn;
    public event EventHandler OnPlayerTriggeredOut;

    private List<DogAI> dogAIList = new List<DogAI>();
    private DogAI currentDogAI;
    [SerializeField] private bool useDebugDogType;
    [SerializeField] private bool useDebugDogSkin;
    [SerializeField] private Dog.DogType debugDogType;
    [SerializeField] private Dog.DogSkin debugDogSkin;
    [SerializeField] private DogAI.State initialIdleState;
    [SerializeField] private DogAI.State initialState;

    [SerializeField] private List<DogTypeSkins> dogTypeSkins;
    private DogAI.State currentIdleState;

    public event EventHandler OnIdleStateChanged;
    public event EventHandler OnPlayerCalledDog;
    public event EventHandler OnPlayerStayDog;
    public event EventHandler<OnDogTypeChangedEventArgs> OnDogTypeChanged;
    public event EventHandler<OnDogTypeChangedEventArgs> OnDogSkinChanged;

    public class OnDogTypeChangedEventArgs:EventArgs {
        public bool selectedFromMenu;
    }

    private void Awake() {
        Instance = this; 
        
        DogAI[] dogAIArray = GetComponents<DogAI>();
        foreach (DogAI dogAI in dogAIArray) {
            dogAIList.Add(dogAI);
        }

        SetCurrentDogType();
        SetCurrentDogSkin();
        SetCurrentDogAI();
    }

    private void Start() {
        GameInput.Instance.OnPlayerCallDogPerformed += GameInput_OnPlayerCallDogPerformed;
        currentIdleState = initialIdleState;

        OnDogTypeChanged?.Invoke(this, new OnDogTypeChangedEventArgs {
            selectedFromMenu = false
        });

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            if(SavingManager_Level.Instance.GetLoadingSavedLevel()) {
                SetPosition(new Vector3(UnityEngine.Random.Range(-10, 10), 0, 0));
            }
        }
    }

    private void SetCurrentDogType() {
        dogType = ES3.Load("dogType", DogType.GermanShepherd);
        if (useDebugDogType) {
            dogType = debugDogType;
        }
    }
    private void SetCurrentDogSkin() {
        dogSkin = LoadSkinForType(dogType);

        if (useDebugDogSkin)
            dogSkin = debugDogSkin;
    }

    private DogSkin LoadSkinForType(DogType type) {
        DogSkin defaultSkin = GetDefaultSkinFromType(type);
        DogSkin loadedSkin = ES3.Load(type + "_skin", defaultSkin);
        bool manual = ES3.Load(type + "_skinManual", false);

        if (!manual)
            return defaultSkin;

        return loadedSkin;
    }

    private void SetCurrentDogAI() {
        foreach (DogAI dogAI in dogAIList) {
            dogAI.enabled = false;
            if (dogAI.GetDogAIType() == dogType) {
                dogAI.enabled = true;
                currentDogAI = dogAI;
            }
        }

        if(dogType != DogType.GermanShepherd) {
            GetComponent<DogDigAbility>().enabled = false;
        }

    }

    private void GameInput_OnPlayerCallDogPerformed(object sender, EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (PetDog.Instance.GetPlayerCanPetDog()) return;
        if (PlayerShoot.Instance.GetHeldGun().GetGunJammedAndNextInputSequence(GameInput.Binding.callDoggo)) return;

        if (currentIdleState == DogAI.State.walkWithPlayer) {

            currentIdleState = DogAI.State.stay;
            OnPlayerStayDog?.Invoke(this, EventArgs.Empty);

        } else if (currentIdleState == DogAI.State.stay || currentIdleState == DogAI.State.idle) {

            currentIdleState = DogAI.State.walkWithPlayer;
            OnPlayerCalledDog?.Invoke(this, EventArgs.Empty);

        }

        currentDogAI.SetIdleBehaviorState(currentIdleState);
        OnIdleStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Player>() != null) {
            OnPlayerTriggeredIn?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            OnPlayerTriggeredOut?.Invoke(this, EventArgs.Empty);
        }
    }

    public void MoveOnTeleporter(Transform teleporterPlayerPosition) {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        SetPosition(teleporterPlayerPosition.position);

        // To make the dog sit down
        currentDogAI.SetState(DogAI.State.idle);
        currentDogAI.SetState(DogAI.State.stay);
    }
    public void MoveOutTeleporter() {
        currentDogAI.SetState(GetIdleState());
    }

    public DogAI.State GetIdleState() {
        return currentIdleState;
    }

    public DogAI.State GetInitialState() {
        return initialState;
    }

    public void SetPosition(Vector3 position) {
        transform.position = position;
        GetComponent<MobMovement>().SetMoveTarget(position);
        currentDogAI.SetInitialPosition(position);
    }

    public DogType GetDogType() {
        return dogType;
    }

    public DogSkin GetDogSkin() {
        return dogSkin;
    }

    public void SetDogType(DogType dogType, bool selectedFromMenu = false) {
        this.dogType = dogType;

        dogSkin = LoadSkinForType(dogType);

        ES3.Save("dogType", dogType);

        SetCurrentDogAI();

        OnDogTypeChanged?.Invoke(this, new OnDogTypeChangedEventArgs {
            selectedFromMenu = selectedFromMenu
        });
    }

    public void SetDogSkin(DogSkin skin, bool selectedFromMenu = false) {
        dogSkin = skin;

        ES3.Save(dogType + "_skin", skin);
        ES3.Save(dogType + "_skinManual", true);

        OnDogSkinChanged?.Invoke(this, new OnDogTypeChangedEventArgs {
            selectedFromMenu = selectedFromMenu
        });
    }

    [Button]
    public void UnlockDogType(DogType dogType) {
        if(dogType == DogType.GoldenRetreiver) {
            MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HUBMerchantItem_DogTamerItem.DogTamerItemType.Dog_GoldenRetreiver.ToString(), true);
            MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HUBMerchantItem_DogTamerItem.DogTamerItemType.Dog_GoldenRetreiver.ToString(), true);
            MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.DogTamer, true);
            DogStats.Instance.UnlockRetreiver();
            DogStats.Instance.UnlockRetreiverBiteAbility();
        }

        if (dogType == DogType.DarkCompanion) {
            MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(HUBMerchantItem_DogTamerItem.DogTamerItemType.Dog_DarkCompanion.ToString(), true);
            MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(HUBMerchantItem_DogTamerItem.DogTamerItemType.Dog_DarkCompanion.ToString(), true);
            MetaProgressionManager.Instance.SetHubMerchantNewItemsToSale(HubMerchant.HubMerchantType.DogTamer, true);
            DogStats.Instance.UnlockDarkCompanion();
            DogStats.Instance.UnlockDarkCompanionBiteAbility();
        }

        this.dogType = dogType;
        SetCurrentDogAI();
        OnDogTypeChanged?.Invoke(this, new OnDogTypeChangedEventArgs {
            selectedFromMenu = false
        });

        // For Hub (TooltipManager)
        ES3.Save("prepareSwapDogTooltip", true);
    }

    public List<DogAI> GetDogAIList() {
        return dogAIList;
    }

    public DogAI GetCurrentDogAI() {
        return currentDogAI;
    }

    private DogSkin GetDefaultSkinFromType(DogType dogType) {
        switch (dogType) {
            default:
            case DogType.GermanShepherd: return DogSkin.GermanShepherdSkin;
            case DogType.GoldenRetreiver: return DogSkin.GoldenRetreiverSkin;
            case DogType.DarkCompanion: return DogSkin.DarkCompanionSkin;
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerCallDogPerformed -= GameInput_OnPlayerCallDogPerformed;
    }
}
