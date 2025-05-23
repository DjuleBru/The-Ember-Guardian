using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_GemMerchantItem : HubMerchantItem
{

    [SerializeField] private StructureSO.StructureType structureType;

    public override string GetItemType() {
        return structureType.ToString() + itemLevel;
    }

    public StructureSO.StructureType GetStructureType() {
        return structureType;
    }

}
