using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class PowerUpSpawner
    {
        private const int CandyBarThreshold = 5;

        private readonly GridModel _grid;
        private readonly IItemFactory _itemFactory;

        public PowerUpSpawner(GridModel grid, IItemFactory itemFactory)
        {
            _grid = grid;
            _itemFactory = itemFactory;
        }

        // 5+ match olduğunda tıklanan hücreye CandyBar koyar.
        // Yön (yatay/dikey) random seçilir.
        // Spawn olduysa true döner — caller bu hücreyi clear listesinden çıkarmalı.
        public bool TrySpawnAt(GridPosition pos, int matchSize)
        {
            if (matchSize < CandyBarThreshold) return false;

            var direction = UnityEngine.Random.Range(0, 2) == 0
                ? CandyBarDirection.Horizontal
                : CandyBarDirection.Vertical;

            var candyBar = _itemFactory.CreateCandyBar(direction);

            var cell = _grid.GetCell(pos);
            cell.SetItem(candyBar);
            candyBar.Position = pos;

            return true;
        }
    }
}