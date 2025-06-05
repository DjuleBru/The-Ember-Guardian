using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunJammedPS : MonoBehaviour
{
    [SerializeField] private ParticleSystem gunJammedPS;
    [SerializeField] private ParticleSystem gunJamHitPS;
    [SerializeField] private ParticleSystem gunJamTryShootPS;

    private void Start() {
        Gun.OnAnyGunJammed += Gun_OnAnyGunJammed;
        GunJamHandler.OnAnyJamSequenceProgressed += GunJamHandler_OnAnyJamSequenceProgressed;
        GunJamHandler.OnAnyJamSequenceFailed += GunJamHandler_OnAnyJamSequenceFailed;
        PlayerShoot.Instance.OnPlayerTryShoot_GunJammed += PlayerShoot_OnPlayerTryShoot_GunJammed;
    }

    private void GunJamHandler_OnAnyJamSequenceFailed(object sender, System.EventArgs e) {
        gunJammedPS.Play();
    }

    private void GunJamHandler_OnAnyJamSequenceProgressed(object sender, System.EventArgs e) {
        gunJamHitPS.Play();
    }

    private void Gun_OnAnyGunJammed(object sender, System.EventArgs e) {
        StartCoroutine(StartGunJamPSSequence());
    }

    private IEnumerator StartGunJamPSSequence() {
        gunJamTryShootPS.Play();
        yield return new WaitForSeconds(.2f);
        gunJammedPS.Play();
        yield return new WaitForSeconds(.2f);
        gunJamTryShootPS.Play();
    }

    private void PlayerShoot_OnPlayerTryShoot_GunJammed(object sender, System.EventArgs e) {
        gunJamTryShootPS.Play();
    }

    private void OnDestroy() {
        Gun.OnAnyGunJammed -= Gun_OnAnyGunJammed;
        GunJamHandler.OnAnyJamSequenceProgressed -= GunJamHandler_OnAnyJamSequenceProgressed;
        GunJamHandler.OnAnyJamSequenceFailed -= GunJamHandler_OnAnyJamSequenceFailed;
    }
}
