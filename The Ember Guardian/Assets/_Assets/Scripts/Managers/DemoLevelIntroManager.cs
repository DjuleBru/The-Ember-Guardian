using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoLevelIntroManager : MonoBehaviour {

    [SerializeField] private List<Structure> initialStructures;
    [SerializeField] private List<StructureLocation> structureLocationsUnlocked;
    [SerializeField] private Chest initialChest;
    [SerializeField] private GameObject initialChestIndicator;

    private bool playerWithinCamp;
    private bool dayNightCyclePaused;
    private bool ammoCollected;
    private bool playerReloaded;

    public static DemoLevelIntroManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
        PlayerShoot.Instance.OnPlayerAmmoRefilled += PlayerShoot_OnPlayerAmmoRefilled;
        Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
        initialChest.OnChestOpened += InitialChest_OnChestOpened;
        CreaturesSpawnManager.Instance.SetDemoWave();
        LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;

        StartCoroutine(SetInitialObjective());
        StartCoroutine(SetPlayerCurrenciesAfterDelay());

        PlayerShoot.Instance.SetCanStartSurgeWindow(false);
        Fire.Instance.ActivateInitialFire();
        Fire.Instance.gameObject.SetActive(true);

        foreach (Structure structure in initialStructures) {
            structure.gameObject.SetActive(true);
        }

        foreach (StructureLocation location in structureLocationsUnlocked) {
            location.UnlockStructureLocation();
        }
    }

    private void LevelManager_OnLevelFailed(object sender, System.EventArgs e) {
        MetaProgressionManager.Instance.SetTutorialCompleted();
    }

    private void InitialChest_OnChestOpened(object sender, System.EventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.OpenChest, LevelUI_ObjectiveUI.SubObjectiveType.LoadBelt);
        ammoCollected = true;
    }

    private void Update() {

        if(dayNightCyclePaused) return;

        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk && DayNightManager.Instance.GetCycleTimer() > 29 && !DayNightManager.Instance.GetCyclePaused()) {
            DayNightManager.Instance.SetCyclePaused(true);
        }
    }

    private void Player_OnPlayerEnteredCamp(object sender, System.EventArgs e) {
        if (playerWithinCamp) return;

        playerWithinCamp = true;
        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.HeadBackToCamp);
        CheckNightStart();

    }

    private void PlayerShoot_OnPlayerAmmoRefilled(object sender, PlayerShoot.OnAmmoRefilledEventArgs e) {
        LevelUI_ObjectiveUI.Instance.SetNextSubObjective(LevelUI_ObjectiveUI.SubObjectiveType.LoadBelt, LevelUI_ObjectiveUI.SubObjectiveType.ReloadGun);
    }

    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        if (playerReloaded) return;

        LevelUI_ObjectiveUI.Instance.SetSubObjectiveCompleted(LevelUI_ObjectiveUI.SubObjectiveType.ReloadGun);
        playerReloaded = true;
        CheckNightStart();
    }

    private IEnumerator SetInitialObjective() {
        yield return new WaitForSeconds(5f);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.PrepareForNight);

        List<LevelUI_ObjectiveUI.SubObjectiveType> subObjectiveTypes = new List<LevelUI_ObjectiveUI.SubObjectiveType> {
            LevelUI_ObjectiveUI.SubObjectiveType.HeadBackToCamp,
            LevelUI_ObjectiveUI.SubObjectiveType.OpenChest,
        };

        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUI(subObjectiveTypes);
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        initialChestIndicator.SetActive(true);
    }

    private void CheckNightStart() {
        if (!playerWithinCamp) return;
        if (!playerReloaded) return;

        StartCoroutine(StartNightAfterDelay());
    }

    private IEnumerator StartNightAfterDelay() {
        yield return new WaitForSeconds(6f);

        LevelUI_ObjectiveUI.Instance.SetNewObjectiveUI(LevelUI_ObjectiveUI.ObjectiveType.TrySurvive);

        if (dayNightCyclePaused) {
            DayNightManager.Instance.SetCyclePaused(false);
        }
        else {

            DayNightManager.Instance.ChangeState(DayNightManager.State.Night);
        }
    }

    public void ShowRunTooltip() {
        PlayerTooltipManager.Instance.GetTooltipRight().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_toRun"), InputControlIcons.Control.Run, 5f);
    }

    private IEnumerator SetPlayerCurrenciesAfterDelay() {
        yield return new WaitForSeconds(.05f);
        PlayerShoot.Instance.SetGunAmmo(PlayerShoot.Instance.GetHeldGunSO(), 0, 0);
        PlayerShoot.Instance.SetCanShoot(true);
        PlayerUI_AmmoBar.Instance.RefreshAmmoBar();
        PlayerCurrencies.Instance.SetCarryingEmber(false);
        UICurrencyManager.PlayerInventoryUI.RemoveCurrencyFromBag(PlayerCurrencies.CurrencyType.ember, 1);

        Fire.Instance.ManualSetFireCurrentMaxFuelTreshold(Fire.State.calm);
        Fire.Instance.SetFireInteractionsUpdateLocked(true);
    }
}
