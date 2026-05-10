using SweetMatch.Data;
using SweetMatch.Model;
using SweetMatch.Systems;
using UnityEngine;

namespace SweetMatch.Presentation.Game
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private CellView cellPrefab;
        [SerializeField] private RectTransform gridRoot;
        [SerializeField] private ItemVisualConfigSO visualConfig;
        [SerializeField] private float cellSize = 100f;
        [SerializeField] private float cellSpacing = 4f;

        public float CellSize => cellSize;
        public float CellSpacing => cellSpacing;

        private GridModel _model;
        private CellView[,] _cellViews;

        public void Build(GridModel model, InputHandler inputHandler)
        {
            _model = model;
            _cellViews = new CellView[model.Width, model.Height];

            for (int x = 0; x < model.Width; x++)
            {
                for (int y = 0; y < model.Height; y++)
                {
                    var cellView = Instantiate(cellPrefab, gridRoot);
                    cellView.Initialize(new GridPosition(x, y), inputHandler);
                    cellView.transform.localPosition = CalculatePosition(x, y);
                    _cellViews[x, y] = cellView;
                }
            }

            RenderAll();
        }

        public void RenderAll()
        {
            if (_model == null || _cellViews == null) return;

            foreach (var cell in _model.AllCells())
                _cellViews[cell.X, cell.Y].Bind(cell, visualConfig);
        }

        public void RenderCell(GridPosition pos)
        {
            if (_model == null || _cellViews == null) return;
            var cell = _model.GetCell(pos);
            if (cell == null) return;
            _cellViews[pos.X, pos.Y].Bind(cell, visualConfig);
        }
        public CellView GetCellView(GridPosition pos) => _cellViews[pos.X, pos.Y];

        private Vector2 CalculatePosition(int x, int y)
        {
            float totalCellSize = cellSize + cellSpacing;
            float offsetX = -(_model.Width - 1) * totalCellSize * 0.5f;
            float offsetY = -(_model.Height - 1) * totalCellSize * 0.5f;
            return new Vector2(offsetX + x * totalCellSize, offsetY + y * totalCellSize);
        }
    }
}