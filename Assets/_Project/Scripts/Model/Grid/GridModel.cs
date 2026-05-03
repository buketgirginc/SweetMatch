using System.Collections.Generic;

namespace SweetMatch.Model
{
    public class GridModel
    {
        public int Width { get; }
        public int Height { get; }

        private readonly CellModel[,] _cells;

        public GridModel(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new CellModel[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = new CellModel(new GridPosition(x, y));
                }
            }
        }

        public CellModel GetCell(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;
            return _cells[x, y];
        }

        public CellModel GetCell(GridPosition pos)
        {
            return GetCell(pos.X, pos.Y);
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsInBounds(GridPosition pos)
        {
            return IsInBounds(pos.X, pos.Y);
        }

        public IEnumerable<CellModel> AllCells()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return _cells[x, y];
                }
            }
        }
    }
}