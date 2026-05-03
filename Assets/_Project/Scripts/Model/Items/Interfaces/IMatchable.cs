namespace SweetMatch.Model.Items
{
    /// <summary>
    /// Match algılama sistemine dahil olabilen item'lar.
    /// SweetItem implement eder. CandyBar/Cupcake/Croissant etmez → match'e dahil olmazlar.
    /// </summary>
    public interface IMatchable
    {
        string GetMatchKey();    /// Örn: SweetType.Bonbon → "bonbon"
    }
}