using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUI_WaveInfoUI : MonoBehaviour
{
    public static LevelUI_WaveInfoUI Instance;
    [SerializeField] private Animator uiAnimator;

    private bool waveInfoShown;
    private void Awake() {
        Instance = this;
    }

    public void ShowWaveInfoUI() {
        if (waveInfoShown) return;

        uiAnimator.ResetTrigger("Hide");
        uiAnimator.SetTrigger("Show");

        waveInfoShown = true;
    }
    public void HideWaveInfoUI() {
        if (!waveInfoShown) return;

        uiAnimator.ResetTrigger("Show");
        uiAnimator.SetTrigger("Hide");

        waveInfoShown = false;
    }
}
