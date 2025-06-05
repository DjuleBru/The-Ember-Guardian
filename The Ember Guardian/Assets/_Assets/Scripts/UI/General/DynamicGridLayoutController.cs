using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicGridLayoutController : MonoBehaviour
{
    public float spacing = 10f;
    public float fixedScale = 50f; // 50 pixels = 1 unité logique

    public void RefreshLayout() {
        RectTransform container = GetComponent<RectTransform>();
        float panelWidth = container.rect.width;

        float x = 0f;
        float y = 0f;
        float spacing = 10f;

        List<(RectTransform, Vector2)> currentRow = new List<(RectTransform, Vector2)>();
        float rowWidth = 0f;
        float rowHeight = 0f;

        // Collecte des enfants actifs
        List<(RectTransform, Sprite)> entries = new List<(RectTransform, Sprite)>();

        foreach (RectTransform child in container) {
            if (!child.gameObject.activeSelf) continue;

            Image image = child.GetComponentInChildren<Image>();
            if (image == null || image.sprite == null) continue;

            entries.Add((child, image.sprite));
        }

        foreach (var (child, sprite) in entries) {
            // taille en pixels de la texture du sprite
            float width = sprite.rect.width;
            float height = sprite.rect.height;

            // appliquer un scale fixe
            Vector2 size = new Vector2(width, height) * (fixedScale / 100f);

            // gestion du retour à la ligne
            if (rowWidth + size.x > panelWidth && currentRow.Count > 0) {
                // positionner la ligne complète
                float lineY = y - rowHeight;

                float offsetX = 0f;
                foreach (var (item, itemSize) in currentRow) {
                    item.sizeDelta = itemSize;
                    item.anchoredPosition = new Vector2(offsetX, lineY);
                    offsetX += itemSize.x + spacing;
                }

                y -= rowHeight + spacing;
                currentRow.Clear();
                rowWidth = 0f;
                rowHeight = 0f;
            }

            currentRow.Add((child, size));
            rowWidth += size.x + spacing;
            rowHeight = Mathf.Max(rowHeight, size.y);
        }

        // positionner la dernière ligne
        if (currentRow.Count > 0) {
            float lineY = y - rowHeight;
            float offsetX = 0f;
            foreach (var (item, itemSize) in currentRow) {
                item.sizeDelta = itemSize;
                item.anchoredPosition = new Vector2(offsetX, lineY);
                offsetX += itemSize.x + spacing;
            }
            y -= rowHeight + spacing;
        }

        // ajuster la hauteur container
        container.sizeDelta = new Vector2(container.sizeDelta.x, Mathf.Abs(y));
    }
}
