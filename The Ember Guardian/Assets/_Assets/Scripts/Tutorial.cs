using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private TutorialCollider firstCreatureCollider;
    [SerializeField] private TutorialCollider blockingWorkersCollider;

    private bool moveTooltipShown;
    private bool moveTooltipHidden;
    private bool transferAmmoTooltipShown;
    private bool reloadTooltipShown;
    private bool reloadTooltipHidden;
    private bool saveAmmoTooltipShown;

    private bool shootTipShown;
    private bool shootTipHidden;
    private bool firstCreatureDied;

    private bool dropOrbShown;
    private int workerNumberRecruited;

    private bool showingGetReady;

    private void Start() {
        PlayerShoot.Instance.SetCanShoot(false);
        StartCoroutine(SetGunAmmoAfterDelay());

        UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        PlayerShoot.Instance.OnPlayerAmmoRefilled += PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        Creature.OnAnyMobDied += Creature_OnAnyMobDied;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        PlayerShoot.Instance.OnPlayerShot += PlayerSHoot_OnPlayerShot;

        StartCoroutine(ShowMoveTooltipAfterDelay());
    }

    private void Update() {
        if(moveTooltipShown && !moveTooltipHidden) {
            if(GameInput.Instance.GetMovementFloatNormalized() != 0) {
                moveTooltipHidden = true;
                StartCoroutine(HideTooltipAfterDelay(1.5f));
            }
        }

        if(!reloadTooltipHidden) {
            HandleBlockingCollider(firstCreatureCollider.transform.position, "I should get my gun ready first");
        }

        if(reloadTooltipHidden && workerNumberRecruited < 4) {
            HandleBlockingCollider(blockingWorkersCollider.transform.position, "I should recruit the lost souls first");
        }
    }

    private void HandleBlockingCollider(Vector3 colliderPosition, string textToShow) {
        if (Mathf.Abs(Player.Instance.transform.position.x - colliderPosition.x) < 1.5f && !showingGetReady) {
            showingGetReady = true;
            PlayerUI_World.Instance.GetTooltipRight().ShowTooltip(textToShow, 3f);
        }

        if (Mathf.Abs(Player.Instance.transform.position.x - colliderPosition.x) > 1.5f && showingGetReady) {
            showingGetReady = false;
            PlayerUI_World.Instance.GetTooltipRight().HideTooltip();
        }
    }

    private void Worker_OnAnyWorkerRecruited(object sender, System.EventArgs e) {
        workerNumberRecruited++;

        if(workerNumberRecruited == 4) {
            blockingWorkersCollider.SetColliderTrigger();
            StartCoroutine(HideTooltipAfterDelay(0f));
            StartCoroutine(StartGuardingWorkersObjective(2f));
        }
    }

    private void Creature_OnAnyMobDied(object sender, System.EventArgs e) {
        if (firstCreatureDied) return;
        StartCoroutine(TransitionToTutorialCameraCoroutine(3f));
        firstCreatureDied = true;
    }

    private void PlayerSHoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (!shootTipShown) return;
        if (shootTipHidden) return;

        StartCoroutine(HideTooltipAfterDelay(2f));
        shootTipHidden = true;
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (shootTipShown) return;

        StartCoroutine(ShowTooltipAfterDelay(.2f, "Press", "To shoot", InputControlIcons.Control.Shoot));
        shootTipShown = true;
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        Debug.Log(PlayerShoot.Instance.GetCurrentBullets());

        if (reloadTooltipShown && !reloadTooltipHidden) {
            PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();
            reloadTooltipHidden = true;
            showingGetReady = false; 
            firstCreatureCollider.SetColliderTrigger();
            return;
        }

        if(!saveAmmoTooltipShown) {
            if(PlayerShoot.Instance.GetCurrentBullets() != PlayerShoot.Instance.GetMaxBulletsPerClip()) {
                PlayerUI_World.Instance.GetTooltipRight().ShowTooltip("I should save my ammo ... ", 3f);
                saveAmmoTooltipShown = true;
            }
        }
    }

    private void PlayerShoot_OnPlayerAmmoRefilled(object sender, PlayerShoot.OnAmmoRefilledEventArgs e) {
        if (reloadTooltipShown) return;
        reloadTooltipShown = true;
        StartCoroutine(SwapReloadInstructionsCoroutine(1.5f));
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (!transferAmmoTooltipShown) {
            if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ammo) {
                StartCoroutine(ShowTooltipAfterDelay(1f, "Hold", "To transfer ammo to rifle", InputControlIcons.Control.Reload));
                transferAmmoTooltipShown = true;
                return;
            }
        };

        if(!dropOrbShown) {
            if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb) {
                Debug.Log("ShowRecruit tooltip");
                StartCoroutine(ShowTooltipAfterDelay(1f, "Press", "To recruit a lost soul", InputControlIcons.Control.Interact));
                dropOrbShown = true;
                return;
            }
        }

    }

    private IEnumerator SwapReloadInstructionsCoroutine(float delay) {
        Debug.Log("SwapReloadInstructionsCoroutine");
        yield return new WaitForSeconds(delay);

        PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();

        yield return new WaitForSeconds(3f);

        Debug.Log(PlayerShoot.Instance.GetCurrentBullets());

        if (PlayerShoot.Instance.GetCurrentBullets() == 0) {
            StartCoroutine(ShowTooltipAfterDelay(0f, "Press", "To reload", InputControlIcons.Control.Reload));
        } else {
            reloadTooltipHidden = true;
        }

    }

    private IEnumerator ShowMoveTooltipAfterDelay() {
        StartCoroutine(ShowTooltipAfterDelay(2f, "Use", "To move", InputControlIcons.Control.Move));
        yield return new WaitForSeconds(2f);
        moveTooltipShown = true;
    }

    private IEnumerator SetGunAmmoAfterDelay() {
        yield return new WaitForSeconds(.05f);
        PlayerShoot.Instance.SetGunAmmo(PlayerShoot.Instance.GetHeldGunSO(), 0);
        PlayerShoot.Instance.SetCanShoot(true);
        PlayerUI_AmmoBar.Instance.RefreshAmmoBar();
    }

    private IEnumerator ShowTooltipAfterDelay(float delay, string text1, string text2, InputControlIcons.Control control) {
        yield return new WaitForSeconds(delay);
        List<Sprite> spriteList = InputControlIcons.Instance.GetControlIconSprite(control);

        if(spriteList.Count == 1) {
            PlayerUI_World.Instance.GetTooltipLeft().ShowTooltipInstruction(text1, text2, spriteList[0]);
        }

        if (spriteList.Count == 2) {
            PlayerUI_World.Instance.GetTooltipLeft().ShowTooltipInstruction(text1, text2, spriteList[0], spriteList[1]);
        }

    }

    private IEnumerator HideTooltipAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        PlayerUI_World.Instance.GetTooltipLeft().HideTooltip();
    }

    private IEnumerator StartGuardingWorkersObjective(float delay) {
        yield return new WaitForSeconds(1f);

        TransitionToCombatCamera();

        yield return new WaitForSeconds(delay);

        LevelUI_ObjectiveUI.Instance.ShowObjectiveUI("Protect your workers");

        yield return new WaitForSeconds(2f);

        MusicManager.Instance.FadeInMusic(8f);

        yield return new WaitForSeconds(4f);

        StartCoroutine(ShowTooltipAfterDelay(0f, "Hold", "To run", InputControlIcons.Control.Run));
        StartCoroutine(HideTooltipAfterDelay(4f));
    }

    private IEnumerator TransitionToTutorialCameraCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        TransitionToTutorialCamera();
    }

    public void TransitionToCombatCamera() {
        CameraManager.Instance.ZoomOut(false, .7f, 2f);
    }

    public void TransitionToTutorialCamera() {
        CameraManager.Instance.ZoomOut(false, 1.3f, 2f);
    }
}
