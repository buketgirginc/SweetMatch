namespace SweetMatch.Model.Items
{
    /// <summary>
    /// Komşusunda match olduğunda tetiklenen item'lar.
    /// CupcakeItem implement eder (yanında match → patlar).
    /// </summary>
    public interface INeighborSensitive
    {
        void OnNeighborMatched();
    }
}