using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresManager : MonoBehaviour
{
    public static StructuresManager Instance;
    [SerializeField] private List<StructureSO> allStructureSOList;

    private List<Structure> structuresBuiltList = new List<Structure>();
    private List<StructureLocation> structureLocationsList = new List<StructureLocation>();

    private void Awake() {
        Instance = this;
    }

    public StructureSO GetStructureSO(StructureSO.StructureType structureType) {

        foreach (StructureSO so in allStructureSOList) {
            if (so.structureType == structureType) return so;
        }

        return allStructureSOList[0];
    }

    public List<Structure> GetBuiltStructureList() {
        return structuresBuiltList;
    }
    public List<StructureLocation> GetStructureLocationsList() {
        return structureLocationsList;
    }
    public void AddStructureLocation(StructureLocation location) {
        structureLocationsList.Add(location);
    }

    public void AddBuiltStructure(Structure structure) {
        structuresBuiltList.Add(structure);
    }
    public void RemoveBuiltStructure(Structure structure) {
        structuresBuiltList.Remove(structure);
    }
    public void RemoveStructureLocation(StructureLocation structureLocation) {
        structureLocationsList.Remove(structureLocation);
    }
}
