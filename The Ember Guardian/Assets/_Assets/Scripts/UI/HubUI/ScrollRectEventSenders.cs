using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollRectEventSenders : MonoBehaviour, IBeginDragHandler, IEndDragHandler {

    public event EventHandler OnDragStarted;
    public event EventHandler OnDragEnded;

    public void OnBeginDrag(PointerEventData eventData) {
        OnDragStarted?.Invoke(this, EventArgs.Empty);
    }

    public void OnEndDrag(PointerEventData eventData) {
        OnDragEnded?.Invoke(this, EventArgs.Empty);
    }
}
