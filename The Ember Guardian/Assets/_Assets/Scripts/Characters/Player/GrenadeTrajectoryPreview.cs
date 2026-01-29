using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeTrajectoryPreview : MonoBehaviour
{
    [SerializeField] private Gun gun;
    public LineRenderer lineRenderer;
    public int segmentCount = 50;           // nombre de points sur la trajectoire
    public float timeStep = 0.02f;          // incrément de temps pour la simulation
    public float maxSimulationTime = 5f;    // durée max de simulation
    public int maxBounces = 2;              // rebonds max à simuler

    // Paramètres du Rigidbody2D du projectile
    public float gravityScale = 2.5f;
    public float linearDrag = 1f;
    public float bounciness = 0.5f;
    public float mass = 1f;

    private float fadeDuration = 0.3f;
    private Coroutine fadeCoroutine;
    private Gradient baseGradient;

    private void Awake() {
        baseGradient = lineRenderer.colorGradient;
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerReload += Instance_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadEnded += Instance_OnPlayerReloadEnded;
        PlayerShoot.Instance.OnPlayerSwappedGunStarted += Instance_OnPlayerSwappedGunStarted;
        PlayerShoot.Instance.OnPlayerSwappedGunEnded += Instance_OnPlayerSwappedGunEnded;
        PlayerMovement.Instance.OnPlayerRoll += Instance_OnPlayerRoll;
        PlayerMovement.Instance.OnPlayerRollEnded += Instance_OnPlayerRollEnded;
    }

    private void Instance_OnPlayerRollEnded(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        ShowLine();
    }

    private void Instance_OnPlayerRoll(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        HideLine();
    }

    private void Instance_OnPlayerSwappedGunEnded(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        ShowLine();
    }

    private void Instance_OnPlayerSwappedGunStarted(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        HideLine();
    }

    private void Instance_OnPlayerReloadEnded(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        ShowLine();
    }

    private void Instance_OnPlayerReload(object sender, PlayerShoot.OnPlayerReloadEventArgs e) {
        if (PlayerShoot.Instance.GetHeldGun() != gun) return;
        HideLine();
    }

    private void Update() {
        float loadingShotTimer = PlayerShoot.Instance.GetLoadingShotTimerNormalized(); // 0 -> 1
        float minLoadShotForceNormalized = PlayerShoot.Instance.GetHeldGunSO().minLoadShotForceNormalized;

        // Remapper pour que 0 -> minForce, 1 -> 1
        float loadingShotMultiplier = Mathf.Lerp(minLoadShotForceNormalized, 1f, loadingShotTimer);

        Vector2 initialForce = loadingShotMultiplier * PlayerAim.Instance.GetEffectiveAimDir().normalized * PlayerShoot.Instance.GetHeldGun().GetBulletSpeed();
        DrawTrajectory(lineRenderer.transform.position, initialForce);
    }

    public void DrawTrajectory(Vector2 startPosition, Vector2 initialVelocity) {
        List<Vector3> points = new List<Vector3>();
        Vector2 pos = startPosition;
        Vector2 vel = initialVelocity;

        points.Add(pos);

        int bounces = 0;
        float dt = timeStep;
        float elapsed = 0f;

        while (elapsed < maxSimulationTime && points.Count < segmentCount) {
            // Gravité simulée
            vel += Physics2D.gravity * gravityScale * dt;

            // Drag linéaire
            vel *= Mathf.Max(0f, 1f - linearDrag * dt);

            // Déplacement
            Vector2 nextPos = pos + vel * dt;

            // Check collision simple avec "sol" (y = 0)
            if (nextPos.y <= 0f) {
                bounces++;
                if (bounces > maxBounces)
                    break;

                // Inverser la vitesse verticale et appliquer bounciness
                vel.y = -vel.y * bounciness;

                // Appliquer friction simplifiée (réduction vitesse horizontale)
                vel.x *= (1f - 0.4f);

                // Poser le point sur le sol
                nextPos.y = 0f;
            }

            points.Add(nextPos);
            pos = nextPos;
            elapsed += dt;
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void ShowLine() {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeLine(1f));
    }

    public void HideLine() {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeLine(0f));
    }

    private IEnumerator FadeLine(float targetAlpha) {
        Gradient startGradient = lineRenderer.colorGradient;
        float timer = 0f;

        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            float alpha = Mathf.Lerp(GetGradientAlpha(startGradient), targetAlpha, t);
            SetGradientAlpha(alpha);
            yield return null;
        }

        SetGradientAlpha(targetAlpha);
        fadeCoroutine = null;
    }

    private void SetGradientAlpha(float alpha) {
        Gradient g = new Gradient();
        GradientColorKey[] colorKeys = baseGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorKeys.Length];

        for (int i = 0; i < colorKeys.Length; i++) {
            alphaKeys[i] = new GradientAlphaKey(alpha * baseGradient.Evaluate(colorKeys[i].time).a, colorKeys[i].time);
        }

        g.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = g;
    }

    private float GetGradientAlpha(Gradient g) {
        // Moyenne des alpha sur les clés
        float sum = 0f;
        foreach (var aKey in g.alphaKeys) {
            sum += aKey.alpha;
        }
        return sum / g.alphaKeys.Length;
    }
}
