using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    [SerializeField] private float mainTracksAudioVolume = .2f;
    [SerializeField] private float backgroundTracksAudioVolume = .1f;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip endLevelMusic;
    [SerializeField] private AudioClip discoverNewLocationMusic;
    private List<AudioClip> levelRandomBackgroundTracks;

    private float targetVolume;
    private float peacefulTimer;
    private float minPeacefulTimerDelay = 20f;
    private float playMusicAttemptTimer;
    private float playMusicAttemptRate = 5f;

    private bool waitingToDiscoverLocation;
    private bool isDuskOrNight;
    private bool isMainMenuScene;
    private bool isLevelScene;
    private bool isPlayingPeacefulMusic;
    private bool isPlayingEndLevelAreaMusic;
    private AudioSource audioSource;

    private void Awake() {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        SetAudioVolume(mainTracksAudioVolume);
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

            bool levelRegionUnlocked = MetaProgressionManager.Instance.GetLevelRegionUnlocked(LevelManager.Instance.GetLevelSO().environmentType);

            if (!levelRegionUnlocked) {
                LevelManager.Instance.OnNewLocationShown += LevelManager_OnNewLocationShown;
                waitingToDiscoverLocation = true;
            }
        }

        if(isMainMenuScene) {
            audioSource.clip = mainMenuMusic;
            PlayMusicDelayed(2f);
        }
    }

    private void LevelManager_OnNewLocationShown(object sender, System.EventArgs e) {
        audioSource.clip = LevelManager.Instance.GetLevelSO().newEnvironmentDiscoveryAudioClip;
        PlayMusicDelayed(4f);
        waitingToDiscoverLocation = false;
        isPlayingPeacefulMusic = true;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        isDuskOrNight = true;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        isPlayingPeacefulMusic = false;
        FadeOutMusic(2f);
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        isDuskOrNight = false;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (!isLevelScene) return;
        if (isPlayingEndLevelAreaMusic) return;

        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        isPlayingPeacefulMusic = false;
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
                        AudioClip randomMusic = levelRandomBackgroundTracks[Random.Range(0, levelRandomBackgroundTracks.Count)];
                        audioSource.clip = randomMusic;
                        targetVolume = backgroundTracksAudioVolume;
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

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        FadeOutMusic(1f);
    }

    public void FadeOutMusic(float fadeDuration) {
        StartCoroutine(FadeOutCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutCoroutine(float fadeDuration) {
        float startVolume = audioSource.volume;

        // Réduire progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique pour le ressenti
            audioSource.volume = Mathf.Lerp(startVolume, 0, Mathf.Sqrt(progress));
            yield return null;
        }

        // S'assurer que le volume est bien à 0 à la fin
        audioSource.volume = 0;
        audioSource.Stop(); // Arrêter la musique
    }

    private IEnumerator FadeInCoroutine(float fadeDuration) {
        float endVolume = targetVolume;
        audioSource.volume = 0;
        audioSource.Play(); // Assure que la musique démarre

        Debug.Log(targetVolume);
        // Augmenter progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique inverse pour le ressenti
            audioSource.volume = Mathf.Lerp(0, endVolume, progress * progress);
            yield return null; // Attendre le prochain frame
        }

        // S'assurer que le volume atteint la valeur finale
        audioSource.volume = endVolume;
    }

    private IEnumerator FadeOutThenInCoroutine(float fadeOutDuration, float fadeInDuration, AudioClip audioClip) {
        // Exécuter le fade-out
        yield return StartCoroutine(FadeOutCoroutine(fadeOutDuration));

        audioSource.clip = audioClip;

        // Exécuter le fade-in
        yield return StartCoroutine(FadeInCoroutine(fadeInDuration));
    }

    public void SetAudioVolume(float volume) {
        audioSource.volume = volume;
    }

    public void SetAudioTargerVolume(float volume) {
        targetVolume = volume;
    }

    public void SetTargetVolumeToMainTrack() {
        targetVolume = mainTracksAudioVolume;
    }

    public void FadeInMusic(float fadeDuration) {
        StartCoroutine(FadeInCoroutine(fadeDuration));
    }

    public void SetEndLevelMusic(float fadeInDuration) {
        if (isPlayingEndLevelAreaMusic) return;

        targetVolume = mainTracksAudioVolume;

        if (audioSource.isPlaying) {

            StartCoroutine(FadeOutThenInCoroutine(fadeInDuration, fadeInDuration, endLevelMusic));

        } else {
            Debug.Log("SetEndLevelMusic FadeIn ");
            audioSource.clip = endLevelMusic;
            isPlayingEndLevelAreaMusic = true;
            FadeInMusic(fadeInDuration);
        }

    }

    public void StopEndLevelMusic() {
        isPlayingEndLevelAreaMusic = false;
        FadeOutMusic(3f);
    }

    private void OnDestroy() {
        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
    }


}
