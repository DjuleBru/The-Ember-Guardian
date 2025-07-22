using Sirenix.OdinInspector;
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
        none,
    }

    [SerializeField] private NewLocationMusicInterruptionSource discoveryMusicInterruptionSource;

    [SerializeField] private float discoverNewLocationAudioVolume = .4f;
    [SerializeField] private float backgroundTracksAudioVolume = .2f;
    [SerializeField] private float nightMusicAudioVolume = .4f;
    [SerializeField] private float mainMenuMusicAudioVolume = .4f;
    private float musicSettingVolume;

    [SerializeField] private AudioClip mainMenuMusic; 
    [SerializeField] private AudioClip mainMenuMusicStreamerMode;

    [SerializeField] private AudioClip hubMusic;
    [SerializeField] private AudioClip hubMusicStreamerMode;

    [SerializeField] private AudioClip nightMusicIntro;
    [SerializeField] private AudioClip nightMusicIntroLoop;
    [SerializeField] private AudioClip nightMusicOutro;

    [SerializeField] private List<AudioClip> nightMusicTension1Loops;
    [SerializeField] private List<AudioClip> nightMusicTension2Loops;
    [SerializeField] private List<AudioClip> nightMusicTension3Loops;
    [SerializeField] private List<AudioClip> nightMusicTension4Loops;

    [SerializeField] private AudioClip nightMusicIntroStreamer;
    [SerializeField] private AudioClip nightMusicIntroLoopStreamer;
    [SerializeField] private AudioClip nightMusicOutroStreamer;

    [SerializeField] private List<AudioClip> nightMusicTension1LoopsStreamer;
    [SerializeField] private List<AudioClip> nightMusicTension2LoopsStreamer;
    [SerializeField] private List<AudioClip> nightMusicTension3LoopsStreamer;
    [SerializeField] private List<AudioClip> nightMusicTension4LoopsStreamer;
    private Queue<AudioClip> nightMusicQueue = new Queue<AudioClip>();
    private Coroutine nightCoroutine;

    [SerializeField] private AudioClip endLevelMusic;
    [SerializeField] private AudioClip clearingObstacleCreaturesSpawningClip;
    private List<AudioClip> levelRandomBackgroundTracks;
    private List<AudioClip> levelRandomBackgroundTracksPooled;
    private List<AudioClip> levelExplorationTracks;
    private List<AudioClip> levelExplorationTracksStreamerMode;
    private List<AudioClip> levelExplorationTracksPooled;

    private float targetVolume;
    private float peacefulTimer;
    private float minPeacefulTimerDelay = 20f;
    private float playMusicAttemptTimer;
    private float playMusicAttemptRate = 10f;
    private float playMusicAttemptProbability = 0f;
    private int playExplorationMusicTick;
    private int explorationMusicTickAmountToPlay = 2;

    private float volumeBeforeTalkingToNPC;

    private bool waitingToDiscoverLocation;
    private bool isDuskOrNight;
    private bool isMainMenuScene;
    private bool isHUBScene;
    private bool isLevelScene;
    private bool isPlayingLevelDiscoveryMusic;
    private bool isPlayingPeacefulMusic;
    private bool isPlayingExplorationMusic;
    private bool isPlayingClearRubbleMusic;
    private bool isPlayingEndLevelAreaMusic;
    private bool isPlayingNightMusic;
    private bool musicAudioLevelReducedWithPause;
    private AudioSource audioSourceA;
    private AudioSource audioSourceB;
    private bool isUsingAudioSourceA = true;
    private bool isPlayingNightIntroMusic;

    private bool streamerMode;

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
        SettingsManager.Instance.OnSteamerModeChanged += SettingsManager_OnSteamerModeChanged;
        musicSettingVolume = SettingsManager.Instance.GetMusicVolume();
        streamerMode = SettingsManager.Instance.GetStreamerMode();

        if (PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        }

        SetAudioVolume(discoverNewLocationAudioVolume);
        targetVolume = discoverNewLocationAudioVolume;
        audioSourceA.ignoreListenerPause = true;
        audioSourceB.ignoreListenerPause = true;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        }
        
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;
        isMainMenuScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;
        isHUBScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;

        if(isLevelScene) {
            levelRandomBackgroundTracks = LevelManager.Instance.GetLevelSO().levelRandomBackgroundTracks;
            levelExplorationTracks = LevelManager.Instance.GetLevelSO().levelExplorationTracks;
            levelExplorationTracksStreamerMode = LevelManager.Instance.GetLevelSO().levelExplorationTracksStreamerMode;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            CreaturesManager.Instance.OnAllCreaturesAtNightKilled += CreaturesManager_OnAllCreaturesAtNightKilled;
            Player.Instance.OnPlayerDied += Player_OnPlayerDied;
            Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
            HubMerchant.OnPlayerStartedTalkingWithAnyHubMerchant += HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
            Fire.Instance.OnFireDamageTaken += Fire_OnFireDamageTaken;
            Player.Instance.OnPlayerStartedExploring += Player_OnPlayerStartedExploring;
            Player.Instance.OnPlayerStoppedExploring += Player_OnPlayerStoppedExploring;

            LevelManager.Instance.OnNewLocationShown += LevelManager_OnNewLocationShown;
            LevelManager.Instance.OnLevelFailed += LevelManager_OnLevelFailed;

            if(LevelManager.Instance.GetLevelSO().isNewEnvironmentDiscoveryLevel) {
                waitingToDiscoverLocation = true;
            }

            ResetLevelBackgroundTracksPooled();
            ResetLevelExplorationTracksPooled();

            audioSourceA.loop = false;
            audioSourceB.loop = false;
        }

        if (isMainMenuScene) {
            SetAudioVolume(mainMenuMusicAudioVolume);
            if (streamerMode) {
                audioSourceA.clip = mainMenuMusicStreamerMode;
            } else {
                audioSourceA.clip = mainMenuMusic;
            }
            if(!VersioningManager.Instance.GetNewSaveFile()) {
                PlayMusicDelayed(2f);
            }
        }

        if (isHUBScene) {
            if (streamerMode) {
                audioSourceA.clip = hubMusicStreamerMode;
            }
            else {
                audioSourceA.clip = hubMusic;
            }
        }

    }
    private void Update() {
        if (fireDamageTakenRecently != 0) {
            fireDamageTakenTimer -= Time.deltaTime;
            if (fireDamageTakenTimer < 0) {
                fireDamageTakenTimer = fireDamageTakenRemoveRate;
                fireDamageTakenRecently--;
            }
        }

        if (!CanPlayDayTrack()) return;
        if (isPlayingLevelDiscoveryMusic) return;
        if (isPlayingPeacefulMusic) return;
        if (isPlayingExplorationMusic) return;
        if (isPlayingClearRubbleMusic) return;

        if (isLevelScene) {
            peacefulTimer += Time.deltaTime;

            if (peacefulTimer >= minPeacefulTimerDelay) {
                playMusicAttemptTimer += Time.deltaTime;

                if (playMusicAttemptTimer > playMusicAttemptRate) {
                    playMusicAttemptTimer = 0;

                    float randomNumber = UnityEngine.Random.Range(0, 1f);

                    if (randomNumber < playMusicAttemptProbability) {
                        PlayRandomPeacefulMusic();
                    }
                }
            }
        }
    }
    private void ResetLevelBackgroundTracksPooled() {
        levelRandomBackgroundTracksPooled = new List<AudioClip>();

        foreach (AudioClip audioClip in levelRandomBackgroundTracks) {
            levelRandomBackgroundTracksPooled.Add(audioClip);
        }
    }
    private void ResetLevelExplorationTracksPooled() {
        levelExplorationTracksPooled = new List<AudioClip>();

        List<AudioClip> levelExplorationTracksToPool = new List<AudioClip>();
        if(streamerMode) {
            levelExplorationTracksToPool = levelExplorationTracksStreamerMode;
        } else {
            levelExplorationTracksToPool = levelExplorationTracks;
        }

        foreach (AudioClip audioClip in levelExplorationTracksToPool) {
            levelExplorationTracksPooled.Add(audioClip);
        }
    }

    private void Player_OnPlayerStoppedExploring(object sender, EventArgs e) {
        if(isPlayingExplorationMusic) {
            StopCurrentMusic(2f);
        }
    }

    private void Player_OnPlayerStartedExploring(object sender, EventArgs e) {
        TryPlayExplorationMusic();
    }

    private void TryPlayExplorationMusic() {
        if (!CanPlayDayTrack()) return;

        int ammoInHeldGun = PlayerShoot.Instance.GetHeldGun().GetCurrentAmmoClip();
        int ammoInInventory = UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.ammo).Count;
        int ammoInSecondaryGun = 0;

        if (PlayerShoot.Instance.GetSecondaryGunSO() != null) {
            ammoInSecondaryGun = PlayerShoot.Instance.GetGun(PlayerShoot.Instance.GetSecondaryGunSO()).GetCurrentAmmoClip();
        }
        int totalAmmo = ammoInHeldGun + ammoInInventory + ammoInSecondaryGun;
        int totalHealth = Player.Instance.GetHP();
        float dayTimerNormalized = DayNightManager.Instance.GetDayDurationNormalized();

        if (totalAmmo >= 6 && totalHealth >= 5 && dayTimerNormalized <= .3f) {
            // All conditions met : tick for music
            playExplorationMusicTick++;

            if (isPlayingPeacefulMusic) return;

            if (playExplorationMusicTick >= explorationMusicTickAmountToPlay) {
                playExplorationMusicTick = 0;
                isPlayingExplorationMusic = true;
                PlayRandomExplorationMusic();

            }
        }
    }

    private void Fire_OnFireDamageTaken(object sender, EventArgs e) {
        fireDamageTakenRecently ++;
        fireDamageTakenTimer = fireDamageTakenRemoveRate;
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {

        if (isPlayingLevelDiscoveryMusic || isPlayingPeacefulMusic || isPlayingExplorationMusic) {
            StartCoroutine(FadeInCoroutine(1f, volumeBeforeTalkingToNPC / 1.5f));
        }
    }

    private void HubMerchant_OnPlayerStartedTalkingWithAnyHubMerchant(object sender, EventArgs e) {

        if (isPlayingLevelDiscoveryMusic || isPlayingPeacefulMusic || isPlayingExplorationMusic) {
            volumeBeforeTalkingToNPC = audioSourceA.volume;
            StartCoroutine(FadeOutCoroutine(1f, volumeBeforeTalkingToNPC / 1.5f));
        }

    }

    private void PauseMenuUI_OnPauseMenuOpened(object sender, EventArgs e) {
        if(isPlayingEndLevelAreaMusic || isPlayingLevelDiscoveryMusic || isPlayingNightMusic || isPlayingPeacefulMusic || isPlayingExplorationMusic) {

            musicAudioLevelReducedWithPause = true;
            audioSourceA.volume /= 2.5f;
        }
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, EventArgs e) {
        if(musicAudioLevelReducedWithPause) {
            audioSourceA.volume *= 2.5f;
        }
    }


    private void SettingsManager_OnSteamerModeChanged(object sender, EventArgs e) {
        streamerMode = SettingsManager.Instance.GetStreamerMode();
        Debug.Log(audioSourceA.clip);
        if(isMainMenuScene) {
            if(streamerMode) {
                StartCoroutine(FadeOutThenInCoroutine(1f, 1f, mainMenuMusicStreamerMode));
            } else {
                StartCoroutine(FadeOutThenInCoroutine(1f, 1f, mainMenuMusic));
            }
        }
        if (isHUBScene) {
            if (streamerMode) {
                StartCoroutine(FadeOutThenInCoroutine(1f, 1f, hubMusicStreamerMode));
            }
            else {
                StartCoroutine(FadeOutThenInCoroutine(1f, 1f, hubMusic));
            }
        }
    }

    private void SettingsManager_OnMusicVolumeChanged(object sender, System.EventArgs e) {
        musicSettingVolume = SettingsManager.Instance.GetMusicVolume();
        SetAudioVolume(musicSettingVolume);
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource == NewLocationMusicInterruptionSource.buildFire) {
            StopCurrentMusic(2f);
        }

        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource == NewLocationMusicInterruptionSource.none) {
            float startVolume = GetCurrentMusicVolume();
            StartCoroutine(FadeOutCoroutine(2f, startVolume/2f));
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (isPlayingNightMusic || isPlayingNightIntroMusic) return;
        StopCurrentMusic(1f);
    }

    private void LevelManager_OnNewLocationShown(object sender, System.EventArgs e) {
        audioSourceA.clip = LevelManager.Instance.GetLevelSO().newEnvironmentDiscoveryAudioClip;
        audioSourceA.volume = discoverNewLocationAudioVolume;
        PlayMusicDelayed(4f);
        isPlayingLevelDiscoveryMusic = true;
        waitingToDiscoverLocation = false;
    }

    private void LevelManager_OnLevelFailed(object sender, EventArgs e) {
        StopCurrentMusic(5f);
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {

        audioSourceA.loop = true;
        audioSourceB.loop = true;
        isPlayingNightMusic = true;

        StartCoroutine(PlayIntroNighMusicDelayed(2f));

        SetAudioTargerVolume(nightMusicAudioVolume * musicSettingVolume);
        //StartCoroutine(FadeInDelayedCoroutine(3f, 4f));
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        isDuskOrNight = true;
        StopCurrentMusic(2f);
    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (!isLevelScene) return;
        if (isPlayingEndLevelAreaMusic) return;
        if (isPlayingNightMusic) return;
        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource != NewLocationMusicInterruptionSource.creatureAggro) return;
        if (isPlayingExplorationMusic) return;
        if (isPlayingClearRubbleMusic) return;

        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        FadeOutMusic(2f);
    }

    private bool CanPlayDayTrack() {
        if (waitingToDiscoverLocation) return false;
        if (isDuskOrNight) return false;
        if (isPlayingEndLevelAreaMusic) return false;

        return true;
    }


    [Button]
    private void PlayRandomPeacefulMusic() {

        if (levelRandomBackgroundTracks.Count == 0) return;
        AudioClip randomMusic = levelRandomBackgroundTracksPooled[UnityEngine.Random.Range(0, levelRandomBackgroundTracksPooled.Count)];
        audioSourceA.clip = randomMusic;
        targetVolume = backgroundTracksAudioVolume * musicSettingVolume;
        FadeInMusic(5f);

        levelRandomBackgroundTracksPooled.Remove(randomMusic);
        if(levelRandomBackgroundTracksPooled.Count == 0) {
            ResetLevelBackgroundTracksPooled();
        }

        isPlayingPeacefulMusic = true;
    }

    [Button]
    private void PlayRandomExplorationMusic() {

        if (levelExplorationTracks.Count == 0) return;

        AudioClip randomMusic = levelExplorationTracksPooled[UnityEngine.Random.Range(0, levelExplorationTracksPooled.Count)];
        audioSourceA.clip = randomMusic;
        targetVolume = backgroundTracksAudioVolume * musicSettingVolume;
        FadeInMusic(5f);

        levelExplorationTracksPooled.Remove(randomMusic);
        if (levelExplorationTracksPooled.Count == 0) {
            ResetLevelExplorationTracksPooled();
        }

        isPlayingExplorationMusic = true;
    }

    private void CreaturesManager_OnAllCreaturesAtNightKilled(object sender, EventArgs e) {
        if (!isLevelScene) return;

        audioSourceA.loop = false;
        audioSourceB.loop = false;

        if(nightCoroutine != null) {
            StopCoroutine(nightCoroutine);
        }
        AudioClip outroAudioClip = nightMusicOutro;
        if(streamerMode) {
            outroAudioClip = nightMusicOutroStreamer;
        }
        CrossfadeToNextNightClip(outroAudioClip);

        StartCoroutine(FadeOutDelayedCoroutine(5f, 2f));
        isDuskOrNight = false;
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        isPlayingNightMusic = false;
    }

    private IEnumerator PlayIntroNighMusicDelayed(float delay) {
        yield return new WaitForSeconds(delay);
        float length = 0f;
        if(streamerMode) {
            audioSourceA.clip = nightMusicIntroStreamer;
            length = nightMusicIntroStreamer.length;

        } else {
            audioSourceA.clip = nightMusicIntro;
            length = nightMusicIntro.length;
        }
        
        isPlayingNightIntroMusic = true;
        audioSourceA.volume = nightMusicAudioVolume * musicSettingVolume;
        audioSourceA.Play();
        isUsingAudioSourceA = true;

        StartCoroutine(WaitForClipToEnd(length));
    }

    private void PlayFirstNightClip() {

        AudioClip nextClip = nightMusicIntroLoop;
        if(streamerMode) {
            nextClip = nightMusicIntroLoopStreamer;
        }

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

        List<AudioClip> selectedNightMusicTension1Loops = nightMusicTension1Loops;
        List<AudioClip> selectedNightMusicTension2Loops = nightMusicTension2Loops;
        List<AudioClip> selectedNightMusicTension3Loops = nightMusicTension3Loops;
        List<AudioClip> selectedNightMusicTension4Loops = nightMusicTension4Loops;

        if(streamerMode) {
            selectedNightMusicTension1Loops = nightMusicTension1LoopsStreamer;
            selectedNightMusicTension2Loops = nightMusicTension2LoopsStreamer;
            selectedNightMusicTension3Loops = nightMusicTension3LoopsStreamer;
            selectedNightMusicTension4Loops = nightMusicTension4LoopsStreamer;
        }

        if (fireDamageTakenRecently >= 3) {
            // fire just took a bunch of damage : player in deep ****
            if (tensionLevelMusicPlaying == 4) {
                selectedAudioClip = currentAudioClipPlaying;
            }
            else {
                selectedAudioClip = selectedNightMusicTension4Loops[UnityEngine.Random.Range(0, selectedNightMusicTension4Loops.Count)];
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
                        selectedAudioClip = selectedNightMusicTension3Loops[UnityEngine.Random.Range(0, selectedNightMusicTension3Loops.Count)];
                    }
                    tensionLevelMusicPlaying = 3;
                }
                else {
                    // big amount of creatures outside player camp : player in deep ****
                    if (tensionLevelMusicPlaying == 4) {
                        selectedAudioClip = currentAudioClipPlaying;
                    }
                    else {
                        selectedAudioClip = selectedNightMusicTension4Loops[UnityEngine.Random.Range(0, selectedNightMusicTension4Loops.Count)];
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
                    selectedAudioClip = selectedNightMusicTension4Loops[UnityEngine.Random.Range(0, selectedNightMusicTension4Loops.Count)];
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
                    selectedAudioClip = selectedNightMusicTension1Loops[UnityEngine.Random.Range(0, selectedNightMusicTension1Loops.Count)];
                }
                tensionLevelMusicPlaying = 1;
            }

            if (creaturesCloseToPlayerCamp >= 5 && creaturesCloseToPlayerCamp < 12) {
                if (tensionLevelMusicPlaying == 2) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension2Loops[UnityEngine.Random.Range(0, selectedNightMusicTension2Loops.Count)];
                }
                tensionLevelMusicPlaying = 2;
            }

            if (creaturesCloseToPlayerCamp >= 12 && creaturesCloseToPlayerCamp < 20) {
                if (tensionLevelMusicPlaying == 3) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension3Loops[UnityEngine.Random.Range(0, selectedNightMusicTension3Loops.Count)];
                }
                tensionLevelMusicPlaying = 3;
            }

            if (creaturesCloseToPlayerCamp >= 20) {
                if (tensionLevelMusicPlaying == 4) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension4Loops[UnityEngine.Random.Range(0, selectedNightMusicTension4Loops.Count)];
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
        float maxVolume = nightMusicAudioVolume * musicSettingVolume;

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
        StartCoroutine(PlayMusicDelayedCoroutine(delay));
    }
    public IEnumerator PlayMusicDelayedCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        audioSourceA.Play();
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

    private void StopCurrentMusic(float delay) {
        peacefulTimer = 0;
        playMusicAttemptTimer = 0;
        FadeOutMusic(delay);
    }

    public void FadeOutMusic(float fadeDuration) {
        isPlayingLevelDiscoveryMusic = false;
        isPlayingPeacefulMusic = false;
        isPlayingEndLevelAreaMusic = false;
        isPlayingExplorationMusic = false;
        isPlayingClearRubbleMusic = false;

        StartCoroutine(FadeOutCoroutine(fadeDuration, 0));
    }

    private IEnumerator FadeOutCoroutine(float fadeDuration, float targetFadeVolume) {
        float startVolume = GetCurrentMusicVolume();

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

    private float GetCurrentMusicVolume() {
        AudioSource activeSource = isUsingAudioSourceA ? audioSourceA : audioSourceB;
        return activeSource.volume;
    }
    public void SetAudioVolume(float volume) {
        audioSourceA.volume = volume * musicSettingVolume;
        audioSourceB.volume = volume * musicSettingVolume;
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

    public void SetClearingObstacleMusic(float fadeInDuration) {
        isPlayingClearRubbleMusic = true;
        targetVolume = discoverNewLocationAudioVolume * musicSettingVolume;

        if (audioSourceA.isPlaying) {

            StartCoroutine(FadeOutThenInCoroutine(fadeInDuration, fadeInDuration, clearingObstacleCreaturesSpawningClip));

        }
        else {
            audioSourceA.clip = clearingObstacleCreaturesSpawningClip;
            FadeInMusic(fadeInDuration);
        }
    }
    public void StopCurrentMusic() {
        StopCurrentMusic(3f);
    }

    public void PauseMusic() {
        audioSourceA.Pause();
    }

    public void PlayMusic() {
        audioSourceA.Play();
    }
    public bool GetPlayingMusic() {
        return audioSourceA.isPlaying;
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
