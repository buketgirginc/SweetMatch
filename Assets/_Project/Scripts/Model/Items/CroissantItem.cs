namespace SweetMatch.Model.Items
{
    public class CroissantItem : GridItem, IBottomSensitive
    {
        public void OnReachedBottom()
        {
            Destroy();
        }

        public override string GetVisualKey() => "croissant";

        public override string GetGoalSignature() => "croissant";
    }
}