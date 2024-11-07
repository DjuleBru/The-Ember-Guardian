using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual : MonoBehaviour
{
    private GunSO gunSO;
    [SerializeField] private SpriteRenderer gunLightsSpriteRenderer;
    private List<Sprite> gunReloadSprites;

    private void Start() {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;

        PlayerShoot.Instance.OnClipsChanged += PlayerShoot_OnClipsChanged;

        gunSO = PlayerShoot.Instance.GetGunSO();
        gunReloadSprites = gunSO.shotCountSprites;
    }

    private void PlayerShoot_OnClipsChanged(object sender, System.EventArgs e) {
        float clipsAmountNormalized = (float)PlayerShoot.Instance.GetCurrentClips()/ (float)PlayerShoot.Instance.GetMaxClips();

        int currentGunReloadSpriteIndex = Mathf.RoundToInt(clipsAmountNormalized * gunReloadSprites.Count);

        gunLightsSpriteRenderer.sprite = gunReloadSprites[currentGunReloadSpriteIndex ];
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        gameObject.SetActive(true);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }
}
