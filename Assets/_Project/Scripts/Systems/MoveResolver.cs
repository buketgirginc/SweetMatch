using System.Collections.Generic;
using System.Linq;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Model.Items;

namespace SweetMatch.Systems
{
    public class MoveResolver
    {
        private readonly EventBus _eventBus;
        private readonly GridModel _grid;
        private readonly MatchDetector _matchDetector;
        private readonly ClearSystem _clearSystem;
        private readonly NeighborTrigger _neighborTrigger;
        private readonly PowerUpSpawner _powerUpSpawner;
        private readonly FallSystem _fallSystem;
        private readonly FillSystem _fillSystem;
        private readonly BottomTrigger _bottomTrigger;
        private readonly MovesTracker _movesTracker;
        private readonly GameStateMachine _stateMachine;

        public MoveResolver(
            EventBus eventBus,
            GridModel grid,
            MatchDetector matchDetector,
            ClearSystem clearSystem,
            NeighborTrigger neighborTrigger,
            PowerUpSpawner powerUpSpawner,
            FallSystem fallSystem,
            FillSystem fillSystem,
            BottomTrigger bottomTrigger,
            MovesTracker movesTracker,
            GameStateMachine stateMachine)
        {
            _eventBus = eventBus;
            _grid = grid;
            _matchDetector = matchDetector;
            _clearSystem = clearSystem;
            _neighborTrigger = neighborTrigger;
            _powerUpSpawner = powerUpSpawner;
            _fallSystem = fallSystem;
            _fillSystem = fillSystem;
            _bottomTrigger = bottomTrigger;
            _movesTracker = movesTracker;
            _stateMachine = stateMachine;

            _eventBus.Subscribe<CellClickedEvent>(OnCellClicked);
        }

        // Tıklama akışının ana giriş noktası.
        // Item türüne göre farklı senaryolara dallanıyoruz.
        private void OnCellClicked(CellClickedEvent e)
        {
            var cell = _grid.GetCell(e.Position);

            // Boş hücreye veya geçersiz pozisyona tıklandı → görmezden gel
            if (cell == null || cell.IsEmpty) return;

            var item = cell.Item;

            // Senaryo 1: Tıklanabilir item (CandyBar gibi)
            if (item is IClickable clickable)
            {
                ResolveClickableAction(e.Position, clickable);
                return;
            }

            // Senaryo 2: Match'lenebilir item (Sweet)
            if (item is IMatchable)
            {
                ResolveMatchAction(e.Position);
                return;
            }

            // Senaryo 3: Cupcake/Croissant gibi tıklamaya tepkisiz item'lar
            // Hiçbir şey yapma, hamle harcanmaz
        }

        // CandyBar tıklandığında: etkilenen hücreleri bulup patlat
        private void ResolveClickableAction(GridPosition pos, IClickable clickable)
        {
            _stateMachine.SetState(GameState.Resolving);
            _movesTracker.TryUseMove();

            clickable.OnClick();

            // CandyBar'ın etkilediği hücreleri al
            var affected = new List<CellModel>();
            if (clickable is CandyBarItem candyBar)
            {
                var affectedPositions = candyBar.GetAffectedCells(_grid.Width, _grid.Height);
                foreach (var p in affectedPositions)
                {
                    var c = _grid.GetCell(p);
                    if (c != null && !c.IsEmpty)
                        affected.Add(c);
                }
            }

            // CandyBar'ın kendisi de patlayanlar listesinde
            affected.Add(_grid.GetCell(pos));

            _clearSystem.Clear(affected);
            ResolveAfterAction();
        }

        // Sweet tıklandığında: match var mı kontrol et, varsa işle
        private void ResolveMatchAction(GridPosition pos)
        {
            var match = _matchDetector.FindMatchAt(pos);
            if (match == null) return;  // match yoksa hamle harcanmaz

            _stateMachine.SetState(GameState.Resolving);
            _movesTracker.TryUseMove();

            // 5+ match olduysa tıklanan hücreye CandyBar koy
            bool spawnedCandyBar = _powerUpSpawner.TrySpawnAt(pos, match.Count);

            // Match'in komşularındaki cupcake'leri bul
            var triggeredNeighbors = _neighborTrigger.FindTriggeredNeighbors(match);

            // Patlatılacak hücre listesini birleştir
            var toClear = new List<CellModel>(match);
            toClear.AddRange(triggeredNeighbors);

            // CandyBar yerleştirildiyse o hücre patlatılmayacak
            if (spawnedCandyBar)
                toClear.RemoveAll(c => c.Position == pos);

            _clearSystem.Clear(toClear);
            ResolveAfterAction();
        }

        // Patlatma sonrası ortak akış: fall, fill, bottom check
        private void ResolveAfterAction()
        {
            _fallSystem.ApplyFall();
            _fillSystem.FillEmpty();

            // Alta düşmüş croissant'ları yakala ve patlat
            var bottomTriggered = _bottomTrigger.FindTriggeredAtBottom();
            if (bottomTriggered.Count > 0)
            {
                _clearSystem.Clear(bottomTriggered);
                _fallSystem.ApplyFall();
                _fillSystem.FillEmpty();
            }

            // Akış bittiğinde Idle'a dön (Won/Lost'a geçtiysek olduğu yerde kal)
            if (_stateMachine.Current == GameState.Resolving)
                _stateMachine.SetState(GameState.Idle);
        }
    }
}