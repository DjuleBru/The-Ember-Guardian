using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_TickTemplate : MonoBehaviour
{
    private Rigidbody2D rb;
    private Image image;
    [SerializeField] private MMF_Player outMmfPlayer;
    [SerializeField] private MMF_Player inMmfPlayer;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        image = GetComponent<Image>();
    }

    public void AddTick() {
        inMmfPlayer.PlayFeedbacks();
    }

    public void RemoveTick() {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1.5f;

        Vector2 force = new Vector2(0, Random.Range(5, 8));
        float torque = Random.Range(-2f, 2f);

        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(torque, ForceMode2D.Impulse);

        outMmfPlayer.PlayFeedbacks();

        StartCoroutine(DestroyGameObjectAfterDelay(1f));
    }

    private IEnumerator DestroyGameObjectAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void SetImageAlphaFull() {
        image.color = Color.white;
    }
}
