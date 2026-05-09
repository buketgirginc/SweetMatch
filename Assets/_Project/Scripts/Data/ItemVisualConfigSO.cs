using System;
using System.Collections.Generic;
using UnityEngine;

namespace SweetMatch.Data
{
    [CreateAssetMenu(fileName = "ItemVisualConfig", menuName = "SweetMatch/ItemVisualConfig")]
    public class ItemVisualConfigSO : ScriptableObject
    {
        [Serializable]
        public class VisualEntry
        {
            public string key;
            public Sprite sprite;
            public float scale = 1f;
            public float rotation = 0f;
        }

        [SerializeField] private List<VisualEntry> entries;
        private Dictionary<string, VisualEntry> _lookup;

        private void OnEnable()
        {
            _lookup = new Dictionary<string, VisualEntry>();
            if (entries == null) return;
            foreach (var entry in entries)
                if (!string.IsNullOrEmpty(entry.key))
                    _lookup[entry.key] = entry;
        }

        public VisualEntry Get(string key)
        {
            return _lookup != null && _lookup.TryGetValue(key, out var entry) ? entry : null;
        }
    }
}