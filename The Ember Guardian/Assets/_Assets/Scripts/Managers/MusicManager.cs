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
    [SerializeField] private AudioClip nightMusicIntro;
    [SerializeField] private AudioClip nightMusicIntroLoop;
    [SerializeField] private AudioClip nightMusicOutro;
    [SerializeField] private List<AudioClip> nightMusicTension1Loops;
    [SerializeField] private List<AudioClip> nightMusicTension2Loops;
    [SerializeField] private List<AudioClip> nightMusicTension3Loops;
    [SerializeField] private List<AudioClip> nightMusicTension4Loops;
    private Queue<AudioClip> nightMusicQueue = new Queue<AudioClip>();
    private Coroutine nightCoroutine;

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
    private AudioSource audioSourceA;
    private AudioSource audioSourceB;
    private bool isUsingAudioSourceA = true;
    private bool isPlayingNightIntroMusic;

    private int tensionLevelMusicPlaying;
    private int fireDamageTakenRecently;
    private float fireDamageTakenTimer;
    private float fireDamageTakenRemoveRate = 1f;


    private void Awake() {
        Instance = this; 
        
        audioSourceA = GetComponent<AudioSource>(); 
        audioSourceB = gameObject.AddComponent<AudioSource>();

        audioSourceA.loop = true;
        audioSourceB.loop = true;
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
        audioSourceA.ignoreListenerPause = true;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        }
        
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;
        isMainMenuScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;

        if(isLevelScene) {
            levelRandomBackgroundTracks = LevelManager.Instance.GetLevelSO().levelAudioClips;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            CreaturesManager.Instance.OnAllCreaturesAtNightKilled += CreaturesManager_OnAllCreaturesAtNightKilled;
            Player.Instance.OnPlayerDied += Player_OnPlayerDied;
            Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
            HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            Fire.Instance.OnFireDamageTaken += Fire_OnFireDamageTaken;

            LevelManager.Instance.OnNewLocationShown += LevelManager_OnNewLocationShown;
            LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;
            waitingToDiscoverLocation = true;
        }

        if (isMainMenuScene) {
            audioSourceA.clip = mainMenuMusic;
            PlayMusicDelayed(2f);
        }
    }


    private void Fire_OnFireDamageTaken(object sender, EventArgs e) {
        fireDamageTakenRecently ++;
        fireDamageTakenTimer = fireDamageTakenRemoveRate;
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {

        if (isPlayingLevelDiscoveryMusic || isPlayingPeacefulMusic) {
            StartCoroutine(FadeInCoroutine(1f, volumeBeforeTalkingToNPC / 1.5f));
        }
    }

    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, EventArgs e) {

        if (isPlayingLevelDiscoveryMusic || isPlayingPeacefulMusic) {
            volumeBeforeTalkingToNPC = audioSourceA.volume;
            StartCoroutine(FadeOutCoroutine(1f, volumeBeforeTalkingToNPC / 1.5f));
        }

    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, EventArgs e) {
        if(isPlayingEndLevelAreaMusic || isPlayingLevelDiscoveryMusic || isPlayingNightMusic || isPlayingPeacefulMusic) {

            musicAudioLevelReducedWithPause = true;
            audioSourceA.volume /= 2.5f;
        }
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, EventArgs e) {
        if(musicAudioLevelReducedWithPause) {
            audioSourceA.volume *= 2.5f;
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
        audioSourceA.clip = LevelManager.Instance.GetLevelSO().newEnvironmentDiscoveryAudioClip;
        audioSourceA.volume = discoverNewLocationAudioVolume;
        PlayMusicDelayed(4f);
        isPlayingLevelDiscoveryMusic = true;
        waitingToDiscoverLocation = false;
        isPlayingPeacefulMusic = true;
    }

    private void LevelManager_OnLevelFailed(object sender, EventArgs e) {
        FadeOutMusic(5f);
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (!isLevelScene) return;

        audioSourceA.loop = true;
        audioSourceB.loop = true;
        isPlayingNightMusic = true;

        StartCoroutine(PlayIntroNighMusicDelayed(2f));

        SetAudioTargerVolume(nightMusicAudioVolume);
        //StartCoroutine(FadeInDelayedCoroutine(3f, 4f));
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        isDuskOrNight = true;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        FadeOutMusic(2f);
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

        if(fireDamageTakenRecently != 0) {
            fireDamageTakenTimer -= Time.deltaTime;
            if(fireDamageTakenTimer < 0) {
                fireDamageTakenTimer = fireDamageTakenRemoveRate;
                fireDamageTakenRecently--;
            }
        }

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
                        audioSourceA.clip = randomMusic;
                        targetVolume = backgroundTracksAudioVolume * musicSettingVolume;
                        FadeInMusic(5f);

                        isPlayingPeacefulMusic = true;
                    }
                }
            }
        }
    }

    private void CreaturesManager_OnAllCreaturesAtNightKilled(object sender, EventArgs e) {
        if (!isLevelScene) return;

        audioSourceA.loop = false;
        audioSourceB.loop = false;
        StopCoroutine(nightCoroutine);

        CrossfadeToNextNightClip(nightMusicOutro);

        StartCoroutine(FadeOutDelayedCoroutine(5f, 2f));
        isDuskOrNight = false;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        isPlayingNightMusic = false;
    }

    private IEnumerator PlayIntroNighMusicDelayed(float delay) {
        yield return new WaitForSeconds(delay);
        audioSourceA.clip = nightMusicIntro;
        isPlayingNightIntroMusic = true;
        audioSourceA.volume = nightMusicAudioVolume;
        audioSourceA.Play();
        isUsingAudioSourceA = true;

        StartCoroutine(WaitForClipToEnd(nightMusicIntro.length));
    }

    private void PlayFirstNightClip() {
        AudioClip nextClip = nightMusicIntroLoop;
        isPlayingNightIntroMusic = false;
        audioSourceA.clip = nextClip;
        audioSourceA.Play();
        StartCoroutine(WaitForClipToEnd(nextClip.length));
        return;
    }

    private void PlayNextNightMusicSegment() {

        if (isPlayingNightIntroMusic) {
            PlayFirstNightClip();
            return;
        }

        AudioClip nextClip = GetNightClipBasedOnRemainingCreatures();

        AudioSource activeSource = isUsingAudioSourceA ? audioSourceA : audioSourceB;
        AudioClip currentAudioClipPlaying = activeSource.clip;


        if (nextClip != currentAudioClipPlaying) {
            
            CrossfadeToNextNightClip(nextClip);
            nightCoroutine = StartCoroutine(WaitForClipToEnd(nextClip.length / 2));

           
        }
        else
        {
            nightCoroutine = StartCoroutine(WaitForClipToEnd(nextClip.length / 2));
        }

    }

    private IEnumerator WaitForClipToEnd(float duration) {
        yield return new WaitForSeconds(duration);

        PlayNextNightMusicSegment();
    }

    public AudioClip GetNightClipBasedOnRemainingCreatures() {
        AudioSource activeSource = isUsingAudioSourceA ? audioSourceA : audioSourceB;

        AudioClip currentAudioClipPlaying = activeSource.clip;
        int creaturesCloseToPlayerCamp = CreaturesManager.Instance.GetNightCreaturesCloseToPlayerCamp(15f);
        int creaturesInsidePlayerCamp = CreaturesManager.Instance.GetNightCreaturesCloseToPlayerCamp(0);

        AudioClip selectedAudioClip = activeSource.clip;


        if (fireDamageTakenRecently >= 3) {
            // fire just took a bunch of damage : player in deep ****
            Debug.Log("fireDamageTakenRecently " + fireDamageTakenRecently);
            if (tensionLevelMusicPlaying == 4) {
                selectedAudioClip = currentAudioClipPlaying;
            }
            else {
                selectedAudioClip = nightMusicTension4Loops[UnityEngine.Random.Range(0, nightMusicTension4Loops.Count)];
            }
            tensionLevelMusicPlaying = 4;
        }
        else if (creaturesInsidePlayerCamp != 0) {

            if (creaturesInsidePlayerCamp < 5) {
                // small amount of creatures inside player camp
                if (creaturesCloseToPlayerCamp < 5) {
                    // small amount of creatures outside player camp : probably end of subwave
                    if (tensionLevelMusicPlaying == 3) {
                        selectedAudioClip = currentAudioClipPlaying;
                    }
                    else {
                        selectedAudioClip = nightMusicTension3Loops[UnityEngine.Random.Range(0, nightMusicTension3Loops.Count)];
                    }
                    tensionLevelMusicPlaying = 3;
                }
                else {
                    // big amount of creatures outside player camp : player in deep ****
                    if (tensionLevelMusicPlaying == 4) {
                        selectedAudioClip = currentAudioClipPlaying;
                    }
                    else {
                        selectedAudioClip = nightMusicTension4Loops[UnityEngine.Random.Range(0, nightMusicTension4Loops.Count)];
                    }
                    tensionLevelMusicPlaying = 4;
                }

            }
            if (creaturesInsidePlayerCamp >= 5) {
                // big amount of creatures outside player camp : player in deep ****
                if (tensionLevelMusicPlaying == 4) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = nightMusicTension4Loops[UnityEngine.Random.Range(0, nightMusicTension4Loops.Count)];
                }
                tensionLevelMusicPlaying = 4;

            }

        }
        else {

            if (creaturesCloseToPlayerCamp < 5) {
                if (tensionLevelMusicPlaying == 1) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = nightMusicTension1Loops[UnityEngine.Random.Range(0, nightMusicTension1Loops.Count)];
                }
                tensionLevelMusicPlaying = 1;
            }

            if (creaturesCloseToPlayerCamp >= 5 && creaturesCloseToPlayerCamp < 12) {
                if (tensionLevelMusicPlaying == 2) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = nightMusicTension2Loops[UnityEngine.Random.Range(0, nightMusicTension2Loops.Count)];
                }
                tensionLevelMusicPlaying = 2;
            }

            if (creaturesCloseToPlayerCamp >= 12 && creaturesCloseToPlayerCamp < 20) {
                if (tensionLevelMusicPlaying == 3) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = nightMusicTension3Loops[UnityEngine.Random.Range(0, nightMusicTension3Loops.Count)];
                }
                tensionLevelMusicPlaying = 3;
            }

            if (creaturesCloseToPlayerCamp >= 20) {
                if (tensionLevelMusicPlaying == 4) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = nightMusicTension4Loops[UnityEngine.Random.Range(0, nightMusicTension4Loops.Count)];
                }
                tensionLevelMusicPlaying = 4;
            }
        }


        return selectedAudioClip;
    }

    private void CrossfadeToNextNightClip(AudioClip newClip) {
        AudioSource activeSource = isUsingAudioSourceA ? audioSourceA : audioSourceB;
        AudioSource nextSource = isUsingAudioSourceA ? audioSourceB : audioSourceA;

        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        StartCoroutine(CrossfadeCoroutine(activeSource, nextSource, 2f));

        isUsingAudioSourceA = !isUsingAudioSourceA;
    }

    private IEnumerator CrossfadeCoroutine(AudioSource fromSource, AudioSource toSource, float duration) {
        float elapsedTime = 0f;
        float maxVolume = nightMusicAudioVolume;

        float clipTime = fromSource.time; // Récupère le temps de lecture actuel
        if (clipTime > fromSource.clip.length - 1f) {
            clipTime = 0;
        }
        toSource.time = clipTime;

        while (elapsedTime < duration) {
            float t = elapsedTime / duration;

            // Courbe exponentielle pour une transition plus naturelle
            float fadeOutFactor = Mathf.SmoothStep(1f, 0f, t);
            float fadeInFactor = Mathf.SmoothStep(0f, 1f, t);

            fromSource.volume = fadeOutFactor * maxVolume;
            toSource.volume = fadeInFactor * maxVolume;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fromSource.volume = 0f;
        fromSource.Stop();
        toSource.volume = maxVolume; // Assure un retour au volume normal
    }



    public void PlayMusicDelayed(float delay) {
        audioSourceA.PlayDelayed(delay);
    }
    public IEnumerator FadeInDelayedCoroutine(float delayToFadeIn, float fadeInDuration) {
        yield return new WaitForSeconds(delayToFadeIn);
        StartCoroutine(FadeInCoroutine(fadeInDuration, 0));
    }
    public IEnumerator FadeOutDelayedCoroutine(float delayToFadeOut, float fadeOutDuration) {
        yield return new WaitForSeconds(delayToFadeOut);
        StartCoroutine(FadeOutCoroutine(fadeOutDuration, 0));
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
        AudioSource activeSource = isUsingAudioSourceA ? audioSourceA : audioSourceB;
        float startVolume = activeSource.volume;

        // Réduire progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique pour le ressenti
            audioSourceA.volume = Mathf.Lerp(startVolume, targetFadeVolume, Mathf.Sqrt(progress));
            audioSourceB.volume = Mathf.Lerp(startVolume, targetFadeVolume, Mathf.Sqrt(progress));
            yield return null;
        }

        // S'assurer que le volume est bien à 0 à la fin
        audioSourceA.volume = targetFadeVolume;
        audioSourceB.volume = targetFadeVolume;

        if(targetFadeVolume == 0) {
            audioSourceA.Stop(); // Arrêter la musique
            audioSourceB.Stop(); // Arrêter la musique
        }

        isUsingAudioSourceA = !isUsingAudioSourceA;
    }

    private IEnumerator FadeInCoroutine(float fadeDuration, float initialVolume) {
        audioSourceA.volume = initialVolume;

        if(initialVolume == 0) {
            audioSourceA.Play(); // Assure que la musique démarre
        }

        // Augmenter progressivement le volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float progress = t / fadeDuration;

            // Appliquer une courbe logarithmique inverse pour le ressenti
            audioSourceA.volume = Mathf.Lerp(initialVolume, targetVolume, progress * progress);
            yield return null; // Attendre le prochain frame
        }

        // S'assurer que le volume atteint la valeur finale
        audioSourceA.volume = targetVolume;
    }

    private IEnumerator FadeOutThenInCoroutine(float fadeOutDuration, float fadeInDuration, AudioClip audioClip) {
        // Exécuter le fade-out
        yield return StartCoroutine(FadeOutCoroutine(fadeOutDuration, 0));

        audioSourceA.clip = audioClip;

        // Exécuter le fade-in
        yield return StartCoroutine(FadeInCoroutine(fadeInDuration, 0));
    }

    public void SetAudioVolume(float volume) {
        audioSourceA.volume = volume * musicSettingVolume;
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

        if (audioSourceA.isPlaying) {

            StartCoroutine(FadeOutThenInCoroutine(fadeInDuration, fadeInDuration, endLevelMusic));

        } else {
            audioSourceA.clip = endLevelMusic;
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
            CreaturesManager.Instance.OnAllCreaturesAtNightKilled += CreaturesManager_OnAllCreaturesAtNightKilled;
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
