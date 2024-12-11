using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadePiece : MonoBehaviour {

    [SerializeField] private Material unhoveredMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Material repairBarricadeMaterial;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 initialPosition;

    private void Awake() {
        initialPosition = transform.position;
        DisableBarricadePiece();
    }

    public void DisableBarricadePiece() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        spriteRenderer.enabled = false;
        animator.enabled = false;
    }

    public void EnableBarricadePiece() {
        gameObject.SetActive(true);
        initialPosition = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;
        spriteRenderer.enabled = true;
        animator.enabled = true;
    }

    public void BarricadePieceFell() {
        Vector2 force = new Vector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(2, 4));
        float torque = UnityEngine.Random.Range(-2, 2);

        rb.gravityScale = 1.5f;
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(torque, ForceMode2D.Impulse);
        rb.bodyType = RigidbodyType2D.Dynamic;


        StartCoroutine(DeactivateBarricadeSpriteAfterDelay());
    }

    public void ShowBarricadePieceRepairable() {
        Debug.Log("ShowRepair");
        EnableBarricadePiece();
        spriteRenderer.material = repairBarricadeMaterial;
        animator.SetTrigger("ShowRepair");
    }

    public void BarricadePieceDamaged() {
        animator.SetTrigger("Damaged");
    }

    public void BarricadePieceBuilt() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;

        animator.ResetTrigger("Damaged");
        animator.ResetTrigger("ShowRepair");
        transform.position = initialPosition;
        animator.SetTrigger("Build");
    }
    private IEnumerator DeactivateBarricadeSpriteAfterDelay() {
        yield return new WaitForSeconds(2f);
        DisableBarricadePiece();
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;
    }

    public void SetHovered(bool isHovered) {
        if(isHovered) {
            spriteRenderer.material = hoveredMaterial;
        } else {
            spriteRenderer.material = unhoveredMaterial;
        }
    }
}
