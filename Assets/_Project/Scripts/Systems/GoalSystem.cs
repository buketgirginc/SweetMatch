using System.Collections.Generic;
using System.Linq;
using SweetMatch.Data;
using SweetMatch.Events;

namespace SweetMatch.Systems
{
    public class GoalSystem
    {
        private readonly EventBus _eventBus;
        private readonly Dictionary<string, int> _remainingGoals;

        public GoalSystem(IReadOnlyList<GoalEntry> goals, EventBus eventBus)
        {
            _eventBus = eventBus;
            _remainingGoals = new Dictionary<string, int>();

            foreach (var goal in goals)
                _remainingGoals[goal.Signature] = goal.Count;

            _eventBus.Subscribe<ItemsClearedEvent>(OnItemsCleared);
        }

        // Patlayan item'ları goal listesiyle karşılaştırıp sayaçları azaltır.
        // Eşleşmeyen item'lar (CandyBar gibi null signature) görmezden gelinir.
        private void OnItemsCleared(ItemsClearedEvent e)
        {
            foreach (var item in e.ClearedItems)
            {
                string sig = item.GetGoalSignature();
                if (sig == null) continue;

                // Goal listesinde olmayan signature'ları atla
                if (!_remainingGoals.ContainsKey(sig)) continue;
                if (_remainingGoals[sig] <= 0) continue;

                _remainingGoals[sig]--;
                _eventBus.Raise(new GoalProgressEvent(sig, _remainingGoals[sig]));
            }

            if (AllGoalsComplete())
                _eventBus.Raise(new AllGoalsCompletedEvent());
        }

        private bool AllGoalsComplete()
        {
            return _remainingGoals.Values.All(v => v <= 0);
        }
    }
}