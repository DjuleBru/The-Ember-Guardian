using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CreatureVisual : MobVisual
{
    private Creature creature;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private GameObject debuffedGameObject;
    [SerializeField] private SpriteRenderer glowSpriteRenderer;
    [SerializeField] private SpriteRenderer glowSpriteRenderer2;
    [SerializeField] private SpriteRenderer glowSpriteRenderer3;
    [SerializeField] private SpriteRenderer vfxSprite;
    [SerializeField] private ParticleSystem creatureElitePS;
    [SerializeField] private Color damageEliteOutlineColor;
    [SerializeField] private Color speedEliteOutlineColor;

    protected float dieFadeOutDuration = 1f;

    protected override void Awake() {
        base.Awake();
        creature = GetComponentInParent<Creature>();
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
        creature.OnMobDied += Creature_OnMobDied;
        creature.OnCreatureEnabled += Creature_OnCreatureEnabled;
        glowSpriteRenderer.sortingOrder = currentMaxSortingOrder + 1;

        if(glowSpriteRenderer2 != null) {
            glowSpriteRenderer2.sortingOrder = currentMaxSortingOrder + 2;
        }
        if (glowSpriteRenderer3 != null) {
            glowSpriteRenderer3.sortingOrder = currentMaxSortingOrder + 2;
        }
    }

    private void Creature_OnCreatureEnabled(object sender, System.EventArgs e) {
        SetVisuals();
    }

    private void Creature_OnMobDied(object sender, System.EventArgs e) {
        bodySpriteRenderer.material.SetFloat("_OutlinePixelWidth", 0f);
        bodySpriteRenderer.material.SetFloat("_OutlineAlpha", 0f);
        creatureElitePS.Stop();

        //StartCoroutine(DisableRendererAfterDelay());
    }

    //private IEnumerator DisableRendererAfterDelay() {

    //    CreatureSO creatureSO = creature.GetCreatureSO();
    //    bool fadeOutAfterDieAnimation = creatureSO;
    //    float dieAnimationDuration = creatureSO.dieAnimationDuration;

    //    if (!creatureSO.dieAnimationUsesGlowSpriteRenderer1) {
    //        if (glowSpriteRenderer != null) {
    //            glowSpriteRenderer.enabled = false;
    //        }
    //    }
    //    if (!creatureSO.dieAnimationUsesGlowSpriteRenderer2) {
    //        if (glowSpriteRenderer2 != null) {
    //            glowSpriteRenderer2.enabled = false;
    //        }
    //    }
    //    if (!creatureSO.dieAnimationUsesVFXSpriteRenderer) {
    //        if (vfxSprite != null) {
    //            vfxSprite.enabled = false;
    //        }
    //    }

    //    yield return new WaitForSeconds(dieAnimationDuration);

    //    Color bodyColor = bodySpriteRenderer.color;
    //    if (fadeOutAfterDieAnimation) {
    //        float elapsed = 0f;
    //        while (elapsed < dieFadeOutDuration) {
    //            elapsed += Time.deltaTime;
    //            float alpha = Mathf.Lerp(1f, 0f, elapsed / dieFadeOutDuration);

    //            bodySpriteRenderer.color = new Color(bodyColor.r, bodyColor.g, bodyColor.b, alpha);
    //            yield return null;
    //        }
    //    }

    //    bodySpriteRenderer.enabled = false;

    //    if (glowSpriteRenderer != null) {
    //        glowSpriteRenderer.enabled = false;
    //    }
    //    if (glowSpriteRenderer2 != null) {
    //        glowSpriteRenderer2.enabled = false;
    //    }
    //    if (vfxSprite != null) {
    //        vfxSprite.enabled = false;
    //    }
    //}

    protected void Start() {
        SetVisuals();
    }

    protected void SetVisuals() {
        bodySpriteRenderer.enabled = true;

        if (glowSpriteRenderer != null) {
            glowSpriteRenderer.enabled = true;
        }
        if (glowSpriteRenderer2 != null) {
            glowSpriteRenderer2.enabled = true;
        }
        if (glowSpriteRenderer3 != null) {
            glowSpriteRenderer3.enabled = true;
        }
        if (vfxSprite != null) {
            vfxSprite.enabled = true;
        }

        bodySpriteRenderer.material = cleanMaterial;
        debuffedGameObject.SetActive(false);
        if (creature.GetIsEliteCreature()) {
            bodySpriteRenderer.material.SetFloat("_OutlinePixelWidth", 1f);
            bodySpriteRenderer.material.SetFloat("_OutlineAlpha", 1f);
            creatureElitePS.Play();

            if (creature.GetIsEliteDamageCreature()) {
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
