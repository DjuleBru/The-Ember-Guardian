using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeBlock : MonoBehaviour
{
    [SerializeField] private Transform gridAndObstacleParent;
    [SerializeField] private float blockWidth = 10f;
    [SerializeField] private float bridgeWidth = 3f;
    private float direction;

    public float GetTotalSpacing() {
        return blockWidth + bridgeWidth;
    }

    public void SetDir(float dir) {
        direction = dir;
        if (dir < 0) {
            Vector3 scale = new Vector3(-1, 1, 1);
            gridAndObstacleParent.transform.localScale = scale;
        } 
    }

    public Vector3 GetRightExitPosition() {
        // Toujours à droite dans l'espace monde
        return transform.position + Vector3.right * GetTotalSpacing() * direction;
    }

    public void PlayerEnteredBlock() {
        HordeModeMapGenerationManager.Instance.PlayerEnteredBlock(this);
    }

}
