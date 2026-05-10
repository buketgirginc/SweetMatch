using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SweetMatch.Events;
using SweetMatch.Model;
using SweetMatch.Presentation.Game;
using UnityEngine;

namespace SweetMatch.Presentation.Animation
{
    public class BoardAnimator : MonoBehaviour
    {
        [SerializeField] private GridView gridView;

        private const float ClearDuration = 0.3f;
        private const float FallDuration = 0.3f;
        private const float FillDuration = 0.3f;
        private const float CroissantExitDuration = 0.2f;
        private const float CroissantExitDistance = 20f;

        private EventBus _eventBus;
        private IReadOnlyList<FallMove> _pendingFalls;
        private IReadOnlyList<SpawnInfo> _pendingSpawns;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ItemsFellEvent>(OnItemsFell);
            _eventBus.Subscribe<ItemsSpawnedEvent>(OnItemsSpawned);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<ItemsFellEvent>(OnItemsFell);
            _eventBus?.Unsubscribe<ItemsSpawnedEvent>(OnItemsSpawned);
        }

        // Event handler'lar payload'u cache'liyor; gerçek animasyonu Coroutine oynatıyor.
        private void OnItemsFell(ItemsFellEvent e) => _pendingFalls = e.Moves;
        private void OnItemsSpawned(ItemsSpawnedEvent e) => _pendingSpawns = e.Spawns;

        public IEnumerator PlayClearAnimation(List<CellModel> cleared)
        {
            foreach (var cell in cleared)
            {
                var cellView = gridView.GetCellView(cell.Position);
                PlayPopAndShrink(cellView);
            }
            yield return new WaitForSeconds(ClearDuration);
        }

        // Croissant'lar alta düşüp kaybolurken oynatılır.
        // bottomTriggered listesi sadece croissant içerir (BottomTrigger garantiler).
        public IEnumerator PlayCroissantExitAnimation(List<CellModel> bottomTriggered)
        {
            foreach (var cell in bottomTriggered)
            {
                var cellView = gridView.GetCellView(cell.Position);
                PlayCroissantExit(cellView);
            }
            yield return new WaitForSeconds(CroissantExitDuration);
        }

        public IEnumerator PlayFallAnimation()
        {
            if (_pendingFalls == null || _pendingFalls.Count == 0)
                yield break;

            foreach (var move in _pendingFalls)
            {
                var fromView = gridView.GetCellView(move.From);
                var toView = gridView.GetCellView(move.To);
                var t = fromView.ItemView.transform;

                // ItemView eski parent'ında kalır, dünyada hedef CellView'ın yerine uçar.
                // Animation bitince RenderAll model state'iyle hizalar.
                t.DOMove(toView.transform.position, FallDuration).SetEase(Ease.OutQuad);
            }

            yield return new WaitForSeconds(FallDuration);

            _pendingFalls = null;
            gridView.RenderAll();
        }

        public IEnumerator PlayFillAnimation()
        {
            if (_pendingSpawns == null || _pendingSpawns.Count == 0)
                yield break;

            // Önce RenderAll: yeni item'lar bind'lenir, sprite'ları yüklenir, görünür olurlar.
            gridView.RenderAll();

            float spawnOffset = gridView.CellSize + gridView.CellSpacing;

            foreach (var spawn in _pendingSpawns)
            {
                var cellView = gridView.GetCellView(spawn.Position);
                var t = cellView.ItemView.transform;
                Vector3 targetPos = t.position;

                // Bind'lenen item'ı 1 hücre yukarı kaydır, sonra hedefe düşür.
                t.position = targetPos + Vector3.up * spawnOffset;
                t.DOMove(targetPos, FillDuration).SetEase(Ease.OutQuad);
            }

            yield return new WaitForSeconds(FillDuration);

            _pendingSpawns = null;
        }

        // CandyBar gibi spawn olan tek bir item'ı görünür hale getirir.
        // Şu an sadece RenderCell çağırıyor; Commit 5'te scale-in animation eklenecek.
        public IEnumerator PlaySpawnAnimation(GridPosition pos)
        {
            gridView.RenderCell(pos);
            yield break;
        }

        // Sprite önce 1.15 katına büyür (anticipation), sonra 0'a küçülür (collapse).
        // OnComplete'te scale orijinaline geri alınır → ItemView tekrar Bind'lenebilir.
        private void PlayPopAndShrink(CellView cellView)
        {
            var itemView = cellView.ItemView;
            var t = itemView.transform;
            var originalScale = t.localScale;

            Sequence seq = DOTween.Sequence();
            seq.Append(t.DOScale(originalScale * 1.15f, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(t.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                itemView.SetVisible(false);
                t.localScale = originalScale;
            });
        }

        // Aşağı doğru hafif kayma + fade-out (paralel). 0.2s.
        // OnComplete'te position ve alpha resetlenir → ItemView tekrar Bind'lenebilir.
        private void PlayCroissantExit(CellView cellView)
        {
            var itemView = cellView.ItemView;
            var t = itemView.transform;
            var image = itemView.Image;
            var originalY = t.localPosition.y;
            var originalColor = image.color;

            Sequence seq = DOTween.Sequence();
            seq.Append(t.DOLocalMoveY(originalY - CroissantExitDistance, CroissantExitDuration));
            seq.Join(image.DOFade(0f, CroissantExitDuration));
            seq.OnComplete(() =>
            {
                itemView.SetVisible(false);
                t.localPosition = new Vector3(t.localPosition.x, originalY, t.localPosition.z);
                image.color = originalColor;
            });
        }
    }
}