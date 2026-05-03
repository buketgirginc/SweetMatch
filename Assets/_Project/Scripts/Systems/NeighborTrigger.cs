using System.Collections.Generic;
using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class NeighborTrigger
    {
        private readonly GridModel _grid;

        public NeighborTrigger(GridModel grid)
        {
            _grid = grid;
        }

        // Match olan hücrelerin komşularındaki INeighborSensitive item'ları bul.
        // Cupcake gibi item'lar match yanında olduğunda patlamalı.
        public List<CellModel> FindTriggeredNeighbors(IEnumerable<CellModel> matchedCells)
        {
            // HashSet kullanıyoruz çünkü iki match hücresi aynı komşuya bakabilir
            var triggered = new HashSet<CellModel>();

            foreach (var cell in matchedCells)
            {
                CheckNeighbor(cell.Position.Up(), triggered);
                CheckNeighbor(cell.Position.Down(), triggered);
                CheckNeighbor(cell.Position.Left(), triggered);
                CheckNeighbor(cell.Position.Right(), triggered);
            }

            return new List<CellModel>(triggered);
        }

        private void CheckNeighbor(GridPosition pos, HashSet<CellModel> result)
        {
            var cell = _grid.GetCell(pos);
            if (cell == null || cell.IsEmpty) return;

            if (cell.Item is INeighborSensitive)
                result.Add(cell);
        }
    }
}