using System.Collections.Generic;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class ClearSystem
    {
        private readonly GridModel _grid;
        private readonly EventBus _eventBus;

        public ClearSystem(GridModel grid, EventBus eventBus)
        {
            _grid = grid;
            _eventBus = eventBus;
        }

        // Verilen hücrelerdeki item'ları yok eder ve event fırlatır.
        // Goal/VFX/Sound dinleyicileri event üzerinden tepki verir.
        public void Clear(IEnumerable<CellModel> cells)
        {
            var clearedItems = new List<GridItem>();

            foreach (var cell in cells)
            {
                if (cell.IsEmpty) continue;

                var item = cell.Item;
                item.Destroy();
                cell.Clear();
                clearedItems.Add(item);
            }

            // Hiçbir şey patlatılmadıysa event göndermenin anlamı yok
            if (clearedItems.Count > 0)
                _eventBus.Raise(new ItemsClearedEvent(clearedItems));
        }
    }
}