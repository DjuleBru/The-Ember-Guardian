using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAudioLoadManager : MonoBehaviour
{
    private List<AudioClip> audioClipsPreloaded = new List<AudioClip>();
    private void Start() {
        if (LevelManager.Instance == null) return;

        foreach (CreatureSO creatureSO in LevelManager.Instance.GetLevelSO().nightCreatureTypes) {
            PreloadCreatureClips(creatureSO);
        }
        foreach (CreatureSO creatureSO in LevelManager.Instance.GetLevelSO().dayCreatureTypes) {
            PreloadCreatureClips(creatureSO);
        }

        SceneLoader.Instance.OnSceneFadeOut += SceneLoader_OnSceneFadeOut;
    }

    private void SceneLoader_OnSceneFadeOut(object sender, SceneLoader.OnSceneFadeOutEventArgs e) {
        foreach (CreatureSO creatureSO in LevelManager.Instance.GetLevelSO().nightCreatureTypes) {
            UnloadCreatureClips(creatureSO);
        }
        foreach (CreatureSO creatureSO in LevelManager.Instance.GetLevelSO().dayCreatureTypes) {
            UnloadCreatureClips(creatureSO);
        }
    }

    private void PreloadCreatureClips(CreatureSO creatureSO) {
        Debug.Log("PreloadCreatureClips " + creatureSO);
        foreach(AudioClip clip in creatureSO.spawnAudioClips) {
        }

        foreach (AudioClip clip in creatureSO.dieAudioClips) {
            LoadAudioClip(clip);
        }

        foreach (AudioClip clip in creatureSO.aggroAudioClips) {
            LoadAudioClip(clip);
        }

        foreach (AudioClip clip in creatureSO.idleAudioClips) {
            LoadAudioClip(clip);
        }

        foreach (AudioClip clip in creatureSO.footStepAudioClips) {
            LoadAudioClip(clip);
        }

        if (creatureSO.primaryAttackSO != null) {
            foreach (AudioClip clip in creatureSO.primaryAttackSO.attackAudioClips) {
                LoadAudioClip(clip);
            }

            foreach (AudioClip clip in creatureSO.primaryAttackSO.attackHitAudioClips) {
                LoadAudioClip(clip);
            }

            if(creatureSO.primaryAttackSO.projectileSO != null) {
                foreach (AudioClip clip in creatureSO.primaryAttackSO.projectileSO.projectileInstantiatedAudioClips) {
                    LoadAudioClip(clip);
                }

                foreach (AudioClip clip in creatureSO.primaryAttackSO.projectileSO.projectileHitAudioClips) {
                    LoadAudioClip(clip);
                }
            }


        }

        if (creatureSO.secondaryAttackSO != null) {
            foreach (AudioClip clip in creatureSO.secondaryAttackSO.attackAudioClips) {
                LoadAudioClip(clip);
            }

            foreach (AudioClip clip in creatureSO.secondaryAttackSO.attackHitAudioClips) {
                LoadAudioClip(clip);
            }

            if (creatureSO.secondaryAttackSO.projectileSO != null) {

                foreach (AudioClip clip in creatureSO.secondaryAttackSO.projectileSO.projectileInstantiatedAudioClips) {
                    LoadAudioClip(clip);
                }

                foreach (AudioClip clip in creatureSO.secondaryAttackSO.projectileSO.projectileHitAudioClips) {
                    LoadAudioClip(clip);
                }
            }


        }
    }

    private void UnloadCreatureClips(CreatureSO creatureSO) {
        foreach(AudioClip clip in audioClipsPreloaded) {
            UnloadAudioClip(clip);
        }
    }

    private void LoadAudioClip(AudioClip clip) {
        if (audioClipsPreloaded.Contains(clip)) return;

        clip.LoadAudioData();
        audioClipsPreloaded.Add(clip);
    }
    private void UnloadAudioClip(AudioClip clip) {
        if (!audioClipsPreloaded.Contains(clip)) return;

        clip.UnloadAudioData();
        audioClipsPreloaded.Remove(clip);
    }
}
