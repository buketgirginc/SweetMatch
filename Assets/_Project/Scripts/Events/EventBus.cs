using System;
using System.Collections.Generic;
using UnityEngine;

namespace SweetMatch.Events
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);

            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);

            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public void Raise<TEvent>(TEvent eventData)
        {
            var type = typeof(TEvent);

            if (!_handlers.TryGetValue(type, out var list)) return;

            var snapshot = list.ToArray();

            foreach (var handler in snapshot)
            {
                try
                {
                    ((Action<TEvent>)handler).Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EventBus handler exception: {ex}");
                }
            }
        }
    }
}