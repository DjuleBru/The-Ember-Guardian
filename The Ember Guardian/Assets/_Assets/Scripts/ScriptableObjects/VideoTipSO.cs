using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu()]
public class VideoTipSO : ScriptableObject
{

    public enum VideoTipType {
        Reloading,
        CritHit,
        Roll,
        FireManagement,
        Die,
        HealTent,
        SetupEconomy,
        SetupDefenses,
        EmberExtraction,
        DayNightCycle,
        WorkerCamps,
        Hunters,
        GunTip,
        StoringGems,
        SwapWeapon,
        HuntersFlag,
        DemoEnded,
    }

    public VideoTipType tipType;
    public TextSO tipName;
    public VideoClip tipClip;
    public List<TextSO> tipTextList;
    public List<float> tipTextDelayToShowList;
}
