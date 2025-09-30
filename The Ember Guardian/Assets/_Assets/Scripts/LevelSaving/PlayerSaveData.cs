using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSaveData {
    public float posX, posY;
    public int health;

    // Skills
    public List<SkillData> activeSkills = new List<SkillData>();
    public List<SkillData> passiveSkills = new List<SkillData>();

    //Inventory 
    public Dictionary<string, List<Vector3>> currencyData = new Dictionary<string, List<Vector3>>();
    public Dictionary<string, List<Quaternion>> currencyRotationData = new Dictionary<string, List<Quaternion>>();

    // Weapons
    public GunData primaryGun;
    public GunData secondaryGun;
    public List<GunData> foundGuns = new List<GunData>();
    public bool hasSecondary;
    public bool hasFoundGun;
}

[System.Serializable]
public class SkillData {
    public SkillItem.SkillType skillType;
    public int level;
}

[System.Serializable]
public class GunData {
    public GunSO.GunType gunType;
    public int ammoClip;
    public int currentBullet;
}
