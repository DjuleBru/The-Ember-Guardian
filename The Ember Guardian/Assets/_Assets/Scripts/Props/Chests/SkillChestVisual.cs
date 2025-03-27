using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillChestVisual : ChestVisual
{
    [SerializeField] private SpriteRenderer secondarySpriteRenderer;
    [SerializeField] private Chest_Special skillChest;

    protected override void Start()
    {
        base.Start();
        chest.OnChestUnlocked += Chest_OnChestUnlocked;
        chest.OnChestDisappear += Chest_OnChestDisappear1;
    }

    private void Chest_OnChestDisappear1(object sender, System.EventArgs e) {
        secondarySpriteRenderer.sprite = null;
    }

    private void Chest_OnChestUnlocked(object sender, System.EventArgs e) {
        secondarySpriteRenderer.sprite = skillChest.GetSkillSO().Icon;
        secondarySpriteRenderer.transform.localScale = Vector3.one * 1.1f;
    }
}
