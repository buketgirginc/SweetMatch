using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Events
{
    public class FallMove
    {
        public GridItem Item { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public FallMove(GridItem item, GridPosition from, GridPosition to)
        {
            Item = item;
            From = from;
            To = to;
        }
    }
}