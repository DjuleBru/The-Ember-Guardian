using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    public enum NewLocationMusicInterruptionSource {
        creatureAggro,
        buildFire,
    }

    [SerializeField] private NewLocationMusicInterruptionSource discoveryMusicInterruptionSource;

    [SerializeField] private float discoverNewLocationAudioVolume = .4f;
    [SerializeField] private float backgroundTracksAudioVolume = .2f;
    [SerializeField] private float nightMusicAudioVolume = .4f;
    private float musicSettingVolume;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip nightMusic;
    [SerializeField] private AudioClip endLevelMusic;
    [SerializeField] private AudioClip discoverNewLocationMusic;
    private List<AudioClip> levelRandomBackgroundTracks;

    private float targetVolume;
    private float peacefulTimer;
    private float minPeacefulTimerDelay = 20f;
    private float playMusicAttemptTimer;
    private float playMusicAttemptRate = 5f;
    private float volumeBeforeTalkingToNPC;

    private bool waitingToDiscoverLocation;
    private bool isDuskOrNight;
    private bool isMainMenuScene;
    private bool isLevelScene;
    private bool isPlayingLevelDiscoveryMusic;
    private bool isPlayingPeacefulMusic;
    private bool isPlayingEndLevelAreaMusic;
    private bool isPlayingNightMusic;
    private bool musicAudioLevelReducedWithPause;
    private AudioSource audioSource;

    private void Awake() {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        SettingsManager.Instance.OnMusicVolumeChanged += SettingsManager_OnMusicVolumeChanged;
        musicSettingVolume = SettingsManager.Instance.GetMusicVolume();

        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        }

        SetAudioVolume(discoverNewLocationAudioVolume);
        targetVolume = discoverNewLocationAudioVolume;
        audioSource.ignoreListenerPause = true;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        }
        
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;
        isMainMenuScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;

        if(isLevelScene) {
            levelRandomBackgroundTracks = LevelManager.Instance.GetLevelSO().levelAudioClips;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            Player.Instance.OnPlayerDied += Player_OnPlayerDied;
            Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
            HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;

            LevelManager.Instance.OnNewLocationShown += LevelManager_OnNewLocationShown;
            waitingToDiscoverLocation = true;
        }

        if (isMainMenuScene) {
            audioSource.clip = mainMenuMusic;
            PlayMusicDelayed(2f);
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {

        if (isPlayingLevelDiscoveryMusic || isPlayingPeacefulMusic) {
            StartCoroutine(FadeInCoroutine(1f, volumeBeforeTalkingToNPC / 1.5f));
        }
    }

    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, EventArgs e) {

        if (isPlayingLevelDiscoveryMusic || isPlayingPeacefulMusic) {
            volumeBeforeTalkingToNPC = audioSource.volume;
            StartCoroutine(FadeOutCoroutine(1f, volumeBeforeTalkingToNPC / 1.5f));
        }

    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, EventArgs e) {
        if(isPlayingEndLevelAreaMusic || isPlayingLevelDiscoveryMusic || isPlayingNightMusic || isPlayingPeacefulMusic) {

            musicAudioLevelReducedWithPause = true;
            audioSource.volume /= 2.5f;
        }
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, EventArgs e) {
        if(musicAudioLevelReducedWithPause) {
            audioSource.volume *= 2.5f;
        }
    }

    private void SettingsManager_OnMusicVolumeChanged(object sender, System.EventArgs e) {
        musicSettingVolume = SettingsManager.Instance.GetMusicVolume();
        SetAudioVolume(musicSettingVolume);
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource == NewLocationMusicInterruptionSource.buildFire) {
            peacefulTimer = 0;
            playMusicAttemptTimer = 0;
            FadeOutMusic(2f);
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        FadeOutMusic(1f);
    }

    private void LevelManager_OnNewLocationShown(object sender, System.EventArgs e) {
        audioSource.clip = LevelManager.Instance.GetLevelSO().newEnvironmentDiscoveryAudioClip;
        audioSource.volume = discoverNewLocationAudioVolume;
        PlayMusicDelayed(4f);
        isPlayingLevelDiscoveryMusic = true;
        waitingToDiscoverLocation = false;
        isPlayingPeacefulMusic = true;
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (!isLevelScene) return;

        audioSource.clip = nightMusic;
        isPlayingNightMusic = true;
        SetAudioTargerVolume(nightMusicAudioVolume);
        StartCoroutine(FadeInDelayedCoroutine(3f, 4f));
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        isDuskOrNight = true;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        FadeOutMusic(2f);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        FadeOutMusic(2f);
        isDuskOrNight = false;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        isPlayingNightMusic = false;

    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (!isLevelScene) return;
        if (isPlayingEndLevelAreaMusic) return;
        if (isPlayingNightMusic) return;
        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource != NewLocationMusicInterruptionSource.creatureAggro) return;

        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        FadeOutMusic(2f);
    }

    private void Update() {
        if (waitingToDiscoverLocation) return;
        if (isPlayingPeacefulMusic) return;
        if (isDuskOrNight) return;
        if (isPlayingEndLevelAreaMusic) return;

        if(isLevelScene) {
            peacefulTimer += Time.deltaTime;

            if(peacefulTimer >= minPeacefulTimerDelay) {
                playMusicAttemptTimer += Time.deltaTime;

                if(playMusicAttemptTimer > playMusicAttemptRate) {
                    playMusicAttemptTimer = 0;

                    float randomNumber = UnityEngine.Random.Range(0, 1f);
                    float chanceToPlayMusid = .1f;

                    if (randomNumber < chanceToPlayMusid) {

                        if (levelRandomBackgroundTracks.Count == 0) return;
                        AudioClip randomMusic = levelRandomBackgroundTracks[UnityEngine.Random.Range(0, levelRandomBackgroundTracks.Count)];
                        audioSource.clip = randomMusic;
                        targetVolume = backgroundTracksAudioVolume * musicSettingVolume;
                        FadeInMusic(5f);

                        isPlayingPeacefulMusic = true;
                    }
                }
            }
        }
    }

    public void PlayMusicDelayed(float delay) {
        audioSource.PlayDelayed(delay);
    }
    public IEnumerator FadeInDelayedCoroutine(float delayToFadeIn, float fadeInDuration) {
        yield return new WaitForSeconds(delayToFadeIn);
        StartCoroutine(FadeInCoroutine(fadeInDuration, 0));
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        FadeOutMusic(1f);
    }

    public void FadeOutMusic(float fadeDuration) {
        isPlayingLevelDiscoveryMusic = false;
        isPlayingPeacefulMusic = false;
        isPlayingEndLevelAreaMusic = false;

        StartCoroutine(FadeOutCoroutine(fadeDuration, 0));
    }

    private IEnumerator FadeOutCoroutine(float fadeDuration, float targetFadeVolume) {
        float startVolume = audioSource.volume;

        // Réduire progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique pour le ressenti
            audioSource.volume = Mathf.Lerp(startVolume, targetFadeVolume, Mathf.Sqrt(progress));
            yield return null;
        }

        // S'assurer que le volume est bien à 0 à la fin
        audioSource.volume = targetFadeVolume;

        if(targetFadeVolume == 0) {
            audioSource.Stop(); // Arrêter la musique
        }
    }

    private IEnumerator FadeInCoroutine(float fadeDuration, float initialVolume) {
        audioSource.volume = initialVolume;

        if(initialVolume == 0) {
            audioSource.Play(); // Assure que la musique démarre
        }

        // Augmenter progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique inverse pour le ressenti
            audioSource.volume = Mathf.Lerp(initialVolume, targetVolume, progress * progress);
            yield return null; // Attendre le prochain frame
        }

        // S'assurer que le volume atteint la valeur finale
        audioSource.volume = targetVolume;
    }

    private IEnumerator FadeOutThenInCoroutine(float fadeOutDuration, float fadeInDuration, AudioClip audioClip) {
        // Exécuter le fade-out
        yield return StartCoroutine(FadeOutCoroutine(fadeOutDuration, 0));

        audioSource.clip = audioClip;

        // Exécuter le fade-in
        yield return StartCoroutine(FadeInCoroutine(fadeInDuration, 0));
    }

    public void SetAudioVolume(float volume) {
        audioSource.volume = volume * musicSettingVolume;
    }

    public void SetAudioTargerVolume(float volume) {
        targetVolume = volume * musicSettingVolume;
    }

    public void SetTargetVolumeToMainTrack() {
        targetVolume = discoverNewLocationAudioVolume * musicSettingVolume;
    }

    public void FadeInMusic(float fadeDuration) {
        StartCoroutine(FadeInCoroutine(fadeDuration, 0));
    }

    public void SetEndLevelMusic(float fadeInDuration) {
        if (isPlayingEndLevelAreaMusic) return;
        isPlayingEndLevelAreaMusic = true;

        targetVolume = discoverNewLocationAudioVolume * musicSettingVolume;

        if (audioSource.isPlaying) {

            StartCoroutine(FadeOutThenInCoroutine(fadeInDuration, fadeInDuration, endLevelMusic));

        } else {
            audioSource.clip = endLevelMusic;
            FadeInMusic(fadeInDuration);
        }

    }

    public void StopEndLevelMusic() {
        FadeOutMusic(3f);
    }

    private void OnDestroy() {
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        CreatureAI.OnAnyCreatureAggro -= CreatureAI_OnAnyCreatureAggro;

        if (isLevelScene) {
            DayNightManager.Instance.OnDuskStart -= DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
            DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
            Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
            LevelManager.Instance.OnNewLocationShown -= LevelManager_OnNewLocationShown;
            Fire.Instance.OnInitialFireActivated -= Fire_OnInitialFireActivated;
            HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant -= HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        }

        if (PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed -= PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened -= PauseMenuUI_OnPauseMenuOpened;
        }

    }


}
