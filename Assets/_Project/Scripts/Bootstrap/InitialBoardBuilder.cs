using SweetMatch.Data;
using SweetMatch.Model;
using SweetMatch.Model.Items;
using SweetMatch.Systems;

namespace SweetMatch.Bootstrap
{
    public class InitialBoardBuilder
    {
        private readonly GridModel _grid;
        private readonly LevelConfigSO _config;
        private readonly IItemFactory _factory;

        public InitialBoardBuilder(GridModel grid, LevelConfigSO config, IItemFactory factory)
        {
            _grid = grid;
            _config = config;
            _factory = factory;
        }

        // Grid'i level config'e göre kurar.
        // Önce manuel layout (varsa), sonra kalanları rastgele sweet ile doldurur.
        public void Build()
        {
            if (_config.UseInitialLayout)
                ApplyInitialLayout();

            FillRemainingRandom();
        }

        // InitialCells listesindeki hücrelere belirtilen item'ları yerleştir
        private void ApplyInitialLayout()
        {
            foreach (var initialCell in _config.InitialCells)
            {
                var cell = _grid.GetCell(initialCell.X, initialCell.Y);
                if (cell == null) continue;

                var item = CreateItemFromKey(initialCell.ItemKey);
                if (item == null) continue;

                cell.SetItem(item);
                item.Position = cell.Position;
            }
        }

        // Boş kalan tüm hücrelere rastgele sweet üret
        private void FillRemainingRandom()
        {
            foreach (var cell in _grid.AllCells())
            {
                if (!cell.IsEmpty) continue;

                var item = _factory.CreateRandomSweet();
                cell.SetItem(item);
                item.Position = cell.Position;
            }
        }

        // ItemKey string'inden uygun item'ı yarat.
        // Format: "sweet:bonbon", "cupcake", "croissant", "candybar:horizontal"
        private GridItem CreateItemFromKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            // Sweet → "sweet:bonbon" gibi
            if (key.StartsWith("sweet:"))
            {
                var typeName = key.Substring("sweet:".Length);
                if (System.Enum.TryParse<SweetType>(typeName, ignoreCase: true, out var sweetType))
                    return new SweetItem(sweetType);
                return null;
            }

            // CandyBar → "candybar:horizontal" veya "candybar:vertical"
            if (key.StartsWith("candybar:"))
            {
                var dirName = key.Substring("candybar:".Length);
                if (System.Enum.TryParse<CandyBarDirection>(dirName, ignoreCase: true, out var dir))
                    return _factory.CreateCandyBar(dir);
                return null;
            }

            // Tekil item'lar
            switch (key.ToLower())
            {
                case "cupcake": return _factory.CreateCupcake();
                case "croissant": return _factory.CreateCroissant();
                default: return null;
            }
        }
    }
}