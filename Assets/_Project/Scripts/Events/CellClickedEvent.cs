using SweetMatch.Model;

namespace SweetMatch.Events
{
    public class CellClickedEvent
    {
        public GridPosition Position { get; }

        public CellClickedEvent(GridPosition position)
        {
            Position = position;
        }
    }
}