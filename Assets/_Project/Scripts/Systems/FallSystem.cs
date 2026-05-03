using System.Collections.Generic;
using SweetMatch.Events;
using SweetMatch.Model;

namespace SweetMatch.Systems
{
    public class FallSystem
    {
        private readonly GridModel _grid;
        private readonly EventBus _eventBus;

        public FallSystem(GridModel grid, EventBus eventBus)
        {
            _grid = grid;
            _eventBus = eventBus;
        }

        // Tüm grid için yerçekimini uygular.
        // Her sütun bağımsız işleniyor — bir sütundaki hareket diğerini etkilemez.
        public void ApplyFall()
        {
            var fallMoves = new List<FallMove>();

            for (int x = 0; x < _grid.Width; x++)
            {
                ApplyFallInColumn(x, fallMoves);
            }

            if (fallMoves.Count > 0)
                _eventBus.Raise(new ItemsFellEvent(fallMoves));
        }

        // Bir sütunda alttan yukarıya tarayıp boş hücreleri doldurur.
        // Her boş hücre için üstündeki ilk dolu item'ı bulup indiriyoruz.
        private void ApplyFallInColumn(int x, List<FallMove> moves)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                var cell = _grid.GetCell(x, y);
                if (!cell.IsEmpty) continue;

                // Boş hücre bulundu, üstte düşebilir item ara
                for (int aboveY = y + 1; aboveY < _grid.Height; aboveY++)
                {
                    var aboveCell = _grid.GetCell(x, aboveY);
                    if (aboveCell.IsEmpty) continue;

                    // CanFall=false olan item'ın üstü "kilitli" — daha yukarı bakma
                    if (!aboveCell.Item.CanFall()) break;

                    // Item'ı taşı: üst hücreden çıkar, alt hücreye koy
                    var item = aboveCell.Item;
                    var fromPos = new GridPosition(x, aboveY);
                    var toPos = new GridPosition(x, y);

                    aboveCell.Clear();
                    cell.SetItem(item);
                    item.Position = toPos;

                    moves.Add(new FallMove(item, fromPos, toPos));
                    break;
                }
            }
        }
    }
}