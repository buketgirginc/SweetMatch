using System.Collections.Generic;

using SweetMatch.Events;

using SweetMatch.Model;

namespace SweetMatch.Systems

{

    public class FillSystem

    {

        private readonly GridModel _grid;

        private readonly EventBus _eventBus;

        private readonly IItemFactory _itemFactory;

        public FillSystem(GridModel grid, EventBus eventBus, IItemFactory itemFactory)

        {

            _grid = grid;

            _eventBus = eventBus;

            _itemFactory = itemFactory;

        }

        // Fall'dan sonra kalan boş hücrelere rastgele sweet üretir.

        // Sadece sweet üretir, özel itemler so'da ayarlanır

        public void FillEmpty()

        {

            var spawns = new List<SpawnInfo>();

            foreach (var cell in _grid.AllCells())

            {

                if (!cell.IsEmpty) continue;

                var item = _itemFactory.CreateRandomSweet();

                cell.SetItem(item);

                item.Position = cell.Position;

                spawns.Add(new SpawnInfo(item, cell.Position));

            }

            if (spawns.Count > 0)

                _eventBus.Raise(new ItemsSpawnedEvent(spawns));

        }

    }

}