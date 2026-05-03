namespace SweetMatch.Model.Items
{
    public class SweetItem : GridItem, IMatchable
    {
        public SweetType SweetType { get; }

        public SweetItem(SweetType sweetType)
        {
            SweetType = sweetType;
        }

        public string GetMatchKey() => SweetType.ToString();

        public override string GetVisualKey() => $"sweet_{SweetType.ToString().ToLower()}";

        public override string GetGoalSignature() => $"sweet:{SweetType.ToString().ToLower()}";
    }
}