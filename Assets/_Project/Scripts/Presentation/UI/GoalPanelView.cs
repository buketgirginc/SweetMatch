using System.Collections.Generic;
using SweetMatch.Data;
using SweetMatch.Events;
using UnityEngine;

namespace SweetMatch.Presentation.UI
{
    public class GoalPanelView : MonoBehaviour
    {
        [SerializeField] private GoalItemView goalItemPrefab;
        [SerializeField] private Transform goalContainer;

        private EventBus _eventBus;
        private readonly Dictionary<string, GoalItemView> _goalViews = new();

        public void Initialize(EventBus bus, LevelConfigSO levelConfig, ItemVisualConfigSO visualConfig)
        {
            _eventBus = bus;

            foreach (var goal in levelConfig.Goals)
            {
                var view = Instantiate(goalItemPrefab, goalContainer);
                view.Setup(goal, visualConfig);
                _goalViews[goal.Signature] = view;
            }

            _eventBus.Subscribe<GoalProgressEvent>(OnGoalProgress);
        }

        // Sweet'lerin sayaç güncellemesi BoardAnimator'a delege edildi (fly bitiminde DecrementGoal).
        // Cupcake, Croissant gibi special item'lar fly oynamaz; sayaç event üzerinden anında düşer.
        private void OnGoalProgress(GoalProgressEvent e)
        {
            if (e.Signature.StartsWith("sweet:")) return;
            if (_goalViews.TryGetValue(e.Signature, out var view))
                view.UpdateRemaining(e.Remaining);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<GoalProgressEvent>(OnGoalProgress);
        }

        // Goal item'ının view'ına dışarıdan erişim (BoardAnimator fly target için).
        public GoalItemView GetGoalView(string signature)
        {
            _goalViews.TryGetValue(signature, out var view);
            return view;
        }

        // Sayacı 1 azaltır. BoardAnimator fly tamamlanınca çağırır (sweet'ler için).
        public void DecrementGoal(string signature)
        {
            if (_goalViews.TryGetValue(signature, out var view))
                view.Decrement();
        }
    }
}