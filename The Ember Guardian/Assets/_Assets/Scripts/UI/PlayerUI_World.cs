using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI_World : MonoBehaviour
{
    public static PlayerUI_World Instance;

    [SerializeField] private PlayerWorldUITooltip tooltipLeft;
    [SerializeField] private PlayerWorldUITooltip tooltipRight;

    private void Awake() {
        Instance = this;
    }

    public PlayerWorldUITooltip GetTooltipLeft() {
        return tooltipLeft;
    }

    public PlayerWorldUITooltip GetTooltipRight() {
        return tooltipRight;
    }

}
