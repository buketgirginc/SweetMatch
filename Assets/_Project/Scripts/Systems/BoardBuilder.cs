using System.Collections.Generic;
using SweetMatch.Data;
using SweetMatch.Model;
using SweetMatch.Model.Items;
using UnityEngine;

namespace SweetMatch.Systems
{
    // Grid'i level config'e göre kurar (Build) ve runtime deadlock'ta yeniden karıştırır (Shuffle).
    // Board generation + deadlock recovery sorumluluğu — bir System (GridModel üzerinde oyun
    // kuralı işletir, presentation tanımaz, pure C#). FillSystem/MatchDetector ile kardeş.
    public class BoardBuilder
    {
        // Initial board'da bir match bu boyutu aşamaz (en baştan powerup önleme)
        private const int MAX_INITIAL_MATCH = 4;
        private readonly GridModel _grid;
        private readonly LevelConfigSO _config;
        private readonly IItemFactory _factory;
        private readonly MatchDetector _matchDetector;

        public BoardBuilder(GridModel grid, LevelConfigSO config,
                            IItemFactory factory, MatchDetector matchDetector)
        {
            _grid = grid;
            _config = config;
            _factory = factory;
            _matchDetector = matchDetector;
        }

        // Grid'i level config'e göre kurar.
        // Önce manuel layout (varsa), sonra kalanları rastgele sweet ile doldurur.
        // Hiç match yoksa deadlock önlemek için bir tane garantili yaratır.
        public void Build()
        {
            if (_config.UseInitialLayout)
                ApplyInitialLayout();

            FillRemainingRandom();

            if (!_matchDetector.HasAnyMatch())
                ForceCreateOneMatch();
        }

        // Runtime deadlock'ta çağrılır: sadece SweetItem hücrelerini temizleyip
        // yeniden rastgele doldurur. Özel item'lar (Cupcake/Croissant/CandyBar)
        // IsEmpty olmadığı için FillRemainingRandom tarafından atlanır → yerinde kalır.
        // Build mantığını yeniden kullanır (DRY): aynı MAX_INITIAL_MATCH + match garantisi.
        public void Shuffle()
        {
            foreach (var cell in _grid.AllCells())
            {
                if (!cell.IsEmpty && cell.Item is SweetItem)
                    cell.Clear();
            }

            FillRemainingRandom();

            if (!_matchDetector.HasAnyMatch())
                ForceCreateOneMatch();
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

        // Boş kalan tüm hücrelere rastgele sweet üret.
        // Her atamada match boyutu MAX_INITIAL_MATCH'i aşmıyorsa kabul, aşıyorsa farklı tip dene.
        private void FillRemainingRandom()
        {
            var allTypes = (SweetType[])System.Enum.GetValues(typeof(SweetType));

            foreach (var cell in _grid.AllCells())
            {
                if (!cell.IsEmpty) continue;

                var triedTypes = new HashSet<SweetType>();
                bool placed = false;

                while (triedTypes.Count < allTypes.Length)
                {
                    var randomType = allTypes[Random.Range(0, allTypes.Length)];
                    if (triedTypes.Contains(randomType)) continue;

                    triedTypes.Add(randomType);

                    var item = new SweetItem(randomType);
                    cell.SetItem(item);
                    item.Position = cell.Position;

                    if (FloodFillSize(cell.Position, randomType) <= MAX_INITIAL_MATCH)
                    {
                        placed = true;
                        break;
                    }

                    cell.Clear();
                }

                // Tüm tipler büyük match yaratıyorsa fallback: rastgele kabul
                // (çok ender, sadece çok dar köşelerde olabilir)
                if (!placed)
                {
                    var fallbackItem = _factory.CreateRandomSweet();
                    cell.SetItem(fallbackItem);
                    fallbackItem.Position = cell.Position;
                }
            }
        }

        // Verilen pozisyondan başlayarak aynı sweet türündeki bağlı hücreleri sayar.
        // MAX_INITIAL_MATCH limit kontrolü için (board generation kalite kuralı).
        private int FloodFillSize(GridPosition start, SweetType type)
        {
            var visited = new HashSet<GridPosition>();
            return FloodFillRecursive(start, type, visited);
        }

        private int FloodFillRecursive(GridPosition pos, SweetType type, HashSet<GridPosition> visited)
        {
            if (visited.Contains(pos)) return 0;
            visited.Add(pos);

            var cell = _grid.GetCell(pos);
            if (cell == null || cell.IsEmpty) return 0;
            if (!(cell.Item is SweetItem sweet) || sweet.SweetType != type) return 0;

            int count = 1;
            count += FloodFillRecursive(pos.Up(), type, visited);
            count += FloodFillRecursive(pos.Down(), type, visited);
            count += FloodFillRecursive(pos.Left(), type, visited);
            count += FloodFillRecursive(pos.Right(), type, visited);
            return count;
        }

        // Hiç match yoksa: rastgele bir hücreye komşusunun tipini ata
        // → garantili 2'li match oluşur
        private void ForceCreateOneMatch()
        {
            foreach (var cell in _grid.AllCells())
            {
                if (cell.IsEmpty) continue;
                if (!(cell.Item is SweetItem)) continue;

                // Komşulardan biri sweet'se, ona o tipi ver
                foreach (var neighborPos in new[] { cell.Position.Right(), cell.Position.Down() })
                {
                    var neighbor = _grid.GetCell(neighborPos);
                    if (neighbor == null || neighbor.IsEmpty) continue;
                    if (!(neighbor.Item is SweetItem neighborSweet)) continue;

                    // cell'in tipini neighbor'a ver
                    var cellSweet = (SweetItem)cell.Item;
                    var newItem = new SweetItem(cellSweet.SweetType);
                    neighbor.SetItem(newItem);
                    newItem.Position = neighbor.Position;
                    return;
                }
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