using System.Collections;
using UnityEngine;

public class GunFeedbacks_Minigun : GunFeedbacks {
    [SerializeField] private Gun_Minigun minigun;
    [SerializeField] private ParticleSystem smokePS;
    [SerializeField] private float maxEmissionRate = 30f;
    [SerializeField] private float emissionChangeSpeed = 20f; // Vitesse à laquelle ça monte/descend

    private ParticleSystem.EmissionModule smokeEmission;
    private Coroutine emissionCoroutine;

    protected override void Start() {
        base.Start();
        smokeEmission = smokePS.emission;

        minigun.OnMinigunConsumeAmmoWhileSpinning += Minigun_OnMinigunConsumeAmmoWhileSpinning;
        minigun.OnMinigunStartedSpinning += Minigun_OnMinigunStartedSpinning;
        minigun.OnMinigunStoppedSpinning += Minigun_OnMinigunStoppedSpinning;
    }

    private void Minigun_OnMinigunStartedSpinning(object sender, System.EventArgs e) {
        smokePS.Play();

        if (emissionCoroutine != null)
            StopCoroutine(emissionCoroutine);
        emissionCoroutine = StartCoroutine(ChangeEmissionRate(smokeEmission.rateOverTime.constant, maxEmissionRate));
    }

    private void Minigun_OnMinigunStoppedSpinning(object sender, System.EventArgs e) {
        if (emissionCoroutine != null)
            StopCoroutine(emissionCoroutine);
        emissionCoroutine = StartCoroutine(ChangeEmissionRate(smokeEmission.rateOverTime.constant, 0f));
    }

    private void Minigun_OnMinigunConsumeAmmoWhileSpinning(object sender, System.EventArgs e) {
        shellOutPS.Emit(1);
    }

    private IEnumerator ChangeEmissionRate(float from, float to) {
        float current = from;
        while (!Mathf.Approximately(current, to)) {
            current = Mathf.MoveTowards(current, to, emissionChangeSpeed * Time.deltaTime);
            var rate = smokeEmission.rateOverTime;
            rate.constant = current;
            smokeEmission.rateOverTime = rate;
            yield return null;
        }

        // Stop système si taux à 0
        if (Mathf.Approximately(to, 0f)) {
            smokePS.Stop();
        }
    }
}
