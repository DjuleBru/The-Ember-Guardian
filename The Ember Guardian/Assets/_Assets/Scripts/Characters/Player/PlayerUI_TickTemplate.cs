using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_TickTemplate : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Image image;
    [SerializeField] private MMF_Player outMmfPlayer;
    [SerializeField] private MMF_Player inMmfPlayer;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void AddTick() {
        inMmfPlayer.PlayFeedbacks();
    }

    public void RemoveTick(float forceMultiplier = 1f, bool addForce = true) {
        rb = GetComponent<Rigidbody2D>();

        if(addForce) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.5f;
            Vector2 force = new Vector2(0, Random.Range(5, 8) * forceMultiplier);
            float torque = Random.Range(-2f, 2f);

            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(torque, ForceMode2D.Impulse);
        }

        outMmfPlayer.PlayFeedbacks();

        if(gameObject.activeInHierarchy) {
            StartCoroutine(DestroyGameObjectAfterDelay(1f));
        } else {
            Destroy(gameObject);
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

}
