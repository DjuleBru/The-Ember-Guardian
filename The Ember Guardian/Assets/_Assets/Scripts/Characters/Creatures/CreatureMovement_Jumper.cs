using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement_Jumper : CreatureMovement
{
    private CreatureAI creatureAI;
    private Creature creature;

    [SerializeField] private float jumpForceY = 70f; // Force du saut
    [SerializeField] private float jumpForceX = 35f; // Force du saut
    [SerializeField] private float jumpIntervalMin = 2f; // Intervalle minimum entre les sauts
    [SerializeField] private float jumpIntervalMax = 4f; // Intervalle maximum entre les sauts
    [SerializeField] private float jumpGravityMultiplier = 2f; // Multiplicateur de gravité en chute
    [SerializeField] private float fallGravityMultiplier = 3f; // Multiplicateur de gravité en chute
    [SerializeField] private float jumpAnimationDelay = .1f; // Multiplicateur de gravité en chute
    [SerializeField] private float maxDistanceToPlayerToJump = 5f; // Multiplicateur de gravité en chute

    [SerializeField] private float castDistance;
    [SerializeField] Vector2 boxSize;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask platformLayerMask;

    public event EventHandler OnJumpStarted;
    public event EventHandler OnJumpLanded;
    private bool isJumping;
    private bool isDead;

    protected override void Start() {
        base.Start();
        creatureAI = GetComponent<CreatureAI>();
        creature = GetComponent<Creature>();

        creature.OnCreatureDied += Creature_OnCreatureDied;

        StartCoroutine(JumpRoutine());
    }

    private void Creature_OnCreatureDied(object sender, EventArgs e) {
        isDead = true;
    }

    private IEnumerator JumpRoutine() {
        while (true) {
            // Attendre un délai aléatoire avant le prochain saut
            float waitTime = UnityEngine.Random.Range(jumpIntervalMin, jumpIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Appliquer une force de saut

            float distanceToPlayer = Mathf.Abs(transform.position.x - Player.Instance.transform.position.x);

            if (creatureAI.GetState() == CreatureAI.State.moveToTarget  && IsGrounded() && distanceToPlayer > maxDistanceToPlayerToJump) {
                StartCoroutine(Jump());
            }
        }
    }

    private IEnumerator Jump() {

        SetCanMove(false);

        if (!isJumping && IsGrounded() && !isDead) {

            OnJumpStarted?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(jumpAnimationDelay);

            isJumping = true;

            float jumpForceYRandomized = UnityEngine.Random.Range(jumpForceY - jumpForceY / 4, jumpForceY + jumpForceY / 4);
            float jumpForceXRandomized = UnityEngine.Random.Range(jumpForceX - jumpForceX / 4, jumpForceX + jumpForceX / 4) * moveDirFloat;
            Vector2 force = new Vector2(jumpForceXRandomized, jumpForceYRandomized);

            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private bool IsGrounded() {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayerMask) || Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, platformLayerMask)) {
            return true;
        }
        else {
            return false;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        // Augmenter la gravité lorsque la créature retombe
        if (rb.velocity.y < 0) {
            rb.gravityScale = fallGravityMultiplier;
        }
        else if (rb.velocity.y > 0) {

            rb.gravityScale = jumpGravityMultiplier;

        } else {
            rb.gravityScale = 1f;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        // Vérifie si l'objet entrant est sur le LayerMask "ground"
        if (((1 << collision.gameObject.layer) & groundLayerMask) != 0) {
            OnJumpLanded?.Invoke(this, EventArgs.Empty);
            isJumping = false;
            SetCanMove(true);
        }
    }

}
