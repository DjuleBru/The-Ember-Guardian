using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_TrainerMerchantItem : HubMerchantItem
{
    public enum TrainerItemType {
        Hold2Weapons,
        WeaponSwapTime,
        Flashlight,
        Crouch,
        MaxHP,
        Heal,
        MoveSpeed,
        RunSpeed,
        RunMaxTime,
        BackpackGemSize,
        BackpackOrbSize,
        BackpackAmmoSize,
    }



    [SerializeField] private TrainerItemType trainerItemType;

}
