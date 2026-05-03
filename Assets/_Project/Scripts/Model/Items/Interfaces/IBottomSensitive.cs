namespace SweetMatch.Model.Items
{
    /// <summary>
    /// Grid'in en alt satırına ulaştığında tetiklenen item'lar.
    /// CroissantItem implement eder (alta inince kaybolur, goal'e gider).
    /// </summary>
    public interface IBottomSensitive
    {
        void OnReachedBottom();
    }
}