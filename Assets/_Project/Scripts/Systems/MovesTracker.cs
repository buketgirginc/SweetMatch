using SweetMatch.Events;

namespace SweetMatch.Systems
{
    public class MovesTracker
    {
        private readonly EventBus _eventBus;

        public int Remaining { get; private set; }

        public MovesTracker(int initialMoves, EventBus eventBus)
        {
            Remaining = initialMoves;
            _eventBus = eventBus;
        }

        // Bir hamle harcar. Hamle yoksa false döner.
        // Caller (MoveResolver) bunu match yapıldığında çağırır.
        public bool TryUseMove()
        {
            if (Remaining <= 0) return false;

            Remaining--;
            _eventBus.Raise(new MovesChangedEvent(Remaining));

            // Hamle bittiyse ayrıca özel event fırlat — GameStateMachine bunu dinliyor
            if (Remaining == 0)
                _eventBus.Raise(new NoMovesLeftEvent());

            return true;
        }
    }
}