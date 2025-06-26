using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresManager : MonoBehaviour
{
    public static StructuresManager Instance;
    [SerializeField] private List<StructureSO> allStructureSOList;

    private void Awake() {
        Instance = this;
    }

    public StructureSO GetStructureSO(StructureSO.StructureType structureType) {
        foreach (StructureSO so in allStructureSOList) {
            if (so.structureType == structureType) return so;
        }

        return allStructureSOList[0];
    }
}
