using SweetMatch.Model.Items;

namespace SweetMatch.Model
{
    public class CellModel
    {
        public GridPosition Position { get; }
        public GridItem Item { get; private set; }

        public bool IsEmpty => Item == null;

        public int X => Position.X;
        public int Y => Position.Y;

        public CellModel(GridPosition position)
        {
            Position = position;
            Item = null;
        }

        public void SetItem(GridItem item)
        {
            Item = item;
        }

        public void Clear()
        {
            Item = null;
        }
    }
}