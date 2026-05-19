using System.Collections.Generic;
using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class MatchDetector
    {
        private readonly GridModel _grid;

        public MatchDetector(GridModel grid)
        {
            _grid = grid;
        }

        // Verilen pozisyondan başlayarak aynı türden bağlı grubu bulur.
        // Match yoksa null döner (en az 2 item lazım).
        public List<CellModel> FindMatchAt(GridPosition origin)
        {
            var startCell = _grid.GetCell(origin);
            if (startCell == null || startCell.IsEmpty) return null;

            // Sadece IMatchable item'lar match'e dahil olur.
            // CandyBar/Cupcake/Croissant burada elenir.
            if (startCell.Item is not IMatchable matchable) return null;

            string matchKey = matchable.GetMatchKey();
            var visited = new HashSet<GridPosition>();
            var result = new List<CellModel>();

            FloodFill(origin, matchKey, visited, result);

            return result.Count >= 2 ? result : null;
        }

        // Grid'de oynanabilir herhangi bir match (2+ aynı tür yan yana) var mı?
        // Deadlock tespiti için: hiç match yoksa grid kilitlenmiş demektir.
        // FindMatchAt'i yeniden kullanır — ayrı flood-fill kodu yok (DRY).
        public bool HasAnyMatch()
        {
            foreach (var cell in _grid.AllCells())
            {
                if (cell.IsEmpty) continue;
                if (cell.Item is not IMatchable) continue;

                if (FindMatchAt(cell.Position) != null)
                    return true;
            }

            return false;
        }

        // 4 yöne yayılarak aynı key'e sahip komşuları toplar.
        // visited seti sonsuz döngüyü engelliyor.
        private void FloodFill(GridPosition pos, string key,
                                HashSet<GridPosition> visited,
                                List<CellModel> result)
        {
            if (visited.Contains(pos)) return;
            visited.Add(pos);

            var cell = _grid.GetCell(pos);
            if (cell == null || cell.IsEmpty) return;

            if (cell.Item is not IMatchable m) return;
            if (m.GetMatchKey() != key) return;

            result.Add(cell);

            // Köşegen yok, sadece yatay/dikey komşular
            FloodFill(pos.Up(), key, visited, result);
            FloodFill(pos.Down(), key, visited, result);
            FloodFill(pos.Left(), key, visited, result);
            FloodFill(pos.Right(), key, visited, result);
        }
    }
}