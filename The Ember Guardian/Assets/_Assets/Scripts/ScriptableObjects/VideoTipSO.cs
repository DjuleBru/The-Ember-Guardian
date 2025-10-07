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
        Trap,
        GunJams,
        GunManagement,
        WorldPortals,
        ScavengableObstacle,
        Scavengers,
        ArchitectTable,
        Engineers_basic,
        Engineers_advanced,
        ThroneArtifact,
        Mines,
        SpecialAmmo,
        Wind,
        Guards,
        SecondaryFires,
        SkillsMerchant,
        FindingWeapon,
        Saving,
    }

    public VideoTipType tipType;
    public string tipNameLocalizationKey;
    public VideoClip tipClip;
    public List<string> tipTextLocalizationKeys;
    public List<float> tipTextDelayToShowList;
}
