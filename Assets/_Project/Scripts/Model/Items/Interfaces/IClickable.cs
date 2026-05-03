namespace SweetMatch.Model.Items
{
    /// <summary>
    /// Kullanıcının doğrudan tıklayabildiği item'lar.
    /// CandyBarItem implement eder (tıklanınca aktive olur).
    /// SweetItem etmez — Sweet match grubu olarak işlenir, tek başına tıklama davranışı yoktur.
    /// </summary>
    public interface IClickable
    {
        /// <summary>
        /// Item tıklandığında çağrılır. Item kendine özgü davranışını yapar.
        /// </summary>
        void OnClick();
    }
}