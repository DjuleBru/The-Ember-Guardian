using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormBigOrbCollider : MonoBehaviour
{
    private List<SmallOrb_UI> smallOrbs = new List<SmallOrb_UI>();

    private void OnTriggerEnter2D(Collider2D collision) {
        SmallOrbUI_DetectionCollider smallOrbCollider = collision.GetComponent<SmallOrbUI_DetectionCollider>();
        if (smallOrbCollider == null) return;

        SmallOrb_UI smallOrb = smallOrbCollider.GetComponentInParent<SmallOrb_UI>();
        if (smallOrb.GetFormingOrb()) {
            if(!smallOrbs.Contains(smallOrb)) {
                smallOrbs.Add(smallOrb);
            }
        }

        if(smallOrbs.Count == UIOrbManager.Instance.GetSmallOrbValue()) {
            smallOrbs.Clear();
            UIOrbManager.Instance.MergeSmallOrbs();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        SmallOrbUI_DetectionCollider smallOrbCollider = collision.GetComponent<SmallOrbUI_DetectionCollider>();
        if (smallOrbCollider == null) return;

        SmallOrb_UI smallOrb = smallOrbCollider.GetComponentInParent<SmallOrb_UI>();
        if (smallOrbs.Contains(smallOrb)) {
            smallOrbs.Remove(smallOrb);
        }
    }
}
