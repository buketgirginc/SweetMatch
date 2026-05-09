using SweetMatch.Events;
using TMPro;
using UnityEngine;

namespace SweetMatch.Presentation.UI
{
    public class MovesView : MonoBehaviour
    {
        [SerializeField] private TMP_Text movesText;

        private EventBus _eventBus;

        public void Initialize(EventBus bus, int initialMoves)
        {
            _eventBus = bus;
            _eventBus.Subscribe<MovesChangedEvent>(OnMovesChanged);
            SetMoves(initialMoves);
        }

        private void OnMovesChanged(MovesChangedEvent e) => SetMoves(e.Remaining);

        private void SetMoves(int moves) => movesText.text = $"Moves\n{moves}";

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<MovesChangedEvent>(OnMovesChanged);
        }
    }
}