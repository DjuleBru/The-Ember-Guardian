using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual_LMG : GunVisual
{
    [SerializeField] private SpriteRenderer bipodRenderer;

    private bool bipodEnabled;  

    protected override void Start() {
        base.Start();
        bipodRenderer.enabled = false;

        PlayerShoot.Instance.OnPlayerSetupLMGBipod += PlayerShoot_OnPlayerSetupLMGBipod;
        PlayerShoot.Instance.OnPlayerSetupLMGStopped += PlayerShoot_OnPlayerSetupLMGStopped;
    }

    private void PlayerShoot_OnPlayerSetupLMGStopped(object sender, System.EventArgs e) {
        bipodEnabled = false;
        bipodRenderer.enabled = bipodEnabled;
    }

    private void PlayerShoot_OnPlayerSetupLMGBipod(object sender, System.EventArgs e) {
        bipodEnabled = true;
        bipodRenderer.enabled = bipodEnabled;
    }

}
