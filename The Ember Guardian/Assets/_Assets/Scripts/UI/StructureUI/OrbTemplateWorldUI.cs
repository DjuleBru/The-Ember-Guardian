using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbTemplateWorldUI : MonoBehaviour
{

   [SerializeField] private Image orbImageFill;

    private bool orbPaid;

    public void SetFillAmount(float amount) {
        orbImageFill.fillAmount = amount;
    }

    public void SetOrbPaid(bool paid) {
        orbPaid = paid;
    }

    public bool GetOrbPaid() {
        return orbPaid;
    }
}
