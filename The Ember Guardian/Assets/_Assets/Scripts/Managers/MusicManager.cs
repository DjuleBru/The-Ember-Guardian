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
    private float masterSettingVolume;

    [SerializeField] private AudioClip mainMenuMusic; 
    [SerializeField] private AudioClip mainMenuMusicStreamerMode;

    [SerializeField] private AudioClip hubMusic;
    [SerializeField] private AudioClip hubMusicStreamerMode;

    [SerializeField] private AudioClip hordeModeMusic;

    [SerializeField] private AudioClip creditsMusic;

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
    [SerializeField] private AudioClip finalLevelTrack;

    [SerializeField] private AudioClip finalIntro;
    [SerializeField] private AudioClip finalLoop;
    [SerializeField] private AudioClip finalOutro;
    private float finalTrackVolume = 1f;
    private bool isFinalLoopPlaying;
    private bool finalOutroTriggered;
    private bool levelDiscoveryTrackFadedOut;
    private Coroutine finalMusicCoroutine;
    private Coroutine currentMusicEndCoroutine;

    private List<AudioClip> levelRandomBackgroundTracks;
    private List<AudioClip> levelRandomBackgroundTracksPooled;
    private List<AudioClip> levelExplorationTracks;
    private List<AudioClip> levelExplorationTracksStreamerMode;
    private List<AudioClip> levelExplorationTracksPooled;

    private float targetVolume;
    private float currentTrackVolumeMultiplier;
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
    private bool isTutorialScene;
    private bool isPlayingLevelDiscoveryMusic;
    private bool isPlayingPeacefulMusic;
    private bool isPlayingExplorationMusic;
    private bool isPlayingClearRubbleMusic;
    private bool isPlayingEndLevelAreaMusic;
    private bool isPlayingNightMusic;
    private bool isPlayingFinalLevelMusic;
    private bool musicAudioLevelReducedWithPause;
    private float musicVolumeReductionWithPause = 2.5f;
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
        SettingsManager.Instance.OnMasterVolumeChanged += SettingsManager_OnMasterVolumeChanged;
        musicSettingVolume = SettingsManager.Instance.GetMusicVolume();
        masterSettingVolume = SettingsManager.Instance.GetMasterVolume();
        streamerMode = SettingsManager.Instance.GetStreamerMode();

        if (PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.OnPauseMenuClosed += PauseMenuUI_OnPauseMenuClosed;
            PauseMenuUI.Instance.OnPauseMenuOpened += PauseMenuUI_OnPauseMenuOpened;
        }

        audioSourceA.ignoreListenerPause = true;
        audioSourceB.ignoreListenerPause = true;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        }
        
        CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;
        isLevelScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level;
        isMainMenuScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;
        isHUBScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB;
        isTutorialScene = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial;

        SetAudioVolume(musicSettingVolume);
        SetAudioTargerVolume(musicSettingVolume);

        if (VideoTipUI.Instance != null) {
            VideoTipUI.Instance.OnVideoTipPanelOpened += VideoTip_OnVideoTipPanelOpened;
        }

        if(isLevelScene || isTutorialScene) {
            levelRandomBackgroundTracks = LevelManager.Instance.GetLevelSO().levelRandomBackgroundTracks;
            levelExplorationTracks = LevelManager.Instance.GetLevelSO().levelExplorationTracks;
            levelExplorationTracksStreamerMode = LevelManager.Instance.GetLevelSO().levelExplorationTracksStreamerMode;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
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
            if (streamerMode) {
                audioSourceA.clip = mainMenuMusicStreamerMode;
            } else {
                audioSourceA.clip = mainMenuMusic;
            }

            audioSourceA.loop = false;
            if(!VersioningManager.Instance.GetNewSaveFile()) {
                PlayMusicDelayed(2f);
            }
        }

        if (isHUBScene) {
            SetHubMusic();
        }

    }

    public void FadeInToMainMenuMusic() {
        StartCoroutine(FadeOutThenInCoroutine(1f, 1f, mainMenuMusic));
        targetVolume = .75f;
    }
    public void FadeInToHordeModeMusic() {
        StartCoroutine(FadeOutThenInCoroutine(1f, 1f, hordeModeMusic));
        targetVolume = .5f;
    }

    public void SetHubMusic() {
        if (streamerMode) {
            audioSourceA.clip = hubMusicStreamerMode;
        }
        else {
            audioSourceA.clip = hubMusic;
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
            //StopCurrentMusic(2f);
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
        targetVolume = volumeBeforeTalkingToNPC;
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

            if (musicAudioLevelReducedWithPause) return;

            musicAudioLevelReducedWithPause = true;
            audioSourceA.volume /= musicVolumeReductionWithPause;
        }
    }

    private void PauseMenuUI_OnPauseMenuClosed(object sender, EventArgs e) {
        if(musicAudioLevelReducedWithPause) {
            audioSourceA.volume *= musicVolumeReductionWithPause;
            musicAudioLevelReducedWithPause = false;
        }
    }
    private void VideoTip_OnVideoTipPanelOpened(object sender, EventArgs e) {
        if (isPlayingEndLevelAreaMusic || isPlayingLevelDiscoveryMusic || isPlayingNightMusic || isPlayingPeacefulMusic || isPlayingExplorationMusic) {

            if (musicAudioLevelReducedWithPause) return;

            musicAudioLevelReducedWithPause = true;
            audioSourceA.volume /= musicVolumeReductionWithPause;
        }
    }

    private void SettingsManager_OnSteamerModeChanged(object sender, EventArgs e) {
        streamerMode = SettingsManager.Instance.GetStreamerMode();
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

    private void SettingsManager_OnMasterVolumeChanged(object sender, EventArgs e) {
        masterSettingVolume = SettingsManager.Instance.GetMasterVolume();
        SetAudioVolume(musicSettingVolume);
    }


    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {
        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource == NewLocationMusicInterruptionSource.none) {
            float startVolume = GetCurrentMusicVolume();
            StartCoroutine(FadeOutCoroutine(2f, startVolume/2f));
        }

        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource == NewLocationMusicInterruptionSource.buildFire) {
            FadeOutMusic(2f);
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        if (isPlayingNightMusic || isPlayingNightIntroMusic) return;
        StopCurrentMusic(1f);
    }

    private void LevelManager_OnNewLocationShown(object sender, System.EventArgs e) {
        audioSourceA.clip = LevelManager.Instance.GetLevelSO().newEnvironmentDiscoveryAudioClip;

        PlayMusicDelayed(4f);
        isPlayingLevelDiscoveryMusic = true;
        waitingToDiscoverLocation = false;

        SetAudioVolume(musicSettingVolume);
        StartCoroutine(SetIsPlayingLevelDiscoveryMusicToFalseAfterTrack(audioSourceA.clip.length));
    }

    private IEnumerator SetIsPlayingLevelDiscoveryMusicToFalseAfterTrack(float trackTime) {
        yield return new WaitForSeconds(trackTime);
        isPlayingLevelDiscoveryMusic = false;
    }


    private void LevelManager_OnLevelFailed(object sender, EventArgs e) {
        StopCurrentMusic(5f);
    }

    private void DayNightManager_OnNightStart(object sender, EventArgs e) {
        if (isPlayingFinalLevelMusic) return;

        audioSourceA.loop = true;
        audioSourceB.loop = true;
        isPlayingNightMusic = true;

        StartCoroutine(PlayIntroNighMusicDelayed(2f));
        SetAudioTargerVolume(musicSettingVolume);
    }

    private void DayNightManager_OnDayStart(object sender, EventArgs e) {

        if (LevelManager.Instance.GetIsFinalLevelNight()) {
            PlayFinalLevelMusic();
            isPlayingFinalLevelMusic = true;
        }

    }
    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!isLevelScene) return;

        isDuskOrNight = true;

        if (isPlayingFinalLevelMusic) return;
        StopCurrentMusic(2f);

    }

    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        if (!isLevelScene) return;
        if (isPlayingEndLevelAreaMusic) return;
        if (isPlayingNightMusic) return;
        if (isPlayingLevelDiscoveryMusic && discoveryMusicInterruptionSource != NewLocationMusicInterruptionSource.creatureAggro) return;
        if (isPlayingExplorationMusic) return;
        if (isPlayingClearRubbleMusic) return;

        if (levelDiscoveryTrackFadedOut) return;
        levelDiscoveryTrackFadedOut = true;

        float startVolume = GetCurrentMusicVolume();
        StartCoroutine(FadeOutCoroutine(2f, startVolume / 2.25f));
    }

    private bool CanPlayDayTrack() {
        if (waitingToDiscoverLocation) return false;
        if (isDuskOrNight) return false;
        if (isPlayingEndLevelAreaMusic) return false;
        if (isPlayingExplorationMusic) return false;
        if (isPlayingLevelDiscoveryMusic) return false;

        return true;
    }


    [Button]
    private void PlayRandomPeacefulMusic() {

        if (levelRandomBackgroundTracks.Count == 0) return;
        AudioClip randomMusic = levelRandomBackgroundTracksPooled[UnityEngine.Random.Range(0, levelRandomBackgroundTracksPooled.Count)];
        audioSourceA.clip = randomMusic;
        SetAudioTargerVolume(backgroundTracksAudioVolume);
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
        SetAudioTargerVolume(backgroundTracksAudioVolume);
        FadeInMusic(5f);

        levelExplorationTracksPooled.Remove(randomMusic);
        if (levelExplorationTracksPooled.Count == 0) {
            ResetLevelExplorationTracksPooled();
        }

        isPlayingExplorationMusic = true;
    }

    private void CreaturesManager_OnAllCreaturesAtNightKilled(object sender, EventArgs e) {
        Debug.Log("CreaturesManager_OnAllCreaturesAtNightKilled isPlayingFinalLevelMusic" + isPlayingFinalLevelMusic);


        if (isPlayingFinalLevelMusic) {
            TriggerFinalLevelOutro();
            return;
        }

        if (isLevelScene || isTutorialScene) {

            audioSourceA.loop = false;
            audioSourceB.loop = false;

            if (nightCoroutine != null) {
                StopCoroutine(nightCoroutine);
            }

            AudioClip outroAudioClip = nightMusicOutro;
            if (streamerMode) {
                outroAudioClip = nightMusicOutroStreamer;
            }



            CrossfadeToNextNightClip(outroAudioClip, false, .5f);

            StartCoroutine(FadeOutDelayedCoroutine(5f, 2f));
            isDuskOrNight = false;
            peacefulTimer = 0;
            playMusicAttemptTimer = 0;
            isPlayingNightMusic = false;

        };

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
        audioSourceA.Play();
        SetAudioVolume(musicSettingVolume);
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
        if (isPlayingFinalLevelMusic) return;

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
        if (isPlayingFinalLevelMusic) yield break;
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
            if(fireDamageTakenRecently > 5) {

                if (tensionLevelMusicPlaying == 4) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension4Loops[UnityEngine.Random.Range(0, selectedNightMusicTension4Loops.Count)];
                }
                tensionLevelMusicPlaying = 4;

            } else {

                if (tensionLevelMusicPlaying == 3) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension4Loops[UnityEngine.Random.Range(0, selectedNightMusicTension3Loops.Count)];
                }
                tensionLevelMusicPlaying = 3;

            }

           
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

            if (creaturesCloseToPlayerCamp < 4) {
                if (tensionLevelMusicPlaying == 1) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension1Loops[UnityEngine.Random.Range(0, selectedNightMusicTension1Loops.Count)];
                }
                tensionLevelMusicPlaying = 1;
            }

            if (creaturesCloseToPlayerCamp >= 4 && creaturesCloseToPlayerCamp < 15) {
                if (tensionLevelMusicPlaying == 2) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension2Loops[UnityEngine.Random.Range(0, selectedNightMusicTension2Loops.Count)];
                }
                tensionLevelMusicPlaying = 2;
            }

            if (creaturesCloseToPlayerCamp >= 15 && creaturesCloseToPlayerCamp < 25) {
                if (tensionLevelMusicPlaying == 3) {
                    selectedAudioClip = currentAudioClipPlaying;
                }
                else {
                    selectedAudioClip = selectedNightMusicTension3Loops[UnityEngine.Random.Range(0, selectedNightMusicTension3Loops.Count)];
                }
                tensionLevelMusicPlaying = 3;
            }

            if (creaturesCloseToPlayerCamp >= 25) {
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

    private void CrossfadeToNextNightClip(AudioClip newClip, bool syncClipTimes = true, float duration = 2f) {
        AudioSource activeSource = isUsingAudioSourceA ? audioSourceA : audioSourceB;
        AudioSource nextSource = isUsingAudioSourceA ? audioSourceB : audioSourceA;

        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        StartCoroutine(CrossfadeCoroutine(activeSource, nextSource, duration, syncClipTimes));

        isUsingAudioSourceA = !isUsingAudioSourceA;
    }

    private IEnumerator CrossfadeCoroutine(AudioSource fromSource, AudioSource toSource, float duration, bool syncClipTimes = true) {
        float elapsedTime = 0f;
        float maxVolume = fromSource.volume;

        float clipTime = fromSource.time; // Récupère le temps de lecture actuel
        if (clipTime > fromSource.clip.length - 1f) {
            clipTime = 0;
        }

        if(syncClipTimes) {
            toSource.time = clipTime;
        }


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

    [Button]
    public void PlayFinalLevelMusic() {
        if (isPlayingFinalLevelMusic) return;

        Debug.Log("PlayFinalLevelMusic");

        isPlayingFinalLevelMusic = true;
        isPlayingNightMusic = false;
        isPlayingNightIntroMusic = false;

        if (nightCoroutine != null)
            StopCoroutine(nightCoroutine);

        audioSourceA.Stop();
        audioSourceB.Stop();

        audioSourceA.loop = false;
        audioSourceA.clip = finalIntro;

        SetAudioVolume(finalTrackVolume);
        SetAudioTargerVolume(finalTrackVolume);

        audioSourceA.Play();

        finalMusicCoroutine = StartCoroutine(FinalLevelMusicFlow());
    }

    private IEnumerator FinalLevelMusicFlow() {
        // Attente de la fin de l’intro
        yield return new WaitForSeconds(finalIntro.length);

        if (finalOutroTriggered) yield break;

        // Lancer la loop
        audioSourceA.clip = finalLoop;
        audioSourceA.loop = true;
        audioSourceA.Play();

        isFinalLoopPlaying = true;
    }

    [Button]
    public void TriggerFinalLevelOutro() {
        Debug.Log("TriggerFinalLevelOutro");
        if (!isPlayingFinalLevelMusic) return;
        if (finalOutroTriggered) return;

        finalOutroTriggered = true;

        if (finalMusicCoroutine != null)
            StopCoroutine(finalMusicCoroutine);

        StartCoroutine(PlayFinalOutroCoroutine());
    }

    private IEnumerator PlayFinalOutroCoroutine() {
        // Petite sécurité si on est en loop
        audioSourceA.loop = false;

        // Fade out de la loop
        yield return StartCoroutine(FadeOutCoroutine(2f, 0));

        audioSourceA.clip = finalOutro;
        audioSourceA.volume = 0;
        audioSourceA.Play();

        yield return StartCoroutine(FadeInCoroutine(2f, 0));

        // Laisser l’outro se terminer normalement
        yield return new WaitForSeconds(finalOutro.length);

        isPlayingFinalLevelMusic = false;
        isFinalLoopPlaying = false;
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
        //Debug.Log("SetAudioVolume : " + volume);

        if (isLevelScene && isPlayingLevelDiscoveryMusic) {
            volume *= discoverNewLocationAudioVolume;
        }

        if (isMainMenuScene) {
            volume *= mainMenuMusicAudioVolume;
        }

        if(musicAudioLevelReducedWithPause) {
            volume /= musicVolumeReductionWithPause;
        }

        if(isPlayingNightMusic || isPlayingNightIntroMusic) {
            volume *= nightMusicAudioVolume;
        }

        volume *= musicSettingVolume;
        volume *= masterSettingVolume;

        //Debug.Log("Audio volume set : " + volume + " isPlayingNightIntroMusic " + isPlayingNightIntroMusic + " isPlayingNightMusic " + isPlayingNightMusic);

        audioSourceA.volume = volume;
        audioSourceB.volume = volume;
    }

    public void SetAudioTargerVolume(float volume) {
        Debug.Log("SetAudioTargerVolume : " + volume);
        if (isLevelScene && isPlayingLevelDiscoveryMusic) {
            volume *= discoverNewLocationAudioVolume;
        }

        if (isMainMenuScene) {
            volume *= mainMenuMusicAudioVolume;
        }

        if (musicAudioLevelReducedWithPause) {
            volume /= musicVolumeReductionWithPause;
        }

        if (isPlayingNightMusic || isPlayingNightIntroMusic) {
            volume *= nightMusicAudioVolume;
        }

        volume *= musicSettingVolume;
        volume *= masterSettingVolume;

        Debug.Log("Audio volume target set : " + volume + " isPlayingNightIntroMusic " + isPlayingNightIntroMusic + " isPlayingNightMusic " + isPlayingNightMusic);
        targetVolume = volume;
    }

    public void SetTargetVolumeToMainTrack() {
        targetVolume = discoverNewLocationAudioVolume * musicSettingVolume * masterSettingVolume;
    }

    public void PlayCreditsMusic() {
        Debug.Log("PlayCreditsMusic");
        audioSourceA.clip = creditsMusic;
        audioSourceA.Play();
    }

    public void FadeInMusic(float fadeDuration) {
        StartCoroutine(FadeInCoroutine(fadeDuration, 0));
    }

    public void SetEndLevelMusic(float fadeInDuration) {
        Debug.Log("SetEndLevelMusic ");
        if (isPlayingEndLevelAreaMusic) return;
        isPlayingEndLevelAreaMusic = true;

        SetAudioTargerVolume(discoverNewLocationAudioVolume);

        if (audioSourceA.isPlaying) {

            StartCoroutine(FadeOutThenInCoroutine(fadeInDuration, fadeInDuration, endLevelMusic));

        } else {
            audioSourceA.clip = endLevelMusic;
            FadeInMusic(fadeInDuration);
        }

    }

    public void SetClearingObstacleMusic(float fadeInDuration) {
        isPlayingClearRubbleMusic = true;
        SetAudioTargerVolume(discoverNewLocationAudioVolume);

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

        if (isLevelScene ||isTutorialScene) {
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
