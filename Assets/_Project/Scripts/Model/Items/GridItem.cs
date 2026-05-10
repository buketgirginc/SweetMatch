namespace SweetMatch.Model.Items
{
    /// <summary>
    /// Grid üzerindeki tüm item'ların temel sınıfı.
    /// Concrete item'lar (SweetItem, CandyBarItem, vs.) bu sınıftan türer ve
    /// kendi yeteneklerini interface'lerle (IMatchable, IClickable...) ekler.
    /// </summary>
    public abstract class GridItem
    {
        public GridPosition Position { get; set; }  //itemin hangi posda olduğu, fallsystem değiştirir
        public bool IsAlive { get; protected set; } = true;
        public abstract string GetVisualKey();  // View katmanı bu anahtar üzerinden ItemVisualConfigSO'dan sprite'ı alır.


        /// <summary>
        /// Goal sisteminin bu item'ı tanıdığı imza. null = goal'e dahil değil.
        /// Örn: "sweet:bonbon", "cupcake", "croissant", null (CandyBar için).
        /// </summary>
        public abstract string GetGoalSignature();

        public virtual bool CanFall() => true;   //virtual method — "alt sınıf override edebilir, etmek zorunda değil"
        public virtual bool CanBeClearedByPowerUp() => true;
        public virtual void Destroy()
        {
            IsAlive = false;
        }
    }
}