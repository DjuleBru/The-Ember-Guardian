using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraWidePanelResizer : MonoBehaviour
{

    [SerializeField] private float ultraWidePanelOffsetMinX;
    [SerializeField] private float ultraWidePanelOffsetMinY;
    [SerializeField] private float ultraWidePanelOffsetMaxX;
    [SerializeField] private float ultraWidePanelOffsetMaxY;

    [SerializeField] private float ultraWidePanelPosX;
    [SerializeField] private float ultraWidePanelPosY;
    [SerializeField] private float ultraWidePanelWidth;
    [SerializeField] private float ultraWidePanelHeight;

    [SerializeField] private bool onlyVerticalResize;
    [SerializeField] private bool stretchedPanel;

    private RectTransform rt;
    void Start() {
        rt= GetComponent<RectTransform>();

        float aspectRatio = (float)Screen.width / Screen.height;

        if (aspectRatio >= 2.33f) {
            if(stretchedPanel) {

                if (onlyVerticalResize) {
                    rt.offsetMin = new Vector2(rt.offsetMin.x, ultraWidePanelOffsetMinY);
                    rt.offsetMax = new Vector2(rt.offsetMax.x, ultraWidePanelOffsetMaxY);
                }
                else {
                    rt.offsetMin = new Vector2(ultraWidePanelOffsetMinX, ultraWidePanelOffsetMinY);
                    rt.offsetMax = new Vector2(ultraWidePanelOffsetMaxX, ultraWidePanelOffsetMaxY);
                }
            } else {
                rt.sizeDelta = new Vector2(ultraWidePanelWidth, ultraWidePanelHeight);
                rt.anchoredPosition = new Vector2(ultraWidePanelPosX, ultraWidePanelPosY);
            }
        }
    }
}
