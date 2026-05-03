using System.Collections.Generic;

namespace SweetMatch.Model.Items
{
    public class CandyBarItem : GridItem, IClickable
    {
        public CandyBarDirection Direction { get; }

        public CandyBarItem(CandyBarDirection direction)
        {
            Direction = direction;
        }

        public void OnClick()
        {
        }

        public List<GridPosition> GetAffectedCells(int gridWidth, int gridHeight)
        {
            var cells = new List<GridPosition>();

            if (Direction == CandyBarDirection.Horizontal)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (x != Position.X)
                        cells.Add(new GridPosition(x, Position.Y));
                }
            }
            else
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (y != Position.Y)
                        cells.Add(new GridPosition(Position.X, y));
                }
            }

            return cells;
        }

        public override string GetVisualKey()
        {
            return Direction == CandyBarDirection.Horizontal ? "candybar_h" : "candybar_v";
        }

        public override string GetGoalSignature() => null;
    }
}