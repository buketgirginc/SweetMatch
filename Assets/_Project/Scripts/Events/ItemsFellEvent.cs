using System.Collections.Generic;

namespace SweetMatch.Events
{
    public class ItemsFellEvent
    {
        public IReadOnlyList<FallMove> Moves { get; }

        public ItemsFellEvent(IReadOnlyList<FallMove> moves)
        {
            Moves = moves;
        }
    }
}