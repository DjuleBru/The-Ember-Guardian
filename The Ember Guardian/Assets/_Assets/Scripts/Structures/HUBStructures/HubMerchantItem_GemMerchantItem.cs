using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubMerchantItem_GemMerchantItem : HubMerchantItem
{

    [SerializeField] private StructureSO.StructureType structureType;
    [SerializeField] private int structureLevel;

   public override string GetItemType() {
        return structureType.ToString() + structureLevel;
   }

}
