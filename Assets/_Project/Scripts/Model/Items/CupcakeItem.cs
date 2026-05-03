namespace SweetMatch.Model.Items
{
    public class CupcakeItem : GridItem, INeighborSensitive
    {
        public void OnNeighborMatched()
        {
            Destroy();
        }

        public override string GetVisualKey() => "cupcake";

        public override string GetGoalSignature() => "cupcake";
    }
}