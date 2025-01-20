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

        PlayerShoot.Instance.OnPlayerSwitchedFireMode += PlayerShoot_OnPlayerSwitchedFireMode;
    }

    private void PlayerShoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        bipodEnabled = !bipodEnabled;
        bipodRenderer.enabled = bipodEnabled;
    }
}
