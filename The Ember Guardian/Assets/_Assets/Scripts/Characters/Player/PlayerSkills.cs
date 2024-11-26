using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    public static PlayerSkills Instance;

    private List<SkillItem> passiveSkillList = new List<SkillItem>();
    private SkillItem activeSkillLeft;
    private SkillItem activeSkillRight;

    public event EventHandler<OnSkillAddedEventArgs> OnActiveSkillAdded;
    public event EventHandler<OnSkillAddedEventArgs> OnPassiveSkillAdded;

    public class OnSkillAddedEventArgs : EventArgs {
        public SkillItem skillItemAdded;
    }

    private void Awake() {
        Instance = this;
    }

    public void AddActiveSkill(SkillItem skillItem) {
        if (activeSkillLeft != null && activeSkillRight != null) return;

        if(activeSkillLeft == null) {
            activeSkillLeft = skillItem;
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem
            });
            return;
        }

        if (activeSkillRight == null) {
            activeSkillRight = skillItem;
            OnActiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
                skillItemAdded = skillItem
            });
            return;
        }
    }

    public void AddPassiveSkill(SkillItem skillItem) {

        SkillSO skillSO = skillItem.GetSkillSO();
        SkillItem skillItemCopy = new SkillItem();
        skillItemCopy.Initialize(skillSO);

        SkillItem foundSkillItem = null;

        foreach (SkillItem passiveSkill in passiveSkillList) {
            if (passiveSkill.itemName == skillItemCopy.itemName) {
                foundSkillItem = passiveSkill;
            }
        }

        if (foundSkillItem != null) {
            foundSkillItem.currentLevel++;
        } else {
            passiveSkillList.Add(skillItemCopy);
        }

        OnPassiveSkillAdded?.Invoke(this, new OnSkillAddedEventArgs {
            skillItemAdded = skillItemCopy
        });

        ApplyPassiveSkillEffect(skillItem);
    }

    public void ApplyPassiveSkillEffect(SkillItem skillItem) {
        SkillSO skillItemSO = skillItem.GetSkillSO();
        PassiveSkillEffectSO skillEffect = skillItemSO.passiveSkillEffect;

        float buffEffectValue = skillEffect.GetValueAtLevel(skillItem.currentLevel);

        if(skillItem.currentLevel > 1) {
            buffEffectValue -= skillEffect.GetValueAtLevel(skillItem.currentLevel-1);
        }

        if (skillEffect != null) {
            switch (skillEffect.skillType) {
                case SkillItem.SkillType.passiveMoveSpeedBuff:

                    PlayerStats.Instance.BuffMoveSpeed(buffEffectValue/100);

                    break;

                case SkillItem.SkillType.passiveRunAccelerationFactorBuff:

                    PlayerStats.Instance.BuffRunAccelerationFactor(buffEffectValue / 100);

                    break;

                case SkillItem.SkillType.passiveRunMaxTimeBuff:

                    PlayerStats.Instance.BuffRunMaxTime(buffEffectValue);

                    break;

                default:
                    Debug.LogWarning($"Unhandled skill type: {skillEffect.skillType}");
                    break;
            }
        }
    }

    public SkillItem GetActiveSkillLeft() { return activeSkillLeft;}

    public SkillItem GetActiveSkillRight() { return activeSkillRight;}

    public List<SkillItem> GetActiveSkillList() {

        List < SkillItem > activeSkillList = new List<SkillItem> ();
        activeSkillList.Add(activeSkillRight);
        activeSkillList.Add (activeSkillLeft);

        return activeSkillList; 
    }

    public List<SkillItem> GetPassiveSkillList() { return passiveSkillList; }


    public int GetCurrentSkillLevel(SkillItem skillItem) {
        foreach(SkillItem playerSkillItem in passiveSkillList) {
            if(playerSkillItem.itemType == skillItem.itemType) {
                return playerSkillItem.currentLevel;
            }
        }

        if(skillItem == activeSkillLeft) {
            return activeSkillLeft.currentLevel;
        }
        if (skillItem == activeSkillRight) {
            return activeSkillRight.currentLevel;
        }

        return 0;
    }

}
