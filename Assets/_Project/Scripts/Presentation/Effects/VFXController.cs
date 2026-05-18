using SweetMatch.Events;
using SweetMatch.Model.Items;
using SweetMatch.Presentation.Game;
using UnityEngine;

namespace SweetMatch.Presentation.Effects
{
    // Event-driven VFX playback. 3 farklı particle prefab Inspector'dan atanır.
    // Particle'lar Canvas'a parent'lanır (Canvas Screen Space - Camera lossyScale'i
    // otomatik uygulansın diye), prefab değerleri Canvas-local pixel cinsinden tasarlanır.
    // Croissant ve CandyBar için VFX yok.
    public class VFXController : MonoBehaviour
    {
        [SerializeField] private GridView gridView;
        [SerializeField] private RectTransform canvasRect;

        [Header("VFX Prefabs")]
        [SerializeField] private ParticleSystem sweetPopPrefab;
        [SerializeField] private ParticleSystem cupcakePopPrefab;
        [SerializeField] private ParticleSystem goalCollectPrefab;

        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ItemsClearedEvent>(OnItemsCleared);
            _eventBus.Subscribe<GoalCollectedEvent>(OnGoalCollected);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<ItemsClearedEvent>(OnItemsCleared);
            _eventBus?.Unsubscribe<GoalCollectedEvent>(OnGoalCollected);
        }

        private void OnItemsCleared(ItemsClearedEvent e)
        {
            foreach (var item in e.ClearedItems)
            {
                var cellView = gridView.GetCellView(item.Position);
                if (cellView == null) continue;

                Vector3 worldPos = cellView.transform.position;

                if (item is SweetItem)
                    SpawnVFX(sweetPopPrefab, worldPos);
                else if (item is CupcakeItem)
                    SpawnVFX(cupcakePopPrefab, worldPos);
            }
        }

        private void OnGoalCollected(GoalCollectedEvent e)
        {
            SpawnVFX(goalCollectPrefab, e.WorldPosition);
        }

        // Particle'ı Canvas'a parent'la → lossyScale otomatik uygulanır, grid'le aynı ölçek.
        // worldPositionStays:false → SetParent sonrası world pos'u biz set ederiz.
        private void SpawnVFX(ParticleSystem prefab, Vector3 worldPosition)
        {
            if (prefab == null) return;
            var instance = Instantiate(prefab, canvasRect);
            instance.transform.SetAsLastSibling();
            instance.transform.position = worldPosition;
        }
    }
}