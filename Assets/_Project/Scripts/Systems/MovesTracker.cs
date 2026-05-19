using SweetMatch.Events;

namespace SweetMatch.Systems
{
    public class MovesTracker
    {
        private readonly EventBus _eventBus;

        public int Remaining { get; private set; }

        // Hamle kalmadı mı? Lose değerlendirmesi için MoveResolver akış sonunda okur.
        public bool IsOutOfMoves => Remaining <= 0;

        public MovesTracker(int initialMoves, EventBus eventBus)
        {
            Remaining = initialMoves;
            _eventBus = eventBus;
        }

        // Bir hamle harcar. Hamle yoksa false döner.
        // NoMovesLeftEvent BURADA raise EDİLMEZ — hamlenin sonucu (goal tamamlandı mı?)
        // henüz belli değil. Lose kararı hamle tamamen çözüldükten sonra MoveResolver'da
        // verilir; o ana kadar goal tamamlandıysa zaten Won olur (win önceliği, race yok).
        public bool TryUseMove()
        {
            if (Remaining <= 0) return false;

            Remaining--;
            _eventBus.Raise(new MovesChangedEvent(Remaining));
            return true;
        }
    }
}