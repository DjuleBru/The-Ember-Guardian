using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHandleXScale : MonoBehaviour
{
    [SerializeField] private List<Transform> transformAffectedByXScalList;
    [SerializeField] private Transform gunTransform;
    [SerializeField] private Transform gunShellPSTransform;
    [SerializeField] private RectTransform ammoBarTransform;
    [SerializeField] private RectTransform ammoBarLeftPosition;
    [SerializeField] private RectTransform ammoBarRightPosition;

    private void Start() {
        PlayerAim.Instance.OnXAimDirChanged += PlayerAim_OnXAimDirChanged;
    }

    private void PlayerAim_OnXAimDirChanged(object sender, System.EventArgs e) {
        Vector3 localScale = new Vector3(1, 1, 1);

        if(PlayerAim.Instance.GetAimDir().x <0) {
            localScale.x = -1;
            ammoBarTransform.position = ammoBarRightPosition.position;

            Vector3 gunLocalScale = new Vector3(-1, -1, 1);
            gunTransform.localScale = gunLocalScale;
            gunShellPSTransform.localScale = gunLocalScale;

        } else {
            ammoBarTransform.position = ammoBarLeftPosition.position;

            Vector3 gunLocalScale = new Vector3(1, 1, 1);
            gunTransform.localScale = gunLocalScale;
            gunShellPSTransform.localScale = gunLocalScale;

        }

        foreach(Transform t in transformAffectedByXScalList) {
            t.localScale = localScale;
        }
    }

}
