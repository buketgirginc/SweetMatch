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

        private void OnGoalProgress(GoalProgressEvent e)
        {
            if (_goalViews.TryGetValue(e.Signature, out var view))
                view.UpdateRemaining(e.Remaining);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<GoalProgressEvent>(OnGoalProgress);
        }
    }
}