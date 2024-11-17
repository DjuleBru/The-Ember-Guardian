using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbTemplateWorldUI : MonoBehaviour
{

   [SerializeField] private Image orbImageFill;

    private bool orbPaid;

    public event EventHandler OnOrbPaid;

    public void SetFillAmount(float amount) {
        orbImageFill.fillAmount = amount;
    }

    public void SetOrbPaid(bool paid) {

        if(paid) {
            OnOrbPaid?.Invoke(this, EventArgs.Empty);
            
        }
        orbPaid = paid;
    }

    public bool GetOrbPaid() {
        return orbPaid;
    }
}
