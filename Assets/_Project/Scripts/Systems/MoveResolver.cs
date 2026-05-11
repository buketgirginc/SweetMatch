using System.Collections;
using System.Collections.Generic;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Model.Items;
using SweetMatch.Presentation.Animation;
using UnityEngine;

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
        private readonly BoardAnimator _animator;
        private readonly GoalFlyController _flyController;
        private readonly MonoBehaviour _coroutineHost;

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
            GameStateMachine stateMachine,
            BoardAnimator animator,
            GoalFlyController flyController,
            MonoBehaviour coroutineHost)
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
            _animator = animator;
            _flyController = flyController;
            _coroutineHost = coroutineHost;

            _eventBus.Subscribe<CellClickedEvent>(OnCellClicked);
        }

        private void OnCellClicked(CellClickedEvent e)
        {
            _coroutineHost.StartCoroutine(OnCellClickedRoutine(e));
        }

        private IEnumerator OnCellClickedRoutine(CellClickedEvent e)
        {
            var cell = _grid.GetCell(e.Position);

            if (cell == null || cell.IsEmpty) yield break;

            var item = cell.Item;

            if (item is IClickable clickable)
            {
                yield return ResolveClickableActionRoutine(e.Position, clickable);
                yield break;
            }

            if (item is IMatchable)
            {
                yield return ResolveMatchActionRoutine(e.Position);
                yield break;
            }
        }

        private IEnumerator ResolveClickableActionRoutine(GridPosition pos, IClickable clickable)
        {
            _stateMachine.SetState(GameState.Resolving);
            _movesTracker.TryUseMove();

            clickable.OnClick();

            var affected = new List<CellModel>();
            CandyBarItem candyBar = clickable as CandyBarItem;
            if (candyBar != null)
            {
                var affectedPositions = candyBar.GetAffectedCells(_grid.Width, _grid.Height);
                foreach (var p in affectedPositions)
                {
                    var c = _grid.GetCell(p);
                    if (c != null && !c.IsEmpty && c.Item.CanBeClearedByPowerUp())
                        affected.Add(c);
                }
            }

            affected.Add(_grid.GetCell(pos));

            // CandyBar akışında item referanslarını Clear'dan önce yakalarız.
            // Clear sonrası cell.Item null olur, ama fly tetiği için item bilgisi gerekli.
            List<(CellModel cell, GridItem item)> affectedSnapshot = null;
            if (candyBar != null)
            {
                affectedSnapshot = new List<(CellModel, GridItem)>();
                foreach (var c in affected)
                    affectedSnapshot.Add((c, c.Item));
            }

            // CandyBar akışında goal fly'lar Clear anında değil, koreografi içindeki DelayedCall'larda tetiklenir.
            // ItemsClearedEvent'in fly başlatmasını geçici olarak bastırırız.
            if (candyBar != null) _flyController.SetFlySuppressed(true);
            _clearSystem.Clear(affected);
            if (candyBar != null) _flyController.SetFlySuppressed(false);

            if (candyBar != null)
                yield return _animator.PlayCandyBarActivation(candyBar, affectedSnapshot);
            else
                yield return _animator.PlayClearAnimation(affected);

            yield return ResolveAfterActionRoutine();
        }

        private IEnumerator ResolveMatchActionRoutine(GridPosition pos)
        {
            var match = _matchDetector.FindMatchAt(pos);
            if (match == null) yield break;

            _stateMachine.SetState(GameState.Resolving);
            _movesTracker.TryUseMove();

            bool spawnedCandyBar = _powerUpSpawner.TrySpawnAt(pos, match.Count);
            if (spawnedCandyBar)
                yield return _animator.PlaySpawnAnimation(pos);

            var triggeredNeighbors = _neighborTrigger.FindTriggeredNeighbors(match);

            var toClear = new List<CellModel>(match);
            toClear.AddRange(triggeredNeighbors);

            if (spawnedCandyBar)
                toClear.RemoveAll(c => c.Position == pos);

            _clearSystem.Clear(toClear);
            yield return _animator.PlayClearAnimation(toClear);

            yield return ResolveAfterActionRoutine();
        }

        private IEnumerator ResolveAfterActionRoutine()
        {
            _fallSystem.ApplyFall();
            yield return _animator.PlayFallAnimation();

            _fillSystem.FillEmpty();
            yield return _animator.PlayFillAnimation();

            while (true)
            {
                var bottomTriggered = _bottomTrigger.FindTriggeredAtBottom();
                if (bottomTriggered.Count == 0) break;

                _clearSystem.Clear(bottomTriggered);
                yield return _animator.PlayCroissantExitAnimation(bottomTriggered);

                _fallSystem.ApplyFall();
                yield return _animator.PlayFallAnimation();

                _fillSystem.FillEmpty();
                yield return _animator.PlayFillAnimation();
            }

            if (_stateMachine.Current == GameState.Resolving)
                _stateMachine.SetState(GameState.Idle);
        }
    }
}