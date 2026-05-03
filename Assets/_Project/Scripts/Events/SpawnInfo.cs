using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Events
{
    public class SpawnInfo
    {
        public GridItem Item { get; }
        public GridPosition Position { get; }

        public SpawnInfo(GridItem item, GridPosition position)
        {
            Item = item;
            Position = position;
        }
    }
}