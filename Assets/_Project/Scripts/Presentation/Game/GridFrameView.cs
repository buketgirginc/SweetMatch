using UnityEngine;

namespace SweetMatch.Presentation.Game
{
    public class GridFrameView : MonoBehaviour
    {
        [SerializeField] private RectTransform outerFrame;
        [SerializeField] private RectTransform innerFrame;
        [SerializeField] private float padding = 20f;

        public void Fit(int gridWidth, int gridHeight, float cellSize, float cellSpacing)
        {
            float totalCellSize = cellSize + cellSpacing;
            float innerW = gridWidth * totalCellSize;
            float innerH = gridHeight * totalCellSize;

            innerFrame.sizeDelta = new Vector2(innerW, innerH);
            outerFrame.sizeDelta = new Vector2(innerW + padding * 2, innerH + padding * 2);
        }
    }
}