using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SweetMatch.Model;
using SweetMatch.Presentation.Game;
using UnityEngine;

namespace SweetMatch.Presentation.Animation
{
    public class BoardAnimator : MonoBehaviour
    {
        [SerializeField] private GridView gridView;

        private const float ClearDuration = 0.3f;

        public IEnumerator PlayClearAnimation(List<CellModel> cleared)
        {
            foreach (var cell in cleared)
            {
                var cellView = gridView.GetCellView(cell.Position);
                PlayPopAndShrink(cellView);
            }
            yield return new WaitForSeconds(ClearDuration);
        }

        public IEnumerator PlayFallAnimation()
        {
            gridView.RenderAll();
            yield break;
        }

        public IEnumerator PlayFillAnimation()
        {
            gridView.RenderAll();
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
    }
}