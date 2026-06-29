using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamDeckPanelResizer : MonoBehaviour {
    [Header("ULTRA WIDE - STRETCHED (RectTransform: Left / Bottom / Right / Top)")]

    [Tooltip("RectTransform.offsetMin.x => Left")]
    [SerializeField] private float ultraWidePanelOffsetMinX;

    [Tooltip("RectTransform.offsetMin.y => Bottom")]
    [SerializeField] private float ultraWidePanelOffsetMinY;

    [Tooltip("RectTransform.offsetMax.x => Right")]
    [SerializeField] private float ultraWidePanelOffsetMaxX;

    [Tooltip("RectTransform.offsetMax.y => Top")]
    [SerializeField] private float ultraWidePanelOffsetMaxY;

    [Header("ULTRA WIDE - NON STRETCHED (RectTransform: Pos X / Pos Y / Width / Height)")]

    [Tooltip("RectTransform.anchoredPosition.x => Pos X")]
    [SerializeField] private float ultraWidePanelPosX;

    [Tooltip("RectTransform.anchoredPosition.y => Pos Y")]
    [SerializeField] private float ultraWidePanelPosY;

    [Tooltip("RectTransform.sizeDelta.x => Width")]
    [SerializeField] private float ultraWidePanelWidth;

    [Tooltip("RectTransform.sizeDelta.y => Height")]
    [SerializeField] private float ultraWidePanelHeight;

    [Header("VERTICAL MODE (RectTransform: Pos X / Pos Y / Width / Height)")]

    [Tooltip("RectTransform.anchoredPosition.x => Pos X")]
    [SerializeField] private float verticalPanelPosX;
    [Tooltip("RectTransform.anchoredPosition.y => Pos Y")]
    [SerializeField] private float verticalPanelPosY;
    [Tooltip("RectTransform.sizeDelta.x => Width")]
    [SerializeField] private float verticalPanelWidth;
    [Tooltip("RectTransform.sizeDelta.y => Height")]
    [SerializeField] private float verticalPanelHeight;

    [SerializeField] private bool onlyVerticalResize;
    [SerializeField] private bool stretchedPanel;

    private RectTransform rt;
    void Start() {
        rt = GetComponent<RectTransform>();

        float aspectRatio = (float)Screen.width / Screen.height;
        StartCoroutine(SetAfterFrames());
    }

    private IEnumerator SetAfterFrames() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (SceneLoader.Instance.IsSteamDeck()) {
            Debug.Log("test cedric");
            if (stretchedPanel) {

                if (onlyVerticalResize) {
                    rt.offsetMin = new Vector2(rt.offsetMin.x, ultraWidePanelOffsetMinY);
                    rt.offsetMax = new Vector2(rt.offsetMax.x, ultraWidePanelOffsetMaxY);
                }
                else {
                    rt.offsetMin = new Vector2(ultraWidePanelOffsetMinX, ultraWidePanelOffsetMinY);
                    rt.offsetMax = new Vector2(ultraWidePanelOffsetMaxX, ultraWidePanelOffsetMaxY);
                }
            }
            else {
                rt.sizeDelta = new Vector2(ultraWidePanelWidth, ultraWidePanelHeight);
                rt.anchoredPosition = new Vector2(ultraWidePanelPosX, ultraWidePanelPosY);
            }
        }
    }
}
