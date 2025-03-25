using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadePiece : MonoBehaviour {

    [SerializeField] private Material unhoveredMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Material repairBarricadeMaterial;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D pieceCollider;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 initialPosition;

    private Coroutine deactivateCoroutine;

    private void Awake() {
        initialPosition = transform.position;
        DisableBarricadePiece();
    }

    public void DisableBarricadePiece() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Static;
        spriteRenderer.enabled = false;
        pieceCollider.enabled = false;
        animator.SetTrigger("Idle");
    }

    public void EnableBarricadePiece() {
        gameObject.SetActive(true);
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;
        pieceCollider.enabled = true;

        rb.bodyType = RigidbodyType2D.Static;
        spriteRenderer.enabled = true;
        animator.ResetTrigger("Idle");


        if(deactivateCoroutine != null) {
            StopCoroutine(deactivateCoroutine);
        }
    }

    public void BuildBarricadePiece() {
        gameObject.SetActive(true);
        transform.position = initialPosition;

        rb.bodyType = RigidbodyType2D.Static;
        spriteRenderer.enabled = true;
        pieceCollider.enabled = true;
        animator.SetTrigger("Build");
    }

    public void BarricadePieceFell() {
        Vector2 force = new Vector2(UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(2f, 4f));
        float torque = UnityEngine.Random.Range(-50f, 50f);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 1.5f;
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(torque, ForceMode2D.Force);

        deactivateCoroutine = StartCoroutine(DeactivateBarricadeSpriteAfterDelay());
    }

    public void ShowBarricadePieceRepairable() {
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
