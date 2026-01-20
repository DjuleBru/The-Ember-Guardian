using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerSound : SoundObject
{
    [SerializeField] private AudioClip[] workerFootstepAudioClips;
    [SerializeField] private AudioClip[] guardFootstepAudioClips;
    [SerializeField] private AudioClip[] guardSpearHitAudioClips;
    [SerializeField] private AudioClip[] pickaxeHitAudioClips;
    [SerializeField] private AudioClip[] turnWrenchAudioClips;
    [SerializeField] private AudioSource footStepAudioSource;
    [SerializeField] private float footstepVolumeMultiplier;
    [SerializeField] private float spearHitVolumeMultiplier;
    [SerializeField] private float pickaxeHitVolumeMultiplier;
    [SerializeField] private float turnWrenchVolumeMultiplier;

    [SerializeField] private WorkerAI workerAI;
    [SerializeField] private MobAttack workerAttack;
    [SerializeField] private WorkerAnimatorManager workerAnimator;
    [SerializeField] private EngineerJob engineerJob;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Start() {
        base.Start();
        workerAttack.OnMobAttackHit += WorkerAttack_OnMobAttackHit;
        workerAnimator.OnFootstepTriggered += WorkerAnimator_OnFootstepTriggered;
        engineerJob.OnEngineerTurnsWrench += EngineerJob_OnEngineerTurnsWrench;

        footStepAudioSource.GetComponent<SoundVolume2D>().SetVolumeMultiplier(footstepVolumeMultiplier);
    }

    private void EngineerJob_OnEngineerTurnsWrench(object sender, System.EventArgs e) {
        PlaySound2D(turnWrenchAudioClips[Random.Range(0, turnWrenchAudioClips.Length)], sfxVolume * turnWrenchVolumeMultiplier);
    }

    private void WorkerAnimator_OnFootstepTriggered(object sender, System.EventArgs e) {
        if (!footStepAudioSource.enabled) return;
        if (footStepAudioSource.isPlaying) return;

        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            footStepAudioSource.clip = guardFootstepAudioClips[Random.Range(0, guardFootstepAudioClips.Length)];
        }
        else {
            footStepAudioSource.clip = workerFootstepAudioClips[Random.Range(0, workerFootstepAudioClips.Length)];
        }
        footStepAudioSource.Play();
    }

    private void WorkerAttack_OnMobAttackHit(object sender, System.EventArgs e) {
        if(workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            audioSource.PlayOneShot(guardSpearHitAudioClips[Random.Range(0, guardSpearHitAudioClips.Length)], sfxVolume * spearHitVolumeMultiplier);
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            audioSource.PlayOneShot(pickaxeHitAudioClips[Random.Range(0, pickaxeHitAudioClips.Length)], sfxVolume * pickaxeHitVolumeMultiplier);
        }
    }
}
