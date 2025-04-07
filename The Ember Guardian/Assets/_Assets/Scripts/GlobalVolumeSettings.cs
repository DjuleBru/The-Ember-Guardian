using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeSettings : MonoBehaviour
{

    private Volume globalVolume;
    private LiftGammaGain liftGammaGain;

    private void Start() {
        globalVolume= GetComponent<Volume>();   
        if (!globalVolume.profile.TryGet(out liftGammaGain)) throw new System.NullReferenceException(nameof(liftGammaGain));
        
        liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, SettingsManager.Instance.GetGammaLevel()));
    }
}
