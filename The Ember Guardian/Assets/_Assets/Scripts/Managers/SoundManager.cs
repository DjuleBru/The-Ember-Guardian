using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundRefsSO soundRefsSO;
    
    private AudioSource audioSource2D;

    private void Awake() {
        Instance = this;
        audioSource2D = GetComponent<AudioSource>();
        audioSource2D.spatialBlend = 0;
    }

    private void Start() {
        StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
        Structure.OnAnyStructureUpgraded += Structure_OnAnyStructureUpgraded;

        PlayerShoot.Instance.OnPlayerShotProjectile += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerCooldownTrigger += PlayerShoor_OnPlayerCooldownSFXTrigger;
        ParticleCollision.OnAnyBulletHitEnemy += ParticleCollision_OnAnyBulletHitEnemy;
        ParticleCollision.OnAnyBulletHitGround += ParticleCollision_OnAnyBulletHitGround;

        Collectible.OnAnyCollectibleTouchedFloor += Collectible_OnAnyCollectibleTouchedFloor;

        PlayerCurrencies.Instance.OnBlueOrbDroppedOnTheFloor += PlayerCurrencies_OnBlueOrbDroppedOnTheFloor;
    }

    #region CURRENCIES

    private void Collectible_OnAnyCollectibleTouchedFloor(object sender, System.EventArgs e) {
        Collectible collectible = (Collectible)sender;

        if(collectible.GetCurrencyType() == PlayerCurrencies.CurrencyType.blueOrb) {
            PlaySound3D(soundRefsSO.orbTouchedFloor, (sender as MonoBehaviour).transform.position);
        }
    }

    private void PlayerCurrencies_OnBlueOrbDroppedOnTheFloor(object sender, PlayerCurrencies.OnBlueOrbDroppedOnTheFloorEventArgs e) {
        PlaySound3D(soundRefsSO.orbDropped, (sender as MonoBehaviour).transform.position);
    }

    #endregion

    #region SHOOTING
    private void ParticleCollision_OnAnyBulletHitGround(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().bulletHitGroundSound;
        PlaySound3D(audioClipArray, (sender as MonoBehaviour).transform.position, .5f);
    }

    private void ParticleCollision_OnAnyBulletHitEnemy(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().bulletHitEnemySound;
        PlaySound3D(audioClipArray, (sender as MonoBehaviour).transform.position, .5f);
    }

    private void PlayerShoor_OnPlayerCooldownSFXTrigger(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().cooldownGunSound;
        PlaySound2D(audioClipArray);
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().reloadGunSound;
        PlaySound2D(audioClipArray);
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        AudioClip[] audioClipArray = PlayerShoot.Instance.GetGunSO().shootGunSound;
        PlaySound2D(audioClipArray);
    }

    #endregion

    #region STRUCTURES
    private void Structure_OnAnyStructureUpgraded(object sender, System.EventArgs e) {
        AudioClip audioClip = (sender as Structure).GetStructureSO().upgradeAudioClip;
        PlaySound2D(audioClip);
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, System.EventArgs e) {
        AudioClip audioClip = (sender as StructureLocation).GetStructureSOToBuild().buildAudioClip;
        PlaySound2D(audioClip);
    }

    #endregion

    #region PLAY SOUNDS

    private void PlaySound3D(AudioClip[] audioClipArray, Vector3 position, float volume = 1f) {
        PlaySound3D(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound3D(AudioClip audioClip, Vector3 position, float volume = 1f) {
        Vector3 newPosition = new Vector3(position.x, position.y, Camera.main.transform.position.z);
        AudioSource.PlayClipAtPoint(audioClip, newPosition, volume);
    }

    private void PlaySound2D(AudioClip[] audioClipArray, float volume = 1f) {
        PlaySound2D(audioClipArray[Random.Range(0, audioClipArray.Length)], volume);
    }

    private void PlaySound2D(AudioClip audioClip, float volume = 1f) {
        audioSource2D.PlayOneShot(audioClip, volume);
    }

    #endregion

}
