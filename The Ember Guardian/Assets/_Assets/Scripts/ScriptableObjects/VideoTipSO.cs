using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu()]
public class VideoTipSO : ScriptableObject
{
    public TextSO tipName;
    public VideoClip tipClip;
    public List<TextSO> tipTextList;
    public List<float> tipTextDelayToShowList;
}
