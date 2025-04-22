using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CreatureVisual : MobVisual
{
    private Creature creature;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private GameObject debuffedGameObject;
    [SerializeField] private SpriteRenderer glowSpriteRenderer;
    [SerializeField] private ParticleSystem creatureElitePS;
    [SerializeField] private Color damageEliteOutlineColor;
    [SerializeField] private Color speedEliteOutlineColor;

    protected override void Awake() {
        base.Awake();
        creature = GetComponentInParent<Creature>();
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
        creature.OnMobDied += Creature_OnMobDied;
        glowSpriteRenderer.sortingOrder = currentMaxSortingOrder + 1;
    }

    private void Creature_OnMobDied(object sender, System.EventArgs e) {
        bodySpriteRenderer.material.SetFloat("_OutlinePixelWidth", 0f);
        bodySpriteRenderer.material.SetFloat("_OutlineAlpha", 0f);
        creatureElitePS.Stop();
    }

    protected void Start() {
        bodySpriteRenderer.material = cleanMaterial;
        debuffedGameObject.SetActive(false);

        if (creature.GetIsEliteCreature()) {
            bodySpriteRenderer.material.SetFloat("_OutlinePixelWidth", 1f);
            bodySpriteRenderer.material.SetFloat("_OutlineAlpha", 1f);
            creatureElitePS.Play();

            if(creature.GetIsEliteDamageCreature()) {
                bodySpriteRenderer.material.SetColor("_OutlineColor", damageEliteOutlineColor);
            }
            if (creature.GetIsEliteSpeedCreature()) {
                bodySpriteRenderer.material.SetColor("_OutlineColor", speedEliteOutlineColor);
            }
        }
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        //creatureHitAreaPS.Play();
    }
}
