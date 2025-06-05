using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalUI_StatTemplate : MonoBehaviour
{

    [SerializeField] private GameObject statTickTemplateFill;

    public void SetFilled(bool filled) {
        statTickTemplateFill.SetActive(filled);
    }

}
