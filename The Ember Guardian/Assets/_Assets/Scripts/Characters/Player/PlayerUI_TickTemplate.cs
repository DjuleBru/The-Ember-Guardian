using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_TickTemplate : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Image image;
    [SerializeField] private MMF_Player outMmfPlayer;
    [SerializeField] private MMF_Player outMmfPlayer_surgeReload;
    [SerializeField] private MMF_Player inMmfPlayer;
    [SerializeField] private Animator shineBulletAnimator;

    public static event EventHandler OnAnyBulletPingShine;
    public static event EventHandler OnAnyBulletPingShineDropped;
    public static event EventHandler OnAnyBulletPingShineReachedGun;

    private bool isSpinningBullet;
    private bool isSpinningBulletSuccess;
    private bool isMoving;

    private float acceleration = 100f;  // accélération
    private float maxSpeed = 50f;       // vitesse max
    private float currentSpeed = 20f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        PlayerShoot.Instance.OnSpinningBulletSuccess += PlayerShoot_OnSpinningBulletSuccess;
    }

    private void Update() {
        if (isMoving) {

            // on accélère
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            // direction normalisée vers la cible
            Vector3 dir = (PlayerShoot.Instance.GetHeldGun().transform.position - transform.position).normalized;

            // déplacement
            transform.position += dir * currentSpeed * Time.deltaTime;

            // check arrivée
            if (Vector3.Distance(transform.position, PlayerShoot.Instance.GetHeldGun().transform.position) < 0.25f) {
                OnAnyBulletPingShineReachedGun?.Invoke(this, EventArgs.Empty);
                PlayerShoot.Instance.GetHeldGun().ApplySurgeWindowBuff();
                Destroy(gameObject);
            }

        }
    }

    private void PlayerShoot_OnSpinningBulletSuccess(object sender, EventArgs e) {
        if (!isSpinningBullet) return;

        isSpinningBulletSuccess = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

    }

    public void AddTick() {
        inMmfPlayer.PlayFeedbacks();
    }

    public void RemoveTick(float forceYMultiplier = 1f, bool addForce = true, float torqueMultiplier = 1f, float forceXMultiplier = 0f, bool surgeReload = false, bool randomizeYForce = true, float YForceRandomizer = 1.5f, bool randomizeTorque = true) {
        rb = GetComponent<Rigidbody2D>();

        if(addForce) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.5f;

            float yForce = 6.5f;
            if(randomizeYForce) {
                yForce += UnityEngine.Random.Range(-YForceRandomizer, YForceRandomizer);
            }

            Vector2 force = new Vector2(UnityEngine.Random.Range(-8, 8) * forceXMultiplier, yForce * forceYMultiplier);

            float torque = 2f;
            if (randomizeTorque) {
                torque = UnityEngine.Random.Range(-2f * torqueMultiplier, 2f * torqueMultiplier);
            }

            if (surgeReload) {
                isSpinningBullet = true;
                rb.gravityScale = 1.25f;
                shineBulletAnimator.enabled = true;
                outMmfPlayer_surgeReload.PlayFeedbacks();
                StartCoroutine(SurgeReloadCoroutine(.8f, 100f, .3f, .1f));
                torque *= 10f;
            }
            else {
                outMmfPlayer.PlayFeedbacks();
            }

            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(torque, ForceMode2D.Impulse);
        }


        if(gameObject.activeInHierarchy) {
            StartCoroutine(DestroyGameObjectAfterDelay(2f));
        } else {
            Destroy(gameObject);
        }
    }

    private IEnumerator SurgeReloadCoroutine(float initialDelay,  float drag, float surgeWindowDelay, float speedReductionFactor) {
        OnAnyBulletPingShineDropped?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(initialDelay);

        // court ralentissement
        float originalGravity = rb.gravityScale;
        float originalAngularDrag = rb.angularDrag;
        float originalDrag = rb.drag;

        rb.velocity = rb.velocity * speedReductionFactor;  // on coupe la vitesse
        rb.angularVelocity = rb.angularVelocity * speedReductionFactor;

        //rb.gravityScale = gravityScale;      // réduit la chute
        rb.drag = drag;                // amortit le déplacement
        rb.angularDrag = drag;         // amortit la rotation

        OnAnyBulletPingShine?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(surgeWindowDelay); // le "slow" dure un court instant

        // on remet les valeurs normales
        rb.gravityScale = originalGravity;
        rb.drag = originalDrag;
        rb.angularDrag = originalAngularDrag;

        if(isSpinningBulletSuccess) {
            isMoving = true;
        }
    }

    private IEnumerator DestroyGameObjectAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void SetImageAlphaFull() {
        image.color = Color.white;
    }

    public void SetImageFill(float fillAmount) {
        image.fillAmount = fillAmount;
    }

    public void SetImageColor(Color color) {
        image.color = color;
    }

    public void SetImageSprite(Sprite sprite) {
        image.sprite = sprite;
    }

    public void StopInFeedbacks() {
        inMmfPlayer.StopFeedbacks();
    }

    private void OnDestroy() {
        PlayerShoot.Instance.OnSpinningBulletSuccess -= PlayerShoot_OnSpinningBulletSuccess;
    }

}
