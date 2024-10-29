using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineVisual : StructureVisual
{
    [SerializeField] protected Animator shrineAnimator;
    protected Shrine shrine;

    protected override void Awake() {
        base.Awake();
        shrine = GetComponentInParent<Shrine>();
    }

    protected override void Start() {
        shrine.OnShrineActivated += Shrine_OnShrineActivated;
    }

    private void Shrine_OnShrineActivated(object sender, System.EventArgs e) {
        shrineAnimator.SetTrigger("ActivateShrine");
    }
}
