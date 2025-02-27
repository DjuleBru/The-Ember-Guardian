using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CreatureVisual : MobVisual
{
    private Creature creature;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private GameObject debuffedGameObject;
    [SerializeField] private ParticleSystem creatureElitePS;

    protected override void Awake() {
        base.Awake();
        creature = GetComponentInParent<Creature>();
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
        creature.OnMobDied += Creature_OnMobDied;
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
        }
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        //creatureHitAreaPS.Play();
    }
}
