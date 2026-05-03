namespace SweetMatch.Events
{
    public class MovesChangedEvent
    {
        public int Remaining { get; }

        public MovesChangedEvent(int remaining)
        {
            Remaining = remaining;
        }
    }
}