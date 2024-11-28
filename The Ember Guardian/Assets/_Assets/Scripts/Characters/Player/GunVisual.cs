using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual : MonoBehaviour
{
    private GunSO gunSO;
    [SerializeField] private SpriteRenderer gunLightsSpriteRenderer;
    [SerializeField] private Transform gunSportLightTransform;

    private List<Sprite> gunReloadSprites;
    private int gunLightSpriteIndex;

    private void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;

        PlayerShoot.Instance.OnBulletsChanged += PlayerShoot_OnClipsChanged;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;

        PlayerAim.Instance.OnXAimDirChanged += PlayerAim_OnXAimDirChanged;

        gunSO = PlayerShoot.Instance.GetGunSO();
        gunReloadSprites = gunSO.shotCountSprites;
        gunLightSpriteIndex = gunSO.shotCountSprites.Count -1;
    }


    private void PlayerAim_OnXAimDirChanged(object sender, System.EventArgs e) {
        Vector3 scale = new Vector3(1, 1, 1);
        if(PlayerAim.Instance.GetAimDir().x < 0) {
            //scale = new Vector3(-1, -1, 1);
        }
        gunSportLightTransform.localScale = scale;
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        int finalGunReloadSpriteIndex = gunReloadSprites.Count;

        float delayBetweenSprites = PlayerStats.Instance.GetReloadTime() / (float)Mathf.Abs(finalGunReloadSpriteIndex - gunLightSpriteIndex);


        StartCoroutine(ChangeRemainingBulletsVisuals(delayBetweenSprites, gunLightSpriteIndex, finalGunReloadSpriteIndex));

    }

    private void PlayerShoot_OnClipsChanged(object sender, System.EventArgs e) {
        float bulletsAmountNormalized = (float)PlayerShoot.Instance.GetCurrentBullets()/ (float)PlayerShoot.Instance.GetMaxBulletsPerClip();

        int finalGunReloadSpriteIndex = Mathf.RoundToInt(bulletsAmountNormalized * gunReloadSprites.Count);
        float delayBetweenSprites = PlayerStats.Instance.GetShootCooldownTime() / (float)Mathf.Abs(finalGunReloadSpriteIndex - gunLightSpriteIndex);


        StartCoroutine(ChangeRemainingBulletsVisuals(delayBetweenSprites, gunLightSpriteIndex, finalGunReloadSpriteIndex));

    }

    private IEnumerator ChangeRemainingBulletsVisuals(float delayBetweenSprites, int initialSpriteIndex, int finalSpriteIndex) {
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
            // Reloading: on monte les index
            for (int i = initialSpriteIndex + 1; i <= finalSpriteIndex; i++) {
                gunLightsSpriteRenderer.sprite = gunReloadSprites[i];
                yield return new WaitForSeconds(delayBetweenSprites);
                gunLightSpriteIndex = i;
            }
        }
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        gameObject.SetActive(true);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }
}
