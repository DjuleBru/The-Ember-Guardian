using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureShieldVisual : MonoBehaviour
{
    [SerializeField] private Creature_Shielded creature_Shielded;
    [SerializeField] private Animator shieldAnimator;
    [SerializeField] private ParticleSystem shieldDamagedPS;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer shieldSpriteRenderer;


    private void Awake() {
        creature_Shielded.OnShieldDestroyed += Creature_Shielded_OnShieldDestroyed;
        creature_Shielded.OnShieldRegenerated += Creature_Shielded_OnShieldRegenerated;
        creature_Shielded.OnShieldTakesDamage += Creature_Shielded_OnShieldTakesDamage;
    }

    private void Start() {
        shieldSpriteRenderer.sortingOrder = bodySpriteRenderer.sortingOrder + 3;
    }

    private void Creature_Shielded_OnShieldTakesDamage(object sender, System.EventArgs e) {
        shieldAnimator.SetTrigger("TakeDamage");
        shieldDamagedPS.Play();
    }

    private void Creature_Shielded_OnShieldRegenerated(object sender, System.EventArgs e) {
        shieldAnimator.SetTrigger("Activate");
    }

    private void Creature_Shielded_OnShieldDestroyed(object sender, System.EventArgs e) {
        shieldAnimator.SetTrigger("Destroy");
        shieldDamagedPS.Play();
    }
}
