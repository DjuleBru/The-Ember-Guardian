using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunJamHandler : MonoBehaviour
{
    public enum QTEType {
        InputSequence,
        SpamButton,
        TimingChallenge,
    }
    [SerializeField] private bool useDebugJam;
    [SerializeField] private QTEType debugJamQTEType;

    private QTEType currentQTEType;
    private Gun gun;
    private bool gunJammed;
    private bool isInGunJamQTE;

    private Queue<GameInput.Binding> initialInputSequence;
    private Queue<GameInput.Binding> currentInputSequence;

    List<GameInput.Binding> allowedQTEInputs = new List<GameInput.Binding> {
    GameInput.Binding.reload,
    GameInput.Binding.ability1,
    GameInput.Binding.ability2,
    GameInput.Binding.buildingFunctionLeft,
    GameInput.Binding.buildingFunctionRight,
    GameInput.Binding.callDoggo,
    GameInput.Binding.torchOnOff,
    GameInput.Binding.roll,
    };

    private int currentInputIndex;
    private bool isProgressing;
    private bool justFailed;
    private float jamHitAnimationDuration = .3f;

    private int timingIndexAmount;

    private float spamProgress;
    private float spamTargetProgress;
    private float spamDecayRate = .4f;
    private int spamStageCount;
    private int lastStageReached;
    private float spamIncreasePerPress = .25f;

    private Coroutine progressInInputSequenceQTECoroutine;

    public event EventHandler OnCorrectJamSequenceInput;
    public static event EventHandler OnAnyCorrectJamSequenceInput;
    public static event EventHandler OnAnySpamButtonPressed;
    public static event EventHandler OnAnyTimingButtonPressed;
    public static event EventHandler<OnAnyJamSequenceProgressedEventArgs> OnAnyJamSequenceProgressed;
    public static event EventHandler<OnSpamQTEProgressedEventArgs> OnSpamQTEProgressed;
    public static event EventHandler<OnJamSequenceGeneratedEventArgs> OnAnyJamSequenceGenerated;
    public event EventHandler OnJamSequenceCompleted;
    public static event EventHandler OnAnyJamSequenceCompleted;
    public event EventHandler OnJamSequenceFailStarted;
    public static event EventHandler OnAnyJamSequenceFailStarted;
    public event EventHandler OnJamSequenceFailed;
    public static event EventHandler OnAnyJamSequenceFailed;
    public static event EventHandler OnAnyJamSequenceCancelled;
    public static event EventHandler OnAnyJamSequenceRestarted;
    public class OnSpamQTEProgressedEventArgs : EventArgs {
        public float spamProgress;
    }
    
    public class OnAnyJamSequenceProgressedEventArgs : EventArgs {
        public int currentIndex;
    }
    public class OnJamSequenceGeneratedEventArgs : EventArgs {
        public QTEType qteType;
        public float spamTargetProgress;
        public Queue<GameInput.Binding> inputSequence;
    }

    private void Awake() {
        gun = GetComponent<Gun>();
    }

    private void Start() {
        gun.OnGunJammed += Gun_OnGunJammed;

        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoo_OnPlayerSwappedGun;

        GameInput.Instance.OnPlayerReloadPerformed += GameInput_OnPlayerReloadPerformed;
        GameInput.Instance.OnPlayerLeftSkillPerformed += GameInput_OnPlayerLeftSkillPerformed;
        GameInput.Instance.OnPlayerRightSkillPerformed += GameInput_OnPlayerRightSkillPerformed;
        GameInput.Instance.OnPlayerGunLightSwitch += GameInput_OnPlayerGunLightSwitch;
        GameInput.Instance.OnPlayerLeftSwitchPerformed += GameInput_OnPlayerLeftSwitchPerformed;
        GameInput.Instance.OnPlayerRightSwitchPerformed += GameInput_OnPlayerRightSwitchPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerJumpPerformed += GameInput_OnPlayerJumpPerformed;
    }


    private void Update() {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.SpamButton) return;

        if (spamProgress > 0f) {
            spamProgress -= spamDecayRate * Time.deltaTime;
            spamProgress = Mathf.Max(spamProgress, lastStageReached);


            OnSpamQTEProgressed?.Invoke(this, new OnSpamQTEProgressedEventArgs {
                spamProgress = spamProgress
            });
        }

        int stage = Mathf.FloorToInt(spamProgress);
        if (stage > lastStageReached && stage <= spamStageCount) {
            lastStageReached = stage;
            OnCorrectJamSequenceInput?.Invoke(this, EventArgs.Empty);
            OnAnyCorrectJamSequenceInput?.Invoke(this, EventArgs.Empty);
            StartCoroutine(ProgressInJamSequenceAfterDelay(jamHitAnimationDuration));
        }
    }

    private void PlayerShoo_OnPlayerSwappedGun(object sender, EventArgs e) {
        if (!gunJammed) return;

        if (PlayerShoot.Instance.GetHeldGun() == gun) {
            isInGunJamQTE = true;
            OnAnyJamSequenceRestarted?.Invoke(this, EventArgs.Empty);

        }
        else {
            isInGunJamQTE = false;
            CancelGunJamMiniGame();
        }
        
    }

    private void Gun_OnGunJammed(object sender, System.EventArgs e) {
        StartJamMiniGame();
    }

    private void StartJamMiniGame() {
        isInGunJamQTE = true;
        gunJammed = true;

        QTEType qteType = (QTEType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(QTEType)).Length);
        currentQTEType = qteType;

        if (useDebugJam) {
            currentQTEType = debugJamQTEType;
        }

        if (currentQTEType == QTEType.SpamButton) {
            spamProgress = 0f;
            lastStageReached = 0;
            spamStageCount = PlayerShoot.Instance.GetHeldGun().GetJamRepairHitAmount();
            spamTargetProgress = spamStageCount; // 1 touche = 1 unité

            currentInputSequence = new Queue<GameInput.Binding>();
            currentInputSequence.Enqueue(GameInput.Binding.roll);
        }

        if (currentQTEType == QTEType.TimingChallenge) {
            currentInputSequence = new Queue<GameInput.Binding>();
            currentInputSequence.Enqueue(GameInput.Binding.roll);

            timingIndexAmount = PlayerShoot.Instance.GetHeldGun().GetJamRepairHitAmount();
        }

        if (currentQTEType == QTEType.InputSequence) {
            currentInputSequence = GenerateRandomSequence(PlayerShoot.Instance.GetHeldGun().GetJamRepairHitAmount());
        }

        OnAnyJamSequenceGenerated?.Invoke(this, new OnJamSequenceGeneratedEventArgs {
            qteType = currentQTEType,
            spamTargetProgress = spamTargetProgress,
            inputSequence = currentInputSequence
        });

        initialInputSequence = new Queue<GameInput.Binding>();
        foreach (GameInput.Binding binding in currentInputSequence) {
            initialInputSequence.Enqueue(binding);
        }
        
        currentInputIndex = 0;
    }

    private Queue<GameInput.Binding> GenerateRandomSequence(int length = 3) {
        Queue<GameInput.Binding> sequence = new Queue<GameInput.Binding>();
        for (int i = 0; i < length; i++) {
            GameInput.Binding randomInput = allowedQTEInputs[UnityEngine.Random.Range(0, allowedQTEInputs.Count)];
            sequence.Enqueue(randomInput);
        }
        return sequence;
    }


    public bool GetIsExpectedBinding(GameInput.Binding binding) {
        GameInput.Binding expected = currentInputSequence.Peek();

        if (expected == binding) return true;
        return false;
    }

    public float GetSpamProgressNormalized() {
        return spamProgress / spamTargetProgress;
    }

    #region INPUT RESPONSE
    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.InputSequence) return;
        OnBindingPressed(GameInput.Binding.callDoggo);
    }

    private void GameInput_OnPlayerRightSwitchPerformed(object sender, System.EventArgs e) {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.InputSequence) return;
        OnBindingPressed(GameInput.Binding.buildingFunctionRight);
    }

    private void GameInput_OnPlayerLeftSwitchPerformed(object sender, System.EventArgs e) {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.InputSequence) return;
        OnBindingPressed(GameInput.Binding.buildingFunctionLeft);
    }

    private void GameInput_OnPlayerGunLightSwitch(object sender, System.EventArgs e) {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.InputSequence) return;
        OnBindingPressed(GameInput.Binding.torchOnOff);
    }

    private void GameInput_OnPlayerRightSkillPerformed(object sender, System.EventArgs e) {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.InputSequence) return;
        OnBindingPressed(GameInput.Binding.ability2);
    }

    private void GameInput_OnPlayerLeftSkillPerformed(object sender, System.EventArgs e) {
        if (!isInGunJamQTE) return;
        if (currentQTEType != QTEType.InputSequence) return;
        OnBindingPressed(GameInput.Binding.ability1);
    }

    private void GameInput_OnPlayerReloadPerformed(object sender, System.EventArgs e) {
        if (!isInGunJamQTE) return;

        OnBindingPressed(GameInput.Binding.reload);
    }

    private void GameInput_OnPlayerJumpPerformed(object sender, EventArgs e) {
        if (!isInGunJamQTE) return;

        if (currentQTEType == QTEType.SpamButton) {
            OnAnySpamButtonPressed?.Invoke(this, EventArgs.Empty);

            spamProgress += spamIncreasePerPress;
            spamProgress = Mathf.Min(spamProgress, spamTargetProgress + 1);
            return;
        }

        if (currentQTEType == QTEType.TimingChallenge) {
            if (isProgressing) return;
            OnAnyTimingButtonPressed?.Invoke(this, EventArgs.Empty);
            bool timingQTEWasRight;

            if (PlayerShoot.Instance.GetHeldGunSO() == PlayerShoot.Instance.GetPrimaryGunSO()) {
                timingQTEWasRight = PlayerUI_GunJam.PrimaryWeaponGunJamUI.TimingQTEIsRight();
            }
            else {
                timingQTEWasRight = PlayerUI_GunJam.SecondaryWeaponGunJamUI.TimingQTEIsRight();
            }

            if (timingQTEWasRight) {
                currentInputIndex++;
            }
            else {
                StartCoroutine(FailGunJamMiniGame());
            }
        }

        OnBindingPressed(GameInput.Binding.roll);
    }

    private void OnBindingPressed(GameInput.Binding binding) {
        if (!isInGunJamQTE) return;
        if (currentInputSequence.Count == 0) return;

        if (currentQTEType == QTEType.InputSequence) {
            OnBindingPressedInputSequence(binding);
            return;
        }

        if (isProgressing) return;
        if (justFailed) return;

        GameInput.Binding expected = currentInputSequence.Peek();
        if (binding == expected) {
            isProgressing = true;
            OnCorrectJamSequenceInput?.Invoke(this, EventArgs.Empty);
            OnAnyCorrectJamSequenceInput?.Invoke(this, EventArgs.Empty);
            StartCoroutine(ProgressInJamSequenceAfterDelay(jamHitAnimationDuration));
        }
        else {
            StartCoroutine(FailGunJamMiniGame());
        }
    }

    private void OnBindingPressedInputSequence(GameInput.Binding binding) {
        if (justFailed) return;
        if (!isInGunJamQTE) return;
        if (currentInputSequence.Count == 0) return;

        if(isProgressing) {
            if(progressInInputSequenceQTECoroutine != null) {
                StopCoroutine(progressInInputSequenceQTECoroutine);
            }
            progressInInputSequenceQTECoroutine = StartCoroutine(SkipCurrentProgressInInputSequence(jamHitAnimationDuration));
        }

        if (currentInputSequence.Count != 0) {

            GameInput.Binding expected = currentInputSequence.Peek();
            if (binding == expected) {

                OnCorrectJamSequenceInput?.Invoke(this, EventArgs.Empty);
                OnAnyCorrectJamSequenceInput?.Invoke(this, EventArgs.Empty);

                isProgressing = true;
                progressInInputSequenceQTECoroutine = StartCoroutine(ProgressInJamSequenceAfterDelay(jamHitAnimationDuration));
            }
            else {
                StartCoroutine(FailGunJamMiniGame());
            }
        }

    }

    private IEnumerator ProgressInJamSequenceAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        OnAnyJamSequenceProgressed?.Invoke(this, new OnAnyJamSequenceProgressedEventArgs {
            currentIndex = currentInputIndex,
        });

        if (currentQTEType == QTEType.InputSequence) {
            currentInputIndex++;
            currentInputSequence.Dequeue();

            if (currentInputSequence.Count == 0) {
                CompleteGunJamMiniGame();
            }
        }

        if (currentQTEType == QTEType.SpamButton) {
            if (spamProgress >= spamTargetProgress) {
                CompleteGunJamMiniGame();
            }
        }

        if (currentQTEType == QTEType.TimingChallenge) {
            if (currentInputIndex == timingIndexAmount) {
                CompleteGunJamMiniGame();
            }
        }

        isProgressing = false;
        progressInInputSequenceQTECoroutine = null;
    }

    private IEnumerator SkipCurrentProgressInInputSequence(float delay) {
        OnAnyJamSequenceProgressed?.Invoke(this, new OnAnyJamSequenceProgressedEventArgs {
            currentIndex = currentInputIndex,
        });

        currentInputIndex++;
        currentInputSequence.Dequeue();

        if (currentInputSequence.Count == 0) {
            yield return new WaitForSeconds(delay);
            CompleteGunJamMiniGame();
        }
    }

    private void CompleteGunJamMiniGame() {
        isInGunJamQTE = false;
        gunJammed = false;
        gun.SetGunUnJammed();
        OnJamSequenceCompleted?.Invoke(this, EventArgs.Empty);
        OnAnyJamSequenceCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void CancelGunJamMiniGame() {
        currentInputSequence = new Queue<GameInput.Binding>();
        foreach (GameInput.Binding binding in initialInputSequence) {
            currentInputSequence.Enqueue(binding);
        }

        currentInputIndex = 0;

        //For player feedbacks
        bool otherWeaponIsJammed = PlayerShoot.Instance.GetHeldGun().GetGunJammed();

        if (!otherWeaponIsJammed) {
        }
    }

    private IEnumerator FailGunJamMiniGame() {
        justFailed = true;
        OnJamSequenceFailStarted?.Invoke(this, EventArgs.Empty);
        OnAnyJamSequenceFailStarted?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(jamHitAnimationDuration);

        OnAnyJamSequenceFailed?.Invoke(this, EventArgs.Empty);
        OnJamSequenceFailed?.Invoke(this, EventArgs.Empty);
        currentInputSequence = new Queue<GameInput.Binding>();
        foreach (GameInput.Binding binding in initialInputSequence) {
            currentInputSequence.Enqueue(binding);
        }

        currentInputIndex = 0;

        justFailed = false;
    }

    #endregion
}
