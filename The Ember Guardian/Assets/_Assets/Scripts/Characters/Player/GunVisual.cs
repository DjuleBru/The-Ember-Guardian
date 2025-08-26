using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual : MonoBehaviour
{
    protected Gun gun;
    protected GunJamHandler gunJamHandler;
    protected GunSO gunSO;
    [SerializeField] protected GameObject gunVisualGameObject;
    [SerializeField] protected GameObject armGameObject;
    [SerializeField] protected SpriteRenderer gunSecondaryAbilityActiveSpriteRenderer;
    [SerializeField] protected SpriteRenderer gunLightsSpriteRenderer;
    [SerializeField] protected SpriteRenderer gunCooldownLightsSpriteRenderer;
    [SerializeField] protected Color outOfAmmoCooldownLightsColor;
    [SerializeField] protected Material initalLightsSpriteRendererMaterial;
    [SerializeField] protected Material weaponSurgeLightsSpriteRendererMaterial;

    protected float tryShootOutOfAmmoAnimationDuration = .3f;
    protected Color initialLightsColor;
    protected Color cooldownLightsColor;
    protected List<Sprite> gunReloadSprites;
    protected int gunLightSpriteIndex;
    protected bool gunSecondaryFireModeActive;

    protected virtual void Awake() {
        gun = GetComponent<Gun>();
        gunJamHandler = GetComponent<GunJamHandler>();
        initialLightsColor = gunLightsSpriteRenderer.color;

        if (gunCooldownLightsSpriteRenderer != null) {
            cooldownLightsColor = gunCooldownLightsSpriteRenderer.color;
        }

        if (gunSecondaryAbilityActiveSpriteRenderer != null) {
            gunSecondaryAbilityActiveSpriteRenderer.enabled = false;
        }
    }

    protected virtual void Start() {
        gun.OnGunJammed += Gun_OnGunJammed;
        gunJamHandler.OnJamSequenceCompleted += GunJamHandler_OnJamSequenceCompleted;
        gun.OnPerfectQTEDamageBuff += Gun_OnPerfectQTEDamageBuff;
        gun.OnPerfectQTEDamageBuffEnded += Gun_OnPerfectQTEDamageBuffEnded;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;

        PlayerShoot.Instance.OnBulletsChanged += PlayerShoot_OnClipsChanged;
        PlayerShoot.Instance.OnPlayerReloadHandEnded += PlayerShoot_OnPlayerReloadHandEnded;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerShoot_OnPlayerSwitchedFireMode;

        gunSO = gun.GetGunSO();
        gunReloadSprites = gunSO.shotCountSprites;
        gunLightSpriteIndex = gunSO.shotCountSprites.Count -1;
        if(gunSecondaryAbilityActiveSpriteRenderer != null) {
            gunSecondaryAbilityActiveSpriteRenderer.enabled = false;
        }
    }

    private void Gun_OnPerfectQTEDamageBuffEnded(object sender, System.EventArgs e) {
        gunLightsSpriteRenderer.material = initalLightsSpriteRendererMaterial;
    }

    private void Gun_OnPerfectQTEDamageBuff(object sender, System.EventArgs e) {
        gunLightsSpriteRenderer.material = weaponSurgeLightsSpriteRendererMaterial;
    }

    private void GunJamHandler_OnJamSequenceCompleted(object sender, System.EventArgs e) {
        StartCoroutine(ReloadBulletsVisual(.05f, 0, gunLightSpriteIndex));
    }

    private void Gun_OnGunJammed(object sender, System.EventArgs e) {
        Sprite currentSprite = gunReloadSprites[0];
        StopCoroutine(ResetGunAmmoSprite(currentSprite));
        StartCoroutine(ResetGunAmmoSprite(currentSprite));
    }

    private void PlayerShoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        if(gunSecondaryAbilityActiveSpriteRenderer != null) {
            gunSecondaryFireModeActive = !gunSecondaryFireModeActive;
            gunSecondaryAbilityActiveSpriteRenderer.enabled = gunSecondaryFireModeActive;
        }
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        if (gunCooldownLightsSpriteRenderer == null) return;
        int bulletAmount = PlayerShoot.Instance.GetCurrentBullets();
        if(bulletAmount == 0) {
            gunCooldownLightsSpriteRenderer.color = outOfAmmoCooldownLightsColor;
        }
    }

    protected void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        if (!gameObject.activeInHierarchy) return;

        float bulletsAmountNormalized = (float)PlayerShoot.Instance.GetCurrentBullets() / (float)PlayerShoot.Instance.GetMaxBulletsPerClip();
        if(bulletsAmountNormalized < 0) {
            bulletsAmountNormalized = 0;
        }
        int reloadSpriteIndex = Mathf.RoundToInt(bulletsAmountNormalized * gunReloadSprites.Count);
        Sprite currentSprite = gunReloadSprites[reloadSpriteIndex];

        StopCoroutine(ResetGunAmmoSprite(currentSprite));
        StartCoroutine(ResetGunAmmoSprite(currentSprite));

        if(PlayerShoot.Instance.GetCurrentBullets() == 0) {
            gunLightsSpriteRenderer.sprite = gunReloadSprites[gunSO.shotCountSprites.Count - 1];
        }
    }

    protected IEnumerator ResetGunAmmoSprite(Sprite sprite) {
        yield return new WaitForSeconds(tryShootOutOfAmmoAnimationDuration);
        gunLightsSpriteRenderer.sprite = sprite;
    }

    private void PlayerShoot_OnPlayerReloadHandEnded(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        if (gunCooldownLightsSpriteRenderer != null) {
            gunCooldownLightsSpriteRenderer.color = cooldownLightsColor;
        };

        gunLightsSpriteRenderer.color = initialLightsColor;

        int finalGunReloadSpriteIndex = gunReloadSprites.Count;
        float reloadTime = PlayerStats.Instance.GetReloadTime();
        float delayBetweenSprites = (reloadTime / (float)Mathf.Abs(finalGunReloadSpriteIndex - gunLightSpriteIndex) / 3);

        StartCoroutine(ReloadBulletsVisual(delayBetweenSprites, gunLightSpriteIndex, finalGunReloadSpriteIndex));
    }

    protected void PlayerShoot_OnClipsChanged(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        float bulletsAmountNormalized = (float)PlayerShoot.Instance.GetCurrentBullets()/ (float)PlayerShoot.Instance.GetMaxBulletsPerClip();

        if(bulletsAmountNormalized == .5f) {
            bulletsAmountNormalized = .49f;
        }

        int finalGunReloadSpriteIndex = Mathf.RoundToInt(bulletsAmountNormalized * gunReloadSprites.Count);
        float delayBetweenSprites = PlayerStats.Instance.GetShootCooldownTime() / (float)Mathf.Abs(finalGunReloadSpriteIndex - gunLightSpriteIndex);

        StartCoroutine(ChangeRemainingBulletsVisuals(delayBetweenSprites, gunLightSpriteIndex, finalGunReloadSpriteIndex));
    }

    protected IEnumerator ChangeRemainingBulletsVisuals(float delayBetweenSprites, int initialSpriteIndex, int finalSpriteIndex) {
        if (!gun.GetGunActive()) yield return null;
        gunLightsSpriteRenderer.sprite = gunReloadSprites[initialSpriteIndex];

        // Vérifie que les indices sont dans les limites du tableau
        if (initialSpriteIndex < 0) initialSpriteIndex = 0;
        if (initialSpriteIndex >= gunReloadSprites.Count) initialSpriteIndex = gunReloadSprites.Count - 1;
        if (finalSpriteIndex < 0) finalSpriteIndex = 0;
        if (finalSpriteIndex >= gunReloadSprites.Count) finalSpriteIndex = gunReloadSprites.Count - 1;

        gunLightsSpriteRenderer.sprite = gunReloadSprites[initialSpriteIndex];

        if (finalSpriteIndex < initialSpriteIndex) {
            // Shooting: on descend les index
            for (int i = initialSpriteIndex - 1; i >= finalSpriteIndex; i--) {
                gunLightsSpriteRenderer.sprite = gunReloadSprites[i];
                yield return new WaitForSeconds(delayBetweenSprites);
                gunLightSpriteIndex = i;
            }
        }
        else {
            
        }
    }

    protected IEnumerator ReloadBulletsVisual(float delayBetweenSprites, int initialSpriteIndex, int finalSpriteIndex) {
        if (!gun.GetGunActive()) yield return null;
        gunLightsSpriteRenderer.sprite = gunReloadSprites[initialSpriteIndex];

        // Vérifie que les indices sont dans les limites du tableau
        if (initialSpriteIndex < 0) initialSpriteIndex = 0;
        if (initialSpriteIndex >= gunReloadSprites.Count) initialSpriteIndex = gunReloadSprites.Count - 1;
        if (finalSpriteIndex < 0) finalSpriteIndex = 0;
        if (finalSpriteIndex >= gunReloadSprites.Count) finalSpriteIndex = gunReloadSprites.Count - 1;
        
        // Reloading: on monte les index
        for (int i = initialSpriteIndex + 1; i <= finalSpriteIndex; i++) {
            gunLightsSpriteRenderer.sprite = gunReloadSprites[i];
            yield return new WaitForSeconds(delayBetweenSprites);
            gunLightSpriteIndex = i;
        }
    }

    protected void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        gunVisualGameObject.SetActive(true);
        armGameObject.SetActive(true);
    }

    protected void Player_OnPlayerDied(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        gunVisualGameObject.SetActive(false);
        armGameObject.SetActive(false);
    }
}
