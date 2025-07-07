using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureShieldSounds : SoundObject
{
    [SerializeField] private Creature_Shielded creature_Shielded;
    [SerializeField] private AudioClip[] generateShieldAudioClips;
    [SerializeField] private AudioClip[] destroyShieldAudioClips;
    [SerializeField] private AudioClip[] takeDamageShieldAudioClips;
    [SerializeField] private float destroyShieldVolumeMultiplier = 1f;
    [SerializeField] private float generateShieldVolumeMultiplier = 1f;
    [SerializeField] private float takeDamageShieldVolumeMultiplier = 1f;
    [SerializeField] private float shieldGenerationSFXDelay;


    private void Awake() {
        creature_Shielded.OnShieldDestroyed += Creature_Shielded_OnShieldDestroyed;
        creature_Shielded.OnShieldRegenerated += Creature_Shielded_OnShieldRegenerated;
        creature_Shielded.OnShieldTakesDamage += Creature_Shielded_OnShieldTakesDamage;
    }

    private void Creature_Shielded_OnShieldTakesDamage(object sender, System.EventArgs e) {
        PlaySound2D(takeDamageShieldAudioClips, takeDamageShieldVolumeMultiplier);
    }

    private void Creature_Shielded_OnShieldRegenerated(object sender, System.EventArgs e) {
        PlaySFXAfterDelay(generateShieldAudioClips, shieldGenerationSFXDelay, generateShieldVolumeMultiplier);
    }

    private void Creature_Shielded_OnShieldDestroyed(object sender, System.EventArgs e) {
        PlaySound2D(destroyShieldAudioClips, destroyShieldVolumeMultiplier);
    }
}
