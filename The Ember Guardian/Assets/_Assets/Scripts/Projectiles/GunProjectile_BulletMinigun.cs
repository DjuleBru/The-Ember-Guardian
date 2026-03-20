using UnityEngine;

public class GunProjectile_BulletMinigun : GunProjectile_Bullet {
    private bool useNoisyTrajectory;

    private float noiseSeed;
    private float noiseTime;

    [SerializeField] private float noiseFrequency = 8f;
    [SerializeField] private float noiseAmplitude = 10f; // en degrés

    protected void Start() {
        if (PlayerShoot.Instance.GetMinigunExplosiveBulletsActive()) {
            explodeOnContact = true;
            useNoisyTrajectory = true;

            noiseSeed = Random.Range(0f, 1000f);
        }
    }

    protected override void Update() {
        base.Update();

        if (!useNoisyTrajectory) {
            return;
        }

        ApplyNoiseTrajectory();
    }

    private void ApplyNoiseTrajectory() {
        noiseTime += Time.deltaTime * noiseFrequency;

        float noise = Mathf.PerlinNoise(noiseSeed, noiseTime);
        noise = (noise - 0.5f) * 2f; // [-1, 1]

        float angleOffset = noise * noiseAmplitude;

        Vector2 velocity = rb.velocity;

        if (velocity.sqrMagnitude <= 0.001f) {
            return;
        }

        float currentAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        float newAngle = currentAngle + angleOffset;

        float speed = velocity.magnitude;

        Vector2 newDir = new Vector2(
            Mathf.Cos(newAngle * Mathf.Deg2Rad),
            Mathf.Sin(newAngle * Mathf.Deg2Rad)
        );

        rb.velocity = newDir * speed;
    }
}