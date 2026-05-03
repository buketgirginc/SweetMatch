using System.Collections.Generic;
using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class BottomTrigger
    {
        private readonly GridModel _grid;

        public BottomTrigger(GridModel grid)
        {
            _grid = grid;
        }

        // Grid'in en alt satırındaki IBottomSensitive item'ları bul.
        // Croissant alta düşünce kaybolacak.
        public List<CellModel> FindTriggeredAtBottom()
        {
            var triggered = new List<CellModel>();
            int bottomY = 0;  // (0,0) sol-alt convention

            for (int x = 0; x < _grid.Width; x++)
            {
                var cell = _grid.GetCell(x, bottomY);
                if (cell.IsEmpty) continue;

                if (cell.Item is IBottomSensitive)
                    triggered.Add(cell);
            }

            return triggered;
        }
    }
}