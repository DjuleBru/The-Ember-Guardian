using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureSaveData {

    public float posX, posY;
    public StructureSO.StructureType structureType;
    public bool structureBuilt;
    public bool structureUnlocked;
    public int structureLevel;
}

public class TrapSaveData {

    public int currentUseIndex;
    public int currentRearmIndex;

}
