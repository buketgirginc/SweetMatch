using SweetMatch.Events;
using UnityEngine;

namespace SweetMatch.Systems
{
    public class GameStateMachine
    {
        private readonly EventBus _eventBus;

        public GameState Current { get; private set; }

        public GameStateMachine(EventBus eventBus)
        {
            _eventBus = eventBus;
            Current = GameState.Loading;

            _eventBus.Subscribe<AllGoalsCompletedEvent>(OnAllGoalsCompleted);
            _eventBus.Subscribe<NoMovesLeftEvent>(OnNoMovesLeft);
        }

        // State değişimi tek noktadan. Geçerli olmayan geçişleri reddediyoruz.
        public void SetState(GameState newState)
        {
            if (Current == newState) return;

            if (!IsValidTransition(Current, newState))
            {
                Debug.LogWarning($"Invalid state transition: {Current} → {newState}");
                return;
            }

            var previous = Current;
            Current = newState;

            _eventBus.Raise(new GameStateChangedEvent(previous, newState));
        }

        // Geçiş kuralları. Win terminal ve önceliklidir: Lost→Won bir güvenlik ağıdır
        // (normal akışta MoveResolver lose'u win'den sonra değerlendirdiği için tetiklenmez,
        // ama CandyBar/croissant zinciri gibi edge-case'lerde win'in lose'u ezmesini garanti eder).
        // Won→Lost asla geçerli değil — kazanılan oyun kaybedilemez.
        private bool IsValidTransition(GameState from, GameState to)
        {
            return (from, to) switch
            {
                (GameState.Loading, GameState.Idle) => true,
                (GameState.Idle, GameState.Resolving) => true,
                (GameState.Resolving, GameState.Idle) => true,
                (GameState.Resolving, GameState.Won) => true,
                (GameState.Resolving, GameState.Lost) => true,
                (GameState.Lost, GameState.Won) => true,
                _ => false
            };
        }

        private void OnAllGoalsCompleted(AllGoalsCompletedEvent e)
        {
            SetState(GameState.Won);
        }

        // Win öncelikli: zaten kazanmışsak hamle bitse de Lost'a geçme.
        // (MoveResolver zaten Won'sa NoMovesLeftEvent raise etmiyor, bu ikinci güvenlik.)
        private void OnNoMovesLeft(NoMovesLeftEvent e)
        {
            if (Current != GameState.Won)
                SetState(GameState.Lost);
        }
    }
}