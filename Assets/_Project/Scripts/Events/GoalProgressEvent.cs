namespace SweetMatch.Events
{
    public class GoalProgressEvent
    {
        public string Signature { get; }
        public int Remaining { get; }

        public GoalProgressEvent(string signature, int remaining)
        {
            Signature = signature;
            Remaining = remaining;
        }
    }
}