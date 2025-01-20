using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual : MonoBehaviour
{
    protected Gun gun;
    protected GunSO gunSO;
    [SerializeField] protected SpriteRenderer gunLightsSpriteRenderer;
    [SerializeField] protected SpriteRenderer gunCooldownLightsSpriteRenderer;
    [SerializeField] protected Color outOfAmmoCooldownLightsColor;

    protected float tryShootOutOfAmmoAnimationDuration = .3f;
    protected Color cooldownLightsColor;
    protected List<Sprite> gunReloadSprites;
    protected int gunLightSpriteIndex;

    protected virtual void Awake() {
        gun = GetComponent<Gun>();

        if (gunCooldownLightsSpriteRenderer != null) {
            cooldownLightsColor = gunCooldownLightsSpriteRenderer.color;
        }
    }

    protected virtual void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;

        PlayerShoot.Instance.OnBulletsChanged += PlayerShoot_OnClipsChanged;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerTryShoot_OutOfAmmo += PlayerShoot_OnPlayerTryShoot_OutOfAmmo;

        gunSO = gun.GetGunSO();
        gunReloadSprites = gunSO.shotCountSprites;
        gunLightSpriteIndex = gunSO.shotCountSprites.Count -1;
    }

    protected void PlayerShoot_OnPlayerTryShoot_OutOfAmmo(object sender, System.EventArgs e) {
        if (!gameObject.activeInHierarchy) return;

        float bulletsAmountNormalized = (float)PlayerShoot.Instance.GetCurrentBullets() / (float)PlayerShoot.Instance.GetMaxBulletsPerClip();
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

    protected void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        int finalGunReloadSpriteIndex = gunReloadSprites.Count;
        float reloadTime = PlayerStats.Instance.GetReloadTime();
        float delayBetweenSprites = (reloadTime / (float)Mathf.Abs(finalGunReloadSpriteIndex - gunLightSpriteIndex)/3);

        StartCoroutine(ReloadBulletsVisual(reloadTime, delayBetweenSprites, gunLightSpriteIndex, finalGunReloadSpriteIndex));

    }

    protected void PlayerShoot_OnClipsChanged(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        float bulletsAmountNormalized = (float)PlayerShoot.Instance.GetCurrentBullets()/ (float)PlayerShoot.Instance.GetMaxBulletsPerClip();

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

    protected IEnumerator ReloadBulletsVisual(float reloadTime, float delayBetweenSprites, int initialSpriteIndex, int finalSpriteIndex) {
        if (!gun.GetGunActive()) yield return null;
        gunLightsSpriteRenderer.sprite = gunReloadSprites[initialSpriteIndex];

        // Vérifie que les indices sont dans les limites du tableau
        if (initialSpriteIndex < 0) initialSpriteIndex = 0;
        if (initialSpriteIndex >= gunReloadSprites.Count) initialSpriteIndex = gunReloadSprites.Count - 1;
        if (finalSpriteIndex < 0) finalSpriteIndex = 0;
        if (finalSpriteIndex >= gunReloadSprites.Count) finalSpriteIndex = gunReloadSprites.Count - 1;

        yield return new WaitForSeconds(reloadTime/3*2);
        
        // Reloading: on monte les index
        for (int i = initialSpriteIndex + 1; i <= finalSpriteIndex; i++) {
            gunLightsSpriteRenderer.sprite = gunReloadSprites[i];
            yield return new WaitForSeconds(delayBetweenSprites);
            gunLightSpriteIndex = i;
        }
    }

    protected void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        gameObject.SetActive(true);
    }

    protected void Player_OnPlayerDied(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;
        gameObject.SetActive(false);
    }
}
