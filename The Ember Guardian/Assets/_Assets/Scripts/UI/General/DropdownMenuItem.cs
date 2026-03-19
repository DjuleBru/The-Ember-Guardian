using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownMenuItem : MonoBehaviour, ISubmitHandler {
    private int index;
    private DropdownMenuUI menu;

    public void Initialize(int index, DropdownMenuUI menu) {
        this.index = index;
        this.menu = menu;
    }

    public void OnSubmit(BaseEventData eventData) {
        //menu.SetResolution(index);
    }

}